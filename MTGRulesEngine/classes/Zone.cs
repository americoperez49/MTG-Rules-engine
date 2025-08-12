namespace MTGRulesEngine
{
    /// <summary>
    /// Represents the different zones in a Magic: The Gathering game.
    /// Rule 400.1: There are normally seven zones: library, hand, battlefield, graveyard, stack, exile, and command.
    /// Some older cards also use the ante zone.
    /// </summary>
    public enum Zone
    {
        Library,
        Hand,
        Battlefield,
        Graveyard,
        Stack,
        Exile,
        Command,
        Ante // For older cards/variants
    }
}
