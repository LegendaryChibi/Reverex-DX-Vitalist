using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameReplayDummy : MinigameState
{
    private MinigameStateMachine stateMachine;
    public MinigameReplayDummy(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        stateMachine = minigameStateMachine;
    }
    public override void Enter()
    {
        Vitalist.SetText.Invoke(() => { return true; }, "Demo");
    }

    public override void Exit()
    {
        Vitalist.SetText.Invoke(() => { return true; }, "Vitals");
    }

    public override void FixedUpdate()
    {
    }

    public override void HandleInput()
    {
    }

    public override void Update()
    {
    }
}
