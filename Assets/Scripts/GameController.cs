using System.Collections.Generic;
using UnityEngine;

namespace ReflecBeat
{
    /// <summary>
    /// Central game loop: chart storage, note spawning, movement, judging, scoring.
    /// For the prototype the chart is hardcoded; later this can be loaded from a file.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        [Header("References")]
        [Tooltip("AudioSource that plays the song.")]
        public AudioSource musicSource;

        [Tooltip("Prefab to instantiate for each note.")]
        public GameObject notePrefab;

        [Tooltip("Transform where notes first appear (center of playfield).")]
        public Transform spawnPoint;

        [Tooltip("All target zones, ordered by lane index.")]
        public List<TargetZone> targetZones = new List<TargetZone>();

        [Tooltip("UI manager that displays score/combo/judgment.")]
        public UIManager uiManager;

        [Header("Timing")]
        [Tooltip("Seconds a note spends traveling from spawn to target.")]
        public float travelTime = 1.5f;

        public float perfectWindow = 0.050f;
        public float greatWindow = 0.100f;
        public float goodWindow = 0.150f;
        public float missWindow = 0.200f;

        [Header("Scoring")]
        public int perfectScore = 100;
        public int greatScore = 70;
        public int goodScore = 40;

        private readonly List<NoteData> chart = new List<NoteData>();
        private readonly List<Note> activeNotes = new List<Note>();
        private int nextSpawnIndex = 0;
        private float songStartDspTime;
        private bool songStarted = false;

        private int score = 0;
        private int combo = 0;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            BuildHardcodedChart();

            if (musicSource == null)
            {
                Debug.LogError("GameController.musicSource is not assigned.");
                enabled = false;
                return;
            }

            // Schedule music to start 2 seconds from now so the player has a moment to prepare.
            songStartDspTime = (float)AudioSettings.dspTime + 2.0f;
            musicSource.PlayScheduled(songStartDspTime);
            songStarted = true;
        }

        void Update()
        {
            if (!songStarted) return;

            float songTime = CurrentSongTime;

            SpawnNotes(songTime);
            MoveNotes(songTime);
            CheckMisses(songTime);
        }

        /// <summary>
        /// Current song time in seconds, measured against the audio DSP clock.
        /// </summary>
        private float CurrentSongTime => (float)AudioSettings.dspTime - songStartDspTime;

        /// <summary>
        /// Builds a simple 120 BPM hardcoded chart for the prototype.
        /// Notes cycle through lanes 0-3, one per beat, starting a few beats in.
        /// </summary>
        private void BuildHardcodedChart()
        {
            const float bpm = 120f;
            const float beat = 60f / bpm;

            // 16 notes, one per beat, starting at beat 4.
            for (int i = 4; i < 20; i++)
            {
                chart.Add(new NoteData(i * beat, i % 4));
            }
        }

        private void SpawnNotes(float songTime)
        {
            while (nextSpawnIndex < chart.Count &&
                   chart[nextSpawnIndex].hitTime - travelTime <= songTime)
            {
                SpawnNote(chart[nextSpawnIndex]);
                nextSpawnIndex++;
            }
        }

        private void SpawnNote(NoteData data)
        {
            if (notePrefab == null || spawnPoint == null) return;

            GameObject go = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
            Note note = go.GetComponent<Note>();
            if (note == null)
            {
                Debug.LogError("Note prefab is missing the Note component.");
                Destroy(go);
                return;
            }

            Vector3 target = targetZones[data.lane].transform.position;
            note.Initialize(
                data.lane,
                data.hitTime,
                data.hitTime - travelTime,
                spawnPoint.position,
                target);

            activeNotes.Add(note);
        }

        private void MoveNotes(float songTime)
        {
            foreach (Note note in activeNotes)
            {
                if (note.WasHit || note.WasMissed) continue;

                float t = 1f - ((note.HitTime - songTime) / travelTime);
                t = Mathf.Clamp01(t);
                note.transform.position = Vector3.Lerp(note.StartPosition, note.TargetPosition, t);
            }
        }

        private void CheckMisses(float songTime)
        {
            for (int i = activeNotes.Count - 1; i >= 0; i--)
            {
                Note note = activeNotes[i];
                if (note.WasHit || note.WasMissed) continue;

                if (songTime > note.HitTime + missWindow)
                {
                    RegisterMiss(note);
                }
            }
        }

        /// <summary>
        /// Called by TargetZone when the player taps a target.
        /// </summary>
        public void HandleTargetTap(int lane)
        {
            Note candidate = null;
            float bestDelta = float.MaxValue;
            float songTime = CurrentSongTime;

            foreach (Note note in activeNotes)
            {
                if (note.Lane != lane || note.WasHit || note.WasMissed) continue;

                float delta = Mathf.Abs(songTime - note.HitTime);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    candidate = note;
                }
            }

            if (candidate == null) return;

            if (bestDelta <= perfectWindow)
                ApplyJudgment(candidate, "PERFECT", perfectScore);
            else if (bestDelta <= greatWindow)
                ApplyJudgment(candidate, "GREAT", greatScore);
            else if (bestDelta <= goodWindow)
                ApplyJudgment(candidate, "GOOD", goodScore);
            else if (bestDelta <= missWindow)
                ApplyJudgment(candidate, "MISS", 0);
            // If the closest note is outside the miss window, ignore the tap.
        }

        private void ApplyJudgment(Note note, string judgment, int points)
        {
            note.WasHit = true;

            if (judgment == "MISS")
            {
                combo = 0;
            }
            else
            {
                score += points;
                combo++;
            }

            uiManager?.ShowJudgment(judgment, combo, score);
            activeNotes.Remove(note);
            Destroy(note.gameObject, 0.05f);
        }

        private void RegisterMiss(Note note)
        {
            note.WasMissed = true;
            combo = 0;
            uiManager?.ShowJudgment("MISS", combo, score);
            activeNotes.Remove(note);
            Destroy(note.gameObject, 0.05f);
        }
    }
}
