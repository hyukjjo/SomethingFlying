using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool VoiceInputMode = true;

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
    [SerializeField]
    private TextMeshProUGUI _distance;
    [SerializeField]
    private GameObject _selectOptionUI;

    private float _timeDown = 0f;
    private bool freqSet = false;
    private bool dbSet = false;

    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("GameManager is null");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;

        if (!VoiceInputMode)
        {
            _voiceInputController.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _distance.text = ((int)_launcher.GetLaunchObject().position.x + 10).ToString() + " m";

        if (!VoiceInputMode && Input.GetKeyDown(KeyCode.S))
        {
            _launcher.SetLaunchForceValue(Random.Range(5f, 10f));
            _launcher.SetLaunchAngleValue(Random.Range(30f, 60f));
            _launcher.Launch();
        }

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
            _launcher.Launch();
            freqSet = false;
            dbSet = false;
        }
    }

    public void OpenSelectOption()
    {
        _selectOptionUI.SetActive(true);
    }

    public void Fight()
    {
        _launcher.SetLaunchAngleValue(Random.Range(0f, 360f));
        _launcher.SetLaunchForceValue(Random.Range(0f, 10f));
        _launcher.Launch();
        _selectOptionUI.SetActive(false);
    }

    public void BecomeFriends()
    {
        _launcher.SetLaunchAngleValue(Random.Range(30f, 90f));
        _launcher.SetLaunchForceValue(Random.Range(5f, 10f));
        _launcher.Launch();
        _selectOptionUI.SetActive(false);
    }

    private void ResetGame()
    {
        _distance.text = string.Empty;
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
