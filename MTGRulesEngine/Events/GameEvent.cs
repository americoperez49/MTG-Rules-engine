using System;
using MTGRulesEngine; // For Player, Card, Phase, Step, Zone
using static MTGRulesEngine.GameState; // For Permanent (nested class)

namespace MTGRulesEngine.Events
{
    /// <summary>
    /// Base class for all game events.
    /// </summary>
    public abstract class GameEvent
    {
        public DateTime Timestamp { get; private set; }

        protected GameEvent()
        {
            Timestamp = DateTime.UtcNow; // Use UTC for consistent timestamps
        }
    }

    /// <summary>
    /// Event triggered when a player loses life.
    /// </summary>
    public class LifeLostEvent : GameEvent
    {
        public Player Player { get; private set; }
        public int Amount { get; private set; }

        public LifeLostEvent(Player player, int amount)
        {
            Player = player;
            Amount = amount;
        }
    }

    /// <summary>
    /// Event triggered when damage is dealt.
    /// </summary>
    public class DamageDealtEvent : GameEvent
    {
        public Permanent? Source { get; private set; } // The permanent that dealt damage, if any
        public Player? TargetPlayer { get; private set; } // The player that received damage, if any
        public Permanent? TargetPermanent { get; private set; } // The permanent that received damage, if any
        public int Amount { get; private set; }
        public bool IsCombatDamage { get; private set; }

        public DamageDealtEvent(Permanent? source, Player? targetPlayer, Permanent? targetPermanent, int amount, bool isCombatDamage)
        {
            Source = source;
            TargetPlayer = targetPlayer;
            TargetPermanent = targetPermanent;
            Amount = amount;
            IsCombatDamage = isCombatDamage;
        }
    }

    /// <summary>
    /// Event triggered when a card enters the battlefield.
    /// </summary>
    public class CardEntersBattlefieldEvent : GameEvent
    {
        public Permanent Permanent { get; private set; }

        public CardEntersBattlefieldEvent(Permanent permanent)
        {
            Permanent = permanent;
        }
    }

    /// <summary>
    /// Event triggered when a card leaves the battlefield.
    /// </summary>
    public class PermanentLeavesBattlefieldEvent : GameEvent
    {
        public Permanent Permanent { get; private set; }
        public Zone DestinationZone { get; private set; } // The zone the permanent moved to

        public PermanentLeavesBattlefieldEvent(Permanent permanent, Zone destinationZone)
        {
            Permanent = permanent;
            DestinationZone = destinationZone;
        }
    }

    /// <summary>
    /// Event triggered when a spell is cast.
    /// </summary>
    public class SpellCastEvent : GameEvent
    {
        public Card SpellCard { get; private set; }
        public Player Caster { get; private set; }

        public SpellCastEvent(Card spellCard, Player caster)
        {
            SpellCard = spellCard;
            Caster = caster;
        }
    }

    /// <summary>
    /// Event triggered when a phase begins.
    /// </summary>
    public class PhaseBeginsEvent : GameEvent
    {
        public Phase Phase { get; private set; }

        public PhaseBeginsEvent(Phase phase)
        {
            Phase = phase;
        }
    }

    /// <summary>
    /// Event triggered when a step begins.
    /// </summary>
    public class StepBeginsEvent : GameEvent
    {
        public Step Step { get; private set; }

        public StepBeginsEvent(Step step)
        {
            Step = step;
        }
    }

    // Add more specific event types as needed (e.g., CounterAddedEvent, LifeGainedEvent, etc.)
}
