using System.Collections.Generic;
using System.Linq;
using ReflecBeatSimulator.Audio;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.UI;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Drives the gameplay loop: spawn notes, judge touches, track score, finish.</summary>
    public sealed class GameController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _audioClip;

        [Header("Playfield")]
        [SerializeField] private PlayfieldRenderer _playfield;
        [SerializeField] private TouchInput _touchInput;
        [SerializeField] private GameObject _tapNotePrefab;
        [SerializeField] private GameObject _longNotePrefab;
        [SerializeField] private GameObject _verticalNotePrefab;
        [SerializeField] private GameObject _topNotePrefab;
        [SerializeField] private GameObject _bigBangNotePrefab;

        [Header("UI")]
        [SerializeField] private GameplayUI _gameplayUI;
        [SerializeField] private ResultsUI _resultsUI;
        [SerializeField] private DuelUI _duelUI;

        [Header("Timing")]
        [SerializeField] private double _approachTime = Constants.DefaultApproachTime;
        [SerializeField] private double _scheduleDelay = 0.5;

        private Chart _chart;
        private SongClock _songClock;
        private Score _score;
        private double _startDspTime;
        private double _audioOffsetSec;
        private double _inputOffsetSec;
        private bool _autoplay;
        private bool _syncCheck;
        private SyncReport _syncReport;
        private NoteObjectPool _notePool;

        private List<Note> _sessionNotes;
        private bool _isDuel;
        private DuelState _duelState;
        private CpuRival _cpuRival;

        private readonly List<NoteObject> _activeNotes = new List<NoteObject>();
        private readonly HashSet<Note> _judgedNotes = new HashSet<Note>();
        private readonly HashSet<Note> _spawnedNotes = new HashSet<Note>();

        private readonly Dictionary<int, (NoteObject noteObject, Note note, Judgment headGrade)> _heldLongNotes =
            new Dictionary<int, (NoteObject, Note, Judgment)>();

        private readonly HashSet<Note> _heldLongNotesByNote = new HashSet<Note>();
        private readonly HashSet<Note> _autoplayDownSent = new HashSet<Note>();
        private readonly Dictionary<Note, int> _autoplayLongTouchIds = new Dictionary<Note, int>();
        private int _nextSyntheticTouchId = -1;

        private bool _started;
        private bool _finished;

        private void Start()
        {
            if (_audioSource == null)
            {
                Debug.LogError("GameController: AudioSource is not assigned.");
                enabled = false;
                return;
            }

            _chart = ChartSession.HasChart ? ChartSession.Chart : BuildDemoChart();
            if (ChartSession.AudioClip != null)
                _audioClip = ChartSession.AudioClip;

            if (_audioClip == null)
            {
                Debug.LogError("GameController: No audio clip assigned or selected in ChartSession.");
                enabled = false;
                return;
            }

            _autoplay = ChartSession.Autoplay;
            _syncCheck = ChartSession.SyncCheck;
            if (_syncCheck)
            {
                _autoplay = true;
                _syncReport = new SyncReport();
            }

            new TimingConverter(_chart.Timing).ApplyTimes(_chart.Notes);
            _sessionNotes = _chart.Notes;

            _approachTime = ChartSession.IsPreview
                ? Constants.DefaultApproachTime
                : ChartSession.ApproachTime;
            if (_approachTime <= 0.0)
                _approachTime = Constants.DefaultApproachTime;

            if (ChartSession.IsPreview)
            {
                double previewStart = ChartSession.PreviewStartSec;
                double previewEnd = ChartSession.PreviewEndSec;
                if (previewStart >= previewEnd)
                {
                    Debug.LogError($"GameController: Invalid preview range [{previewStart}, {previewEnd}).");
                    enabled = false;
                    return;
                }

                _sessionNotes = ChartSection.GetNotesInRange(_chart.Notes, previewStart, previewEnd);
                _autoplay = true;
            }

            _isDuel = !ChartSession.IsPreview && HasRivalNotes(_sessionNotes);
            int scoreableNotes = _isDuel ? CountPlayerNotes(_sessionNotes) : _sessionNotes.Count;
            _score = new Score(scoreableNotes);

            if (_touchInput != null)
            {
                _touchInput.OnTouchBegan += HandleTouchBegan;
                _touchInput.OnTouchEnded += HandleTouchEnded;
            }

            if (_isDuel)
            {
                RivalConfig rivalConfig = _chart.Rival ?? RivalConfig.Default;
                _duelState = new DuelState(rivalConfig);
                _cpuRival = new CpuRival(_sessionNotes, rivalConfig);
                _duelUI?.Initialize(
                    _duelState.PlayerHp, _duelState.MaxPlayerHp,
                    _duelState.RivalHp, _duelState.MaxRivalHp);
                if (_duelUI != null)
                    _duelUI.OnReflectRequested += ActivateJustReflect;
            }

            _inputOffsetSec = CalibrationStore.Load();
            double chartOffsetSec = _chart.Timing.AudioOffsetMs / 1000.0;

            var prefabsByType = new Dictionary<string, GameObject>
            {
                { NoteType.Tap, _tapNotePrefab },
                { NoteType.Long, _longNotePrefab },
                { NoteType.Vertical, _verticalNotePrefab },
                { NoteType.Top, _topNotePrefab },
                { NoteType.BigBang, _bigBangNotePrefab }
            };
            _notePool = new NoteObjectPool(prefabsByType, transform);

            if (ChartSession.IsPreview)
            {
                _audioOffsetSec = chartOffsetSec - ChartSession.PreviewStartSec;
                _startDspTime = AudioPlayback.Schedule(_audioSource, _audioClip, _scheduleDelay, ChartSession.PreviewStartSec);
            }
            else
            {
                _audioOffsetSec = chartOffsetSec;
                _startDspTime = AudioPlayback.Schedule(_audioSource, _audioClip, _scheduleDelay);
            }

            _songClock = new SongClock(_startDspTime, _audioOffsetSec);
            _started = true;
        }

        private void OnDestroy()
        {
            if (_touchInput != null)
            {
                _touchInput.OnTouchBegan -= HandleTouchBegan;
                _touchInput.OnTouchEnded -= HandleTouchEnded;
            }

            _notePool?.Clear();
        }

        private void Update()
        {
            if (!_started || _finished)
                return;

            double songTime = _songClock.Tick(Time.unscaledTimeAsDouble);
            if (_syncReport != null)
                _syncReport.RecordClockDriftMs(Math.Abs(_songClock.LastDrift) * 1000.0);

            SpawnNotes(songTime);
            UpdateNotePositions(songTime);

            if (_autoplay)
                ProcessAutoplay(songTime);

            if (_isDuel)
                ProcessCpuRival(songTime);

            CheckMisses(songTime);
            CheckFinish(songTime);
        }

        private void SpawnNotes(double songTime)
        {
            foreach (var note in _sessionNotes)
            {
                if (_spawnedNotes.Contains(note))
                    continue;

                if (note.TimeSec - songTime <= _approachTime)
                {
                    SpawnNote(note);
                    _spawnedNotes.Add(note);
                }
            }
        }

        private void SpawnNote(Note note)
        {
            if (_playfield == null)
                return;

            if (ChartSession.HideRivalVisuals && note.IsRival)
                return;

            if (_notePool == null)
            {
                Debug.LogError("GameController: NoteObjectPool is not initialized.");
                return;
            }

            float visualX = GetVisualX(note);
            Vector3 spawnPos = note.Type == NoteType.Top
                ? _playfield.GetTopSpawnPosition(note.Zone)
                : _playfield.GetSpawnPosition(visualX);

            NoteObject noteObject = _notePool.Get(note.Type, spawnPos);
            if (noteObject == null)
            {
                Debug.LogError($"GameController: NoteObjectPool failed to provide a note for type '{note.Type}'.");
                return;
            }

            noteObject.Initialize(note, _playfield, _approachTime, visualX, _notePool);
            _activeNotes.Add(noteObject);
        }

        private static float GetVisualX(Note note)
        {
            if (note.Type == NoteType.BigBang)
                return 0.5f;

            if (note.Type == NoteType.Top)
            {
                return note.Zone switch
                {
                    0 => 1f / 6f,
                    2 => 5f / 6f,
                    _ => 0.5f
                };
            }

            return note.X;
        }

        private void UpdateNotePositions(double songTime)
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var noteObject = _activeNotes[i];
                if (noteObject == null || !noteObject.gameObject.activeInHierarchy)
                {
                    _activeNotes.RemoveAt(i);
                    continue;
                }

                noteObject.UpdatePosition(songTime);
            }
        }

        private void HandleTouchBegan(TouchData touch)
        {
            if (!_started || _finished)
                return;

            double adjustedTouchTime = ToSongTime(touch.DspTime);
            var candidates = CollectUnjudgedCandidates();
            var result = Judge.JudgeTouch(adjustedTouchTime, touch.NormalizedX, candidates);

            if (!result.HasValue)
                return;

            var (note, grade) = result.Value;
            if (note.Type == NoteType.Long)
            {
                if (_heldLongNotesByNote.Contains(note))
                    return;

                NoteObject noteObject = FindActiveNoteObject(note);
                if (noteObject == null)
                    return;

                noteObject.MarkHeld();
                _heldLongNotes[touch.TouchId] = (noteObject, note, grade);
                _heldLongNotesByNote.Add(note);
            }
            else
            {
                ApplyJudgment(note, grade);
            }
        }

        private void HandleTouchEnded(TouchData touch)
        {
            if (!_started || _finished)
                return;

            if (!_heldLongNotes.TryGetValue(touch.TouchId, out var held))
                return;

            _heldLongNotes.Remove(touch.TouchId);
            _heldLongNotesByNote.Remove(held.note);

            if (held.noteObject == null)
                return;

            double adjustedReleaseTime = ToSongTime(touch.DspTime);
            Judgment releaseGrade = Judge.JudgeRelease(adjustedReleaseTime, held.note);
            Judgment finalGrade = Judge.Worse(held.headGrade, releaseGrade);
            ApplyJudgment(held.note, finalGrade);
        }

        private IEnumerable<Note> CollectUnjudgedCandidates()
        {
            foreach (var noteObject in _activeNotes)
            {
                if (noteObject == null || noteObject.IsJudged)
                    continue;

                Note note = noteObject.Note;
                if (note.IsRival)
                    continue;

                if (!_judgedNotes.Contains(note))
                    yield return note;
            }
        }

        private NoteObject FindActiveNoteObject(Note note)
        {
            foreach (var noteObject in _activeNotes)
            {
                if (noteObject != null && noteObject.Note == note)
                    return noteObject;
            }

            return null;
        }

        private void ProcessAutoplay(double songTime)
        {
            foreach (var noteObject in _activeNotes)
            {
                if (noteObject == null || noteObject.IsJudged)
                    continue;

                Note note = noteObject.Note;
                if (note.IsRival)
                    continue;

                if (_autoplayDownSent.Contains(note))
                {
                    if (note.Type == NoteType.Long &&
                        _autoplayLongTouchIds.TryGetValue(note, out int touchId))
                    {
                        if (songTime >= note.EndTimeSec)
                        {
                            double releaseDsp = ToDspTime(note.EndTimeSec);
                            HandleTouchEnded(new TouchData(touchId, GetVisualX(note), releaseDsp));
                            _autoplayLongTouchIds.Remove(note);
                        }
                    }

                    continue;
                }

                if (songTime < note.TimeSec)
                    continue;

                int syntheticTouchId = _nextSyntheticTouchId--;
                double dsp = ToDspTime(note.TimeSec);
                if (_syncReport != null)
                {
                    double deltaMs = Math.Abs(AudioSettings.dspTime - dsp) * 1000.0;
                    _syncReport.RecordTimingDeltaMs(deltaMs);
                }

                HandleTouchBegan(new TouchData(syntheticTouchId, GetVisualX(note), dsp));
                _autoplayDownSent.Add(note);

                if (note.Type == NoteType.Long)
                    _autoplayLongTouchIds[note] = syntheticTouchId;
            }
        }

        private void CheckMisses(double songTime)
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var noteObject = _activeNotes[i];
                if (noteObject == null || noteObject.IsJudged)
                    continue;

                Note note = noteObject.Note;
                if (note.IsRival)
                    continue;

                if (note.Type == NoteType.Long)
                {
                    if (_heldLongNotesByNote.Contains(note))
                    {
                        if (songTime > note.EndTimeSec + Constants.GoodWindow)
                        {
                            ForceLongNoteMiss(note);
                            _activeNotes.RemoveAt(i);
                        }
                        continue;
                    }

                    if (songTime > note.EndTimeSec + Constants.GoodWindow)
                    {
                        ApplyJudgment(note, Judgment.Miss);
                        _activeNotes.RemoveAt(i);
                    }
                }
                else
                {
                    if (songTime > note.TimeSec + Constants.GoodWindow)
                    {
                        ApplyJudgment(note, Judgment.Miss);
                        _activeNotes.RemoveAt(i);
                    }
                }
            }
        }

        private void ApplyJudgment(Note note, Judgment grade)
        {
            if (!_judgedNotes.Add(note))
                return;

            _syncReport?.RecordJudged();
            if (grade == Judgment.Miss)
                _syncReport?.RecordMiss();

            _score.Apply(grade);
            if (_isDuel && !note.IsRival)
                _duelState.OnPlayerJudgment(note, grade);

            _gameplayUI?.SetScore(_score);
            _gameplayUI?.ShowJudgment(grade);

            if (_playfield != null)
            {
                if (note.Type == NoteType.Top)
                    _playfield.FlashTopCircle(note.Zone);
                else
                    _playfield.FlashJudgmentLine();
            }

            SyncDuelUI();

            foreach (var noteObject in _activeNotes)
            {
                if (noteObject != null && noteObject.Note == note)
                {
                    noteObject.MarkJudged();
                    break;
                }
            }
        }

        private void ForceLongNoteMiss(Note note)
        {
            int? touchIdToRemove = null;
            foreach (var pair in _heldLongNotes)
            {
                if (pair.Value.note == note)
                {
                    touchIdToRemove = pair.Key;
                    break;
                }
            }

            if (touchIdToRemove.HasValue)
            {
                _heldLongNotes.Remove(touchIdToRemove.Value);
                _heldLongNotesByNote.Remove(note);
            }

            ApplyJudgment(note, Judgment.Miss);
        }

        private void CheckFinish(double songTime)
        {
            bool allJudged = _judgedNotes.Count >= _sessionNotes.Count;

            if (_isDuel)
            {
                if (!_duelState.IsFinished && !allJudged)
                    return;

                _finished = true;
                DuelOutcome outcome = _duelState.ResolveOutcome();

                if (_audioSource != null)
                    _audioSource.Stop();

                if (_gameplayUI != null)
                    _gameplayUI.gameObject.SetActive(false);

                _duelUI?.gameObject.SetActive(false);

                foreach (var noteObject in _activeNotes)
                {
                    if (noteObject != null)
                        noteObject.ReturnToPool();
                }
                _activeNotes.Clear();
                _notePool?.Clear();

                _resultsUI?.Show(_score, outcome, _syncReport);
                return;
            }

            if (_judgedNotes.Count < _sessionNotes.Count)
                return;

            _finished = true;
            if (_audioSource != null)
                _audioSource.Stop();

            if (_gameplayUI != null)
                _gameplayUI.gameObject.SetActive(false);

            foreach (var noteObject in _activeNotes)
            {
                if (noteObject != null)
                    noteObject.ReturnToPool();
            }
            _activeNotes.Clear();
            _notePool?.Clear();

            _resultsUI?.Show(_score, null, _syncReport);
        }

        private double ToSongTime(double dspTime)
        {
            return dspTime - _startDspTime - _audioOffsetSec - _inputOffsetSec;
        }

        private double ToDspTime(double songTime)
        {
            return _startDspTime + songTime + _audioOffsetSec + _inputOffsetSec;
        }

        private void ProcessCpuRival(double songTime)
        {
            if (!_isDuel || _duelState == null || _duelState.IsFinished)
                return;

            _cpuRival?.Process(songTime, OnCpuJudgment);
            SyncDuelUI();
        }

        private void OnCpuJudgment(Note note, Judgment grade)
        {
            if (!_judgedNotes.Add(note))
                return;

            _duelState?.OnCpuJudgment(note, grade);

            foreach (var noteObject in _activeNotes)
            {
                if (noteObject != null && noteObject.Note == note)
                {
                    noteObject.MarkJudged();
                    break;
                }
            }

            SyncDuelUI();
        }

        private void ActivateJustReflect()
        {
            if (!_isDuel || _duelState == null)
                return;

            if (_duelState.ActivateJustReflect())
            {
                SyncDuelUI();
                if (_duelState.IsFinished)
                {
                    double songTime = _songClock.Tick(Time.unscaledTimeAsDouble);
                    CheckFinish(songTime);
                }
            }
        }

        private void SyncDuelUI()
        {
            if (_duelUI == null || _duelState == null)
                return;

            _duelUI.SetPlayerHp(_duelState.PlayerHp, _duelState.MaxPlayerHp);
            _duelUI.SetRivalHp(_duelState.RivalHp, _duelState.MaxRivalHp);
            _duelUI.SetGauge(_duelState.Gauge, Constants.MaxGauge);
        }

        private static bool HasRivalNotes(List<Note> notes)
        {
            return notes.Any(n => n.IsRival);
        }

        private static int CountPlayerNotes(List<Note> notes)
        {
            return notes.Count(n => !n.IsRival);
        }

        private static Chart BuildDemoChart()
        {
            var chart = new Chart
            {
                Meta = new ChartMeta
                {
                    Title = "M1 Demo",
                    Artist = "Original",
                    Charter = "simulator",
                    AudioFile = "PlaceholderBeat.wav",
                    Difficulty = "EASY",
                    Level = 1
                },
                Timing = new ChartTiming
                {
                    Ppq = Constants.Ppq,
                    AudioOffsetMs = 0,
                    BpmChanges = new List<BpmChange>
                    {
                        new BpmChange { Tick = 0, Bpm = 120 }
                    }
                }
            };

            const int LeadInBeats = 2;
            const float DemoStepX = 1f / 6f;
            for (int i = 0; i < 12; i++)
            {
                float x = 0.25f + (i % 4) * DemoStepX;
                chart.Notes.Add(new Note
                {
                    Tick = (i + LeadInBeats) * Constants.Ppq,
                    Type = NoteType.Tap,
                    X = x
                });
            }

            return chart;
        }
    }
}
