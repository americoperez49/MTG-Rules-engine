using System;
using System.Collections.Generic;
using MTGRulesEngine;

namespace MTGRulesEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Magic: The Gathering Rules Engine Simulation...");

            // Create two players
            Player player1 = new Player(startingLife: 20, startingHandSize: 7);
            Player player2 = new Player(startingLife: 20, startingHandSize: 7);

            // For demonstration, let's give them some dummy cards in their libraries
            // In a real scenario, decks would be loaded from external data
            for (int i = 0; i < 55; i++)
            {
                player1.Library.Add(new Card($"Player1_Card_{i}", "{1}", 1, new List<string> { "Colorless" }, new List<string> { "Colorless" }, new List<string> { "Artifact" }, new List<string>(), new List<string>(), new List<Ability>(), new List<CardKeyword>(), player1));
                player2.Library.Add(new Card($"Player2_Card_{i}", "{1}", 1, new List<string> { "Colorless" }, new List<string> { "Colorless" }, new List<string> { "Artifact" }, new List<string>(), new List<string>(), new List<Ability>(), new List<CardKeyword>(), player2));
            }

            // Add some creatures with keywords to test the new logic
            player1.Library.Add(new Card("Grizzly Bears", "{1}{G}", 2, new List<string> { "Green" }, new List<string> { "Green" }, new List<string> { "Creature" }, new List<string> { "Bear" }, new List<string>(), new List<Ability>(), new List<CardKeyword>(), player1, 2, 2));
            player1.Library.Add(new Card("Wind Drake", "{2}{U}", 3, new List<string> { "Blue" }, new List<string> { "Blue" }, new List<string> { "Creature" }, new List<string> { "Drake" }, new List<string>(), new List<Ability>(), new List<CardKeyword> { new CardKeyword(KeywordAbility.Flying) }, player1, 2, 2));
            player1.Library.Add(new Card("Craw Wurm", "{4}{G}{G}", 6, new List<string> { "Green" }, new List<string> { "Green" }, new List<string> { "Creature" }, new List<string> { "Wurm" }, new List<string>(), new List<Ability>(), new List<CardKeyword> { new CardKeyword(KeywordAbility.Trample) }, player1, 6, 4));
            player1.Library.Add(new Card("Vampire Nighthawk", "{1}{B}{B}", 3, new List<string> { "Black" }, new List<string> { "Black" }, new List<string> { "Creature" }, new List<string> { "Vampire", "Shaman" }, new List<string>(), new List<Ability>(), new List<CardKeyword> { new CardKeyword(KeywordAbility.Flying), new CardKeyword(KeywordAbility.Lifelink), new CardKeyword(KeywordAbility.Deathtouch) }, player1, 2, 3));
            player1.Library.Add(new Card("Wall of Stone", "{1}{R}{R}", 3, new List<string> { "Red" }, new List<string> { "Red" }, new List<string> { "Creature" }, new List<string> { "Wall" }, new List<string>(), new List<Ability>(), new List<CardKeyword> { new CardKeyword(KeywordAbility.Defender) }, player1, 0, 8));

            player2.Library.Add(new Card("Giant Spider", "{3}{G}", 4, new List<string> { "Green" }, new List<string> { "Green" }, new List<string> { "Creature" }, new List<string> { "Spider" }, new List<string>(), new List<Ability>(), new List<CardKeyword> { new CardKeyword(KeywordAbility.Reach) }, player2, 2, 4));
            player2.Library.Add(new Card("Goblin Piker", "{1}{R}", 2, new List<string> { "Red" }, new List<string> { "Red" }, new List<string> { "Creature" }, new List<string> { "Goblin", "Warrior" }, new List<string>(), new List<Ability>(), new List<CardKeyword>(), player2, 2, 1));
            player2.Library.Add(new Card("Fugitive Wizard", "{U}", 1, new List<string> { "Blue" }, new List<string> { "Blue" }, new List<string> { "Creature" }, new List<string> { "Human", "Wizard" }, new List<string>(), new List<Ability>(), new List<CardKeyword>(), player2, 1, 1));
            player2.Library.Add(new Card("Savannah Lions", "{W}", 1, new List<string> { "White" }, new List<string> { "White" }, new List<string> { "Creature" }, new List<string> { "Cat" }, new List<string>(), new List<Ability>(), new List<CardKeyword>(), player2, 2, 1));
            player2.Library.Add(new Card("Scathe Zombies", "{2}{B}", 3, new List<string> { "Black" }, new List<string> { "Black" }, new List<string> { "Creature" }, new List<string> { "Zombie" }, new List<string>(), new List<Ability>(), new List<CardKeyword>(), player2, 2, 2));

            List<Player> players = new List<Player> { player1, player2 };

            // Initialize the game manager
            GameManager gameManager = new GameManager(players);

            // Start the game
            gameManager.StartGame();

            // For demonstration, let's advance a few turns
            for (int i = 0; i < 3; i++)
            {
                gameManager.AdvanceTurn();
            }

            Console.WriteLine("\nSimulation complete.");
        }
    }
}
