using Steamworks;
using System;
using System.Collections;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class Vitalist : MonoBehaviour
{
    #region Input, StateMachine & Animator
    public string currentState;
    public AController Input //This probably should be a copy
    {
        get { return ControllerManager.VitalistController; }
        private set { ControllerManager.VitalistController = value; } 
    }

    static Synchronized<Boolean> netHasController = new Synchronized<Boolean>("net_has_controller_vitalist", true, false);
    public bool hasController
    {
        get
        {
            if (Whoami.AmIP2())
            {
                if (Input == null) { return true; }
                netHasController.SetValue(Input.isController, () => { return true; });
                return Input.isController;
            }
            else
            {
                return (bool)netHasController.GetValue();
            }
        }
    }

    public MinigameStateMachine minigameStateMachine;
    public static MinigameStateMachine MinigameStateMachine { get { return GameManager.Instance.Vitalist.minigameStateMachine; } }
    

    private static Synchronized<Boolean> tutorialMode = new Synchronized<Boolean>("net_vitalist_tutorial_mode", (Boolean)true);
    public bool TutorialMode { get { return (bool)tutorialMode.GetValue(); } set { tutorialMode.SetValue(value, () => Whoami.AmIP1()); } }
    public NetRoutine<Interger> unlockButton;
    private bool minigameCompleteRunning = false;

    public NetRoutine<MinigameQueue.Minigames> minigameTrigger;

    public Animator buttonFX;
    #endregion

    #region Haptics
    public Rumble nerf;
    public Rumble buff;
    public Rumble defibComplete;
    public Rumble defibCharge;
    //public Rumble electroPour;
    public Rumble adrenalineJump;
    public Rumble antibioticsHit;
    public Rumble eyedropPress;
    public Rumble hitBed;
    public Rumble pour;
    public Rumble emptyRumble;

    #endregion

    #region Menu Setup
    [Header("Vitalist Menu Setup")]
    public GameObject warningImage;

    private static Synchronized<Boolean> warningActive = new Synchronized<Boolean>("net_vitalist_warning_active", (Boolean)false);
    public bool WarningActive { get { return (bool)warningActive.GetValue(); } set { warningActive.SetValue(value, () => Whoami.AmIP2()); } }

    private NetRoutine<Boolean> warningToggle;

    public GameObject vitalistButtonParent;
    public TMP_Text[] selectButtonsText { get; private set; }

    private bool pauseButtonWasPressed = false;
    private bool pauseButtonReleased = false;

    public GameObject[] icons; 
    #endregion

    #region Adrenaline Setup
    [Header("Adrenaline Setup")]
    public GameObject adrenalineLevelParent;
    public GameObject adrenalineLevel0;
    public GameObject adrenalineLevel1;
    public GameObject adrenalineLevel2;
    public GameObject adrenalineLevel3;
    public GameObject adrenalineLevel4;
    public GameObject adrenalineLevel5;
    public GameObject adrenalineLevel6;
    public GameObject adrenalineLevel7;
    public GameObject Mouth;
    public GameObject MouthOpen;
    public GameObject MouthNeutral;
    public GameObject MouthClosed;
    public Rigidbody2D PillRb;
    public GameObject adrenalineParent;
    #endregion

    #region Electrolytes Setup
    [Header("Electrolytes Setup")]
    public GameObject electrolytesParent;
    public GameObject energyDrink;
    public GameObject x;
    public GameObject checkmark;
    public GameObject spillPoint;
    public GameObject highPour;
    public GameObject goodPour;
    public GameObject lowPour;
    public GameObject highPourFrame1;
    public GameObject highPourFrame2;
    public GameObject goodPourFrame1;
    public GameObject goodPourFrame2;
    public GameObject lowPourFrame1;
    public GameObject lowPourFrame2;
    #endregion

    #region Antibiotics Setup
    [Header("Antibiotics Setup")]
    public GameObject antibioticsParent;
    public GameObject spinner;
    public GameObject neutralFace;
    public GameObject happyFace;
    public GameObject germ1;
    public GameObject germ2;
    public GameObject germ3;
    #endregion

    #region Fracture Setup
    [Header("Fracture Setup")]
    public GameObject fractureParent;
    public GameObject jumperIdle;
    public GameObject jumperJump;
    public GameObject jumperLand;
    public GameObject lvl0;
    public GameObject lvl1;
    public GameObject lvl2;
    public GameObject lvl3;
    public GameObject lvl4;
    public GameObject lvl5;
    public Rigidbody2D jumperRb;
    #endregion

    #region Exhaustion Setup
    [Header("Exhaustion Setup")]
    public GameObject exhaustionParent;
    public GameObject arrowLeft;
    public GameObject arrowRight;
    public Rigidbody2D runnerRb;
    public GameObject bedLvl0;
    public GameObject bedLvl1;
    public GameObject bedLvl2;
    public GameObject bedLvl3;
    public GameObject bedLvl4;
    public GameObject bedLvl5;
    public GameObject sleep;
    public GameObject currentBedLvl;
    [SerializeField] private GameObject lane1;
    [SerializeField] private GameObject lane2;
    #endregion

    #region Eyedrops Setup
    [Header("Eyedrops Setup")]
    public GameObject eyedropsParent;
    public GameObject[] eyeDrops;
    public Rigidbody2D eyeDropRigidbody;
    public GameObject eyeBall;
    public GameObject eyeBallClosed;
    public GameObject eyeBallClear;
    public GameObject allEyeDrops;
    public GameObject aButtonSprite;
    public GameObject bButtonSprite;
    public GameObject xButtonSprite;
    public GameObject yButtonSprite;
    #endregion

    #region IV Setup
    [Header("IV Setup")]
    public GameObject ivParent;
    public GameObject needle;
    public GameObject target;
    public Rigidbody2D needleRigidbody;
    #endregion

    #region Rewind Setup
    [Header("Rewind Setup")]
    public GameObject rewindParent;
    public GameObject arrows;
    public Animator rewindAnimator;
    #endregion

    #region Defib Setup
    [Header("Debrillator Setup")]
    public GameObject defibParent;
    public GameObject joySticks;
    public GameObject shockSprite;
    public GameObject shockCompleteSprite;
    public GameObject meterNode;
    public GameObject meterSprite1;
    public GameObject meterSprite2;
    public GameObject meterSprite3;
    public GameObject meterSprite4;
    public GameObject meterSprite5;
    public GameObject meterSprite6;
    public GameObject meterSprite7;
    public GameObject meterSprite8;
    #endregion

    #region Setup Text & Time
    [Header("Minigame Setup")]
    private int timeLimit = 10;
    [SerializeField] private TMP_Text instructionsText;
    [SerializeField] private TMP_Text timerText;
    #endregion

    #region Holder Variables 
    public static NetAction<Interger> SetTime = new NetAction<Interger>("net_set_menu_timer");
    public static NetAction<String> SetText = new NetAction<String>("net_set_menu_text");

    private static NetAction<MinigameQueue.Minigames> PlayNextMinigame = new NetAction<MinigameQueue.Minigames>("net_play_next_game");

    public static NetAction<MinigameQueue.Minigames> popupTriggered = new NetAction<MinigameQueue.Minigames>("net_play_popup");

    public MinigameQueue popupQueue;

    private bool MinigameTimerIsRunning = false;

    private static MinigameState nextGame;

    #endregion

    #region Difficuly Levels
    //Popup Difficulty Levels
    private int fractureLevel = 0;
    public int FractureLevel { get { return fractureLevel; } set { if (value > 5) { fractureLevel = 5; } else fractureLevel = value; } }

    private int exhaustionLevel = 0;
    public int ExhaustionLevel { get { return exhaustionLevel; } set { if (value > 3) { exhaustionLevel = 3;  } else exhaustionLevel = value; } }

    private int eyedropsLevel = 0;
    public int EyedropsLevel { get { return eyedropsLevel; } set { if (value > 3) { eyedropsLevel = 3; } else eyedropsLevel = value; } }

    private int ivLevel = 0;
    public int IVLevel { get { return ivLevel; } set { if (value > 3) { ivLevel = 3; } else ivLevel = value; } }

    //Selectable Difficulty Levels
    static Synchronized<Interger> adrenalineLevel = new Synchronized<Interger>("net_adrenaline_level", 0);
    public int AdrenalineLevel { get { return (int)adrenalineLevel.GetValue(); } set { if (value > 7) { adrenalineLevel.SetValue(UnityEngine.Random.Range(1, 8), () => Whoami.AmIP2()); } else adrenalineLevel.SetValue(value, () => Whoami.AmIP2()); } }

    static Synchronized<Interger> electrolytesLevel = new Synchronized<Interger>("net_electrolytes_level", 0);
    public int ElectrolytesLevel { get { return (int)electrolytesLevel.GetValue(); } set { if (value > 5) { electrolytesLevel.SetValue(UnityEngine.Random.Range(1, 6), () => Whoami.AmIP2()); } else electrolytesLevel.SetValue(value, () => Whoami.AmIP2()); } }

    private static Synchronized<Interger> antibioticsLevel = new Synchronized<Interger>("net_antibiotics_level", 0);
    public int AntibioticsLevel { get { return (int)antibioticsLevel.GetValue(); } set { if (value > 5) { antibioticsLevel.SetValue(UnityEngine.Random.Range(1, 6), () => Whoami.AmIP2()); } else antibioticsLevel.SetValue(value, () => Whoami.AmIP2()); } }
    #endregion
    public NetRoutine OnMinigameComplete;
    #region Constructor/ Deconstructor
    public Vitalist()
    {
        VitalistEventManager.Instance.StartVitalist += OnGameStart;
        SetTime += setTime;
        SetText += setText;
        PlayNextMinigame += PlayNextGame;
        StatsManager.DefibTriggered += StartDefibMinigame;
        StatsManager.RewindTriggered += StartRewindMinigame;
    }

    private void OnDestroy()
    {
        VitalistEventManager.Instance.StartVitalist -= OnGameStart;
        SetTime -= setTime;
        SetText -= setText;
        PlayNextMinigame -= PlayNextGame;
        StatsManager.DefibTriggered -= StartDefibMinigame;
        StatsManager.RewindTriggered -= StartRewindMinigame;
    }
    #endregion

    private void Awake()
    {
        OnMinigameComplete = new NetRoutine(OnGameComplete, Ownership.Vitalist);
        adrenalineLevel.SetValue(0, () => { return true; });
        electrolytesLevel.SetValue(0, () => { return true; });
        antibioticsLevel.SetValue(0, () => { return true; });
        selectButtonsText = vitalistButtonParent.GetComponentsInChildren<TMP_Text>();

        minigameStateMachine = new MinigameStateMachine(this);

        popupQueue = new MinigameQueue();

        RestartPopupTimer();
        nextGame = MinigameStateMachine.menuState;
        minigameTrigger = new NetRoutine<MinigameQueue.Minigames>(MinigameTriggerPlay, Ownership.Both);
        warningToggle = new NetRoutine<Boolean>(ToggleWarning, Ownership.Vitalist);
    }

    private void Start()
    {
        if (GameManager.instance.Navigator.MovementStateMachine != null && GameManager.instance.Navigator.MovementStateMachine.GetCurrentState() is NavigatorDemoState)
        {
            MinigameStateMachine.ChangeState(MinigameStateMachine.minigameReplayDummy);
        }
        else
        {
            MinigameStateMachine.ChangeState(MinigameStateMachine.menuState);
        }
        if (Input != null)
        {
            netHasController.SetValue(Input.isController, () => { return Whoami.AmIP2(); });
        }
    }

    private void Update()
    {
        MinigameStateMachine.HandleInput();

        MinigameStateMachine.Update();

        PauseCheck();
    }

    private void FixedUpdate()
    {
       MinigameStateMachine.FixedUpdate();
    }

    // Methods
    public void OnGameStart()
    {
        GameManager.Instance.VitalistBody.SetActive(true);
    }

    private void OnEnable()
    {
        buttonFX.SetBool("Default", true);
    }

    public void VitalistShutdown()
    {
        gameObject.SetActive(false);
        StatsManager.Instance.StopAllEffects();
        StopAllCoroutines();
        buttonFX.SetBool("Default", false);
        buttonFX.SetBool("Buff", false);
        buttonFX.SetBool("Debuff", false);
        buttonFX.SetBool("Defib", false);
        buttonFX.SetBool("Rewind", false);
        warningImage.SetActive(false);
    }

    IEnumerator MinigameComplete()
    {
        OnMinigameComplete?.Invoke();
        minigameCompleteRunning = true;
        SetText.Invoke(() => Whoami.AmIP2(), "Completed!");
        int countdown = 2;
        while (countdown > 0)
        {
            SetTime.Invoke(() => Whoami.AmIP2(), countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }
        SetTime.Invoke(() => Whoami.AmIP2(), 0);

        PlayNextMinigame.Invoke(() => Whoami.AmIP2(), MinigameQueue.StateToPopupMinigame(nextGame));
        minigameCompleteRunning = false;
    }

    private void OnGameComplete()
    {
    }

    public void PlayNextGame(MinigameQueue.Minigames state)
    {
        MinigameStateMachine.ChangeState(MinigameQueue.MinigameToState(state));
        SoundManager.Instance.PlaySFXOnline("Vitalist_Minigame_Start");
        nextGame = MinigameStateMachine.menuState;
    }

    //Only use for selectables
    IEnumerator MinigameTimer()
    {
        MinigameTimerIsRunning = true;
        int countdown = timeLimit;
        while (countdown > 0)
        {
            SetTime.Invoke(() => Whoami.AmIP2(), countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }
        SetTime.Invoke(() => Whoami.AmIP2(), 0);

        PlayNextMinigame.Invoke(() => Whoami.AmIP2(), MinigameQueue.StateToPopupMinigame(nextGame));
        MinigameTimerIsRunning = false;
        SoundManager.Instance.PlaySFXOnline("Vitalist_Minigame_Loss");
    }

    IEnumerator PopupTimer()
    {
        while (true)
        {
            float countdown = 15f;
            while (countdown > 0)
            {
                yield return new WaitForSeconds(1f);
                countdown--;
                if (countdown <= 2f)
                {
                    warningToggle.Invoke(true);
                    SoundManager.Instance.PlaySFXOnline("Navigator_SystemFailure", GameManager.instance.Navigator.transform.position);
                }
            }

            switch (MinigameQueue.popups[0])
            {
                case MinigameQueue.Minigames.fracture:
                    InsertPopupAsNext(MinigameStateMachine.fractureState, MinigameQueue.Minigames.fracture);
                    break;
                case MinigameQueue.Minigames.exhaustion:
                    InsertPopupAsNext(MinigameStateMachine.exhaustionState, MinigameQueue.Minigames.exhaustion);
                    break;
                case MinigameQueue.Minigames.eyeDrops:
                    InsertPopupAsNext(MinigameStateMachine.eyedropsState, MinigameQueue.Minigames.eyeDrops);
                    break;
                case MinigameQueue.Minigames.IV:
                    InsertPopupAsNext(MinigameStateMachine.ivState, MinigameQueue.Minigames.IV);
                    break;
            }
            popupTriggered?.Invoke(() => Whoami.AmIP2(), MinigameQueue.StateToPopupMinigame(nextGame));
            PausePopupTimer();
        }
    }

    private void InsertPopupAsNext(MinigameState state, MinigameQueue.Minigames popup)
    {
        nextGame = state;
        MinigameQueue.pushMinigameBack(popup);
    }

    //Have to do this so states can reference it. (Monobehaviour silliness)
    public void StartMinigameComplete()
    {
        if (Whoami.AmIP2())
        {
            StopCoroutine(nameof(MinigameTimer));
            StartCoroutine(nameof(MinigameComplete));
            PlayMinigameWinSFX();
        }
    }

    public void StartMinigameComplete(MinigameQueue.Minigames pushback)
    {
        StartMinigameComplete();
        if (Whoami.AmIP2())
        {
            MinigameQueue.pushMinigameBack(pushback);
        }
    }

    public void PlayMinigameWinSFX()
    {
        SoundManager.Instance.PlaySFXOnline("Vitalist_Minigame_Win");
    }

    //Have to do this so states can reference it. (Monobehaviour silliness)
    public void StartMinigameTimer()
    {
        if (Whoami.AmIP2())
        {
            StartCoroutine(nameof(MinigameTimer));
        }
    }

    public void StopMinigameTimer()
    {
        if (Whoami.AmIP2())
        {
            if (MinigameTimerIsRunning)
            {
                StopCoroutine(nameof(MinigameTimer));
                MinigameTimerIsRunning = false;
            }
            SetTime.Invoke(() => Whoami.AmIP2(), 0);
        }
    }

    public void PausePopupTimer()
    {
        if (Whoami.AmIP2())
        {
            StopCoroutine(nameof(PopupTimer));
            warningToggle.Invoke(false);
        }
    }

    public void RestartPopupTimer()
    {
        if (Whoami.AmIP2() && !TutorialMode)
        {
            StopCoroutine(nameof(PopupTimer));
            StartCoroutine(nameof(PopupTimer));
        }
    }

    private void StartDefibMinigame()
    {
        if (minigameCompleteRunning)
        {
            StopCoroutine(nameof(MinigameComplete));
        }
        MinigameStateMachine.ChangeState(MinigameStateMachine.defibState);
        nextGame = MinigameStateMachine.menuState;
    }

    private void StartRewindMinigame()
    {
        if (minigameCompleteRunning)
        {
            StopCoroutine(nameof(MinigameComplete));
        }
        if (MinigameQueue.StateToPopupMinigame(MinigameStateMachine.CurrentState) != MinigameQueue.Minigames.none)
        {
            nextGame = MinigameStateMachine.CurrentState;
        }
        minigameCompleteRunning = false;
        MinigameStateMachine.ChangeState(MinigameStateMachine.rewindState);
    }

    private void MinigameTriggerPlay(MinigameQueue.Minigames state)
    {
        if (minigameCompleteRunning)
        {
            StopCoroutine(nameof(MinigameComplete));
        }
        StopMinigameTimer();
        MinigameStateMachine.ChangeState(MinigameQueue.MinigameToState(state));
        nextGame = MinigameStateMachine.menuState;
    }

    private void setText(String t)
    {
        instructionsText.text = (string)t;
    }

    private void setTime(Interger t)
    {
        if ((int?)t > 0)
        {
            timerText.text = ((int)t).ToString();
            return;
        }
        timerText.text = "";
    }

    private void PauseCheck()
    {
        if (ControllerManager.VitalistController == null) { return; }
        // If pause button is pressed
        if (ControllerManager.VitalistController.StartButton)
        {
            pauseButtonWasPressed = true;
        }
        else if (!ControllerManager.VitalistController.StartButton && pauseButtonWasPressed)
        {
            pauseButtonWasPressed = false;
            pauseButtonReleased = true;
            // button was pressed then released
        }
        else
            pauseButtonWasPressed = false;

        if (pauseButtonReleased)
        {
            pauseButtonReleased = false;
            GameManager.Instance.PauseGame();
            pauseButtonWasPressed = false;
        }
    }

    public void ToggleWarning (Boolean toggle)
    {
        WarningActive = (bool)toggle;
        warningImage.SetActive((bool)toggle);
    }

    #region Coroutine Animations
    //Minigame Coroutine Animations
    IEnumerator EyedropsFinalAnimation()
    {
        yield return new WaitForSeconds(1.2f);
        eyeDrops[3].SetActive(false);
        eyeBall.SetActive(false);
        eyeBallClosed.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        eyeBallClosed.SetActive(false);
        eyeBallClear.SetActive(true);
    }

    public void StartEyedropsFinalAnimation()
    {
        StartCoroutine(nameof(EyedropsFinalAnimation));
    }

    IEnumerator SleepAnim()
    {
        sleep.SetActive(true);
        currentBedLvl.SetActive(false);
        runnerRb.gameObject.SetActive(false);
        lane1.SetActive(false);
        lane2.SetActive(false);
        yield return new WaitForSeconds(2f);
        sleep.SetActive(false);
        currentBedLvl.SetActive(true);
        runnerRb.gameObject.SetActive(true);
        lane1.SetActive(true);
        lane2.SetActive(true);
    }

    public void StartSleepAnim()
    {
        StartCoroutine(nameof(SleepAnim));
    }

    IEnumerator MouthAnim()
    {
        MouthOpen.SetActive(false);
        MouthClosed.SetActive(true);
        yield return new WaitForSeconds(1f);
        MouthClosed.SetActive(false);
        MouthNeutral.SetActive(true);
        yield return new WaitForSeconds(3f);
        MouthOpen.SetActive(true);
        MouthClosed.SetActive(false);
        MouthNeutral.SetActive(false);

    }

    public void StartMouthAnim()
    {
        StartCoroutine(nameof(MouthAnim));
    }

    private IEnumerator AnimatePour()
    {
        while (true)
        {
            highPourFrame2.SetActive(true);
            highPourFrame1.SetActive(false);
            goodPourFrame1.SetActive(true);
            goodPourFrame2.SetActive(false);
            lowPourFrame1.SetActive(true);
            lowPourFrame2.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            highPourFrame2.SetActive(false);
            highPourFrame1.SetActive(true);
            goodPourFrame1.SetActive(false);
            goodPourFrame2.SetActive(true);
            lowPourFrame1.SetActive(false);
            lowPourFrame2.SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void StartAnimatePour()
    {
        StartCoroutine(nameof(AnimatePour));
    }

    public void StopAnimatePour()
    {
        StopCoroutine(nameof(AnimatePour));
    }
    #endregion
}
