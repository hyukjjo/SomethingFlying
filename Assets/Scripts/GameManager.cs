using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Launcher _launcher;
    [SerializeField]
    private VoiceInputController _voiceInputController;
    [SerializeField]
    private float _inputCountdown = 0f;

    [Header("UI")]
    [SerializeField]
    private GameObject _countdownUI;
    [SerializeField]
    private TextMeshProUGUI _countdownValueText;

    private float _timeDown = 0f;
    private bool freqSet = false;
    private bool dbSet = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(StartCountdownForPitch());
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(StartCountdownForVolume());
        }

        if(dbSet && freqSet)
        {
            _launcher.LaunchBear();
            freqSet = false;
            dbSet = false;
        }
    }

    private void ResetGame()
    {
        _launcher.ResetLauncher();
        _voiceInputController.SetState(VoiceInputController.VoiceInputState.Idle);
    }

    private IEnumerator StartCountdownForPitch()
    {
        _voiceInputController.SetState(VoiceInputController.VoiceInputState.ListeningForPitch);
        _countdownUI.SetActive(true);

        while (_timeDown < _inputCountdown)
        {
            _timeDown += Time.deltaTime;
            _countdownValueText.text = ((int)(_inputCountdown - _timeDown)).ToString();
            yield return null;
        }

        _timeDown = 0f;
        _countdownUI.SetActive(false);
        _launcher.SetLaunchAngleValue(_voiceInputController.GetPeakFreqValue());
        _voiceInputController.SetState(VoiceInputController.VoiceInputState.Wait);
        freqSet = true;
    }

    private IEnumerator StartCountdownForVolume()
    {
        _voiceInputController.SetState(VoiceInputController.VoiceInputState.ListeningForVolume);
        _countdownUI.SetActive(true);


        while (_timeDown < _inputCountdown)
        {
            _timeDown += Time.deltaTime;
            _countdownValueText.text = ((int)(_inputCountdown - _timeDown)).ToString();
            yield return null;
        }

        _timeDown = 0f;
        _countdownUI.SetActive(false);
        _launcher.SetLaunchForceValue(_voiceInputController.GetDbValue());
        _voiceInputController.SetState(VoiceInputController.VoiceInputState.Wait);
        dbSet = true;
    }
}
