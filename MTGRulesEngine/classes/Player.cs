namespace MTGRulesEngine
{
    /// <summary>
    /// Represents a player in a Magic: The Gathering game.
    /// Rule 102.1: A player is one of the people in the game.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The unique identifier for the player.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The player's current life total. Rule 119.1: Each player begins the game with a starting life total of 20.
        /// </summary>
        public int LifeTotal { get; set; }

        /// <summary>
        /// The number of poison counters a player has. Rule 122.1f: If a player has ten or more poison counters, that player loses the game.
        /// </summary>
        public int PoisonCounters { get; set; }

        /// <summary>
        /// The player's mana pool. Rule 106.4: When an effect instructs a player to add mana, that mana goes into a player’s mana pool.
        /// This will likely be a dictionary to store different types of mana.
        /// </summary>
        public Dictionary<string, int> ManaPool { get; set; } // e.g., {"White": 0, "Blue": 0, "Colorless": 0}

        /// <summary>
        /// The player's library. Rule 401.1: When a game begins, each player’s deck becomes their library.
        /// </summary>
        public List<Card> Library { get; set; }

        /// <summary>
        /// The player's hand. Rule 402.1: The hand is where a player holds cards that have been drawn.
        /// </summary>
        public List<Card> Hand { get; set; }

        /// <summary>
        /// The player's graveyard. Rule 404.1: A player’s graveyard is their discard pile.
        /// </summary>
        public List<Card> Graveyard { get; set; }

        /// <summary>
        /// The player's maximum hand size. Rule 402.2: Each player has a maximum hand size, which is normally seven cards.
        /// </summary>
        public int MaximumHandSize { get; set; }

        /// <summary>
        /// A list of designations currently active for the player (e.g., Monarch, Initiative).
        /// </summary>
        public List<string> Designations { get; set; }

        /// <summary>
        /// Constructor for a new player.
        /// </summary>
        /// <param name="startingLife">The starting life total for the player.</param>
        /// <param name="startingHandSize">The starting hand size for the player.</param>
        public Player(int startingLife = 20, int startingHandSize = 7)
        {
            Id = Guid.NewGuid();
            LifeTotal = startingLife;
            PoisonCounters = 0;
            ManaPool = new Dictionary<string, int>
            {
                { "White", 0 }, { "Blue", 0 }, { "Black", 0 }, { "Red", 0 }, { "Green", 0 }, { "Colorless", 0 }
            };
            Library = new List<Card>();
            Hand = new List<Card>();
            Graveyard = new List<Card>();
            MaximumHandSize = startingHandSize;
            Designations = new List<string>();
        }

        /// <summary>
        /// Adds mana to the player's mana pool. Rule 106.4.
        /// </summary>
        /// <param name="manaType">The type of mana (e.g., "White", "Colorless").</param>
        /// <param name="amount">The amount of mana to add.</param>
        public void AddMana(string manaType, int amount)
        {
            if (ManaPool.ContainsKey(manaType))
            {
                ManaPool[manaType] += amount;
            }
            else
            {
                ManaPool.Add(manaType, amount);
            }
        }

        /// <summary>
        /// Empties the player's mana pool. Rule 106.4.
        /// </summary>
        public void EmptyManaPool()
        {
            foreach (var key in ManaPool.Keys.ToList())
            {
                ManaPool[key] = 0;
            }
        }

        /// <summary>
        /// Adjusts the player's life total. Rule 119.3.
        /// </summary>
        /// <param name="amount">The amount of life to gain (positive) or lose (negative).</param>
        public void AdjustLife(int amount)
        {
            LifeTotal += amount;
        }

        /// <summary>
        /// Adds poison counters to the player.
        /// </summary>
        /// <param name="amount">The number of poison counters to add.</param>
        public void AddPoisonCounters(int amount)
        {
            PoisonCounters += amount;
        }

        /// <summary>
        /// Draws a card from the library. Rule 121.1.
        /// </summary>
        /// <returns>The drawn card, or null if the library is empty.</returns>
        public Card? DrawCard()
        {
            if (Library.Any())
            {
                var card = Library[0];
                Library.RemoveAt(0);
                Hand.Add(card);
                return card;
            }
            return null; // Player attempts to draw from empty library, state-based action will handle loss
        }

        /// <summary>
        /// Discards a card from hand to graveyard. Rule 701.9a.
        /// </summary>
        /// <param name="card">The card to discard.</param>
        public void DiscardCard(Card card)
        {
            if (Hand.Remove(card))
            {
                Graveyard.Add(card);
            }
        }

        /// <summary>
        /// Shuffles the player's library. Rule 701.24a.
        /// </summary>
        public void ShuffleLibrary()
        {
            Random rng = new Random();
            int n = Library.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = Library[k];
                Library[k] = Library[n];
                Library[n] = value;
            }
        }
    }
}
