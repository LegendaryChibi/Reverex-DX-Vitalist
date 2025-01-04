using UnityEngine;

/// <summary>
/// Author: Joshua Varghese
/// Date: 2024-10-30
/// </summary>

public class MinigameRewindState : MinigameState
{
    private float lastDeg = 0;
    private float timeRequired = 1;
    private float degreesPerSecond = 45;
    private float minigameTime = 0;
    private bool completed = false;
    private bool spin = true;
    private Animator animator;
    private NetRoutine<Boolean> rewindTape;

    private GameObject arrows;

    private static Synchronized<Float> rotation = new Synchronized<Float>("net_rewind_rotation", false, 0);

    public MinigameRewindState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        arrows = stateMachine.Vitalist.arrows;
        animator = stateMachine.Vitalist.rewindAnimator;
        rewindTape = new NetRoutine<Boolean>(ToggleRewindTape, Ownership.Vitalist);
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.Vitalist.PausePopupTimer();
        stateMachine.Vitalist.StopMinigameTimer();

        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Spin The Joystick!");
        SoundManager.Instance.PlaySFXOnline("Rewind_Open");
        completed = false;
        minigameTime = 0;
        stateMachine.Vitalist.rewindParent.SetActive(true);

        rotation.SetValue(0, () => Whoami.AmIP2());
        spin = true;
        rewindTape.Invoke(true);
    }

    public override void Exit()
    {
        base.Exit();
        SoundManager.Instance.PlaySFXOnline("Rewind_Close");
        stateMachine.Vitalist.RestartPopupTimer();
        stateMachine.Vitalist.rewindParent.SetActive(false);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Whoami.AmIP2())
        {
            if (GameManager.Instance.Navigator.IsGrounded && !completed)
            {
                spin = false;
                FinishGame(false);
            }

            if (minigameTime > timeRequired && !completed)
            {
                spin = false;
                FinishGame(true);
            }
            float currentDeg = ControllerAngle();

            float diffDeg = currentDeg - lastDeg;

            //Throw out bad readings from the controller.
            if (stateMachine.Vitalist.Input.AnalogueAxis.sqrMagnitude < 0.5 && stateMachine.Vitalist.Input.isController)
            {
                diffDeg = 0;
                return;
            }

            //Account for deltaTime;
            diffDeg /= Time.deltaTime;

            lastDeg = currentDeg;

            if (diffDeg > degreesPerSecond && diffDeg < 50 / Time.deltaTime && !completed)
            {
                Debug.Log($"Amount Completed {minigameTime}");
                minigameTime += Time.deltaTime;
                float newRot = (diffDeg * 0.3f) * Time.deltaTime;
                if (newRot != (float)rotation.GetValue())
                {
                    SoundManager.Instance.PlaySFXOnline("Rewind_Play");
                    rotation.SetValue(newRot, () => Whoami.AmIP2());
                }
            }
            else
            {
                if (minigameTime > 0)
                {
                    minigameTime -= Time.deltaTime;
                }
            }
        }

        if (spin)
        {
            arrows.transform.Rotate(Vector3.forward, (float)rotation.GetValue());
        }
    }

    private void FinishGame(bool success)
    {
        completed = true;
        stateMachine.Vitalist.StartMinigameComplete();
        minigameTime = 0;
        if (success)
        {
            RewindManager.Instance.TriggerRewind.Invoke();
        }
        rewindTape.Invoke(false);
    }

    public float ControllerAngle()
    {
        if (stateMachine.Vitalist.Input.isController)
        {
            if (stateMachine.Vitalist.Input != null)
            {
                return Mathf.Atan2(stateMachine.Vitalist.Input.AnalogueAxis.y, stateMachine.Vitalist.Input.AnalogueAxis.x) * Mathf.Rad2Deg;
            }
            return -1;
        }
        else
        {
            if (stateMachine.Vitalist.Input != null)
            {
                return Mathf.Atan2(stateMachine.Vitalist.Input.AnalogueCAxis.y, stateMachine.Vitalist.Input.AnalogueCAxis.x) * Mathf.Rad2Deg;
            }
            return -1;
        }

    }

    private void ToggleRewindTape(Boolean toggle)
    {
        animator.SetBool("Popup", (bool)toggle);
    }
}
