using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Author: Joshua Varghese
/// Date: 10/25/2024
/// </summary>
public class MinigameDefibState : MinigameState
{
    private GameObject joySticks;
    private GameObject shockSprite;
    private GameObject shockCompleteSprite;
    private GameObject meterNode;
    private GameObject[] meterSprites;

    private float sensitivity = 0.1f;
    private float lastRight = 0;
    private float lastLeft = 0;
    private int tick = 0;
    private int lastTick = 0;
    private float lastTickTime = 0;
    private int tickTotal = 10;
    private float diff = 0.5f;
    private int tickRate = 1;
    private bool previousLTriggerState = false;
    private bool previousRTriggerState = false;
    private float defibTriggersTick = 0;

    private static Synchronized<Interger> currentTick = new Synchronized<Interger>("net_defib_tick");
    private static Synchronized<Boolean> currentShockState = new Synchronized<Boolean>("net_defib_shock");
    private static Synchronized<Boolean> completed = new Synchronized<Boolean>("net_defib_completed");

    public MinigameDefibState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        joySticks = stateMachine.Vitalist.joySticks;
        shockSprite = stateMachine.Vitalist.shockSprite;
        shockCompleteSprite = stateMachine.Vitalist.shockCompleteSprite;
        meterNode = stateMachine.Vitalist.meterNode;
        meterSprites = new GameObject[]
        {
            meterNode,
            stateMachine.Vitalist.meterSprite1,
            stateMachine.Vitalist.meterSprite2,
            stateMachine.Vitalist.meterSprite3,
            stateMachine.Vitalist.meterSprite4,
            stateMachine.Vitalist.meterSprite5,
            stateMachine.Vitalist.meterSprite6,
            stateMachine.Vitalist.meterSprite7,
            stateMachine.Vitalist.meterSprite8,
        };
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.Vitalist.PausePopupTimer();
        stateMachine.Vitalist.StopMinigameTimer();

        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Shock The Heart!");

        HapticsManager.Instance.PlayNavigatorHaptic(GameManager.Instance.Navigator.navigatorClarityRumble);
        stateMachine.Vitalist.defibParent.SetActive(true);
        lastLeft = 0;
        lastRight = 0;
        joySticks.SetActive(true);
        shockSprite.SetActive(false);
        shockCompleteSprite.SetActive(false);
        meterNode.SetActive(true);
        currentTick.SetValue(0, () => true);
        currentShockState.SetValue(false, () => true);
        completed.SetValue(false, () => true);
        stateMachine.Vitalist.buttonFX.SetBool("Defib", true);
    }

    public override void Exit() 
    {
        base.Exit();
        TaffyManager.Instance.MinigameComplete = true;
        stateMachine.Vitalist.RestartPopupTimer();
        stateMachine.Vitalist.defibParent.SetActive(false);
        stateMachine.Vitalist.buttonFX.SetBool("Defib", false);
    }

    public override void Update()
    {
        base.Update();
        SoundManager.Instance.PlaySFXOnline("Vitalist_Defib_Tone");
        float leftStick = 0;
        float rightStick = 0;
        bool currentLTriggerState = false;
        bool currentRTriggerState = false;

        HapticsManager.Instance.PlayNavigatorHaptic(GameManager.instance.Navigator.navigatorDefibRumble);

        if (stateMachine.Vitalist.Input != null)
        {
            if (stateMachine.Vitalist.Input.isController)
            {
                leftStick = stateMachine.Vitalist.Input.AnalogueAxis.y;
                rightStick = stateMachine.Vitalist.Input.AnalogueCAxis.y;
                currentLTriggerState = stateMachine.Vitalist.Input.LeftTrigger > 0.4f;
                currentRTriggerState = stateMachine.Vitalist.Input.RightTrigger > 0.4f;
            }
            else
            {
                leftStick = stateMachine.Vitalist.Input.AnalogueCAxis.y * 0.02f;
                rightStick = stateMachine.Vitalist.Input.AnalogueCAxis.x * 0.02f;
                currentLTriggerState = currentRTriggerState = stateMachine.Vitalist.Input.A;
            }
        }

        // Check if both triggers are pressed at the same time
        bool isShocked = currentLTriggerState != previousLTriggerState && currentRTriggerState != previousRTriggerState;

        if (defibTriggersTick > 0.5f)
        {
            previousLTriggerState = currentLTriggerState;
            previousRTriggerState = currentRTriggerState;
            defibTriggersTick = 0;
        }
        else
        {
            defibTriggersTick += Time.deltaTime;
        }

        if ((bool)completed.GetValue())
        {
            shockSprite.SetActive(false);
            shockCompleteSprite.SetActive(true);
        }

        if ((bool)currentShockState.GetValue() && isShocked && !(bool)completed.GetValue())
        {
            completed.SetValue(true, () => Whoami.AmIP2());
            tick = 0;
            stateMachine.Vitalist.StartMinigameComplete();
            HapticsManager.Instance.PlayNavigatorHaptic(GameManager.Instance.Navigator.navigatorDefibCompleteRumble);
            HapticsManager.Instance.PlayVitalistHaptic(GameManager.Instance.Vitalist.defibComplete);
            SoundManager.Instance.PlaySFXOnline("Vitalist_Shook");
            StatsManager.DefibComplete.Invoke(() => Whoami.AmIP2());
        }

        if (Mathf.Abs(leftStick) > sensitivity && Mathf.Abs(rightStick) > sensitivity && tick < tickTotal)
        {
            if (Mathf.Abs(leftStick - lastLeft) > diff && Mathf.Abs(rightStick - lastRight) > diff)
            {
                tick++;
                HapticsManager.Instance.PlayVitalistHaptic(GameManager.Instance.Vitalist.defibCharge);
            }
        }

        if (tick != lastTick)
        {
            lastTick = tick;
            lastTickTime = 0;
        }
        else
        {
            lastTickTime += Time.deltaTime;
        }

        if (lastTickTime > 1f && tick > 0)
        {
            tick -= tickRate;
        }

        if (tick <= 0)
        {
            tick = 0;
        }

        if ((int)currentTick.GetValue() >= tickTotal)
        {
            joySticks.SetActive(false);
            shockSprite.SetActive(true);
            currentShockState.SetValue(true, () => Whoami.AmIP2());
            meterNode.SetActive(false);
        }

        lastLeft = leftStick;
        lastRight = rightStick;

        if (tick != (int)currentTick.GetValue())
        {
            currentTick.SetValue(tick, () => Whoami.AmIP2());
        }

        int tickPercent = (int)((float)currentTick.GetValue() / tickTotal * 9f);

        if (!(bool)currentShockState.GetValue())
        {
            // Set active based on the current tick percentage
            for (int i = 0; i < meterSprites.Length; i++)
            {
                meterSprites[i].SetActive(i < tickPercent + 1);
            }
        }
    }
}
