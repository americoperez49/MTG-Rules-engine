using System.Collections.Generic;
using System.Linq;
using MTGRulesEngine;

namespace MTGRulesEngine
{
    /// <summary>
    /// Represents the overall state of a Magic: The Gathering game.
    /// This central object encapsulates all game elements and properties.
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// A list of all players participating in the game.
        /// </summary>
        public List<Player> Players { get; set; }

        /// <summary>
        /// The current active player whose turn it is. Rule 102.1.
        /// </summary>
        public Player? ActivePlayer { get; set; }

        /// <summary>
        /// The current defending player in a multiplayer game. Rule 506.2a.
        /// </summary>
        public Player? DefendingPlayer { get; set; }

        /// <summary>
        /// The battlefield zone, containing all permanents. Rule 403.1.
        /// </summary>
        public List<Permanent> Battlefield { get; set; }

        /// <summary>
        /// The stack zone, containing spells and abilities awaiting resolution. Rule 405.1.
        /// </summary>
        public Stack<ObjectOnStack> Stack { get; set; }

        /// <summary>
        /// The exile zone, a holding area for objects removed from the game. Rule 406.1.
        /// </summary>
        public List<Card> Exile { get; set; }

        /// <summary>
        /// The command zone, for special objects like commanders and emblems. Rule 408.1.
        /// </summary>
        public List<Card> CommandZone { get; set; } // Can contain Commanders, Emblems, Plane cards, etc.

        /// <summary>
        /// The current phase of the turn. Rule 500.1.
        /// </summary>
        public Phase CurrentPhase { get; set; }

        /// <summary>
        /// The current step within the current phase. Rule 500.1.
        /// </summary>
        public Step CurrentStep { get; set; }

        /// <summary>
        /// Indicates whether it is currently Day or Night in the game. Rule 730.1.
        /// Can be "Day", "Night", or null/empty if neither.
        /// </summary>
        public DayNightState DayNightDesignation { get; set; }

        /// <summary>
        /// Constructor for initializing a new game state.
        /// </summary>
        /// <param name="players">A list of players participating in the game.</param>
        public GameState(List<Player> players)
        {
            Players = players ?? new List<Player>();
            Battlefield = new List<Permanent>();
            Stack = new Stack<ObjectOnStack>();
            Exile = new List<Card>();
            CommandZone = new List<Card>();

            // Initialize turn structure
            CurrentPhase = Phase.Beginning;
            CurrentStep = Step.Untap;

            DayNightDesignation = DayNightState.None;

            // Set initial active player (will be determined by game start rules, e.g., Rule 103.1)
            ActivePlayer = Players.FirstOrDefault();
            DefendingPlayer = null; // Set during combat phase
        }

        /// <summary>
        /// Represents an object on the stack (a spell or an ability).
        /// </summary>
        public class ObjectOnStack
        {
            public Card? Card { get; set; } // Null for abilities
            public string? AbilityText { get; set; } // For abilities
            public Player Controller { get; set; }
            public List<object> Targets { get; set; } // Can be Player, Permanent, or other objects
            public string Type { get; set; } // "Spell" or "Ability"

            public ObjectOnStack(Card card, Player controller, List<object>? targets = null)
            {
                Card = card;
                Controller = controller;
                Targets = targets ?? new List<object>();
                Type = "Spell";
            }

            public ObjectOnStack(string abilityText, Player controller, List<object>? targets = null)
            {
                AbilityText = abilityText;
                Controller = controller;
                Targets = targets ?? new List<object>();
                Type = "Ability";
            }
        }

        /// <summary>
        /// Represents a permanent on the battlefield.
        /// Rule 110.1: A permanent is a card or token on the battlefield.
        /// </summary>
        public class Permanent
        {
            public Card? Card { get; set; } // The card this permanent represents
            public Guid Id { get; } // Unique ID for this specific permanent instance
            public Player Controller { get; set; }
            public Player Owner { get; set; }
            public bool IsTapped { get; set; }
            public bool IsFlipped { get; set; }
            public bool IsFaceDown { get; set; }
            public bool IsPhasedOut { get; set; }
            public int CurrentPower { get; set; }
            public int CurrentToughness { get; set; }
            public int CurrentLoyalty { get; set; } // For planeswalkers
            public int CurrentDefense { get; set; } // For battles
            public Dictionary<string, int> Counters { get; set; } // e.g., {"+1/+1": 2, "Loyalty": 3}
            public List<CardKeyword> CurrentKeywords { get; set; } // Keywords gained/lost due to continuous effects

            public Permanent(Card card, Player controller, Player owner)
            {
                Id = Guid.NewGuid();
                Card = card;
                Controller = controller;
                Owner = owner;
                IsTapped = false;
                IsFlipped = false;
                IsFaceDown = false;
                IsPhasedOut = false;
                CurrentPower = card.Power ?? 0;
                CurrentToughness = card.Toughness ?? 0;
                CurrentLoyalty = card.Loyalty ?? 0;
                CurrentDefense = card.Defense ?? 0;
                Counters = new Dictionary<string, int>();
                CurrentKeywords = new List<CardKeyword>(card.Keywords); // Initialize with card's keywords
            }

            /// <summary>
            /// Adds a counter of a specific type to the permanent.
            /// </summary>
            /// <param name="counterType">The type of counter (e.g., "+1/+1", "Loyalty").</param>
            /// <param name="amount">The number of counters to add.</param>
            public void AddCounter(string counterType, int amount)
            {
                if (Counters.ContainsKey(counterType))
                {
                    Counters[counterType] += amount;
                }
                else
                {
                    Counters.Add(counterType, amount);
                }
            }

            /// <summary>
            /// Removes a counter of a specific type from the permanent.
            /// </summary>
            /// <param name="counterType">The type of counter.</param>
            /// <param name="amount">The number of counters to remove.</param>
            public void RemoveCounter(string counterType, int amount)
            {
                if (Counters.ContainsKey(counterType))
                {
                    Counters[counterType] = Math.Max(0, Counters[counterType] - amount);
                    if (Counters[counterType] == 0)
                    {
                        Counters.Remove(counterType);
                    }
                }
            }
        }
    }
}
