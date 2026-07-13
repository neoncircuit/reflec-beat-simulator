import wave
import struct
import math

sample_rate = 44100
bpm = 120.0
beat_duration = 60.0 / bpm
num_beats = 24
total_duration = num_beats * beat_duration
click_duration = 0.05
click_freq = 1000.0

num_samples = int(total_duration * sample_rate)

output_path = "/mnt/d/GitHub/reflec-beat-simulator/Assets/Audio/PlaceholderBeat.wav"

with wave.open(output_path, "w") as wav:
    wav.setnchannels(1)
    wav.setsampwidth(2)
    wav.setframerate(sample_rate)

    for i in range(num_samples):
        t = i / sample_rate
        beat_pos = t % beat_duration
        if beat_pos < click_duration:
            sample = math.sin(2 * math.pi * click_freq * beat_pos)
            envelope = 1.0 - (beat_pos / click_duration)
            sample *= envelope
        else:
            sample = 0.0
        wav.writeframes(struct.pack("h", int(sample * 32767)))

print(f"Generated {output_path}")
