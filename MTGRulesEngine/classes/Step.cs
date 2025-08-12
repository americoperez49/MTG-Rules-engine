namespace MTGRulesEngine
{
    /// <summary>
    /// Represents the different steps within phases in a Magic: The Gathering turn.
    /// Rule 500.1: The beginning, combat, and ending phases are further broken down into steps, which proceed in order.
    /// </summary>
    public enum Step
    {
        // Beginning Phase Steps (Rule 501.1)
        Untap,
        Upkeep,
        Draw,

        // Main Phase has no steps (Rule 505.2) - but we might use a "Main" step for consistency in state tracking

        // Combat Phase Steps (Rule 506.1)
        BeginningOfCombat,
        DeclareAttackers,
        DeclareBlockers,
        CombatDamage,
        EndOfCombat,

        // Ending Phase Steps (Rule 512.1)
        End,
        Cleanup
    }
}
