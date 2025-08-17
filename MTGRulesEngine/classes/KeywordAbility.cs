namespace MTGRulesEngine
{
    /// <summary>
    /// Represents common keyword abilities as defined in Rule 702.
    /// </summary>
    public enum KeywordAbility
    {
        None,
        Haste,
        Flying,
        Trample,
        Lifelink,
        Deathtouch,
        Vigilance,
        Defender,
        Reach, // Added Reach
        // Add other keywords as they are implemented
    }

    /// <summary>
    /// Represents an instance of a keyword ability on a card or permanent.
    /// This can be extended to include values for keywords like "Toxic N" or "Ward [cost]".
    /// </summary>
    public class CardKeyword
    {
        public KeywordAbility Type { get; private set; }
        public string? Value { get; private set; } // For keywords with a value, e.g., "Toxic 1", "Ward {2}"

        public CardKeyword(KeywordAbility type, string? value = null)
        {
            Type = type;
            Value = value;
        }
    }
}
