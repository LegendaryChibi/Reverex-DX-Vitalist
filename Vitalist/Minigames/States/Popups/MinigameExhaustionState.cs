using UnityEngine;

//Name: Thomas Berner
//Date: 09/26/24
//Purpose: The exhaustion pop up minigame state script :3

public class MinigameExhaustionState : MinigameState
{
    //Vitalist class
    private GameObject arrowLeft;
    private GameObject arrowRight;
    private Rigidbody2D runnerRb;
    private GameObject bedLvl0;
    private GameObject bedLvl1;
    private GameObject bedLvl2;
    private GameObject bedLvl3;
    private GameObject bedLvl4;
    private GameObject bedLvl5;
    private GameObject sleep;

    //other
    private float speed = 1.2f;
    private bool completed = false;
    private bool sleepy = false;
    private Vector3 startPosRunner;
    private Vector3 startPosLvl;
    private float laneDist = 0.5f;
    private bool resetStick = false;

    private static Synchronized<Interger> laneIndex = new Synchronized<Interger>("net_exhaustion_lane_index"); //lane 0 is center, left is -1, right is 1
    private static NetAction reset = new NetAction("net_exhaustion_reset");
    private static NetAction<Float> changeLane = new NetAction<Float>("net_exhaustion_change_lane");
    private static NetAction<Vector3> levelTransformSync = new NetAction<Vector3>("net_exhaustion_transform");

    public MinigameExhaustionState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        arrowLeft = stateMachine.Vitalist.arrowLeft;
        arrowRight = stateMachine.Vitalist.arrowRight;
        runnerRb = stateMachine.Vitalist.runnerRb;
        bedLvl0 = stateMachine.Vitalist.bedLvl0;
        bedLvl1 = stateMachine.Vitalist.bedLvl1;
        bedLvl2 = stateMachine.Vitalist.bedLvl2;
        bedLvl3 = stateMachine.Vitalist.bedLvl3;
        bedLvl4 = stateMachine.Vitalist.bedLvl4;
        bedLvl5 = stateMachine.Vitalist.bedLvl5;
        sleep = stateMachine.Vitalist.sleep;
        startPosRunner = runnerRb.transform.localPosition;
        startPosLvl = Vector3.zero;
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.Vitalist.PausePopupTimer();
        StatsManager.Instance.StartEffect(StatsManager.EffectType.Speed, StatsManager.Effect.Debuff);

        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Avoid The Beds!");
        RunnerCollision.onRunnerCollsion += RunnerCollide; //runner colision event attach
        reset += ResetRunner;
        changeLane += ChangeLane;
        reset.Invoke(() => Whoami.AmIP2());
        levelTransformSync += SyncTransform;
        Tick.Instance.OnTick += NetworkTransformSync;
        completed = false;
        //set active level
        switch (stateMachine.Vitalist.ExhaustionLevel)
        {
            case 0:
                bedLvl0.SetActive(true);
                bedLvl1.SetActive(false);
                bedLvl2.SetActive(false);
                bedLvl3.SetActive(false);
                bedLvl4.SetActive(false);
                bedLvl5.SetActive(false);
                stateMachine.Vitalist.currentBedLvl = bedLvl0;
                break;
            case 1:
                bedLvl0.SetActive(false);
                bedLvl1.SetActive(true);
                bedLvl2.SetActive(false);
                bedLvl3.SetActive(false);
                bedLvl4.SetActive(false);
                bedLvl5.SetActive(false);
                stateMachine.Vitalist.currentBedLvl = bedLvl1;
                break;
            case 2:
                bedLvl0.SetActive(false);
                bedLvl1.SetActive(false);
                bedLvl2.SetActive(true);
                bedLvl3.SetActive(false);
                bedLvl4.SetActive(false);
                bedLvl5.SetActive(false);
                stateMachine.Vitalist.currentBedLvl = bedLvl2;
                break;
            case 3:
                bedLvl0.SetActive(false);
                bedLvl1.SetActive(false);
                bedLvl2.SetActive(false);
                bedLvl3.SetActive(true);
                bedLvl4.SetActive(false);
                bedLvl5.SetActive(false);
                stateMachine.Vitalist.currentBedLvl = bedLvl3;
                break;
            case 4:
                bedLvl0.SetActive(false);
                bedLvl1.SetActive(false);
                bedLvl2.SetActive(false);
                bedLvl3.SetActive(false);
                bedLvl4.SetActive(true);
                bedLvl5.SetActive(false);
                stateMachine.Vitalist.currentBedLvl = bedLvl4;
                break;
            case 5:
                bedLvl0.SetActive(false);
                bedLvl1.SetActive(false);
                bedLvl2.SetActive(false);
                bedLvl3.SetActive(false);
                bedLvl4.SetActive(false);
                bedLvl5.SetActive(true);
                stateMachine.Vitalist.currentBedLvl = bedLvl5;
                break;
        }
        //setdebuff here
        stateMachine.Vitalist.exhaustionParent.SetActive(true);
    }

    public override void Exit()
    {
        base.Exit();
        TaffyManager.Instance.MinigameComplete = true;
        stateMachine.Vitalist.exhaustionParent.SetActive(false);
        RunnerCollision.onRunnerCollsion -= RunnerCollide;
        reset -= ResetRunner;
        changeLane -= ChangeLane;
        levelTransformSync -= SyncTransform;
        Tick.Unsubscribe(NetworkTransformSync);
        //clear debuff here
        stateMachine.Vitalist.RestartPopupTimer();
        StatsManager.Instance.StopAllEffectAndType(StatsManager.EffectType.Speed, StatsManager.Effect.Debuff);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        //bed hit
        if (sleepy)
        {
            HapticsManager.Instance.PlayVitalistHaptic(stateMachine.Vitalist.hitBed);
            stateMachine.Vitalist.StartSleepAnim();
            reset.Invoke(() => Whoami.AmIP2());
        }

        if (completed)
        {
            completed = false;
            stateMachine.Vitalist.ExhaustionLevel++;
            stateMachine.Vitalist.StartMinigameComplete();
            StatsManager.Instance.StopAllEffectAndType(StatsManager.EffectType.Speed, StatsManager.Effect.Debuff);
        }

        if (movementInput.x <= 0.4f && movementInput.x >= -0.4f) resetStick = true;

        //scroll bed map down
        if (stateMachine.Vitalist.currentBedLvl.activeInHierarchy) stateMachine.Vitalist.currentBedLvl.transform.Translate(Vector3.down * speed * Time.deltaTime);
        
        if (movementInput.x > 0.4 && (int)laneIndex.GetValue() < 1 && resetStick) //move right
        {
            laneIndex.SetValue((int)laneIndex.GetValue() + 1, () => Whoami.AmIP2());
            changeLane.Invoke(() => Whoami.AmIP2(), laneDist);
            resetStick = false;
        } 
        if (movementInput.x < -0.4 && (int)laneIndex.GetValue() > -1 && resetStick) //move left
        {
            laneIndex.SetValue((int)laneIndex.GetValue() - 1, () => Whoami.AmIP2());
            changeLane.Invoke(() => Whoami.AmIP2(), -laneDist);
            resetStick = false;
        }

        switch ((int)laneIndex.GetValue())
        {
            case -1:
                arrowLeft.SetActive(false);
                arrowRight.SetActive(true);
                break;
            case 0:
                arrowLeft.SetActive(true);
                arrowRight.SetActive(true);
                break;
            case 1:
                arrowLeft.SetActive(true);
                arrowRight.SetActive(false);
                break;
        }

    }

    public override void Update() // this is only here for debug skipping -> can be removed later
    {
        base.Update();
    }

    private void RunnerCollide(String collider)
    {
        if ((string)collider == "Bed")
        {
            sleepy = true;
        }
        if ((string)collider == "Finish")
        {
            completed = true;
        }
    }

    private void ChangeLane(Float dist)
    {
        runnerRb.transform.localPosition = runnerRb.transform.localPosition + new Vector3((float)dist, 0, 0);
    }

    private void ResetRunner()
    {
        runnerRb.transform.localPosition = startPosRunner;
        stateMachine.Vitalist.currentBedLvl.transform.localPosition = startPosLvl;
        laneIndex.SetValue(0, () => Whoami.AmIP2());
        sleepy = false;
    }

    void NetworkTransformSync()
    {
        levelTransformSync.Invoke(() => Whoami.AmIP2(), stateMachine.Vitalist.currentBedLvl.transform.position);
    }

    void SyncTransform(Vector3 pos)
    {
        stateMachine.Vitalist.currentBedLvl.transform.position = pos;
    }
}
