using UnityEngine;

//Name: Joshua Varghese
//Date: 11/27/2023
//Purpose: The adrenaline tertiary minigame will run and end here

public class MinigameElectrolytesState : MinigameState
{
    private GameObject energyDrink;
    private GameObject x;
    private GameObject checkmark;
    private GameObject spillPoint;
    private GameObject highPour;
    private GameObject goodPour;
    private GameObject lowPour;

    private float pourTime = 0;
    private float pourTimeLimit;
    private bool isPouring = false;
    private Vector3 startingPosition;
    public bool complete = false;
    private bool completed = false;
    private float energyMax = 100f;
    private float energyMaxMin = 50f;
    private float currEnergy = 0f;
    private float energyIncreaseValue = 600f;
    float increaseTotalTime = 0.1f;
    float increaseTime = 0;
    bool prevButtonState = false;

    Synchronized<Vector3> currentRotation = new Synchronized<Vector3>("net_electrolytes_current_rotation");

    public MinigameElectrolytesState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        energyDrink = stateMachine.Vitalist.energyDrink;
        x = stateMachine.Vitalist.x;
        checkmark = stateMachine.Vitalist.checkmark;
        spillPoint = stateMachine.Vitalist.spillPoint;
        highPour = stateMachine.Vitalist.highPour;
        goodPour = stateMachine.Vitalist.goodPour;
        lowPour = stateMachine.Vitalist.lowPour;

        startingPosition = energyDrink.transform.eulerAngles;
        currEnergy = 0f;
        energyMaxMin = energyMax * 0.5f;
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.Vitalist.StartAnimatePour();
        energyDrink.transform.eulerAngles = startingPosition;
        highPour.SetActive(false);
        goodPour.SetActive(false);
        lowPour.SetActive(false);
        checkmark.SetActive(false);
        x.SetActive(false);
        pourTime = 0;
        complete = false;
        completed = false;
        switch (stateMachine.Vitalist.ElectrolytesLevel)
        {
            case 0:
                energyIncreaseValue = 600;
                pourTimeLimit = 1.5f;
                break;
            case 1:
                energyIncreaseValue = 500;
                pourTimeLimit = 2f;
                break;
            case 2:
                energyIncreaseValue = 400;
                pourTimeLimit = 2.5f;
                break;
            case 3:
                pourTimeLimit = 3f;
                energyIncreaseValue = 300;
                break;
            case 4:
                pourTimeLimit = 3.5f;
                energyIncreaseValue = 250;
                break;
            case 5:
                pourTimeLimit = 4f;
                energyIncreaseValue = 200;
                break;
        }
        stateMachine.Vitalist.electrolytesParent.SetActive(true);
        stateMachine.Vitalist.StartMinigameTimer();
        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Tap \t\t");
    }


    public override void Exit()
    {
        base.Exit();
        stateMachine.Vitalist.StopAnimatePour();
        stateMachine.Vitalist.electrolytesParent.SetActive(false);
    }

    public override void Update()
    {
        base.Update();
        if (increaseTime > 0)
        {
            currEnergy -= energyIncreaseValue * Time.deltaTime;
            increaseTime -= Time.deltaTime;
        }
        spillPoint.transform.eulerAngles = new Vector3(spillPoint.transform.eulerAngles.x, spillPoint.transform.eulerAngles.y, 0);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (currEnergy < -energyMaxMin)
        {
            currEnergy = -energyMaxMin;
        }
        if (currEnergy > energyMaxMin)
        {
            currEnergy = energyMaxMin;
        }

        bool currentButtonState;

        currentButtonState = ReadBButtonInput();


        if (currentButtonState && !prevButtonState && !completed)
        {
            increaseTime = increaseTotalTime;
        }

        prevButtonState = currentButtonState;

        currEnergy = Mathf.Lerp(currEnergy, energyMax, Time.fixedDeltaTime);

        float newRotation = energyDrink.transform.eulerAngles.z + currEnergy * Time.fixedDeltaTime;

        if (newRotation > 180)
        {
            newRotation -= 360;
        }
        else if (newRotation < -180)
        {
            newRotation += 360;
        }

        float clampedRotation = Mathf.Clamp(newRotation, startingPosition.z, 170);
        if (!complete)
        {
            Vector3 bottleTransform = new Vector3(energyDrink.transform.eulerAngles.x, energyDrink.transform.eulerAngles.y, clampedRotation);
            if (bottleTransform != currentRotation.GetValue())
            {
                currentRotation.SetValue(bottleTransform, () => Whoami.AmIP2());
            }
            energyDrink.transform.eulerAngles = currentRotation.GetValue();
        }

        float currRot = energyDrink.transform.localEulerAngles.z;
        if (currRot < 40)
        {
            highPour.SetActive(true);
            goodPour.SetActive(false);
            lowPour.SetActive(false);
            if (!completed)
            {
                checkmark.SetActive(false);
                x.SetActive(true);
            }
            isPouring = false;
            pourTime = 0;
        }
        else if (currRot < 65)
        {
            highPour.SetActive(false);
            goodPour.SetActive(true);
            lowPour.SetActive(false);
            checkmark.SetActive(true);
            x.SetActive(false);
            isPouring = true;
        }
        else
        {
            highPour.SetActive(false);
            goodPour.SetActive(false);
            lowPour.SetActive(true);
            if (!completed)
            {
                checkmark.SetActive(false);
                x.SetActive(true);
            }
            pourTime = 0;
            isPouring = false;
        }

        if (isPouring && !completed)
        {
            pourTime += Time.deltaTime;

            if (HapticsManager.Instance.currentVitalistRumble != stateMachine.Vitalist.pour)
                HapticsManager.Instance.PlayVitalistHaptic(stateMachine.Vitalist.pour);

            SoundManager.Instance.PlaySFXOnline("Vitalist_Electrolytes_Swallow");
        }
        else
        {
            if (HapticsManager.Instance.currentVitalistRumble != stateMachine.Vitalist.emptyRumble)
                HapticsManager.Instance.PlayVitalistHaptic(stateMachine.Vitalist.emptyRumble);
        }

        if (pourTime >= pourTimeLimit && !complete)
        {
            complete = true;
        }

        if (complete && !completed)
        {
            completed = true;
            TaffyManager.Instance.MinigameComplete = true;
            stateMachine.Vitalist.StartMinigameComplete(MinigameQueue.Minigames.fracture);
            stateMachine.Vitalist.ElectrolytesLevel++;
            StatsManager.Instance.StartEffect(StatsManager.EffectType.Jump, StatsManager.Effect.Buff, 15f);
        }
    }
    protected override bool ReadBButtonInput()
    {
        if (stateMachine.Vitalist.Input == null) { return false; }
        if (stateMachine.Vitalist.Input.isController)
        {
            return base.ReadBButtonInput();
        }
        return stateMachine.Vitalist.Input.A;
    }
}
