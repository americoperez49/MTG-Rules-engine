namespace MTGRulesEngine
{
    /// <summary>
    /// Represents the Day/Night designation of the game.
    /// Rule 730.1: Day and night are designations that the game itself can have. The game starts with neither designation.
    /// </summary>
    public enum DayNightState
    {
        None, // Game starts with neither designation
        Day,
        Night
    }
}
