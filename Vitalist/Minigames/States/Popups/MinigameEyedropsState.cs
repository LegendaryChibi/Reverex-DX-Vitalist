using UnityEngine;

public class MinigameEyedropsState : MinigameState
{
    private GameObject[] eyeDrops;
    private Rigidbody2D eyeDropRigidbody;
    private GameObject eyeBall;
    private GameObject eyeBallClosed;
    private GameObject eyeBallClear;
    private Vector3 startingPosition;
    private int currentButtonPress = 0;
    bool complete = false;
    private bool prevButtonState = false;
    float wiggleDistance = 0.15f;
    float wiggleSpeed = 5;
    private GameObject AllEyeDrops;
    private int dropLimit;
    private GameObject a;
    private GameObject b;
    private GameObject x;
    private GameObject y;
    private float clickTime = 0.1f;
    private Vector3 originalButtonScale;
    private float dropYPos;

    private static Synchronized<Interger> inputIndex = new Synchronized<Interger>("net_eyedrops_input_index");
    private static Synchronized<Interger> buttonPresses = new Synchronized<Interger>("net_eyedrop_presses");
    private static Synchronized<Float> currentYPos = new Synchronized<Float>("net_eyedrop_y_pos");
    private static Synchronized<Boolean> isButtonDepressed = new Synchronized<Boolean>("net_eyedrop_depressed", false, true);

    private static Synchronized<Float> clickButtonTime = new Synchronized<Float>("net_eyedrop_click_time ");

    public MinigameEyedropsState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        eyeDrops = stateMachine.Vitalist.eyeDrops;
        eyeDropRigidbody = stateMachine.Vitalist.eyeDropRigidbody;
        eyeBall = stateMachine.Vitalist.eyeBall;
        eyeBallClosed = stateMachine.Vitalist.eyeBallClosed;
        eyeBallClear = stateMachine.Vitalist.eyeBallClear;
        AllEyeDrops = stateMachine.Vitalist.allEyeDrops;
        a = stateMachine.Vitalist.aButtonSprite;
        b = stateMachine.Vitalist.bButtonSprite;
        x = stateMachine.Vitalist.xButtonSprite;
        y = stateMachine.Vitalist.yButtonSprite;

        originalButtonScale = a.transform.localScale;
        dropYPos = AllEyeDrops.transform.localPosition.y;
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.Vitalist.PausePopupTimer();
        StatsManager.Instance.StartEffect(StatsManager.EffectType.Clarity, StatsManager.Effect.Debuff);

        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Mash");

        AllEyeDrops.transform.localPosition = new Vector3(AllEyeDrops.transform.localPosition.x, dropYPos, AllEyeDrops.transform.localPosition.z);

        stateMachine.Vitalist.eyedropsParent.SetActive(true);

        inputIndex.SetValue(Random.Range(0, 4), () => Whoami.AmIP2());

        HapticsManager.Instance.PlayNavigatorHaptic(GameManager.Instance.Navigator.navigatorClarityRumble);
        eyeDrops[0].SetActive(true);
        eyeDrops[1].SetActive(false);
        eyeDrops[2].SetActive(false);
        eyeDrops[3].SetActive(false);
        eyeBall.SetActive(true);
        eyeBallClosed.SetActive(false);
        eyeBallClear.SetActive(false);
        eyeDrops[3].transform.localPosition = startingPosition;
        eyeDropRigidbody.gravityScale = 0;
        buttonPresses.SetValue(0, () => Whoami.AmIP2());
        eyeDropRigidbody.velocity = Vector3.zero;
        complete = false;
        clickButtonTime.SetValue(clickTime + 1, () => Whoami.AmIP2());
        switch (stateMachine.Vitalist.EyedropsLevel)
        {
            case 0:
                dropLimit = 5;
                break;
            case 1:
                dropLimit = 10;
                break;
            case 2:
                dropLimit = 20;
                break;
            case 3:
                dropLimit = 30;
                break;
        }
    }

    public override void Exit()
    {
        base.Exit();
        TaffyManager.Instance.MinigameComplete = true;
        stateMachine.Vitalist.eyedropsParent.SetActive(false);
        stateMachine.Vitalist.RestartPopupTimer();
        StatsManager.Instance.StopAllEffectAndType(StatsManager.EffectType.Clarity, StatsManager.Effect.Debuff);
    }

    public override void Update()
    {
        base.Update();
        if (currentButtonPress != (int)buttonPresses.GetValue() && !complete)
        {
            currentButtonPress = (int)buttonPresses.GetValue();
        }

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        //HapticsManager.Instance.PlayNavigatorHaptic(GameManager.Instance.Navigator.navigatorClarityRumble);

        bool currentButtonState;

        currentButtonState = ButtonInput();

        if (currentButtonState && !prevButtonState && !complete && (float)clickButtonTime.GetValue() > clickTime)
        {
            buttonPresses.SetValue((int)buttonPresses.GetValue() + 1, () => Whoami.AmIP2());
            HapticsManager.Instance.PlayVitalistHaptic(stateMachine.Vitalist.eyedropPress);
            //eyeDropperSqueeze.SetTrigger("Squeeze");
            clickButtonTime.SetValue(clickTime, () => Whoami.AmIP2());
            isButtonDepressed.SetValue(false, () => Whoami.AmIP2());
        }

        prevButtonState = currentButtonState;


        if ((float)clickButtonTime.GetValue() <= clickTime)
        {
            clickButtonTime.SetValue((float)clickButtonTime.GetValue() - Time.fixedDeltaTime, () => Whoami.AmIP2());
        }

        if ((float)clickButtonTime.GetValue() <= 0)
        {
            clickButtonTime.SetValue(clickTime + 1, () => Whoami.AmIP2());
            isButtonDepressed.SetValue(true, () => Whoami.AmIP2());
        }
        if ((bool)isButtonDepressed.GetValue())
        {
            switch ((int)inputIndex.GetValue())
            {
                case 0:
                    a.transform.localScale = originalButtonScale;
                    break;
                case 1:
                    b.transform.localScale = originalButtonScale;
                    break;
                case 2:
                    y.transform.localScale = originalButtonScale;
                    break;
                case 3:
                    x.transform.localScale = originalButtonScale;
                    break;
            }
        } else
        {
            switch ((int)inputIndex.GetValue())
            {
                case 0:
                    a.transform.localScale = 0.7f * originalButtonScale;
                    break;
                case 1:
                    b.transform.localScale = 0.7f * originalButtonScale;
                    break;
                case 2:
                    y.transform.localScale = 0.7f * originalButtonScale;
                    break;
                case 3:
                    x.transform.localScale = 0.7f * Vector3.one;
                    break;
            }
        }
        

        if (!complete)
        {
            if ((int)buttonPresses.GetValue() > (dropLimit / 3) && (int)buttonPresses.GetValue() < (dropLimit * 2 / 3))
            {
                eyeDrops[0].SetActive(false);
                eyeDrops[1].SetActive(true);
            }
            else if ((int)buttonPresses.GetValue() > (dropLimit * 2 / 3) && (int)buttonPresses.GetValue() < dropLimit)
            {
                eyeDrops[1].SetActive(false);
                eyeDrops[2].SetActive(true);
            }
            else if ((int)buttonPresses.GetValue() > dropLimit)
            {
                eyeDrops[2].SetActive(false);
                eyeDrops[3].SetActive(true);
                eyeDropRigidbody.gravityScale = 0.06f;
                complete = true;
                stateMachine.Vitalist.StartEyedropsFinalAnimation();
                stateMachine.Vitalist.StartMinigameComplete();
                stateMachine.Vitalist.EyedropsLevel++;
                StatsManager.Instance.StopAllEffectAndType(StatsManager.EffectType.Clarity, StatsManager.Effect.Debuff);
            }
        }
    }

    private bool ButtonInput()
    {
        a.SetActive(false);
        b.SetActive(false);
        x.SetActive(false);
        y.SetActive(false);
        switch ((int)inputIndex.GetValue())
        {
            case 0:
                a.SetActive(true);
                return ReadAButtonInput();
            case 1:
                b.SetActive(true);
                return ReadBButtonInput();
            case 2:
                y.SetActive(true);
                return ReadYButtonInput();
            case 3:
                x.SetActive(true);
                return ReadXButtonInput();
        }
        return false;
    }
}
