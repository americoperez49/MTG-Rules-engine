namespace MTGRulesEngine
{
    /// <summary>
    /// Represents the different phases in a Magic: The Gathering turn.
    /// Rule 500.1: A turn consists of five phases, in this order: beginning, precombat main, combat, postcombat main, and ending.
    /// </summary>
    public enum Phase
    {
        Beginning,
        PrecombatMain,
        Combat,
        PostcombatMain,
        Ending
    }
}
