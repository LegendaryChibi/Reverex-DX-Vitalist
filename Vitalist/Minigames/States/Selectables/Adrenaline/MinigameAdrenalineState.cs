using UnityEngine;

//Name: Joshua Varghese
//Date: 11/27/2023
//Purpose: The adrenaline tertiary minigame will run and end here

public class MinigameAdrenalineState : MinigameState
{
  
    //vitalist class
    private GameObject mouth;
    private GameObject mouthClosed;
    private GameObject mouthOpen;
    private Rigidbody2D pillRb;
    private GameObject level0;
    private GameObject level1;
    private GameObject level2;
    private GameObject level3;
    private GameObject level4;
    private GameObject level5;
    private GameObject level6;
    private GameObject level7;
    private GameObject levelParent;

    //other
    private bool completed = false;
    private float scrollSpeed = 1f;
    private Vector3 pillStartPos;
    private Vector3 levelPosition;
    private float pillPower = 2f;
    private bool lastButtonState = false;
    //private bool first = false;

    private static Synchronized<Vector3> pillPosition = new Synchronized<Vector3>("net_adrenaline_pill_position");
    private static NetAction restartGame = new NetAction("net_adrenaline_game_restart");
    private static NetAction completeGame = new NetAction("net_adrenaline_game_complete");
    private static NetAction pillJump = new NetAction("net_adrenaline_pill_jump");
    private static Synchronized<Float> pillForwards = new Synchronized<Float>("net_adrenaline_pill_forwards");
    private static Synchronized<Vector3> levelTransform = new Synchronized<Vector3>("net_adrenaline_level_transform");
    private static Synchronized<Boolean> first = new Synchronized<Boolean>("net_adrenaline_first_launch", true, false);

    public MinigameAdrenalineState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        mouth = stateMachine.Vitalist.Mouth;
        mouthClosed = stateMachine.Vitalist.MouthClosed;
        mouthOpen = stateMachine.Vitalist.MouthOpen;
        pillRb = stateMachine.Vitalist.PillRb;
        level0 = stateMachine.Vitalist.adrenalineLevel0;
        level1 = stateMachine.Vitalist.adrenalineLevel1;
        level2 = stateMachine.Vitalist.adrenalineLevel2;
        level3 = stateMachine.Vitalist.adrenalineLevel3;
        level4 = stateMachine.Vitalist.adrenalineLevel4;
        level5 = stateMachine.Vitalist.adrenalineLevel5;
        level6 = stateMachine.Vitalist.adrenalineLevel6;
        level7 = stateMachine.Vitalist.adrenalineLevel7;
        levelParent = stateMachine.Vitalist.adrenalineLevelParent;

        pillStartPos = pillRb.transform.localPosition;
        levelPosition = levelParent.transform.position;
    }

    public override void Enter()
    {
        base.Enter();
        restartGame += Restart;
        completeGame += Complete;
        PillCollision.onPillCollision += PillCollided;
        pillJump += PillJump;
        switch (stateMachine.Vitalist.AdrenalineLevel)
        {
            case 0:
                level0.SetActive(true);
                level1.SetActive(false);
                level2.SetActive(false);
                level3.SetActive(false);
                level4.SetActive(false);
                level5.SetActive(false);
                level6.SetActive(false);
                level7.SetActive(false);
                break;
            case 1:
                level0.SetActive(false);
                level1.SetActive(true);
                level2.SetActive(false);
                level3.SetActive(false);
                level4.SetActive(false);
                level5.SetActive(false);
                level6.SetActive(false);
                level7.SetActive(false);
                break;
            case 2:
                level0.SetActive(false);
                level1.SetActive(false);
                level2.SetActive(true);
                level3.SetActive(false);
                level4.SetActive(false);
                level5.SetActive(false);
                level6.SetActive(false);
                level7.SetActive(false);
                break;
            case 3:
                level0.SetActive(false);
                level1.SetActive(false);
                level2.SetActive(false);
                level3.SetActive(true);
                level4.SetActive(false);
                level5.SetActive(false);
                level6.SetActive(false);
                level7.SetActive(false);
                break;
            case 4:
                level0.SetActive(false);
                level1.SetActive(false);
                level2.SetActive(false);
                level3.SetActive(false);
                level4.SetActive(true);
                level5.SetActive(false);
                level6.SetActive(false);
                level7.SetActive(false);
                break;
            case 5:
                level0.SetActive(false);
                level1.SetActive(false);
                level2.SetActive(false);
                level3.SetActive(false);
                level4.SetActive(false);
                level5.SetActive(true);
                level6.SetActive(false);
                level7.SetActive(false);
                break;
            case 6:
                level0.SetActive(false);
                level1.SetActive(false);
                level2.SetActive(false);
                level3.SetActive(false);
                level4.SetActive(false);
                level5.SetActive(false);
                level6.SetActive(true);
                level7.SetActive(false);
                break;
            case 7:
                level0.SetActive(false);
                level1.SetActive(false);
                level2.SetActive(false);
                level3.SetActive(false);
                level4.SetActive(false);
                level5.SetActive(false);
                level6.SetActive(false);
                level7.SetActive(true);
                break;
            default:
                break;
        }

        completed = false;
        pillRb.bodyType = RigidbodyType2D.Dynamic;
        stateMachine.Vitalist.adrenalineParent.SetActive(true);
        stateMachine.Vitalist.StartMinigameTimer();
        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Press\t\tto Jump");
        restartGame.Invoke(() => Whoami.AmIP2());
    }

    public override void Exit()
    {
        base.Exit();
        restartGame -= Restart;
        completeGame -= Complete;
        PillCollision.onPillCollision -= PillCollided;
        pillJump -= PillJump;
        first.SetValue(false, () => { return true; });
        pillForwards.SetValue(0, () => { return true; });
        levelTransform.SetValue(Vector3.zero, () => { return true; });
        stateMachine.Vitalist.adrenalineParent.SetActive(false);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (completed)
        {
            completeGame.Invoke(() => Whoami.AmIP2());
        }

        bool buttonState = ReadAButtonInput();
        
        if (!(bool)first.GetValue()) // this is used to keep the player in the air till they jump for the first time
        {
            pillRb.bodyType = RigidbodyType2D.Static;
            if (buttonState)
            {
                first.SetValue(true, () => Whoami.AmIP2());
            }
        }
        else
        {
            pillRb.bodyType = RigidbodyType2D.Dynamic;
        }
            
        // move the level forwards till the mouth is in frame, then move the pill forwards instead
        if (buttonState && !lastButtonState && (bool)first.GetValue())
        {
            //pill go up
            pillJump.Invoke(() => Whoami.AmIP2());
            HapticsManager.Instance.PlayVitalistHaptic(stateMachine.Vitalist.adrenalineJump);
            SoundManager.Instance.PlaySFXOnline("Vitalist_Adrenaline_PillBounce");
        }

        if ((bool)first.GetValue() && levelParent.transform.position.x > -5f)
        {
            Vector3 newLevelTransform = Vector3.left * scrollSpeed * Time.deltaTime;
            if (newLevelTransform != levelTransform.GetValue())
            {
                levelTransform.SetValue(newLevelTransform, () => Whoami.AmIP2());
            }
            levelParent.transform.position += levelTransform.GetValue();
        }
        if ((bool)first.GetValue() && levelParent.transform.position.x < -4.7f)
        {
            pillForwards.SetValue(0.8f, () => Whoami.AmIP2());
        }

        //Handle resyncronizing the pill when they become too far off.
        if (Whoami.AmIOnline())
        {
            pillPosition.SetValue(pillRb.position, () => Whoami.AmIP2());

            if (Whoami.AmIP1() && Vector3.Distance(pillRb.position, pillPosition.GetValue()) > .1f)
            {
                pillRb.position = Vector3.Lerp(pillRb.position, pillPosition.GetValue(), Time.deltaTime * 5f);
            }
        }
        lastButtonState = buttonState;
    }

    public override void Update() //just here for debug skipping
    {
        base.Update();
    }

    private void PillJump()
    {
        pillRb.velocity = Vector2.zero;
        pillRb.AddForce(new Vector2((float)pillForwards.GetValue(), 1f).normalized * pillPower, ForceMode2D.Impulse);
    }

    private void PillCollided(String collider)
    {
        if ((string)collider == "Barrier")
        {
            restartGame.Invoke(() => Whoami.AmIP2());
        }
        else if ((string)collider == "PillBottle")
        {
            restartGame.Invoke(() => Whoami.AmIP2());
        }
        else if ((string)collider == "Mouth")
        {
            completeGame.Invoke(() => Whoami.AmIP2());
            SoundManager.Instance.PlaySFXOnline("Vitalist_Adrenaline_Swallow");
        }
    }

    private void Restart()
    {
        lastButtonState = false;
        first.SetValue(false, () => Whoami.AmIP2());
        pillRb.gameObject.SetActive(true);
        pillForwards.SetValue(0, () => Whoami.AmIP2());
        pillRb.transform.localPosition = pillStartPos;
        levelParent.transform.position = levelPosition;
    }

    private void Complete()
    {
        TaffyManager.Instance.MinigameComplete = true;
        pillRb.bodyType = RigidbodyType2D.Kinematic;
        pillRb.gameObject.SetActive(false);
        stateMachine.Vitalist.StartMouthAnim();
        stateMachine.Vitalist.AdrenalineLevel++;
        stateMachine.Vitalist.StartMinigameComplete(MinigameQueue.Minigames.exhaustion);
        completed = false;
        StatsManager.Instance.StartEffect(StatsManager.EffectType.Speed, StatsManager.Effect.Buff, 15f);
    }
}