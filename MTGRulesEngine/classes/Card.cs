namespace MTGRulesEngine
{
    /// <summary>
    /// Represents a Magic: The Gathering card.
    /// Rule 108: When a rule or text on a card refers to a "card," it means only a Magic card or an object represented by a Magic card.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// The name of the card. Rule 201.1: The name of a card is printed on its upper left corner.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The mana cost of the card. Rule 202.1: A card’s mana cost is indicated by mana symbols near the top of the card.
        /// This will likely be a more complex type to represent various mana symbols.
        /// </summary>
        public string ManaCost { get; set; } // Simplified for now, will need a custom type

        /// <summary>
        /// The mana value of the card. Rule 202.3: The mana value of an object is a number equal to the total amount of mana in its mana cost, regardless of color.
        /// </summary>
        public int ManaValue { get; set; }

        /// <summary>
        /// The colors of the card. Rule 105.2: An object is the color or colors of the mana symbols in its mana cost, regardless of the color of its frame.
        /// This will be a list to accommodate multicolored cards.
        /// </summary>
        public List<string> Colors { get; set; } // e.g., "White", "Blue", "Black", "Red", "Green", "Colorless"

        /// <summary>
        /// The color identity of the card. Rule 903.4: The color identity of a card is the color or colors of any mana symbols in that card’s mana cost or rules text, plus any colors defined by its characteristic-defining abilities or color indicator.
        /// Crucial for Commander.
        /// </summary>
        public List<string> ColorIdentity { get; set; }

        /// <summary>
        /// The card types of the card. Rule 205.2a: The card types are artifact, battle, conspiracy, creature, dungeon, enchantment, instant, kindred, land, phenomenon, plane, planeswalker, scheme, sorcery, and vanguard.
        /// A card can have multiple types (e.g., "Artifact Creature").
        /// </summary>
        public List<string> CardTypes { get; set; }

        /// <summary>
        /// The subtypes of the card. Rule 205.3: A card can have one or more subtypes printed on its type line.
        /// Examples: "Human", "Elf", "Mountain", "Aura".
        /// </summary>
        public List<string> Subtypes { get; set; }

        /// <summary>
        /// The supertypes of the card. Rule 205.4a: An object can have one or more supertypes. The supertypes are basic, legendary, ongoing, snow, and world.
        /// </summary>
        public List<string> Supertypes { get; set; }

        /// <summary>
        /// The rules text of the card. Rule 207.1: The text box is printed on the lower half of the card. It usually contains rules text defining the card’s abilities.
        /// This will be parsed into specific abilities.
        /// </summary>
        public string RulesText { get; set; } // Raw text for parsing

        /// <summary>
        /// The power of the card (for creatures). Rule 208.1: The first number is its power (the amount of damage it deals in combat).
        /// Nullable for non-creature cards.
        /// </summary>
        public int? Power { get; set; }

        /// <summary>
        /// The toughness of the card (for creatures). Rule 208.1: The second is its toughness (the amount of damage needed to destroy it).
        /// Nullable for non-creature cards.
        /// </summary>
        public int? Toughness { get; set; }

        /// <summary>
        /// The loyalty of the card (for planeswalkers). Rule 209.1: Each planeswalker card has a loyalty number printed in its lower right corner.
        /// Nullable for non-planeswalker cards.
        /// </summary>
        public int? Loyalty { get; set; }

        /// <summary>
        /// The defense of the card (for battles). Rule 210.1: Each battle card has a defense number printed in its lower right corner.
        /// Nullable for non-battle cards.
        /// </summary>
        public int? Defense { get; set; }

        /// <summary>
        /// The hand modifier of the card (for vanguards). Rule 211.1: Each vanguard card has a hand modifier printed in its lower left corner.
        /// Nullable for non-vanguard cards.
        /// </summary>
        public int? HandModifier { get; set; }

        /// <summary>
        /// The life modifier of the card (for vanguards). Rule 212.1: Each vanguard card has a life modifier printed in its lower right corner.
        /// Nullable for non-vanguard cards.
        /// </summary>
        public int? LifeModifier { get; set; }

        // Constructor for easy initialization
        public Card(string name, string manaCost, int manaValue, List<string> colors, List<string> colorIdentity,
                    List<string> cardTypes, List<string> subtypes, List<string> supertypes, string rulesText,
                    int? power = null, int? toughness = null, int? loyalty = null, int? defense = null,
                    int? handModifier = null, int? lifeModifier = null)
        {
            Name = name;
            ManaCost = manaCost;
            ManaValue = manaValue;
            Colors = colors ?? new List<string>();
            ColorIdentity = colorIdentity ?? new List<string>();
            CardTypes = cardTypes ?? new List<string>();
            Subtypes = subtypes ?? new List<string>();
            Supertypes = supertypes ?? new List<string>();
            RulesText = rulesText;
            Power = power;
            Toughness = toughness;
            Loyalty = loyalty;
            Defense = defense;
            HandModifier = handModifier;
            LifeModifier = lifeModifier;
        }
    }
}
