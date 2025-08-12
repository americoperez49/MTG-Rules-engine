using System;
using System.Collections.Generic;
using System.Linq;
using MTGRulesEngine;

namespace MTGRulesEngine
{
    /// <summary>
    /// Manages the overall flow of a Magic: The Gathering game, including turn progression,
    /// phase and step management, and interaction with the game state.
    /// </summary>
    public class GameManager
    {
        private GameState _gameState;

        /// <summary>
        /// Initializes a new instance of the GameManager with the given players.
        /// </summary>
        /// <param name="players">The list of players participating in the game.</param>
        public GameManager(List<Player> players)
        {
            _gameState = new GameState(players);
        }

        /// <summary>
        /// Starts the Magic: The Gathering game.
        /// Rule 103: Starting the Game.
        /// </summary>
        public void StartGame()
        {
            Console.WriteLine("--- Game Start ---");

            // 103.1. Determine starting player
            // For simplicity, let's assume the first player in the list is the starting player.
            // In a real implementation, this would involve coin flips, dice rolls, etc.
            _gameState.ActivePlayer = _gameState.Players.First();
            Console.WriteLine($"{_gameState.ActivePlayer!.Id} is the starting player.");

            // 103.3. Each player shuffles their deck and they become libraries.
            foreach (var player in _gameState.Players)
            {
                player.ShuffleLibrary();
                Console.WriteLine($"{player.Id}'s library shuffled.");
            }

            // 103.4. Each player begins the game with a starting life total of 20.
            // Handled in Player constructor, but can be overridden by variants.
            foreach (var player in _gameState.Players)
            {
                Console.WriteLine($"{player.Id} starts with {player.LifeTotal} life.");
            }

            // 103.5. Each player draws a number of cards equal to their starting hand size (normally seven).
            // Handle mulligans (simplified for now)
            foreach (var player in _gameState.Players)
            {
                for (int i = 0; i < player.MaximumHandSize; i++)
                {
                    player.DrawCard();
                }
                Console.WriteLine($"{player.Id} drew {player.Hand.Count} cards for opening hand.");
            }

            // 103.8. The starting player takes their first turn.
            // 103.8a. In a two-player game, the player who plays first skips the draw step of their first turn.
            // This will be handled in the turn progression.

            Console.WriteLine("--- Game Setup Complete ---");
            ExecuteTurn(_gameState.ActivePlayer!); // Execute the first turn for the starting player
        }

        /// <summary>
        /// Executes a full turn for the given player.
        /// </summary>
        /// <param name="player">The player whose turn it is.</param>
        private void ExecuteTurn(Player player)
        {
            _gameState.ActivePlayer = player;
            Console.WriteLine($"\n--- {_gameState.ActivePlayer!.Id}'s Turn Begins ---");

            // Reset mana pool at the start of each turn (Rule 500.4 - end of step/phase)
            foreach (var p in _gameState.Players) // Use 'p' to avoid conflict with 'player' parameter
            {
                p.EmptyManaPool();
            }

            // Beginning Phase
            _gameState.CurrentPhase = Phase.Beginning;
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            UntapStep();
            UpkeepStep();
            DrawStep();

            // Main Phase (Precombat)
            _gameState.CurrentPhase = Phase.PrecombatMain;
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            // Player actions (casting spells, playing lands) would happen here
            // For now, just advance
            // This is where the external application would interact to get player actions
            Console.WriteLine("  (Player actions in Main Phase)");

            // Combat Phase
            _gameState.CurrentPhase = Phase.Combat;
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            BeginningOfCombatStep();
            DeclareAttackersStep();
            DeclareBlockersStep();
            CombatDamageStep();
            EndOfCombatStep();

            // Main Phase (Postcombat)
            _gameState.CurrentPhase = Phase.PostcombatMain;
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            // Player actions (casting spells, playing lands) would happen here
            Console.WriteLine("  (Player actions in Main Phase)");

            // Ending Phase
            _gameState.CurrentPhase = Phase.Ending;
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            EndStep();
            CleanupStep();

            Console.WriteLine($"--- {_gameState.ActivePlayer.Id}'s Turn Ends ---");

            // Check for game end conditions after each turn (Rule 104)
            CheckGameEndConditions();
        }

        /// <summary>
        /// Advances the game to the next player's turn.
        /// </summary>
        public void AdvanceTurn()
        {
            int currentIndex = _gameState.Players.IndexOf(_gameState.ActivePlayer!);
            Player nextPlayer = _gameState.Players[(currentIndex + 1) % _gameState.Players.Count];
            ExecuteTurn(nextPlayer);
        }

        /// <summary>
        /// Untap Step (Rule 502).
        /// </summary>
        private void UntapStep()
        {
            _gameState.CurrentStep = Step.Untap;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 502.1. Phasing (simplified)
            // 502.2. Day/Night check (simplified)

            // 502.3. Untap permanents
            foreach (var permanent in _gameState.Battlefield.Where(p => p.Controller == _gameState.ActivePlayer))
            {
                permanent.IsTapped = false;
            }
            Console.WriteLine($"    {_gameState.ActivePlayer!.Id}'s permanents untap.");

            // 502.4. No player receives priority during the untap step.
        }

        /// <summary>
        /// Upkeep Step (Rule 503).
        /// </summary>
        private void UpkeepStep()
        {
            _gameState.CurrentStep = Step.Upkeep;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 503.1. Active player gets priority.
            // Triggered abilities from untap step and beginning of upkeep go on stack.
            // (Implementation of stack and priority will handle this)
            Console.WriteLine("    (Priority given, triggered abilities resolve)");
        }

        /// <summary>
        /// Draw Step (Rule 504).
        /// </summary>
        private void DrawStep()
        {
            _gameState.CurrentStep = Step.Draw;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 504.1. Active player draws a card.
            // 103.8a. In a two-player game, the player who plays first skips the draw step of their first turn.
            // For simplicity, assuming not first turn or not two-player game for now.
            // A more robust implementation would track turn number and player count.
            _gameState.ActivePlayer!.DrawCard();
            Console.WriteLine($"    {_gameState.ActivePlayer!.Id} draws a card. Hand size: {_gameState.ActivePlayer!.Hand.Count}");

            // 504.2. Active player gets priority.
            Console.WriteLine("    (Priority given)");
        }

        /// <summary>
        /// Beginning of Combat Step (Rule 507).
        /// </summary>
        private void BeginningOfCombatStep()
        {
            _gameState.CurrentStep = Step.BeginningOfCombat;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 507.1. Choose defending player (multiplayer).
            // For simplicity, assume the next player in turn order is the defending player.
            int activePlayerIndex = _gameState.Players.IndexOf(_gameState.ActivePlayer!);
            _gameState.DefendingPlayer = _gameState.Players[(activePlayerIndex + 1) % _gameState.Players.Count];
            Console.WriteLine($"    {_gameState.ActivePlayer!.Id} chooses {_gameState.DefendingPlayer!.Id} as defending player.");

            // 507.2. Active player gets priority.
            Console.WriteLine("    (Priority given)");
        }

        /// <summary>
        /// Declare Attackers Step (Rule 508).
        /// </summary>
        private void DeclareAttackersStep()
        {
            _gameState.CurrentStep = Step.DeclareAttackers;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 508.1. Active player declares attackers. (Simplified: no attacks for now)
            Console.WriteLine($"    {_gameState.ActivePlayer!.Id} declares no attackers for simplicity.");

            // 508.2. Active player gets priority.
            Console.WriteLine("    (Priority given)");
        }

        /// <summary>
        /// Declare Blockers Step (Rule 509).
        /// </summary>
        private void DeclareBlockersStep()
        {
            _gameState.CurrentStep = Step.DeclareBlockers;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 509.1. Defending player declares blockers. (Simplified: no blockers for now)
            Console.WriteLine($"    {_gameState.DefendingPlayer!.Id} declares no blockers for simplicity.");

            // 509.2. Active player gets priority.
            Console.WriteLine("    (Priority given)");
        }

        /// <summary>
        /// Combat Damage Step (Rule 510).
        /// </summary>
        private void CombatDamageStep()
        {
            _gameState.CurrentStep = Step.CombatDamage;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 510.1. Assign combat damage. (Simplified: no damage dealt for now)
            // 510.2. Deal combat damage.
            Console.WriteLine("    (No combat damage dealt)");

            // 510.3. Active player gets priority.
            Console.WriteLine("    (Priority given)");
        }

        /// <summary>
        /// End of Combat Step (Rule 511).
        /// </summary>
        private void EndOfCombatStep()
        {
            _gameState.CurrentStep = Step.EndOfCombat;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 511.1. Active player gets priority.
            Console.WriteLine("    (Priority given)");

            // 511.3. As soon as the end of combat step ends, all creatures, battles, and planeswalkers are removed from combat.
            // (No combatants to remove in this simplified version)
        }

        /// <summary>
        /// End Step (Rule 513).
        /// </summary>
        private void EndStep()
        {
            _gameState.CurrentStep = Step.End;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 513.1. Active player gets priority.
            Console.WriteLine("    (Priority given)");
        }

        /// <summary>
        /// Cleanup Step (Rule 514).
        /// </summary>
        private void CleanupStep()
        {
            _gameState.CurrentStep = Step.Cleanup;
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 514.1. Discard to hand size.
            while (_gameState.ActivePlayer!.Hand.Count > _gameState.ActivePlayer.MaximumHandSize)
            {
                // For simplicity, discard the first card. In a real game, player chooses.
                var cardToDiscard = _gameState.ActivePlayer.Hand.First();
                _gameState.ActivePlayer.DiscardCard(cardToDiscard);
                Console.WriteLine($"    {_gameState.ActivePlayer!.Id} discards {cardToDiscard.Name}. Hand size: {_gameState.ActivePlayer.Hand.Count}");
            }

            // 514.2. Remove all damage from permanents and end "until end of turn" effects.
            foreach (var permanent in _gameState.Battlefield)
            {
                // Reset damage (if damage tracking was implemented)
            }
            Console.WriteLine("    Damage removed from permanents. 'Until end of turn' effects end.");

            // 514.3. Normally no priority, but check for state-based actions/triggers.
            // (State-based actions and triggers will be handled by a separate system)
            Console.WriteLine("    (State-based actions checked, no priority unless triggers)");
        }

        /// <summary>
        /// Checks for game-ending conditions. Rule 104.
        /// </summary>
        private void CheckGameEndConditions()
        {
            // 104.2a. A player still in the game wins the game if that player’s opponents have all left the game.
            // 104.3a. A player can concede the game at any time.
            // 104.3b. If a player’s life total is 0 or less, that player loses the game.
            // 104.3c. If a player is required to draw more cards than are left in their library, they draw the remaining cards and then lose the game.
            // 104.3d. If a player has ten or more poison counters, that player loses the game.

            List<Player> playersStillInGame = _gameState.Players.Where(p => p.LifeTotal > 0 && p.PoisonCounters < 10 && p.Library.Count > 0).ToList();

            if (playersStillInGame.Count <= 1)
            {
                if (playersStillInGame.Count == 1)
                {
                    Console.WriteLine($"\n--- Game Over! {playersStillInGame.First().Id} wins! ---");
                }
                else // 0 players left
                {
                    Console.WriteLine("\n--- Game Over! It's a draw! ---");
                }
                // In a real engine, this would trigger a game end event and stop execution.
                Environment.Exit(0); // For demonstration, exit the application
            }
        }
    }
}
