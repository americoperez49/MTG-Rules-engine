namespace MTGRulesEngine
{
    /// <summary>
    /// Represents the steps within each phase of a Magic: The Gathering turn. Rule 500.1: A turn consists of five phases, in order: beginning, precombat main, combat, postcombat main, and ending.
    /// </summary>
    public enum Step
    {
        // Beginning Phase
        Untap,
        Upkeep,
        Draw,

        // Main Phase (no specific steps, but actions occur)
        // Combat Phase
        BeginningOfCombat,
        DeclareAttackers,
        DeclareBlockers,
        CombatDamage,
        EndOfCombat,

        // Ending Phase
        End,
        Cleanup
    }
}
