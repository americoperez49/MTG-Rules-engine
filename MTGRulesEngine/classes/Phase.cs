namespace MTGRulesEngine
{
    /// <summary>
    /// Represents the five phases of a Magic: The Gathering turn. Rule 500.1: A turn consists of five phases, in order: beginning, precombat main, combat, postcombat main, and ending.
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
