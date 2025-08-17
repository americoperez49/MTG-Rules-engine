namespace MTGRulesEngine
{
    /// <summary>
    /// Base class for all abilities. Rule 113: Abilities are qualities that define what an object does, or that change the rules of the game.
    /// </summary>
    public abstract class Ability
    {
        public string Description { get; protected set; }

        protected Ability(string description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Represents a static ability. Rule 113.3d: Static abilities are written as statements. They’re simply true.
    /// </summary>
    public class StaticAbility : Ability
    {
        public StaticAbility(string description) : base(description) { }
    }

    /// <summary>
    /// Represents an activated ability. Rule 113.3b: Activated abilities are written as "[Cost]: [Effect.] [Activation instructions (if any).]"
    /// </summary>
    public class ActivatedAbility : Ability
    {
        public string Cost { get; private set; }
        public string Effect { get; private set; }
        public string ActivationInstructions { get; private set; } // Optional

        public ActivatedAbility(string cost, string effect, string activationInstructions = "", string description = "")
            : base(description)
        {
            Cost = cost;
            Effect = effect;
            ActivationInstructions = activationInstructions;
            if (string.IsNullOrEmpty(description))
            {
                Description = $"{Cost}: {Effect}. {ActivationInstructions}".Trim();
            }
        }
    }

    /// <summary>
    /// Represents a triggered ability. Rule 113.3c: Triggered abilities are written as "[When/Whenever/At] [trigger condition or event], [effect]."
    /// </summary>
    public class TriggeredAbility : Ability
    {
        public string TriggerCondition { get; private set; }
        public string Effect { get; private set; }

        public TriggeredAbility(string triggerCondition, string effect, string description = "")
            : base(description)
        {
            TriggerCondition = triggerCondition;
            Effect = effect;
            if (string.IsNullOrEmpty(description))
            {
                Description = $"{TriggerCondition}, {Effect}".Trim();
            }
        }
    }

    /// <summary>
    /// Represents a spell ability (abilities of instants/sorceries that are followed as instructions). Rule 113.3a: Spell abilities are abilities that are followed as instructions while an instant or sorcery spell is resolving.
    /// </summary>
    public class SpellAbility : Ability
    {
        public string Effect { get; private set; }

        public SpellAbility(string effect, string description = "")
            : base(description)
        {
            Effect = effect;
            if (string.IsNullOrEmpty(description))
            {
                Description = Effect;
            }
        }
    }
}
