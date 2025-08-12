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
            for (int i = 0; i < 60; i++)
            {
                player1.Library.Add(new Card($"Player1_Card_{i}", "{1}", 1, new List<string> { "Colorless" }, new List<string> { "Colorless" }, new List<string> { "Artifact" }, new List<string>(), new List<string>(), ""));
                player2.Library.Add(new Card($"Player2_Card_{i}", "{1}", 1, new List<string> { "Colorless" }, new List<string> { "Colorless" }, new List<string> { "Artifact" }, new List<string>(), new List<string>(), ""));
            }

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
