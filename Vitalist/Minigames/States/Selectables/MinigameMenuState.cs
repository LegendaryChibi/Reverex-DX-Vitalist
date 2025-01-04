using UnityEngine;
using System;
using Unity.VisualScripting;

public class MinigameMenuState : MinigameState
{
    private int selectedGame = 0;
    private float delayMenu = 0.2f;
    private float currentDelay = 0.0f;

    private static NetAction<selectable> selectGame = new NetAction<selectable>("net_select_minigame");
    private static NetAction<Interger> updateMenu = new NetAction<Interger>("net_update_minigame_menu");

    [Serializable]
    private class IsEnabled
    {
        public bool[] booleans = { true, false, false };
    }

    private Synchronized<IsEnabled> isEnabled = new Synchronized<IsEnabled>("net_vitalist_menu_buttons_enabled");

    private string[] originalButtonTexts;


    private enum selectable
    {
        adrenaline,
        electrolytes,
        antibiotics
    }

    public MinigameMenuState(MinigameStateMachine minigameStateMachine) : base(minigameStateMachine)
    {
        originalButtonTexts = new string[selectButtonsText.Length];
        for (int i = 0; i < selectButtonsText.Length; i++)
        {
            originalButtonTexts[i] = selectButtonsText[i].text;
        }

        IsEnabled initial = new IsEnabled();

        isEnabled.SetValue(initial, () => { return true; });

        stateMachine.Vitalist.unlockButton = new NetRoutine<Interger>(UnlockButton, Ownership.Navigator);
    }

    public override void Enter()
    {
        base.Enter();
        selectGame += PlayMinigame; 
        Vitalist.popupTriggered += stateMachine.Vitalist.PlayNextGame;
        updateMenu += UpdateButtonHighlight;
        Vitalist.SetTime.Invoke(() => Whoami.AmIP2(), 0);
        Vitalist.SetText.Invoke(() => Whoami.AmIP2(), "Vitals:");
        stateMachine.Vitalist.vitalistButtonParent.SetActive(true);

        UpdateButtonHighlight(selectedGame);
    }

    public override void Exit() 
    { 
        selectGame -= PlayMinigame;
        Vitalist.popupTriggered -= stateMachine.Vitalist.PlayNextGame;
        updateMenu -= UpdateButtonHighlight;
        stateMachine.Vitalist.vitalistButtonParent.SetActive(false);
    }

    public override void Update()
    {
        base.Update();

        currentDelay += Time.deltaTime;

        if (currentDelay > delayMenu)
        {
            UpdateMenu();
        }
    }

    private void UpdateMenu()
    {
        bool changedGame = false;
        Vector2 Dir = movementInput;

        if (ReadAButtonInput() && isEnabled.GetValue().booleans[selectedGame])
        {
            // Trigger the selected game if enabled
            switch (selectedGame)
            {
                case 0:
                    selectGame.Invoke(() => Whoami.AmIP2(), selectable.adrenaline);
                    break;
                case 1:
                    selectGame.Invoke(() => Whoami.AmIP2(), selectable.electrolytes);
                    break;
                case 2:
                    selectGame.Invoke(() => Whoami.AmIP2(), selectable.antibiotics);
                    break;
            }
            SoundManager.Instance.PlaySFXOnline("Vitalist_Cursor_Confirm");
        }

        if (Dir.y > 0.5f) // Stick up
        {
            for (int i = 1; i <= selectButtonsText.Length; i++)
            {
                int nextIndex = (selectedGame - i + selectButtonsText.Length) % selectButtonsText.Length; // Wrapping logic
                if (isEnabled.GetValue().booleans[nextIndex])
                {
                    selectedGame = nextIndex;
                    changedGame = true;
                    break;
                }
            }
            SoundManager.Instance.PlaySFXOnline("Vitalist_Cursor_Move");
        }
        else if (Dir.y < -0.5f) // Stick down
        {
            for (int i = 1; i <= selectButtonsText.Length; i++)
            {
                int nextIndex = (selectedGame + i) % selectButtonsText.Length; // Wrapping logic
                if (isEnabled.GetValue().booleans[nextIndex])
                {
                    selectedGame = nextIndex;
                    changedGame = true;
                    break;
                }
            }
            SoundManager.Instance.PlaySFXOnline("Vitalist_Cursor_Move");
        }

        if (changedGame)
        {
            updateMenu.Invoke(() => Whoami.AmIP2(), selectedGame);
        }
    }


    private void UpdateButtonHighlight(Interger button)
    {
        currentDelay = 0f; // Reset the delay

        for (int t = 0; t < selectButtonsText.Length; t++)
        {
            selectButtonsText[t].text = selectButtonsText[t].text.Replace(">", "").Trim();

            if (isEnabled.GetValue().booleans[t])
            {
                selectButtonsText[t].text = originalButtonTexts[t];
                stateMachine.Vitalist.icons[t].SetActive(true);
            }
            else
            {
                selectButtonsText[t].text = "Locked";
                stateMachine.Vitalist.icons[t].SetActive(false);
            }
        }

        if ((int)button >= 0 && (int)button < selectButtonsText.Length && isEnabled.GetValue().booleans[selectedGame])
        {
            selectButtonsText[(int)button].text = "> " + selectButtonsText[(int)button].text.Trim();
        }
    }

    public void UnlockButton(Interger index)
    {
        if ((int)index >= 0 && (int)index < isEnabled.GetValue().booleans.Length)
        {
            IsEnabled currentArray = isEnabled.GetValue();
            currentArray.booleans[(int)index] = true;

            isEnabled.SetValue(currentArray, () => { return true; });
            updateMenu.Invoke(() => Whoami.AmIP1(), (int)index);
        }
    }

    private void PlayMinigame(selectable minigame)
    {
        if (stateMachine.Vitalist.WarningActive)
        {
            //Play Error Sound Here
            return;
        }


        switch (minigame) 
        {
            case selectable.adrenaline:
                if (!StatsManager.Instance.CheckForBuffEffect(StatsManager.EffectType.Speed))
                {
                    stateMachine.ChangeState(stateMachine.adrenalineState);
                }
                break;
            case selectable.electrolytes:
                if (!StatsManager.Instance.CheckForBuffEffect(StatsManager.EffectType.Jump))
                {
                    stateMachine.ChangeState(stateMachine.electrolytesState);
                }
                break;
            case selectable.antibiotics:
                if (!StatsManager.Instance.CheckForBuffEffect(StatsManager.EffectType.OrganResil))
                {
                    stateMachine.ChangeState(stateMachine.antibioticsState);
                }
                break;
        }
    }
}
