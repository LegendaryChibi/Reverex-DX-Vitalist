using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MinigameIVState : MinigameState
{
    private bool clicked = false;
    private GameObject needle;
    private GameObject target;
    private float controllerThreshold = 0.9f;
    private Rigidbody2D needleRigidbody;
    private Vector3 needleStartingPos;
    private Vector3 targetStartingPos;
    private Vector3 targetStartingScale;
    public bool complete = false;
    int direction = 1;
    float targetRangeOfMotion = 0.4f;

    private static NetAction resetVelocity = new NetAction("net_iv_reset_velocity");
    private static NetAction<Float> pushNeedle = new NetAction<Float>("net_iv_push_needle");
    private static NetAction<Float> setTarget = new NetAction<Float>("net_iv_set_target");
    private static Synchronized<Vector3> needlePosition = new Synchronized<Vector3>("net_iv_needle_pos");
    private static Synchronized<Vector3> targetNewPos = new Synchronized<Vector3>("net_iv_set_target_pos");

    public MinigameIVState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        needleRigidbody = stateMachine.Vitalist.needleRigidbody;
        needle = stateMachine.Vitalist.needle;
        needleStartingPos = needle.transform.localPosition;

        target = stateMachine.Vitalist.target;
        targetStartingPos = target.transform.localPosition;
        targetStartingScale = target.transform.localScale;
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.Vitalist.PausePopupTimer();
        StatsManager.Instance.StartEffect(StatsManager.EffectType.OrganResil, StatsManager.Effect.Debuff);

        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Guide The Needle!");

        TargetCollision.onNeedleCollision += TargetHit;
        BarrierRestart.onBarrierCollision += Reset;
        setTarget += SetTarget;
        resetVelocity += ResetVelocity;
        pushNeedle += PushNeedle;
        stateMachine.Vitalist.ivParent.SetActive(true);
        clicked = false;
        needleRigidbody.bodyType = RigidbodyType2D.Dynamic;
        float randPos = Random.Range(-targetRangeOfMotion, targetRangeOfMotion);
        needle.transform.localPosition = needleStartingPos;
        setTarget.Invoke(() => Whoami.AmIP2(), randPos);
        needleRigidbody.gravityScale = 0.01f;

        switch (stateMachine.Vitalist.IVLevel)
        {
            case 0:
                target.transform.localScale = targetStartingScale * 1.1f;
                break;
            case 1:
                target.transform.localScale = targetStartingScale;
                break;
            case 2:
                target.transform.localScale = targetStartingScale * 0.9f;
                break;
            case 3:
                target.transform.localScale = targetStartingScale * 0.8f;
                break;
        }
    }

    public override void Exit()
    {
        base.Exit();
        TargetCollision.onNeedleCollision -= TargetHit;
        BarrierRestart.onBarrierCollision -= Reset;
        resetVelocity -= ResetVelocity;
        pushNeedle -= PushNeedle;
        setTarget -= SetTarget;
        stateMachine.Vitalist.RestartPopupTimer();
        stateMachine.Vitalist.ivParent.SetActive(false);
        StatsManager.Instance.StopAllEffectAndType(StatsManager.EffectType.OrganResil, StatsManager.Effect.Debuff);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (movementInput.y < -0.5 && Mathf.Abs(movementInput.x) < 0.3)
        {
            needleRigidbody.gravityScale = 0.02f;
        }
        else
        {
            needleRigidbody.gravityScale = 0.02f;
        }


        if (movementInput.x < controllerThreshold && movementInput.x > -controllerThreshold)
        {
            clicked = false;
        }

        if (movementInput.x > controllerThreshold && !clicked)
        {
            //If moving left, reset impulse 
            if (direction == -1) 
            {
                resetVelocity.Invoke(() => Whoami.AmIP2());
                direction = 1;
            }

            pushNeedle.Invoke(() => Whoami.AmIP2(), 0.1f);
            clicked = true;
        }
        else if (movementInput.x < -controllerThreshold && !clicked)
        {
            //If moving right, reset impulse 
            if (direction == 1)
            {
                resetVelocity.Invoke(() => Whoami.AmIP2());
                direction = -1;
            }

            pushNeedle.Invoke(() => Whoami.AmIP2(), -0.1f);
            clicked = true;
        }

        if (stateMachine.Vitalist.IVLevel > 1 && !complete && needleRigidbody.bodyType != RigidbodyType2D.Static)
        {
            float pingPongValue = Mathf.PingPong(Time.time, 1) * 2 - 1;

            Vector3 endPos = new Vector3(targetStartingPos.x + (pingPongValue * targetRangeOfMotion), targetStartingPos.y, targetStartingPos.z);

            Vector3 newPosition = Vector3.Lerp(target.transform.localPosition, endPos, 1f);
            if (newPosition != targetNewPos.GetValue())
            {
                targetNewPos.SetValue(newPosition, () => Whoami.AmIP2());
            }

            target.transform.localPosition = targetNewPos.GetValue();
        }

        //Handle resyncronizing the needle if they get too far off.
        if (Whoami.AmIOnline())
        {
            needlePosition.SetValue(needleRigidbody.position, () => Whoami.AmIP2());

            if (Whoami.AmIP1() && Vector3.Distance(needleRigidbody.position, needlePosition.GetValue()) > .1f)
            {
                needleRigidbody.position = Vector3.Lerp(needleRigidbody.position, needlePosition.GetValue(), Time.deltaTime * 2.5f);
            }
        }

        if (complete)
        {
            needleRigidbody.bodyType = RigidbodyType2D.Static;
            stateMachine.Vitalist.IVLevel++;
            stateMachine.Vitalist.StartMinigameComplete();
            complete = false;
            StatsManager.Instance.StopAllEffectAndType(StatsManager.EffectType.OrganResil, StatsManager.Effect.Debuff);
        }
    }

    private void SetTarget(Float randPos)
    {
        target.transform.localPosition = new Vector3(targetStartingPos.x + (float)randPos, targetStartingPos.y, targetStartingPos.z);
    }

    private void ResetVelocity()
    {
        needleRigidbody.velocity = new Vector2(0, needleRigidbody.velocity.y);
    }

    private void PushNeedle(Float force)
    {
        needleRigidbody.AddForce(new Vector2((float)force, 0), ForceMode2D.Impulse);
    }

    private void Reset()
    {
        needle.transform.localPosition = needleStartingPos;
        needleRigidbody.velocity = Vector2.zero;
    }

    private void TargetHit()
    {
        complete = true;
    }
}
