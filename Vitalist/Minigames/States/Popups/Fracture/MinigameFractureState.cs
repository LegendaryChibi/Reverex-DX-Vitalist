using UnityEditor;
using UnityEngine;

//Name: Thomas Berner
//Date: 09/25/2024
//Summary: the fracture pop up minigame for the vitalist to play

public class MinigameFractureState : MinigameState
{
    //vitalist class
    private GameObject jumperIdle;
    private GameObject jumperJump;
    private GameObject jumperLand;
    private GameObject lvl0;
    private GameObject lvl1;
    private GameObject lvl2;
    private GameObject lvl3;
    private GameObject lvl4;
    private GameObject lvl5;
    private Rigidbody2D jumperRb;

    //other
    private Vector3 startPos; //get init location of jumper for reset
    private float speed = 1.25f;
    private float jump = 4f;
    private bool completed = false;
    private bool failed = false;
    private bool prevButtonState = false;

    private static NetAction resetJumper = new NetAction("net_fracture_reset");
    private static Synchronized<Vector3> jumperPosition = new Synchronized<Vector3>("net_fracture_jumper_position");
    private static Synchronized<Vector2> jumperVelocity = new Synchronized<Vector2>("net_fracture_jumper_velocity");

    public MinigameFractureState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        jumperIdle = stateMachine.Vitalist.jumperIdle;
        jumperJump = stateMachine.Vitalist.jumperJump;
        jumperLand = stateMachine.Vitalist.jumperLand;
        lvl0 = stateMachine.Vitalist.lvl0;
        lvl1 = stateMachine.Vitalist.lvl1;
        lvl2 = stateMachine.Vitalist.lvl2;
        lvl3 = stateMachine.Vitalist.lvl3;
        lvl4 = stateMachine.Vitalist.lvl4;
        lvl5 = stateMachine.Vitalist.lvl5;
        jumperRb = stateMachine.Vitalist.jumperRb;

        startPos = jumperRb.transform.localPosition;
        jumperRb.gravityScale = 2;
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.Vitalist.PausePopupTimer();
        StatsManager.Instance.StartEffect(StatsManager.EffectType.Jump, StatsManager.Effect.Debuff);

        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Tap \t\t to Jump!");
        jumperRb.bodyType = RigidbodyType2D.Dynamic;

        JumperCollision.onJumperCollision += JumperCollide; //:3
        resetJumper += Reset;

        resetJumper.Invoke(() => Whoami.AmIP2());

        stateMachine.Vitalist.fractureParent.SetActive(true);
        jumperIdle.SetActive(true);
        jumperJump.SetActive(false);
        jumperLand.SetActive(false);


        failed = false;
        completed = false;
        switch (stateMachine.Vitalist.FractureLevel)
        {
            case 0:
                lvl0.SetActive(true);
                lvl1.SetActive(false);
                lvl2.SetActive(false);
                lvl3.SetActive(false);
                lvl4.SetActive(false);
                lvl5.SetActive(false);
                break;
            case 1:
                lvl0.SetActive(false);
                lvl1.SetActive(true);
                lvl2.SetActive(false);
                lvl3.SetActive(false);
                lvl4.SetActive(false);
                lvl5.SetActive(false);
                break;
            case 2:
                lvl0.SetActive(false);
                lvl1.SetActive(false);
                lvl2.SetActive(true);
                lvl3.SetActive(false);
                lvl4.SetActive(false);
                lvl5.SetActive(false);
                break;
            case 3:
                lvl0.SetActive(false);
                lvl1.SetActive(false);
                lvl2.SetActive(false);
                lvl3.SetActive(true);
                lvl4.SetActive(false);
                lvl5.SetActive(false);
                break;
            case 4:
                lvl0.SetActive(false);
                lvl1.SetActive(false);
                lvl2.SetActive(false);
                lvl3.SetActive(false);
                lvl4.SetActive(true);
                lvl5.SetActive(false);
                break;
            case 5:
                lvl0.SetActive(false);
                lvl1.SetActive(false);
                lvl2.SetActive(false);
                lvl3.SetActive(false);
                lvl4.SetActive(false);
                lvl5.SetActive(true);
                break;
        }
        //set debuff here
    }

    public override void Exit()
    {
        base.Exit();
        TaffyManager.Instance.MinigameComplete = true;
        stateMachine.Vitalist.fractureParent.SetActive(false);
        JumperCollision.onJumperCollision -= JumperCollide;
        resetJumper -= Reset;
        //clear debuff
        stateMachine.Vitalist.RestartPopupTimer();
        StatsManager.Instance.StopAllEffectAndType(StatsManager.EffectType.Jump, StatsManager.Effect.Debuff);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        //button states
        bool jumpButtonState;
        
        jumpButtonState = ReadAButtonInput();

        if (failed)
        {
            resetJumper.Invoke(() => Whoami.AmIP2());
            failed = false;
        }
        if (completed)
        {
            completed = false;
            jumperRb.bodyType = RigidbodyType2D.Static;
            stateMachine.Vitalist.FractureLevel++;
            stateMachine.Vitalist.StartMinigameComplete();
            StatsManager.Instance.StopAllEffectAndType(StatsManager.EffectType.Jump, StatsManager.Effect.Debuff);
        }
        else //Handles moving side to side
        {
            Vector2 newVelocity = new Vector2(movementInput.x * speed, jumperRb.velocity.y);
            if (newVelocity != jumperVelocity.GetValue())
            {
                jumperVelocity.SetValue(newVelocity, () => Whoami.AmIP2());
            }
            jumperRb.velocity = jumperVelocity.GetValue();
        }
        
        //Handles jumping
        if (jumpButtonState && !prevButtonState && jumperRb.velocity.y == 0)
        {
            Vector2 newVelocity = new Vector2(jumperRb.velocity.x, jump);
            if (newVelocity != jumperVelocity.GetValue())
            {
                jumperVelocity.SetValue(newVelocity, () => Whoami.AmIP2());
            }
            jumperRb.velocity = jumperVelocity.GetValue();
            //play jump sound
        }
        prevButtonState = jumpButtonState;

        if (jumperRb.velocity.y > 0)
        {
            jumperIdle.SetActive(false);
            jumperJump.SetActive(true);
            jumperLand.SetActive(false);
        }
        else if (jumperRb.velocity.y < 0)
        {
            jumperIdle.SetActive(false);
            jumperJump.SetActive(false);
            jumperLand.SetActive(true);
        }
        else
        {
            jumperIdle.SetActive(true);
            jumperJump.SetActive(false);
            jumperLand.SetActive(false);
        }


        //Handle resyncronizing the jumper if they get too far off.
        if (Whoami.AmIOnline())
        {
            jumperPosition.SetValue(jumperRb.position, () => Whoami.AmIP2());

            if (Whoami.AmIP1() && Vector3.Distance(jumperRb.position, jumperPosition.GetValue()) > .1f)
            {
                jumperRb.position = Vector3.Lerp(jumperRb.position, jumperPosition.GetValue(), Time.deltaTime * 2.5f);
            }
        }

    }
    public override void Update() // this is only here for debug skipping -> can remove in the future
    {
        base.Update();
    }

    private void JumperCollide(String collision)
    {
        if ((string)collision == "Finish")
        {
            completed = true;
        }
        if ((string)collision == "Barrier")
        {
            failed = true;
        }
    }

    private void Reset()
    {
        jumperRb.transform.localPosition = startPos;
        //play fail sound here
    }
}
