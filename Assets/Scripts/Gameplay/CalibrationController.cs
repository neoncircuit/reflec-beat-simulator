using System;
using System.Collections.Generic;
using ReflecBeatSimulator.Audio;
using ReflecBeatSimulator.Core;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>
    /// Runs the M0 calibration ritual: plays a metronome, collects tap deltas,
    /// and persists the average as the device input offset.
    /// </summary>
    public sealed class CalibrationController : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _tickClip;
        [SerializeField] private double _bpm = 120.0;
        [SerializeField] private CalibrationUI _ui;
        [SerializeField] private CalibrationBeatMarker _beatMarker;

        private SongClock _songClock;
        private Metronome _metronome;
        private double _startDspTime;
        private readonly List<double> _deltas = new List<double>(Constants.CalibrationTapCount);
        private bool _running;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += OnFingerDown;
        }

        private void OnDisable()
        {
            Touch.onFingerDown -= OnFingerDown;
            EnhancedTouchSupport.Disable();
        }

        private void Start()
        {
            _ui?.SetInstruction("Tap the beat when you hear the click.");
            _ui?.SetDoneVisible(false);
            BeginCalibration();
        }

        /// <summary>Starts or restarts the calibration sequence.</summary>
        public void BeginCalibration()
        {
            _deltas.Clear();
            _running = true;
            _startDspTime = AudioPlayback.Schedule(_audioSource, _tickClip, 0.5);
            _songClock = new SongClock(_startDspTime, 0.0);
            _metronome = new Metronome(_audioSource, _tickClip, _bpm, _startDspTime, Constants.CalibrationTapCount);
            _metronome.BeatScheduled += OnBeatScheduled;

            _ui?.SetAverage(null);
            _ui?.SetLastDelta(null);
            _ui?.SetDoneVisible(false);
        }

        /// <summary>Restarts calibration when the user presses Retry.</summary>
        public void Retry() => BeginCalibration();

        /// <summary>Called when the user finishes calibration.</summary>
        public void Done()
        {
            Debug.Log($"Calibration complete. Stored inputOffsetSec = {CalibrationStore.Load()}");
        }

        private void Update()
        {
            if (!_running)
                return;

            _songClock.Tick(Time.unscaledTimeAsDouble);
            _metronome.Update();

            if (_metronome.IsFinished && _deltas.Count >= Constants.CalibrationTapCount)
            {
                _running = false;
                FinishCalibration();
            }
        }

        private void OnBeatScheduled(int beat)
        {
            _beatMarker?.Pulse();
        }

        private void OnFingerDown(Finger finger)
        {
            if (!_running || _deltas.Count >= Constants.CalibrationTapCount)
                return;

            double tapDsp = AudioSettings.dspTime;
            double secondsPerBeat = 60.0 / _bpm;
            double elapsed = tapDsp - _startDspTime;
            int nearestBeat = (int)Math.Round(elapsed / secondsPerBeat);
            double expectedDsp = _startDspTime + nearestBeat * secondsPerBeat;
            double delta = tapDsp - expectedDsp;

            _deltas.Add(delta);
            _ui?.SetLastDelta(delta);

            if (_deltas.Count >= Constants.CalibrationTapCount)
            {
                _running = false;
                FinishCalibration();
            }
        }

        private void FinishCalibration()
        {
            double sum = 0.0;
            foreach (double d in _deltas)
                sum += d;

            double average = sum / _deltas.Count;
            CalibrationStore.Save(average);

            _ui?.SetAverage(average);
            _ui?.SetInstruction("Calibration complete.");
            _ui?.SetDoneVisible(true);

            Debug.Log($"Calibration finished. Average delta: {average:F4} s");
        }
    }
}
