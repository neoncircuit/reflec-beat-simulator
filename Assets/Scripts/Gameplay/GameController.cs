using System.Collections.Generic;
using ReflecBeatSimulator.Audio;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.UI;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Drives the M1 gameplay loop: spawn notes, judge touches, track score, finish.</summary>
    public sealed class GameController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _audioClip;

        [Header("Playfield")]
        [SerializeField] private PlayfieldRenderer _playfield;
        [SerializeField] private TouchInput _touchInput;
        [SerializeField] private GameObject _tapNotePrefab;

        [Header("UI")]
        [SerializeField] private GameplayUI _gameplayUI;
        [SerializeField] private ResultsUI _resultsUI;

        [Header("Timing")]
        [SerializeField] private double _approachTime = Constants.DefaultApproachTime;
        [SerializeField] private double _scheduleDelay = 0.5;

        private Chart _chart;
        private SongClock _songClock;
        private Score _score;
        private double _startDspTime;
        private double _audioOffsetSec;
        private double _inputOffsetSec;

        private readonly List<NoteObject> _activeNotes = new List<NoteObject>();
        private readonly HashSet<Note> _judgedNotes = new HashSet<Note>();
        private readonly HashSet<Note> _spawnedNotes = new HashSet<Note>();

        private bool _started;
        private bool _finished;

        private void Start()
        {
            if (_audioSource == null || _audioClip == null)
            {
                Debug.LogError("GameController: AudioSource or AudioClip is not assigned.");
                enabled = false;
                return;
            }

            _chart = BuildDemoChart();
            new TimingConverter(_chart.Timing).ApplyTimes(_chart.Notes);
            _score = new Score(_chart.Notes.Count);

            if (_touchInput != null)
                _touchInput.OnTouchBegan += HandleTouchBegan;

            _inputOffsetSec = CalibrationStore.Load();
            _audioOffsetSec = _chart.Timing.AudioOffsetMs / 1000.0;
            _startDspTime = AudioPlayback.Schedule(_audioSource, _audioClip, _scheduleDelay);
            _songClock = new SongClock(_startDspTime, _audioOffsetSec);
            _started = true;
        }

        private void OnDestroy()
        {
            if (_touchInput != null)
                _touchInput.OnTouchBegan -= HandleTouchBegan;
        }

        private void Update()
        {
            if (!_started || _finished)
                return;

            double songTime = _songClock.Tick(Time.unscaledTimeAsDouble);
            SpawnNotes(songTime);
            UpdateNotePositions(songTime);
            CheckMisses(songTime);
            CheckFinish(songTime);
        }

        private void SpawnNotes(double songTime)
        {
            foreach (var note in _chart.Notes)
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
            if (_tapNotePrefab == null || _playfield == null)
                return;

            Vector3 spawnPos = _playfield.GetSpawnPosition(note.X);
            GameObject go = Instantiate(_tapNotePrefab, spawnPos, Quaternion.identity);
            var noteObject = go.GetComponent<NoteObject>();
            if (noteObject == null)
            {
                Debug.LogError("Tap note prefab is missing a NoteObject component.");
                Destroy(go);
                return;
            }

            noteObject.Initialize(note, _playfield, _approachTime);
            _activeNotes.Add(noteObject);
        }

        private void UpdateNotePositions(double songTime)
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var noteObject = _activeNotes[i];
                if (noteObject == null || noteObject.IsJudged)
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

            double adjustedTouchTime = touch.DspTime - _startDspTime - _audioOffsetSec - _inputOffsetSec;
            var candidates = CollectUnjudgedCandidates();
            var result = Judge.JudgeTouch(adjustedTouchTime, touch.NormalizedX, candidates);

            if (result.HasValue)
                ApplyJudgment(result.Value.note, result.Value.grade);
        }

        private IEnumerable<Note> CollectUnjudgedCandidates()
        {
            foreach (var noteObject in _activeNotes)
            {
                if (noteObject == null || noteObject.IsJudged)
                    continue;

                Note note = noteObject.Note;
                if (!_judgedNotes.Contains(note))
                    yield return note;
            }
        }

        private void CheckMisses(double songTime)
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var noteObject = _activeNotes[i];
                if (noteObject == null || noteObject.IsJudged)
                    continue;

                if (songTime > noteObject.Note.TimeSec + Constants.GoodWindow)
                {
                    ApplyJudgment(noteObject.Note, Judgment.Miss);
                    noteObject.MarkJudged();
                    _activeNotes.RemoveAt(i);
                }
            }
        }

        private void ApplyJudgment(Note note, Judgment grade)
        {
            if (!_judgedNotes.Add(note))
                return;

            _score.Apply(grade);
            _gameplayUI?.SetScore(_score);

            foreach (var noteObject in _activeNotes)
            {
                if (noteObject != null && noteObject.Note == note)
                {
                    noteObject.MarkJudged();
                    break;
                }
            }
        }

        private void CheckFinish(double songTime)
        {
            if (_judgedNotes.Count < _chart.Notes.Count)
                return;

            _finished = true;

            if (_gameplayUI != null)
                _gameplayUI.gameObject.SetActive(false);

            foreach (var noteObject in _activeNotes)
            {
                if (noteObject != null)
                    Destroy(noteObject.gameObject);
            }
            _activeNotes.Clear();

            _resultsUI?.Show(_score);
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

            // 12 beats at 120 BPM, matching Assets/Audio/PlaceholderBeat.wav.
            const float DemoStepX = 1f / 6f;
            for (int i = 0; i < 12; i++)
            {
                float x = 0.25f + (i % 4) * DemoStepX; // 0.25, 0.4167, 0.5833, 0.75, repeats
                chart.Notes.Add(new Note
                {
                    Tick = i * Constants.Ppq,
                    Type = NoteType.Tap,
                    X = x
                });
            }

            return chart;
        }
    }
}
