using System;
using System.Collections.Generic;
using System.Linq;
using MTGRulesEngine;
using MTGRulesEngine.Events; // Added for EventBus and GameEvents

namespace MTGRulesEngine
{
    /// <summary>
    /// Manages the overall flow of a Magic: The Gathering game, including turn progression,
    /// phase and step management, and interaction with the game state.
    /// </summary>
    public class GameManager
    {
        private GameState _gameState;
        private Player? _playerWithPriority;
        private EventBus _eventBus; // Declare EventBus

        /// <summary>
        /// Initializes a new instance of the GameManager with the given players.
        /// </summary>
        /// <param name="players">The list of players participating in the game.</param>
        public GameManager(List<Player> players)
        {
            _gameState = new GameState(players);
            _playerWithPriority = null; // No one has priority initially
            _eventBus = new EventBus(); // Initialize EventBus
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
            _eventBus.Publish(new Events.PhaseBeginsEvent(_gameState.CurrentPhase)); // Publish PhaseBeginsEvent
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            UntapStep();
            UpkeepStep();
            DrawStep();

            // Main Phase (Precombat)
            _gameState.CurrentPhase = Phase.PrecombatMain;
            _eventBus.Publish(new Events.PhaseBeginsEvent(_gameState.CurrentPhase)); // Publish PhaseBeginsEvent
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            // Player actions (casting spells, playing lands) would happen here
            // For now, just advance
            // This is where the external application would interact to get player actions
            GivePriority(_gameState.ActivePlayer!); // Give priority in main phase

            // Combat Phase
            _gameState.CurrentPhase = Phase.Combat;
            _eventBus.Publish(new Events.PhaseBeginsEvent(_gameState.CurrentPhase)); // Publish PhaseBeginsEvent
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            BeginningOfCombatStep();
            DeclareAttackersStep();
            DeclareBlockersStep();
            CombatDamageStep();
            EndOfCombatStep();

            // Main Phase (Postcombat)
            _gameState.CurrentPhase = Phase.PostcombatMain;
            _eventBus.Publish(new Events.PhaseBeginsEvent(_gameState.CurrentPhase)); // Publish PhaseBeginsEvent
            Console.WriteLine($"{_gameState.CurrentPhase} Phase:");
            // Player actions (casting spells, playing lands) would happen here
            GivePriority(_gameState.ActivePlayer!); // Give priority in main phase

            // Ending Phase
            _gameState.CurrentPhase = Phase.Ending;
            _eventBus.Publish(new Events.PhaseBeginsEvent(_gameState.CurrentPhase)); // Publish PhaseBeginsEvent
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
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 502.1. Phasing (simplified)
            // 502.2. Day/Night check (simplified)

            // 502.3. Untap permanents
            foreach (var permanent in _gameState.Battlefield.Where(p => p.Controller == _gameState.ActivePlayer))
            {
                // Rule 702.20b: Attacking doesn’t cause creatures with vigilance to tap.
                // This means creatures with vigilance untap normally.
                // For now, all permanents untap unless an effect prevents it.
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
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 503.1a Any abilities that triggered during the untap step and any abilities that triggered at the beginning of the upkeep are put onto the stack before the active player gets priority; the order in which they triggered doesn’t matter. (See rule 603, “Handling Triggered Abilities.”)
            CheckStateBasedActionsAndTriggers(); // Rule 117.5

            // 503.1. Active player gets priority.
            GivePriority(_gameState.ActivePlayer!);
        }

        /// <summary>
        /// Draw Step (Rule 504).
        /// </summary>
        private void DrawStep()
        {
            _gameState.CurrentStep = Step.Draw;
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 504.1. Active player draws a card. This turn-based action doesn’t use the stack.
            // 103.8a. In a two-player game, the player who plays first skips the draw step of their first turn.
            // For simplicity, assuming not first turn or not two-player game for now.
            // A more robust implementation would track turn number and player count.
            _gameState.ActivePlayer!.DrawCard();
            Console.WriteLine($"    {_gameState.ActivePlayer!.Id} draws a card. Hand size: {_gameState.ActivePlayer!.Hand.Count}");

            CheckStateBasedActionsAndTriggers(); // Rule 117.5

            // 504.2. Active player gets priority.
            GivePriority(_gameState.ActivePlayer!);
        }

        /// <summary>
        /// Beginning of Combat Step (Rule 507).
        /// </summary>
        private void BeginningOfCombatStep()
        {
            _gameState.CurrentStep = Step.BeginningOfCombat;
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 507.1. Choose defending player (multiplayer).
            // For simplicity, assume the next player in turn order is the defending player.
            int activePlayerIndex = _gameState.Players.IndexOf(_gameState.ActivePlayer!);
            _gameState.DefendingPlayer = _gameState.Players[(activePlayerIndex + 1) % _gameState.Players.Count];
            Console.WriteLine($"    {_gameState.ActivePlayer!.Id} chooses {_gameState.DefendingPlayer!.Id} as defending player.");

            CheckStateBasedActionsAndTriggers(); // Rule 117.5

            // 507.2. Active player gets priority.
            GivePriority(_gameState.ActivePlayer!);
        }

        /// <summary>
        /// Declare Attackers Step (Rule 508).
        /// </summary>
        private void DeclareAttackersStep()
        {
            _gameState.CurrentStep = Step.DeclareAttackers;
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 508.1. Active player declares attackers.
            // For simplicity, we'll simulate a basic attack declaration.
            // Rule 508.1a: The chosen creatures must be untapped, they can’t also be battles, and each one must either have haste or have been controlled by the active player continuously since the turn began.
            // Rule 702.3b: A creature with defender can’t attack.

            var potentialAttackers = _gameState.Battlefield
                .Where(p => p.Controller == _gameState.ActivePlayer &&
                            p.Card != null && p.Card.CardTypes.Contains("Creature") &&
                            !p.Card.CardTypes.Contains("Battle") && // Battles can't attack as creatures
                            !p.CurrentKeywords.Any(k => k.Type == KeywordAbility.Defender) &&
                            (p.CurrentKeywords.Any(k => k.Type == KeywordAbility.Haste) || !p.IsTapped)) // Rule 302.6, 702.10b (Summoning Sickness)
                .ToList();

            // Simplified: No actual attack declaration logic yet, just logging potential.
            if (potentialAttackers.Any())
            {
                Console.WriteLine($"    {_gameState.ActivePlayer!.Id} has potential attackers: {string.Join(", ", potentialAttackers.Select(p => p.Card!.Name))}");
                Console.WriteLine($"    (Simplified: No actual attack declaration, assuming no attacks for now.)");
            }
            else
            {
                Console.WriteLine($"    {_gameState.ActivePlayer!.Id} has no creatures that can attack.");
            }
            Console.WriteLine($"    {_gameState.ActivePlayer!.Id} declares no attackers for simplicity.");

            CheckStateBasedActionsAndTriggers(); // Rule 117.5

            // 508.2. Active player gets priority.
            GivePriority(_gameState.ActivePlayer!);
        }

        /// <summary>
        /// Declare Blockers Step (Rule 509).
        /// </summary>
        private void DeclareBlockersStep()
        {
            _gameState.CurrentStep = Step.DeclareBlockers;
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 509.1. Defending player declares blockers.
            // For simplicity, we'll simulate a basic blocking declaration.
            // Rule 509.1a: The chosen creatures must be untapped and they can’t also be battles.
            // Rule 702.9b (Flying) & 702.17b (Reach): A creature with flying can’t be blocked except by creatures with flying and/or reach.

            var attackingCreatures = _gameState.Battlefield
                .Where(p => p.Controller == _gameState.ActivePlayer &&
                            p.Card != null && p.Card.CardTypes.Contains("Creature") &&
                            !p.IsTapped) // Assuming attacking creatures are tapped after declaring attackers, but for now, just checking if they are creatures.
                .ToList();

            var potentialBlockers = _gameState.Battlefield
                .Where(p => p.Controller == _gameState.DefendingPlayer &&
                            p.Card != null && p.Card.CardTypes.Contains("Creature") &&
                            !p.IsTapped && !p.Card.CardTypes.Contains("Battle"))
                .ToList();

            if (attackingCreatures.Any() && potentialBlockers.Any())
            {
                Console.WriteLine($"    {_gameState.DefendingPlayer!.Id} has potential blockers: {string.Join(", ", potentialBlockers.Select(p => p.Card!.Name))}");
                Console.WriteLine($"    (Simplified: No actual block declaration, assuming no blockers for now.)");

                foreach (var attacker in attackingCreatures)
                {
                    bool isFlying = attacker.CurrentKeywords.Any(k => k.Type == KeywordAbility.Flying);
                    var validBlockersForThisAttacker = potentialBlockers.Where(blocker =>
                        !isFlying || blocker.CurrentKeywords.Any(k => k.Type == KeywordAbility.Flying || k.Type == KeywordAbility.Reach)
                    ).ToList();

                    if (validBlockersForThisAttacker.Any())
                    {
                        Console.WriteLine($"      {attacker.Card!.Name} (Flying: {isFlying}) can be blocked by: {string.Join(", ", validBlockersForThisAttacker.Select(p => p.Card!.Name))}");
                    }
                    else
                    {
                        Console.WriteLine($"      {attacker.Card!.Name} (Flying: {isFlying}) cannot be blocked by any of {_gameState.DefendingPlayer!.Id}'s creatures.");
                    }
                }
            }
            else
            {
                Console.WriteLine($"    No attacking creatures or no potential blockers for {_gameState.DefendingPlayer!.Id}.");
            }
            Console.WriteLine($"    {_gameState.DefendingPlayer!.Id} declares no blockers for simplicity.");

            CheckStateBasedActionsAndTriggers(); // Rule 117.5

            // 509.2. Active player gets priority.
            GivePriority(_gameState.ActivePlayer!);
        }

        /// <summary>
        /// Combat Damage Step (Rule 510).
        /// </summary>
        private void CombatDamageStep()
        {
            _gameState.CurrentStep = Step.CombatDamage;
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            // 510.1. Assign combat damage.
            // This is a simplified implementation. A full implementation would track
            // which attackers are blocked by which blockers, and handle damage assignment
            // according to Rule 510.1c-d (e.g., trample over blocking creatures).

            Console.WriteLine("    Assigning and dealing combat damage:");

            // For now, we'll assume unblocked attackers deal damage to the defending player,
            // and blockers deal damage to the active player (representing damage to attacking creatures).
            // This is a placeholder for actual combat resolution.

            // Collect all creatures on the battlefield for simplified damage dealing
            var allCreatures = _gameState.Battlefield
                .Where(p => p.Card != null && p.Card.CardTypes.Contains("Creature"))
                .ToList();

            // Simulate damage from active player's creatures (attackers)
            foreach (var attacker in allCreatures.Where(p => p.Controller == _gameState.ActivePlayer))
            {
                int damageToDeal = attacker.CurrentPower;
                if (damageToDeal <= 0) continue;

                bool hasLifelink = attacker.CurrentKeywords.Any(k => k.Type == KeywordAbility.Lifelink);
                bool hasDeathtouch = attacker.CurrentKeywords.Any(k => k.Type == KeywordAbility.Deathtouch);
                bool hasTrample = attacker.CurrentKeywords.Any(k => k.Type == KeywordAbility.Trample);

                // Simplified: Attacker deals damage to defending player.
                // In a real scenario, this would be to a blocker or the defending player/planeswalker/battle.
                Console.WriteLine($"      {attacker.Card!.Name} (P:{attacker.CurrentPower}, T:{attacker.CurrentToughness}) deals {damageToDeal} damage to {_gameState.DefendingPlayer!.Id}.");
                _gameState.DefendingPlayer.AdjustLife(-damageToDeal);
                _eventBus.Publish(new Events.LifeLostEvent(_gameState.DefendingPlayer, damageToDeal)); // Publish LifeLostEvent
                _eventBus.Publish(new Events.DamageDealtEvent(attacker, _gameState.DefendingPlayer, null, damageToDeal, true)); // Publish DamageDealtEvent

                if (hasLifelink)
                {
                    attacker.Controller.AdjustLife(damageToDeal);
                    Console.WriteLine($"        {attacker.Card.Name} has Lifelink. {attacker.Controller.Id} gains {damageToDeal} life. Current Life: {attacker.Controller.LifeTotal}");
                    // TODO: Publish LifeGainedEvent
                }
                if (hasDeathtouch)
                {
                    // Deathtouch makes any amount of damage lethal to creatures.
                    // For players, it just means damage is dealt.
                    Console.WriteLine($"        {attacker.Card.Name} has Deathtouch. (Any damage dealt to creatures would be lethal)");
                }
                if (hasTrample)
                {
                    Console.WriteLine($"        {attacker.Card.Name} has Trample. (Excess damage would carry over to player/planeswalker)");
                }
            }

            // Simulate damage from defending player's creatures (blockers)
            foreach (var blocker in allCreatures.Where(p => p.Controller == _gameState.DefendingPlayer))
            {
                int damageToDeal = blocker.CurrentPower;
                if (damageToDeal <= 0) continue;

                bool hasLifelink = blocker.CurrentKeywords.Any(k => k.Type == KeywordAbility.Lifelink);
                bool hasDeathtouch = blocker.CurrentKeywords.Any(k => k.Type == KeywordAbility.Deathtouch);

                // Simplified: Blocker deals damage to active player.
                // In a real scenario, this would be to an attacking creature.
                Console.WriteLine($"      {blocker.Card!.Name} (P:{blocker.CurrentPower}, T:{blocker.CurrentToughness}) deals {damageToDeal} damage to {_gameState.ActivePlayer!.Id}.");
                _gameState.ActivePlayer.AdjustLife(-damageToDeal);
                _eventBus.Publish(new Events.LifeLostEvent(_gameState.ActivePlayer, damageToDeal)); // Publish LifeLostEvent
                _eventBus.Publish(new Events.DamageDealtEvent(blocker, _gameState.ActivePlayer, null, damageToDeal, true)); // Publish DamageDealtEvent

                if (hasLifelink)
                {
                    blocker.Controller.AdjustLife(damageToDeal);
                    Console.WriteLine($"        {blocker.Card.Name} has Lifelink. {blocker.Controller.Id} gains {damageToDeal} life. Current Life: {blocker.Controller.LifeTotal}");
                    // TODO: Publish LifeGainedEvent
                }
                if (hasDeathtouch)
                {
                    Console.WriteLine($"        {blocker.Card.Name} has Deathtouch. (Any damage dealt to creatures would be lethal)");
                }
            }

            // 510.2. All assigned combat damage is dealt simultaneously.
            Console.WriteLine("    Combat damage dealt.");

            CheckStateBasedActionsAndTriggers(); // Rule 117.5

            // 510.3. Active player gets priority.
            GivePriority(_gameState.ActivePlayer!);
        }

        /// <summary>
        /// End of Combat Step (Rule 511).
        /// </summary>
        private void EndOfCombatStep()
        {
            _gameState.CurrentStep = Step.EndOfCombat;
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            CheckStateBasedActionsAndTriggers(); // Rule 117.5

            // 511.1. Active player gets priority.
            GivePriority(_gameState.ActivePlayer!);

            // 511.3. As soon as the end of combat step ends, all creatures, battles, and planeswalkers are removed from combat.
            // (No combatants to remove in this simplified version)
        }

        /// <summary>
        /// End Step (Rule 513).
        /// </summary>
        private void EndStep()
        {
            _gameState.CurrentStep = Step.End;
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
            Console.WriteLine($"  {_gameState.CurrentStep}:");

            CheckStateBasedActionsAndTriggers(); // Rule 117.5

            // 513.1. Active player gets priority.
            GivePriority(_gameState.ActivePlayer!);
        }

        /// <summary>
        /// Cleanup Step (Rule 514).
        /// </summary>
        private void CleanupStep()
        {
            _gameState.CurrentStep = Step.Cleanup;
            _eventBus.Publish(new Events.StepBeginsEvent(_gameState.CurrentStep)); // Publish StepBeginsEvent
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
            // If state-based actions are performed or triggered abilities are waiting, players get priority, and another cleanup step occurs.
            bool repeatCleanup = CheckStateBasedActionsAndTriggers(); // Rule 117.5

            if (repeatCleanup)
            {
                Console.WriteLine("    State-based actions or triggers occurred, re-entering cleanup step for priority.");
                GivePriority(_gameState.ActivePlayer!); // Give priority if needed
                CleanupStep(); // Recursively call for another cleanup step
            }
            else
            {
                Console.WriteLine("    No state-based actions or triggers. Cleanup step ends.");
            }
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

        /// <summary>
        /// Handles the priority system, allowing players to take actions or pass. Rule 117.
        /// </summary>
        /// <param name="startingPlayer">The player who initially receives priority.</param>
        private void GivePriority(Player startingPlayer)
        {
            _playerWithPriority = startingPlayer;
            Console.WriteLine($"    {_playerWithPriority.Id} receives priority.");

            // This loop simulates the priority passing. In a real engine, this would involve
            // waiting for external input (player action or pass).
            // For now, it's simplified: players will pass until the stack is empty.
            // A more complete implementation would allow players to cast spells/activate abilities.

            int playersPassedInSuccession = 0;
            while (true)
            {
                // Rule 117.5: Perform state-based actions and put triggered abilities on stack before giving priority.
                CheckStateBasedActionsAndTriggers();

                // If stack is empty and all players passed in succession, end step/phase (Rule 117.4)
                if (!_gameState.Stack.Any() && playersPassedInSuccession == _gameState.Players.Count)
                {
                    Console.WriteLine("    All players passed in succession with empty stack. Step/Phase ends.");
                    break; // Exit priority loop, current step/phase ends
                }

                // Simulate player action: For now, always pass.
                // In a real game, this would be where the external application sends player actions.
                Console.WriteLine($"    {_playerWithPriority.Id} passes priority.");
                playersPassedInSuccession++;

                // If stack is not empty and all players passed, resolve top of stack (Rule 117.4)
                if (_gameState.Stack.Any() && playersPassedInSuccession == _gameState.Players.Count)
                {
                    ResolveStack();
                    playersPassedInSuccession = 0; // Reset count after stack resolution (Rule 117.3b)
                    _playerWithPriority = _gameState.ActivePlayer; // Active player gets priority after resolution (Rule 117.3b)
                    Console.WriteLine($"    {_playerWithPriority.Id} receives priority after stack resolution.");
                    continue; // Continue the priority loop
                }

                // Pass priority to the next player in turn order (Rule 117.3d)
                int currentIndex = _gameState.Players.IndexOf(_playerWithPriority);
                _playerWithPriority = _gameState.Players[(currentIndex + 1) % _gameState.Players.Count];

                // If priority returns to the starting player and no actions were taken,
                // and stack is empty, then the step/phase ends.
                // This is handled by the `playersPassedInSuccession` check.
            }
        }

        /// <summary>
        /// Resolves the topmost object on the stack. Rule 608.
        /// </summary>
        private void ResolveStack()
        {
            if (!_gameState.Stack.Any())
            {
                Console.WriteLine("    Stack is empty, nothing to resolve.");
                return;
            }

            var objectToResolve = _gameState.Stack.Pop(); // LIFO
            Console.WriteLine($"    Resolving: {(objectToResolve.Type == "Spell" ? objectToResolve.Card!.Name : objectToResolve.AbilityText)}");

            // TODO: Implement full resolution rules (Rule 608)
            // For now, just remove from stack and handle basic spell types.
            if (objectToResolve.Type == "Spell" && objectToResolve.Card != null)
            {
                if (objectToResolve.Card.CardTypes.Contains("Instant") || objectToResolve.Card.CardTypes.Contains("Sorcery"))
                {
                    // Instants and sorceries go to graveyard (Rule 608.2n)
                    objectToResolve.Controller.Graveyard.Add(objectToResolve.Card);
                    Console.WriteLine($"      {objectToResolve.Card.Name} goes to {objectToResolve.Controller.Id}'s graveyard.");
                }
                else if (objectToResolve.Card.CardTypes.Contains("Creature") ||
                         objectToResolve.Card.CardTypes.Contains("Artifact") ||
                         objectToResolve.Card.CardTypes.Contains("Enchantment") ||
                         objectToResolve.Card.CardTypes.Contains("Planeswalker") ||
                         objectToResolve.Card.CardTypes.Contains("Battle"))
                {
                    // Permanents enter the battlefield (Rule 608.3a)
                    var newPermanent = new GameState.Permanent(objectToResolve.Card, objectToResolve.Controller, objectToResolve.Card.Owner);
                    _gameState.Battlefield.Add(newPermanent);
                    _eventBus.Publish(new Events.CardEntersBattlefieldEvent(newPermanent)); // Publish CardEntersBattlefieldEvent
                    Console.WriteLine($"      {objectToResolve.Card.Name} enters the battlefield under {objectToResolve.Controller.Id}'s control.");
                }
                // Other card types (Land, Conspiracy, Dungeon, Phenomenon, Plane, Scheme, Vanguard) have specific rules for entering battlefield or staying in command zone.
                // For now, simplified.
            }
            // Abilities are removed from stack (Rule 608.2n)
        }

        /// <summary>
        /// Performs state-based actions and puts triggered abilities on the stack. Rule 117.5, 704, 603.3b.
        /// Returns true if any state-based actions were performed or abilities triggered, indicating a need to recheck priority.
        /// </summary>
        private bool CheckStateBasedActionsAndTriggers()
        {
            bool somethingHappened;
            do
            {
                somethingHappened = false;

                // 1. Perform all applicable state-based actions (Rule 704.3)
                bool sbaPerformed = PerformStateBasedActions();
                if (sbaPerformed)
                {
                    somethingHappened = true;
                    Console.WriteLine("    State-based actions performed.");
                }

                // 2. Put triggered abilities on the stack (Rule 603.3b)
                // This is a placeholder. Actual implementation would involve checking for triggered abilities
                // that occurred since the last priority check and adding them to a temporary list,
                // then sorting them by APNAP order and adding to the stack.
                // For now, we'll assume no complex triggers.
                bool triggersPutOnStack = PutTriggeredAbilitiesOnStack(); // Placeholder for actual trigger logic
                if (triggersPutOnStack)
                {
                    somethingHappened = true;
                    Console.WriteLine("    Triggered abilities put on stack.");
                }

            } while (somethingHappened); // Repeat until no more SBAs or triggers

            return somethingHappened; // Return true if anything happened, indicating priority might need to be re-evaluated
        }

        /// <summary>
        /// Placeholder for actual state-based action logic. Rule 704.
        /// Returns true if any state-based actions were performed.
        /// </summary>
        private bool PerformStateBasedActions()
        {
            bool performedAny = false;

            // Rule 704.5a: If a player has 0 or less life, that player loses the game.
            foreach (var player in _gameState.Players.ToList()) // Use ToList to avoid modifying collection during iteration
            {
                if (player.LifeTotal <= 0)
                {
                    Console.WriteLine($"      SBA: {player.Id} loses the game due to 0 or less life.");
                    _gameState.Players.Remove(player); // Player leaves the game
                    performedAny = true;
                }
            }

            // Rule 704.5c: If a player has ten or more poison counters, that player loses the game.
            foreach (var player in _gameState.Players.ToList())
            {
                if (player.PoisonCounters >= 10)
                {
                    Console.WriteLine($"      SBA: {player.Id} loses the game due to 10 or more poison counters.");
                    _gameState.Players.Remove(player);
                    performedAny = true;
                }
            }

            // Rule 704.5f: If a creature has toughness 0 or less, it’s put into its owner’s graveyard.
            foreach (var permanent in _gameState.Battlefield.ToList())
            {
                // Check if it's a creature and its toughness is 0 or less
                if (permanent.Card != null && permanent.Card.CardTypes.Contains("Creature") && permanent.CurrentToughness <= 0)
                {
                    Console.WriteLine($"      SBA: {permanent.Card.Name} put into graveyard due to 0 or less toughness.");
                    _gameState.Battlefield.Remove(permanent);
                    permanent.Owner.Graveyard.Add(permanent.Card);
                    performedAny = true;
                    _eventBus.Publish(new Events.PermanentLeavesBattlefieldEvent(permanent, Zone.Graveyard)); // Publish event
                }
            }

            // Rule 704.5g: If a creature has toughness greater than 0, it has damage marked on it, and the total damage marked on it is greater than or equal to its toughness, that creature has been dealt lethal damage and is destroyed.
            // (Damage tracking not yet implemented, so this is a placeholder)

            // Rule 704.5i: If a planeswalker has loyalty 0, it’s put into its owner’s graveyard.
            foreach (var permanent in _gameState.Battlefield.ToList())
            {
                if (permanent.Card != null && permanent.Card.CardTypes.Contains("Planeswalker") && permanent.CurrentLoyalty <= 0)
                {
                    Console.WriteLine($"      SBA: {permanent.Card.Name} put into graveyard due to 0 loyalty.");
                    _gameState.Battlefield.Remove(permanent);
                    permanent.Owner.Graveyard.Add(permanent.Card);
                    performedAny = true;
                    _eventBus.Publish(new Events.PermanentLeavesBattlefieldEvent(permanent, Zone.Graveyard)); // Publish event
                }
            }

            // Rule 704.5v: If a battle has defense 0 and it isn’t the source of an ability that has triggered but not yet left the stack, it’s put into its owner’s graveyard.
            foreach (var permanent in _gameState.Battlefield.ToList())
            {
                if (permanent.Card != null && permanent.Card.CardTypes.Contains("Battle") && permanent.CurrentDefense <= 0)
                {
                    Console.WriteLine($"      SBA: {permanent.Card.Name} put into graveyard due to 0 defense.");
                    _gameState.Battlefield.Remove(permanent);
                    permanent.Owner.Graveyard.Add(permanent.Card);
                    performedAny = true;
                    _eventBus.Publish(new Events.PermanentLeavesBattlefieldEvent(permanent, Zone.Graveyard)); // Publish event
                }
            }

            // TODO: Implement other state-based actions as needed (e.g., Legend Rule, Aura/Equipment attachments)

            return performedAny;
        }

        /// <summary>
        /// Placeholder for putting triggered abilities on the stack. Rule 603.3b.
        /// Returns true if any abilities were put on the stack.
        /// </summary>
        private bool PutTriggeredAbilitiesOnStack()
        {
            // In a real engine, this would involve:
            // 1. Identifying all triggered abilities that have met their trigger condition since the last check.
            // 2. Grouping them by controller.
            // 3. For each player in APNAP order, allowing them to put their triggered abilities on the stack in any order.
            // 4. Adding these abilities to _gameState.Stack.

            // For now, we assume no triggered abilities are actively triggering.
            // This method will be expanded when actual triggered abilities are implemented.
            return false;
        }
    }
}
