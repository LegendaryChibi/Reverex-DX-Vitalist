using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinigameState : IState
{
    protected MinigameStateMachine stateMachine;

    protected Vector2 movementInput;

    protected TMP_Text[] selectButtonsText;

    public MinigameState(MinigameStateMachine minigameStateMachine)
    {
        stateMachine = minigameStateMachine;

        selectButtonsText = stateMachine.Vitalist.selectButtonsText;
    }
    public virtual void Enter()
    {
        Vitalist.MinigameStateMachine.CurrentState = this;
        stateMachine.Vitalist.currentState = Vitalist.MinigameStateMachine.CurrentState.ToString();
    }

    public virtual void Exit()
    {
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void HandleInput()
    {
        ReadMovementInput();
    }

    public virtual void Update()
    {
    }

    //Main Methods
    private void ReadMovementInput()
    {
        if (stateMachine.Vitalist.Input == null)
        {
            return;
        }
        if (stateMachine.Vitalist.Input.isController)
        {

            movementInput = stateMachine.Vitalist.Input.AnalogueAxis;
        }
        else
        {
            movementInput = stateMachine.Vitalist.Input.DirectionalAxis;
        }
    }

    protected virtual bool ReadAButtonInput()
    {
        if (stateMachine.Vitalist.Input == null)
        {
            return false;
        }
        return stateMachine.Vitalist.Input.A;
    }

    protected virtual bool ReadBButtonInput()
    {
        if (stateMachine.Vitalist.Input == null)
        {
            return false;
        }
        return stateMachine.Vitalist.Input.B;
    }

    protected virtual bool ReadXButtonInput()
    {
        if (stateMachine.Vitalist.Input == null)
        {
            return false;
        }
        return stateMachine.Vitalist.Input.X;
    }

    protected virtual bool ReadYButtonInput()
    {
        if (stateMachine.Vitalist.Input == null)
        {
            return false;
        }
        return stateMachine.Vitalist.Input.Y;
    }
}
