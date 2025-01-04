public class MinigameStateMachine : StateMachine
{
    public Vitalist Vitalist { get; }
    
    // Menu + Selectables
    public MinigameMenuState menuState { get; }
    public MinigameAdrenalineState adrenalineState { get; }
    public MinigameElectrolytesState electrolytesState { get; }
    public MinigameAntibioticsState antibioticsState { get; }

    // Minigames
    public MinigameFractureState fractureState { get; }
    public MinigameExhaustionState exhaustionState { get; }
    public MinigameEyedropsState eyedropsState { get; }
    public MinigameIVState ivState { get; }

    // Overrideables
    public MinigameRewindState rewindState { get; }
    public MinigameDefibState defibState { get; }

    // Dummys
    public MinigameReplayDummy minigameReplayDummy { get; }

    public MinigameState CurrentState { get; set; }


    public MinigameStateMachine(Vitalist vitalist)
    {
        Vitalist = vitalist;    

        menuState = new MinigameMenuState(this);

        adrenalineState = new MinigameAdrenalineState(this);
        electrolytesState = new MinigameElectrolytesState(this);
        antibioticsState = new MinigameAntibioticsState(this);

        fractureState = new MinigameFractureState(this);
        exhaustionState = new MinigameExhaustionState(this);
        eyedropsState = new MinigameEyedropsState(this);
        ivState = new MinigameIVState(this);

        rewindState = new MinigameRewindState(this);
        defibState = new MinigameDefibState(this);

        minigameReplayDummy = new MinigameReplayDummy(this);
    }
}
