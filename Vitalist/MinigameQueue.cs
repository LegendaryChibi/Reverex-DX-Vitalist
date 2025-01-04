using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.AI;

public class MinigameQueue : IReloadable
{
    //Create a new queue
    public static List<Minigames> popups = new List<Minigames>();

    //Define types of the queue
    public enum Minigames
    {
        none,
        fracture,
        exhaustion,
        eyeDrops,
        IV,
        adrenaline,
        electrolytes,
        antibiotics,
        defib,
        rewind,
        menu
    }

    public MinigameQueue()
    {
        if (Whoami.AmIP2())
        {
            popups.Add(Minigames.fracture);
            popups.Add(Minigames.exhaustion);
            popups.Add(Minigames.eyeDrops);
            popups.Add(Minigames.IV);
        }
    }

    public void OnReload()
    {
        popups.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    public static void pushMinigameBack(Minigames input)
    {
        popups.Remove(input);
        popups.Add(input);
    }

    public static MinigameState MinigameToState(Minigames minigame)
    {
        switch (minigame)
        {
            case Minigames.fracture:
                return Vitalist.MinigameStateMachine.fractureState;
            case Minigames.exhaustion:
                return Vitalist.MinigameStateMachine.exhaustionState;
            case Minigames.eyeDrops:
                return Vitalist.MinigameStateMachine.eyedropsState;
            case Minigames.IV:
                return Vitalist.MinigameStateMachine.ivState;
            case Minigames.adrenaline:
                return Vitalist.MinigameStateMachine.adrenalineState;
            case Minigames.electrolytes:
                return Vitalist.MinigameStateMachine.electrolytesState;
            case Minigames.antibiotics:
                return Vitalist.MinigameStateMachine.antibioticsState;
            case Minigames.defib:
                return Vitalist.MinigameStateMachine.defibState;
            case Minigames.menu:
                return Vitalist.MinigameStateMachine.menuState;
        }
        return null;
    }

    public static Minigames StateToPopupMinigame(MinigameState minigame)
    {
        if (minigame == Vitalist.MinigameStateMachine.exhaustionState)
        {
            return Minigames.exhaustion;
        }
        else if (minigame == Vitalist.MinigameStateMachine.fractureState)
        {
            return Minigames.fracture;
        }
        else if (minigame == Vitalist.MinigameStateMachine.ivState)
        {
            return Minigames.IV;
        }
        else if (minigame == Vitalist.MinigameStateMachine.eyedropsState)
        {
            return Minigames.eyeDrops;
        }
        else if (minigame == Vitalist.MinigameStateMachine.menuState)
        {
            return Minigames.menu;
        }
        else if (minigame == Vitalist.MinigameStateMachine.defibState)
        {
            return Minigames.defib;
        }
        return Minigames.none;
    }

    public static Minigames StateToSelectableMinigame(MinigameState minigame)
    {
        if (minigame == Vitalist.MinigameStateMachine.adrenalineState)
        {
            return Minigames.adrenaline;
        }
        else if (minigame == Vitalist.MinigameStateMachine.electrolytesState)
        {
            return Minigames.electrolytes;
        }
        else if (minigame == Vitalist.MinigameStateMachine.antibioticsState)
        {
            return Minigames.antibiotics;
        }
        return Minigames.none;
    }
}
