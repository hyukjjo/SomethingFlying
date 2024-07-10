using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

struct PeakData
{
    public int sampleIndex;
    public float amplitude;
}

public class VoiceInputController : MonoBehaviour
{
    //[Header("Threshold")]
    [SerializeField]
    private float _threshold = 0.01f;
    [SerializeField]
    private float _minFreq;
    [SerializeField]
    private float _maxFreq;
    [SerializeField]
    private float _minDb;
    [SerializeField]
    private float _maxDb;
    [SerializeField]
    private float _minAngle;
    [SerializeField]
    private float _maxAngle;
    [SerializeField]
    private float _minForce;
    [SerializeField]
    private float _maxForce;

    [Header("UI")]
    [SerializeField]
    private GameObject _angleUI;
    [SerializeField]
    private GameObject _forceUI;
    [SerializeField]
    private TextMeshProUGUI _angleValueText;
    [SerializeField]
    private TextMeshProUGUI _forceValueText;

    private string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    private AudioSource audioSource;
    private List<PeakData> peaks;
    private float[] samples;
    private float[] smoothSamples;
    private float[] noteFreqs;
    [SerializeField]
    private float dbValue = 0f;
    [SerializeField]
    private float peakFreq = 0f;

    const int sampleCount = 4096;

    // 상태 enum 정의
    public enum VoiceInputState
    {
        Idle,
        ListeningForVolume,
        ListeningForPitch,
        Wait
    }

    // 현재 상태를 저장하는 변수
    [SerializeField]
    private VoiceInputState currentState;

    void Awake()
    {
        noteFreqs = new float[108]; // 12 * 9 = 108
        for (int i = 0; i < 12 * 9; i++)
        {
            noteFreqs[i] = 440f * Mathf.Pow(2, (i - 57) / 12.0f);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start(null, true, 999, 44100);
        while (!(Microphone.GetPosition(null) > 0)) ;
        audioSource.Play();

        samples = new float[sampleCount];
        smoothSamples = new float[sampleCount];
        peaks = new List<PeakData>();

        // 초기 상태 설정
        currentState = VoiceInputState.Idle;

        Debug.Log("Microphone initialized.");
    }

    void Update()
    {
        if (Microphone.GetPosition(null) == -1)
        {
            Debug.LogWarning("Microphone input stopped.");
            return;
        }

        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        peaks.Clear(); // 피크 리스트 초기화

        // 현재 상태에 따라 입력 처리
        switch (currentState)
        {
            case VoiceInputState.Idle:
                // 아무런 input을 받지 않는 상태
                break;
            case VoiceInputState.ListeningForVolume:
                ProcessVolume();
                break;
            case VoiceInputState.ListeningForPitch:
                ProcessPitch();
                break;
            case VoiceInputState.Wait:
                break;
        }
    }

    private float MapValue(float inputMin, float inputMax, float outputMin, float outputMax, float value)
    {
        float final = (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;

        if (final < outputMin)
        {
            return outputMin;
        }
        else if (final > outputMax)
        {
            return outputMax;
        }
        else
        {
            return final;
        }
    }

    public float GetDbValue()
    {
        return MapValue(_minDb, _maxDb, _minForce, _maxForce, dbValue);
    }

    public float GetPeakFreqValue()
    {
        return MapValue(_minFreq, _maxFreq, _minAngle, _maxAngle, peakFreq);
    }

    // 상태를 변경하는 함수
    public void SetState(VoiceInputState newState)
    {
        currentState = newState;
        Debug.Log($"State changed to: {newState}");

        if (currentState == VoiceInputState.Idle)
        {
            StopMicrophone(); // 마이크 중지
            _angleValueText.text = string.Empty;
            _forceValueText.text = string.Empty;
            _angleUI.SetActive(false);
            _forceUI.SetActive(false);
        }
        else if (currentState == VoiceInputState.ListeningForPitch)
        {
            StartMicrophone(); // 마이크 시작
            _angleUI.SetActive(true);
        }
        else if (currentState == VoiceInputState.ListeningForVolume)
        {
            StartMicrophone(); // 마이크 시작
            _forceUI.SetActive(true);
        }
    }

    // 볼륨 입력을 처리하는 함수
    private void ProcessVolume()
    {
        float sum = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            sum += samples[i] * samples[i];
        }

        float rmsValue = Mathf.Sqrt(sum / sampleCount);
        dbValue = 20 * Mathf.Log10(rmsValue / 0.1f); // 0.1f는 기준값으로 적절히 조정 가능
        Debug.Log($"Db: {dbValue}");
        _forceValueText.text = ((int)MapValue(_minDb, _maxDb, _minForce, _maxForce, dbValue)).ToString();
    }

    // 프리퀀시 입력을 처리하는 함수
    private void ProcessPitch()
    {
        float sum = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            smoothSamples[i] = Mathf.Lerp(smoothSamples[i], samples[i], Time.deltaTime * 10);
            sum += samples[i] * samples[i];
            if (samples[i] > _threshold)
            {
                peaks.Add(new PeakData
                {
                    sampleIndex = i,
                    amplitude = smoothSamples[i]
                });
            }
        }

        if (peaks.Count > 0)
        {
            // 진폭을 기준으로 내림차순 정렬
            peaks.Sort((a, b) => -a.amplitude.CompareTo(b.amplitude));

            peakFreq = (peaks[0].sampleIndex * AudioSettings.outputSampleRate) / (2 * sampleCount);
            Debug.Log($"Peak Frequency: {peakFreq}");
            _angleValueText.text = ((int)MapValue(_minFreq, _maxFreq, _minAngle, _maxAngle, peakFreq)).ToString();

            if (peakFreq <= 100)
            {
                Debug.Log("Peak frequency is too low, ignoring.");
                return;
            }

            int noteNumber = ToNoteNumber(peakFreq);
            if (noteNumber >= 0)
            {
                string note = noteNames[noteNumber % 12];
                int octave = noteNumber / 12;

                string text = $"{note}{octave}"; // 노트 텍스트

                Debug.Log($"Detected Note: {text}");

                // Calculate y position based on peakFreq
                float normalizedFreq = Mathf.Clamp((peakFreq - 100) / 400, 0, 1); // Normalize between 100 and 500
                float targetY = Mathf.Lerp(1, 8, normalizedFreq); // Map to y between 1 and 8
            }
            else
            {
                Debug.LogWarning("Frequency out of range.");
            }
        }
        else
        {
            Debug.Log("No peaks detected.");
        }
    }

    private int ToNoteNumber(float freq)
    {
        for (int i = 1; i < noteFreqs.Length - 1; i++)
        {
            float prev = noteFreqs[i - 1];
            float next = noteFreqs[i + 1];
            float current = noteFreqs[i];
            float min = (prev + current) / 2;
            float max = (next + current) / 2;

            if (min <= freq && freq <= max)
            {
                return i;
            }
        }
        return -1;
    }

    private void StartMicrophone()
    {
        if (Microphone.IsRecording(null))
        {
            Debug.LogWarning("Microphone is already recording.");
            return;
        }

        audioSource.clip = Microphone.Start(null, true, 999, 44100);
        while (!(Microphone.GetPosition(null) > 0)) ;
        audioSource.Play();

        Debug.Log("Microphone started.");
    }

    private void StopMicrophone()
    {
        Microphone.End(null);
        Debug.Log("Microphone stopped.");
    }
}