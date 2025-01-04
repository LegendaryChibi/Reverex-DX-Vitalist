using UnityEngine;

//Name: Joshua Varghese
//Date: 11/27/2023
//Purpose: The pain killer tertiary minigame will run and end here

public class MinigameAntibioticsState : MinigameState
{
    private GameObject spinner;
    private GameObject neutralFace;
    private GameObject happyFace;
    private GameObject germ1;
    private GameObject germ2;
    private GameObject germ3;

    private bool isCompleted = false;
    private bool clockWise = true;
    private bool prevButtonState = false;
    private int spinnerSpeed = 140;
    private int activeGermCount = 0;

    //private bool germCollided = false;
    private GameObject currentGerm;

    private Synchronized<Float> germ1Rotation = new Synchronized<Float>("net_germ_rotation_1");
    private Synchronized<Float> germ2Rotation = new Synchronized<Float>("net_germ_rotation_2");
    private Synchronized<Float> germ3Rotation = new Synchronized<Float>("net_germ_rotation_3");

    private bool germCollided = false;
    private static NetAction startGame = new NetAction("net_germ_control_start");
    private static NetAction<String> onGermHit = new NetAction<String>("net_germ_control_on_germ_hit");
    private static Synchronized<Float> angle = new Synchronized<Float>("net_antibiotics_spinner_angle");

    public MinigameAntibioticsState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        spinner = stateMachine.Vitalist.spinner;
        neutralFace = stateMachine.Vitalist.neutralFace;
        happyFace = stateMachine.Vitalist.happyFace;
        germ1 = stateMachine.Vitalist.germ1;
        germ2 = stateMachine.Vitalist.germ2;
        germ3 = stateMachine.Vitalist.germ3;
    }

    public override void Enter()
    {
        base.Enter();
        onGermHit += OnGermHit;
        startGame += StartGame;
        stateMachine.Vitalist.StartMinigameTimer();
        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Tap \t\t!");

        GermCollision.onGermCollided += GermCollided;

        stateMachine.Vitalist.antibioticsParent.SetActive(true);

        happyFace.SetActive(false);
        neutralFace.SetActive(false);

        germ1Rotation.SetValue(Random.Range(0, 40), () => Whoami.AmIP2());
        germ2Rotation.SetValue(Random.Range(140, 160), () => Whoami.AmIP2());
        germ3Rotation.SetValue(Random.Range(210, 310), () => Whoami.AmIP2());

        startGame.Invoke(() => Whoami.AmIP2());
    }

    public void StartGame()
    {
        // Colin Note:
        // We need to use an action here becuase Synchronized is a non-blocking function, we weren't waiting for the action to occur before trying to use the values.
        germ1.transform.Rotate(0f, 0f, (float)germ1Rotation.GetValue(), Space.Self);
        germ2.transform.Rotate(0f, 0f, (float)germ2Rotation.GetValue(), Space.Self);
        germ3.transform.Rotate(0f, 0f, (float)germ3Rotation.GetValue(), Space.Self);

        germ1.SetActive(true);
        germ2.SetActive(true);
        germ3.SetActive(true);

        isCompleted = false;
        clockWise = true;
        activeGermCount = 3;
        germCollided = false;
        currentGerm = null;

        //Disable some germs on early levels
        switch (stateMachine.Vitalist.AntibioticsLevel)
        {
            case 0:
                activeGermCount = 2;
                germ1.SetActive(true);
                germ2.SetActive(true);
                germ3.SetActive(false);
                break;
            case 1:
                germ1.SetActive(true);
                germ2.SetActive(true);
                germ3.SetActive(true);
                break;
        }
    }

    public override void Exit()
    {
        base.Exit();
        startGame -= StartGame;
        onGermHit -= OnGermHit;
        GermCollision.onGermCollided -= GermCollided;
        stateMachine.Vitalist.antibioticsParent.SetActive(false);
    }

    public override void Update()
    {
        base.Update();

        if (clockWise)
        {
            angle.SetValue(360, () => Whoami.AmIP2());
        }
        else
        {
            angle.SetValue(-1, () => Whoami.AmIP2());
        }

        //Spin Spinner
        try 
        {
            RotateObject(spinner, (float)angle.GetValue(), spinnerSpeed);
        }
        catch
        {

        }

        //Spin Germs if Level Over 1
        switch (stateMachine.Vitalist.AntibioticsLevel)
        {
            case 2:
                RotateObject(germ1, -1, 45);
                RotateObject(germ2, -1, 55);
                RotateObject(germ3, -1, 65);
                break;
            case 3:
                RotateObject(germ1, -1, 85);
                RotateObject(germ2, 360, 95);
                RotateObject(germ3, -1, 105);
                break;
            case 4:
                RotateObject(germ1, 360, 155);
                RotateObject(germ2, -1, 165);
                RotateObject(germ3, -1, 175);
                break;
            case 5:
                RotateObject(germ1, -1, 235);
                RotateObject(germ2, 360, 245);
                RotateObject(germ3, 360, 255);
                break;
        }

        bool currentButtonState;

        currentButtonState = ReadXButtonInput();


        if (currentButtonState && !prevButtonState && germCollided && Whoami.AmIP2())
        {
            clockWise = !clockWise;
            onGermHit.Invoke(() => Whoami.AmIP2(), currentGerm.name);
            germCollided = false;
            currentGerm.SetActive(false);
            HapticsManager.Instance.PlayVitalistHaptic(stateMachine.Vitalist.antibioticsHit);
            activeGermCount--;
        }

        prevButtonState = currentButtonState;

        switch (activeGermCount)
        {
            case 3:
                break;
            case 2:
                neutralFace.SetActive(true);
                break;
            case 1:
                break;
            case 0:
                happyFace.SetActive(true);
                if (!isCompleted)
                {
                    isCompleted = true;
                    if (Whoami.AmIP2())
                    {
                        SoundManager.Instance.PlaySFXOnline("Navigator_Heart_Resilience");
                        TaffyManager.Instance.MinigameComplete = true;
                        stateMachine.Vitalist.StartMinigameComplete(MinigameQueue.Minigames.IV);
                        stateMachine.Vitalist.AntibioticsLevel++;
                        StatsManager.Instance.StartEffect(StatsManager.EffectType.OrganResil, StatsManager.Effect.Buff, 15f);
                    }
                }
                break;
        }
    }

    protected override bool ReadXButtonInput()
    {
        if (stateMachine.Vitalist.Input == null) { return false; }
        if (stateMachine.Vitalist.Input.isController)
        {
            return base.ReadXButtonInput();
        }
        return stateMachine.Vitalist.Input.A;
    }

    private void RotateObject(GameObject obj, float angle, float speed)
    {
        float targetAngle = angle;
        float currentAngle = obj.transform.eulerAngles.z;
        float angleDifference = targetAngle - currentAngle;

        float rotationAmount = Mathf.Min(speed * Time.deltaTime, Mathf.Abs(angleDifference)) * Mathf.Sign(angleDifference);
        obj.transform.Rotate(0f, 0f, rotationAmount, Space.Self);
    }

    private void GermCollided(String germ)
    {
        if ((string)germ != "NONE")
        {
            germCollided = true;
            currentGerm = GermCollision.currentCollision?.gameObject;    
        }
        else
        {
            germCollided = false;
            currentGerm = null;
        }
    }
    private void OnGermHit(String germName)
    {
        if(Whoami.AmIOnline())
        {
            if(Whoami.AmIP1())
            {
                if(germ1.name == (string)germName)
                {
                    germ1.SetActive(false);
                }
                if (germ2.name == (string)germName)
                {
                    germ2.SetActive(false);
                }
                if (germ3.name == (string)germName)
                {
                    germ3.SetActive(false);
                }
                activeGermCount--;
            }
        }
        SoundManager.Instance.PlaySFXOnline("Vitalist_Germ_KillBug");
    }
}
