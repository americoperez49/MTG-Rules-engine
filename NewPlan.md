# Magic: The Gathering Rules Engine - Implementation Plan

This document outlines a detailed plan for implementing a Magic: The Gathering (MTG) rules engine. The engine will be designed to be robust, extensible, and capable of accurately simulating MTG gameplay according to the Comprehensive Rules.

## 1. Core Game State Representation

The foundation of the rules engine will be a comprehensive and accurate representation of the game state. This includes all elements on the battlefield, in players' hands, libraries, graveyards, stacks, exile, and command zones, as well as player-specific information.

### 1.1. Zones (Rule 400)

Each game will have the following zones, with specific rules for each:

- **Library (Rule 401)**: A face-down pile of cards. Players cannot look at or change the order of cards in a library. Any player may count the number of cards in any player's library. If an effect puts two or more cards in a specific position in a library at the same time, the owner of those cards may arrange them in any order. Each player has their own library.
- **Hand (Rule 402)**: A hidden zone where a player holds cards that have been drawn or put into their hand by other effects. Each player has their own hand. Each player has a maximum hand size, normally seven cards, and must discard excess cards during the cleanup step.
- **Battlefield (Rule 403)**: A public zone where permanents exist. It starts out empty. Permanents a player controls are normally kept in front of them. This zone is shared by all players.
- **Graveyard (Rule 404)**: A player's discard pile. Any object that's countered, discarded, destroyed, or sacrificed is put on top of its owner’s graveyard. Each player's graveyard starts out empty. Each graveyard is kept in a single face-up pile. Players can examine cards in any graveyard but normally can't change their order. Each player has their own graveyard.
- **Stack (Rule 405)**: A public zone where spells and abilities wait to resolve. When an object is put on the stack, it's put on top of all objects already there (LIFO - Last-In, First-Out). This zone is shared by all players.
- **Exile (Rule 406)**: A public holding area for objects removed from the game. Exiled cards are, by default, kept face up. Cards "exiled face down" can't be examined by any player except when instructions allow it. This zone is shared by all players.
- **Command Zone (Rule 408)**: A public game area reserved for certain specialized objects that have an overarching effect on the game, yet are not permanents and cannot be destroyed. This includes commanders, emblems, plane cards, phenomenon cards, scheme cards, and conspiracy cards. This zone is shared by all players.

### 1.2. Objects (Rule 109)

All entities in the game are "objects." These include:

- **Cards (Rule 108)**: Traditional Magic cards, which measure approximately 2.5 inches by 3.5 inches. Nontraditional Magic cards are also objects.
- **Tokens (Rule 111)**: Markers used to represent any permanent that isn’t represented by a card. They have characteristics defined by the spell or ability that created them.
- **Spells (Rule 112)**: A card or copy of a card on the stack.
- **Permanents (Rule 110)**: A card or token on the battlefield.
- **Abilities (Rule 113)**: Activated or triggered abilities on the stack.
- **Emblems (Rule 114)**: Markers put into the command zone with one or more abilities, but usually no other characteristics.

Each object has characteristics (name, mana cost, color, color indicator, card type, subtype, supertype, rules text, abilities, power, toughness, loyalty, defense, hand modifier, and life modifier) as defined in Rule 109.3.

### 1.3. Players (Rule 102)

Each player will have and manage:

- **Life total (Rule 119)**: Starts at 20 (or variant-specific totals).
- **Poison counters (Rule 122.1f)**: If a player has ten or more, they lose the game.
- **Mana pool (Rule 106.4)**: Where mana is temporarily stored. Empties at the end of each step and phase.
- **Hand size (Rule 402.2)**: Normally seven cards, with rules for discarding excess.
- **Designations**: These are specific states or roles a player can have, such as:
  - Monarch (Rule 724)
  - Initiative (Rule 725)
  - City's Blessing (Rule 702.131)
  - Ring-bearer (Rule 701.54)
  - Suspected (Rule 701.60)
  - Solved (Rule 702.169)
  - Saddled (Rule 702.171)
  - Goaded (Rule 701.15)
  - Renowned (Rule 702.112)
  - Monstrous (Rule 701.37)
  - Day/Night (Rule 730)

### 1.4. Game State Object

A central `GameState` object will encapsulate all of the above, providing a single source of truth for the current state of the game. This object will be immutable or use a transactional approach to allow for easy rollback and "what-if" scenarios (e.g., for checking legality of actions).

## 2. Card Data Model

Representing MTG cards and their complex rules text is a critical challenge.

### 2.1. Card Definition Structure

Each card will have a data structure that includes:

- **Name (Rule 201)**: The name printed on its upper left corner, always considered the English version.
- **Mana Cost (Rule 202)**: Indicated by mana symbols. Includes generic ({0}, {1}, {2}, etc., {X}), colored ({W}, {U}, {B}, {R}, {G}), hybrid ({W/U}, {2/B}, etc.), Phyrexian ({W/P}, {U/P}, etc.), and snow ({S}) mana symbols.
- **Color (Rule 105, 202.2)**: Derived from mana symbols in its mana cost, color indicator, or characteristic-defining abilities. Can be white, blue, black, red, green, or colorless.
- **Color Identity (Rule 903.4)**: The color or colors of any mana symbols in that card’s mana cost or rules text, plus any colors defined by its characteristic-defining abilities or color indicator. Crucial for Commander.
- **Mana Value (Rule 202.3)**: A number equal to the total amount of mana in its mana cost, regardless of color.
- **Illustration (Rule 203)**: The picture on the card, has no effect on game play.
- **Color Indicator (Rule 204)**: A circular symbol to the left of the type line, defining the card's color(s).
- **Type Line (Rule 205)**: Contains the card’s card type(s), subtype(s), and supertype(s).
  - **Card Types (Rule 205.2a)**: Artifact, Battle, Conspiracy, Creature, Dungeon, Enchantment, Instant, Kindred, Land, Phenomenon, Plane, Planeswalker, Scheme, Sorcery, and Vanguard.
  - **Subtypes (Rule 205.3)**: Specific categories within card types, such as creature types (Human, Elf, Goblin), land types (Forest, Island, Mountain, Plains, Swamp), artifact types (Equipment, Vehicle, Treasure), enchantment types (Aura, Saga, Class), planeswalker types (Jace, Liliana), and spell types (Adventure, Arcane).
  - **Supertypes (Rule 205.4)**: Basic, Legendary, Ongoing, Snow, and World.
- **Rules Text / Abilities (Rule 207, 113)**: The most complex part, defining the card's functionality.
  - **Static Abilities (Rule 113.3d, 604)**: Continuously active effects that are simply true (e.g., "Creatures you control get +1/+1").
  - **Activated Abilities (Rule 113.3b, 602)**: Written as "[Cost]: [Effect.] [Activation instructions (if any).]" (e.g., "{2}, {T}: Draw a card").
  - **Triggered Abilities (Rule 113.3c, 603)**: Written as "[When/Whenever/At] [trigger condition or event], [effect]." (e.g., "When this creature enters the battlefield, draw a card").
  - **Spell Abilities (Rule 113.3a)**: Abilities that are followed as instructions while an instant or sorcery spell is resolving.
  - **Keyword Abilities (Rule 702)**: Shorthand for longer abilities or groups of abilities. Each keyword will require specific implementation:
    - Absorb (702.64), Adapt (701.46), Afflict (702.130), Affinity (702.41), Afterlife (702.135), Aftermath (702.127), Amass (701.47), Amplify (702.38), Annihilator (702.86), Ascend (702.131), Assist (702.132), Aura Swap (702.65), Awaken (702.113), Backup (702.165), Banding (702.22), Bargain (702.166), Battle Cry (702.91), Bestow (702.103), Blitz (702.152), Bloodthirst (702.54), Boast (702.142), Bushido (702.45), Buyback (702.27), Cascade (702.85), Casualty (702.153), Champion (702.72), Changeling (702.73), Cipher (702.99), Cleave (702.148), Cloak (701.58), Compleated (702.150), Companion (702.139), Conspire (702.78), Convoke (702.51), Craft (702.167), Crew (702.122), Cumulative Upkeep (702.24), Cycling (702.29), Dash (702.109), Daybound (702.145), Decayed (702.147), Delve (702.66), Demonstrate (702.144), Dethrone (702.105), Devoid (702.114), Disturb (702.146), Double Strike (702.4), Dredge (702.52), Echo (702.30), Emerge (702.119), Embalm (702.128), Encore (702.141), Enlist (702.154), Entwine (702.42), Epic (702.50), Equip (702.6), Escape (702.138), Escalate (702.120), Eternalize (702.129), Evoke (702.74), Evolve (702.100), Exalted (702.83), Exhaust (702.177), Exploit (702.110), Extort (702.101), Fading (702.32), Fabricate (702.123), Fear (702.36), First Strike (702.7), Flash (702.8), Flashback (702.34), Flying (702.9), Forecast (702.57), Foretell (702.143), Fortify (702.67), Freerunning (702.173), Frenzy (702.68), Friends Forever (702.124j), Fuse (702.102), Gift (702.174), Graft (702.58), Gravestorm (702.69), Haste (702.10), Haunt (702.55), Hexproof (702.11), Hidden Agenda (702.106), Hideaway (702.75), Horsemanship (702.31), Impending (702.176), Improvise (702.126), Infect (702.90), Ingest (702.115), Intimidate (702.13), Job Select (702.182), Jump-Start (702.133), Kicker (702.33), Landwalk (702.14), Level Up (702.87), Lifelink (702.15), Living Metal (702.161), Living Weapon (702.92), Madness (702.35), Max Speed (702.178), Melee (702.121), Menace (702.111), Mentor (702.134), Miracle (702.94), Mobilize (702.181), More Than Meets the Eye (702.162), Morph (702.37), Myriad (702.116), Nightbound (702.145), Ninjutsu (702.49), Offering (702.48), Offspring (702.175), Outlast (702.107), Overload (702.96), Partner (702.124), Persist (702.79), Phasing (702.26), Poisonous (702.70), Protection (702.16), Prototype (702.160), Provoke (702.39), Prowess (702.108), Prowl (702.76), Rampage (702.23), Ravenous (702.156), Read Ahead (702.155), Rebound (702.88), Reconfigure (702.151), Recover (702.59), Regenerate (701.19), Reinforce (702.77), Renown (702.112), Retrace (702.81), Riot (702.136), Ripple (702.60), Saddle (702.171), Scavenge (702.97), Shroud (702.18), Skulk (702.118), Solved (702.169), Soulbond (702.95), Soulshift (702.46), Space Sculptor (702.158), Spectacle (702.137), Split Second (702.61), Spree (702.172), Squad (702.157), Start Your Engines! (702.179), Station (702.184), Storm (702.40), Suspend (702.62), Tiered (702.183), Toxic (702.164), Training (702.149), Trample (702.19), Transfigure (702.71), Transform (701.27), Transmute (702.53), Umbra Armor (702.89), Undying (702.93), Unearth (702.84), Unleash (702.98), Vanishing (702.63), Vigilance (702.20), Visit (702.159), Ward (702.21), Warp (702.185), Wither (702.80).
  - **Keyword Actions (Rule 701)**: Specific verbs with defined game meanings. Each keyword action will be implemented as a specific function:
    - Abandon (701.33), Activate (701.2), Adapt (701.46), Amass (701.47), Assemble (701.45), Attach (701.3), Behold (701.4), Bolster (701.39), Cast (701.5), Clash (701.30), Cleave (702.148), Cloak (701.58), Collect Evidence (701.59), Connive (701.50), Counter (701.6), Craft (702.167), Create (701.7), Crew (702.122), Detain (701.35), Destroy (701.8), Discover (701.57), Discard (701.9), Double (701.10), Endure (701.63), Exchange (701.12), Exile (701.13), Exert (701.43), Explore (701.44), Face a Villainous Choice (701.55), Fateseal (701.29), Fight (701.14), Forage (701.61), Goad (701.15), Harmonize (702.180), Incubate (701.53), Investigate (701.16), Learn (701.48), Lock (709.5g), Manifest (701.40), Manifest Dread (701.62), Meld (701.42), Mill (701.17), Mobilize (702.181), Open an Attraction (701.51), Play (701.18), Planeswalk (701.31), Plot (702.170), Proliferate (701.34), Regenerate (701.19), Reconfigure (702.151), Reveal (701.20), Roll to Visit Your Attractions (701.52), Saddle (702.171), Scry (701.22), Search (701.23), Set in Motion (701.32), Shuffle (701.24), Suspect (701.60), Tap (701.26a), The Ring Tempts You (701.54), Time Travel (701.56), Transform (701.27), Triple (701.11), Unlock (709.5f), Untap (701.26b), Venture into the Dungeon (701.49), Vote (701.38).
- **Power/Toughness (Rule 208)**: For creatures. Handle `*` values (Rule 208.2) and continuous effects (Rule 613).
- **Loyalty (Rule 209)**: For planeswalkers. Indicates starting loyalty and current loyalty counters.
- **Defense (Rule 210)**: For battles. Indicates starting defense and current defense counters.
- **Hand Modifier (Rule 211)**: For vanguard cards, modifies starting and maximum hand size.
- **Life Modifier (Rule 212)**: For vanguard cards, modifies starting life total.
- **Expansion Symbol (Rule 206)**: Indicates set and rarity, no effect on game play.
- **Collector Number (Rule 213)**: Unique identifier within a set, no effect on game play.

### 2.2. Parsing and Interpretation

Given the text-heavy nature of MTG cards, a robust parsing and interpretation layer will be needed. This could involve:

- **Regular Expressions**: For identifying common patterns like costs, keywords, and trigger conditions within the rules text.
- **Abstract Syntax Tree (AST)**: Representing card text as a structured tree to allow for programmatic interpretation of complex interactions and dependencies between abilities.
- **Rule-based System**: Mapping parsed text components to predefined rule functions and data structures that encapsulate the game logic for each ability and keyword.
- **Oracle Text Integration**: Relying on the official Oracle text (Rule 108.1) for definitive wording and up-to-date rules text for all tournament-legal cards. This is crucial for accuracy and handling errata.

## 3. Rule Enforcement System

This is the core logic of the engine, responsible for advancing the game state and ensuring all rules are followed.

### 3.1. Turn Structure (Rule 500)

Implement the five phases and their steps, ensuring strict adherence to the order and timing rules:

- **Beginning Phase (Rule 501)**:
  - **Untap Step (Rule 502)**: All phased-in permanents with phasing phase out, and all phased-out permanents phase in. Day/Night designation check. Active player untaps all permanents they control. No player receives priority during this step.
  - **Upkeep Step (Rule 503)**: Active player gets priority. Any abilities that triggered during the untap step and any abilities that triggered at the beginning of the upkeep are put onto the stack.
  - **Draw Step (Rule 504)**: Active player draws a card (unless it's the first turn of a two-player game or Two-Headed Giant game). Active player gets priority.
- **Main Phase (Rule 505)**: There are two main phases: precombat and postcombat. Active player gets priority. During either main phase, the active player can normally cast artifact, creature, enchantment, planeswalker, and sorcery spells, and play one land card from their hand if the stack is empty and they haven't played a land this turn.
- **Combat Phase (Rule 506)**:
  - **Beginning of Combat Step (Rule 507)**: In multiplayer games where opponents don't all automatically become defending players, the active player chooses one opponent to be the defending player. Active player gets priority.
  - **Declare Attackers Step (Rule 508)**: Active player declares which creatures will attack, and which player, planeswalker, or battle each is attacking. Costs to attack are paid. Attacking creatures become tapped (unless they have vigilance). Triggered abilities related to attacking trigger. Active player gets priority.
  - **Declare Blockers Step (Rule 509)**: Defending player declares blockers. Costs to block are paid. Attacking creatures become blocked or unblocked. Triggered abilities related to blocking trigger. Active player gets priority.
  - **Combat Damage Step (Rule 510)**: Attacking and blocking creatures assign their combat damage. All assigned combat damage is dealt simultaneously. Triggered abilities related to damage trigger. Active player gets priority. (Handle First Strike/Double Strike by creating a second combat damage step if applicable).
  - **End of Combat Step (Rule 511)**: Active player gets priority. All creatures, battles, and planeswalkers are removed from combat as this step ends.
- **Ending Phase (Rule 512)**:
  - **End Step (Rule 513)**: Active player gets priority. Abilities that trigger "at the beginning of the end step" trigger here.
  - **Cleanup Step (Rule 514)**: Active player discards down to their maximum hand size. All damage marked on permanents is removed, and all "until end of turn" and "this turn" effects end. Normally no player receives priority, but if state-based actions are performed or triggered abilities are waiting, players get priority, and another cleanup step occurs.

### 3.2. Priority System (Rule 117)

A robust priority system is essential for correct spell and ability resolution, ensuring players can respond to actions.

- **Receiving Priority**: Players receive priority at the beginning of most steps and phases (after turn-based actions and initial triggers), and after a spell or ability resolves.
- **Actions with Priority**: The player with priority may cast spells, activate abilities, or take special actions.
- **Passing Priority**: If a player has priority and chooses not to take any actions, they pass priority. The next player in turn order receives priority.
- **Stack Resolution/Phase End**: If all players pass in succession (without taking any actions in between passing), the spell or ability on top of the stack resolves. If the stack is empty when all players pass in succession, the current phase or step ends.

### 3.3. Casting Spells and Activating Abilities (Rule 601, 602)

A detailed step-by-step process for casting spells and activating abilities, ensuring all choices and costs are handled correctly:

1.  **Announce Spell/Ability (Rule 601.2a, 602.2a)**: Move the card (for spells) or create the ability object (for abilities) to the stack. It becomes the topmost object.
2.  **Choose Modes, Alternative/Additional Costs, Variables (Rule 601.2b, 602.2b)**: Announce choices for modal spells, intentions to pay alternative or additional costs (e.g., kicker, buyback, overload, mutate, foretell, plot, cleave, spree), and the value of X for variable costs.
3.  **Choose Targets (Rule 601.2c, 602.2b)**: Announce appropriate objects or players for each target the spell/ability requires. The same target cannot be chosen multiple times for a single instance of "target."
4.  **Divide/Distribute Effects (Rule 601.2d, 602.2b)**: If the spell/ability requires dividing or distributing an effect (e.g., damage, counters) among targets, the player announces the division.
5.  **Check Legality (Rule 601.2e)**: The game checks if the proposed spell/ability can legally be cast/activated. If illegal, the action is reversed.
6.  **Determine Total Cost (Rule 601.2f, 602.2b)**: Calculate the final cost, including mana cost or alternative cost, plus all additional costs and cost increases, and minus all cost reductions. This cost becomes "locked in."
7.  **Activate Mana Abilities (Rule 601.2g, 602.2b)**: The player has a chance to activate mana abilities to generate the necessary mana.
8.  **Pay Costs (Rule 601.2h, 602.2b)**: The player pays the total cost. Partial payments are not allowed.
9.  **Become Cast/Activated (Rule 601.2i, 602.2b)**: Effects that modify characteristics of the spell as it's cast are applied. The spell becomes cast or the ability becomes activated. Any abilities that trigger "when a spell is cast" or "when an ability is activated" trigger at this time.

### 3.4. Resolving Spells and Abilities (Rule 608)

The process for a spell or ability to have its effect:

- **Check Intervening "If" Clause (Rule 608.2a)**: If a triggered ability has an intervening "if" clause, it checks if the condition is true. If not, it's removed from the stack.
- **Check Legality of Targets (Rule 608.2b)**: Verify all targets are still legal. If all targets are illegal, the spell/ability doesn't resolve.
- **Follow Instructions (Rule 608.2c)**: The controller follows the instructions in the order written, with replacement effects modifying actions.
- **Handle Choices During Resolution (Rule 608.2d)**: If the effect offers choices, the player announces them.
- **Handle Simultaneous Actions (Rule 608.2e, 608.2f)**: If multiple players are involved in actions, choices are made in APNAP order, then actions happen simultaneously.
- **Handle Information Queries (Rule 608.2h, 608.2i)**: If the effect requires information from the game or specific objects, it's determined once upon application, using current or last known information.
- **Final Steps (Rule 608.2n, 608.2p)**: For instants/sorceries, the spell is put into its owner's graveyard. For abilities, the ability is removed from the stack. Any abilities that trigger "when that spell or ability resolves" trigger.

### 3.5. Continuous Effects (Rule 611) and Layer System (Rule 613)

This is one of the most complex and crucial parts of the engine. Continuous effects modify characteristics, control, or rules, and are applied in a strict layer system to ensure consistent interaction:

- **Layer 1 (Copiable Values) (Rule 613.2)**: Applied first. Includes copy effects (Rule 707) and modifications from face-down status (Rule 708.2).
  - Layer 1a: Copy effects.
  - Layer 1b: Face-down status modifications.
- **Layer 2 (Control) (Rule 613.3)**: Control-changing effects are applied.
- **Layer 3 (Text) (Rule 613.3)**: Text-changing effects (Rule 612) are applied.
- **Layer 4 (Type) (Rule 613.3)**: Type-changing effects (card type, subtype, supertype) are applied.
- **Layer 5 (Color) (Rule 613.3)**: Color-changing effects are applied.
- **Layer 6 (Abilities) (Rule 613.3)**: Ability-adding effects, keyword counters, ability-removing effects, and effects that say an object can’t have an ability are applied.
- **Layer 7 (Power/Toughness) (Rule 613.4)**: Applied last, in sublayers:
  - Layer 7a: Effects from characteristic-defining abilities that define power and/or toughness.
  - Layer 7b: Effects that set power and/or toughness to a specific number or value.
  - Layer 7c: Effects and counters that modify power and/or toughness (e.g., +1/+1 counters, "gets +X/+Y" effects).
  - Layer 7d: Effects that switch a creature’s power and toughness.

Within each layer or sublayer, effects are generally applied in timestamp order (Rule 613.7), but dependency (Rule 613.8) can override this order.

### 3.6. Replacement Effects (Rule 614) and Prevention Effects (Rule 615)

These effects modify how events happen, acting as "shields" around affected objects or players.

- **Replacement Effects (Rule 614)**: Use the word "instead" or "skip" to indicate that an event is replaced with a different event or nothing. Examples: "If a creature would be destroyed, regenerate it instead."
- **Prevention Effects (Rule 615)**: Use the word "prevent" to indicate that damage will not be dealt. Examples: "Prevent the next 3 damage that would be dealt."
- **Application Order (Rule 616)**: If multiple replacement/prevention effects apply to the same event, the affected object's controller (or owner) or the affected player chooses one to apply. Self-replacement effects are applied first. This process repeats until no more effects apply.

### 3.7. State-Based Actions (Rule 704)

These are automatic game actions that happen whenever certain conditions are met. They do not use the stack and are checked continuously.

- **Life Loss (Rule 704.5a)**: If a player has 0 or less life, that player loses the game.
- **Drawing from Empty Library (Rule 704.5b)**: If a player attempted to draw a card from an empty library, that player loses the game.
- **Poison Counters (Rule 704.5c)**: If a player has ten or more poison counters, that player loses the game (modified for Two-Headed Giant).
- **Tokens/Copies Cease to Exist (Rule 704.5d, 704.5e)**: If a token is in a zone other than the battlefield, or a copy of a spell/card is in an inappropriate zone, it ceases to exist.
- **Creature Toughness (Rule 704.5f)**: If a creature has toughness 0 or less, it's put into its owner's graveyard.
- **Lethal Damage (Rule 704.5g)**: If a creature has toughness greater than 0 and has been dealt lethal damage, it is destroyed.
- **Deathtouch (Rule 704.5h)**: If a creature has toughness greater than 0 and has been dealt damage by a source with deathtouch, it is destroyed.
- **Planeswalker Loyalty (Rule 704.5i)**: If a planeswalker has loyalty 0, it's put into its owner's graveyard.
- **Legend Rule (Rule 704.5j)**: If a player controls two or more legendary permanents with the same name, they choose one, and the rest are put into their owners’ graveyards.
- **World Rule (Rule 704.5k)**: If two or more permanents have the supertype world, all except the one with the shortest time as a world permanent are put into their owners’ graveyards.
- **Illegal Attachments (Rule 704.5m, 704.5n, 704.5p)**: Auras attached illegally go to the graveyard. Equipment/Fortifications attached illegally become unattached. Other permanents attached illegally become unattached.
- **Counter Interaction (Rule 704.5q, 704.5r)**: N +1/+1 and N -1/-1 counters are removed. Excess counters beyond a limit are removed.
- **Saga Completion (Rule 704.5s)**: Sagas with enough lore counters are sacrificed.
- **Dungeon Completion (Rule 704.5t)**: Dungeon cards with venture markers on their bottommost room are removed from the game.
- **Space Sculptor (Rule 704.5u)**: Creatures without sector designations get one if a permanent with Space Sculptor is on the battlefield.
- **Battle Defense (Rule 704.5v)**: If a battle has 0 defense, it's put into its owner's graveyard.
- **Battle Protector (Rule 704.5w, 704.5x)**: If a battle has no protector or an illegal protector, a new one is chosen or it's put into the graveyard.
- **Role Tokens (Rule 704.5y)**: Excess Role tokens controlled by the same player attached to a permanent are put into the graveyard.
- **Player Speed (Rule 704.5z)**: If a player controls a permanent with "Start Your Engines!" and has no speed, their speed becomes 1.

### 3.8. Multiplayer Rules (Section 8)

Implement the various multiplayer options and variants, as they significantly alter core game rules:

- **Limited Range of Influence (Rule 801)**: Limits what a player can affect based on seating distance.
- **Attack Multiple Players Option (Rule 802)**: Allows attacking multiple opponents.
- **Attack Left and Attack Right Options (Rule 803)**: Restricts attacks to adjacent opponents.
- **Deploy Creatures Option (Rule 804)**: Allows teammates to gain control of creatures.
- **Shared Team Turns Option (Rule 805)**: Teams take turns and share priority.
- **Free-for-All Variant (Rule 806)**: Players compete individually.
- **Grand Melee Variant (Rule 807)**: Large-scale Free-for-All with multiple simultaneous turns.
- **Team vs. Team Variant (Rule 808)**: Teams compete against each other.
- **Emperor Variant (Rule 809)**: Teams of three with an Emperor and Generals, using limited range of influence and deploy creatures.
- **Two-Headed Giant Variant (Rule 810)**: Two teams of two players with a shared life total and shared team turns.
- **Alternating Teams Variant (Rule 811)**: Teams of equal size with alternating seating.

### 3.9. Casual Variants (Section 9)

Implement the rules for specific casual variants:

- **Planechase (Rule 901)**: Introduces planar decks, plane cards, phenomenon cards, and the planar die, affecting the game with unique abilities and planeswalking.
- **Vanguard (Rule 902)**: Players choose a vanguard card that modifies their starting life total and hand size.
- **Commander (Rule 903)**: Defines deck construction rules (100-card singleton, color identity), commander tax, and commander damage. Includes Brawl and Commander Draft sub-variants.
- **Archenemy (Rule 904)**: One player is the archenemy with a scheme deck, facing a team of opponents.
- **Conspiracy Draft (Rule 905)**: A draft format introducing conspiracy cards with abilities that function during the draft and game.

## 4. Event System

A robust event system is crucial for handling triggers and continuous effects, ensuring that all relevant rules are applied at the correct time.

- **Event Bus**: A central system where all game events are published (e.g., `CardEntersBattlefieldEvent`, `DamageDealtEvent`, `SpellCastEvent`, `PermanentLeavesBattlefieldEvent`, `CounterAddedEvent`, `LifeGainedEvent`, `LifeLostEvent`, `PhaseBeginsEvent`, `StepBeginsEvent`).
- **Subscribers**: Abilities (especially triggered abilities and replacement/prevention effects) will subscribe to relevant events. When an event occurs, the event bus notifies all subscribed abilities.
- **Trigger Queue**: When an event occurs, all triggered abilities that meet their trigger condition are identified and added to a queue. They are then put onto the stack when a player would receive priority (Rule 603.3), following APNAP order.
- **"Looks Back in Time" (Rule 603.10)**: Special handling for triggers that check the game state _before_ an event occurred (e.g., leaves-the-battlefield triggers). The event system must capture the state immediately prior to the event for these checks.

## 5. Interaction with External Application

The rules engine should be a standalone component that can be integrated into other applications (e.g., a game client, an AI player).

- **API**: Define a clear, well-documented API for the external application to:
  - `InitializeGame(players, decks, variantOptions)`: Set up a new game.
  - `AdvanceTurn()`: Progress the game through phases and steps.
  - `ProposeAction(player, actionType, actionDetails)`: Allow a player to propose casting a spell, activating an ability, or taking a special action.
  - `MakeChoice(player, choiceDetails)`: Allow a player to make a choice required by a spell or ability.
  - `QueryGameState()`: Retrieve a snapshot of the current game state.
  - `GetLegalActions(player)`: Return a list of all legal actions a player can take at the current moment.
  - `SubscribeToEvents(eventType, callback)`: Allow the external application to receive notifications of game events.
- **Data Serialization**: Implement mechanisms to serialize and deserialize the entire game state (including all objects, zones, and player information) to/from a persistent format (e.g., JSON, XML) for saving, loading, and replaying games.

## 6. Development Approach

### 6.1. Incremental Development

Start with the most fundamental rules and gradually add complexity, building a solid foundation before tackling intricate interactions.

1.  **Basic Game Setup**: Implement players, decks, starting life totals, drawing opening hands, and mulligans.
2.  **Core Turn Structure**: Implement phases (Beginning, Main, Combat, Ending) and their basic steps (Untap, Upkeep, Draw, Declare Attackers, Declare Blockers, Combat Damage, End, Cleanup).
3.  **Basic Actions**: Implement playing lands and casting simple spells (instants, sorceries, creatures without abilities).
4.  **Fundamental Combat**: Implement attacking, blocking, and combat damage assignment and dealing.
5.  **Initial State-Based Actions**: Implement checks for 0 life, 0 toughness, lethal damage, and 0 loyalty.
6.  **Stack and Priority**: Implement the full priority system and how spells/abilities are put on and resolve from the stack.
7.  **Core Keywords**: Implement common keyword abilities (Haste, Flying, Trample, Lifelink, Deathtouch, Vigilance, Defender).
8.  **Continuous Effects and Layers**: Implement the layer system for applying continuous effects, starting with basic power/toughness modifications.
9.  **Replacement and Prevention Effects**: Implement the logic for these effects and their interaction.
10. **Complex Keywords and Abilities**: Systematically implement all remaining keyword abilities and actions, handling their specific rules and interactions.
11. **Multiplayer Rules**: Implement the various multiplayer options and variants.
12. **Casual Variants**: Implement the rules for Planechase, Vanguard, Commander, Archenemy, and Conspiracy Draft.

### 6.2. Testing

A comprehensive testing strategy is paramount for a rules engine of this complexity.

- **Unit Tests**: Develop granular unit tests for each individual rule, keyword ability, keyword action, and component (e.g., `ManaCost` parsing, `Layer7c` application).
- **Integration Tests**: Create tests that verify the correct interaction between different parts of the engine (e.g., a spell with a triggered ability that creates a token, a replacement effect modifying a damage event).
- **Scenario Tests**: Simulate full game scenarios, including complex interactions and edge cases described in the Comprehensive Rules examples. This will involve setting up specific game states and verifying the outcome of a sequence of actions.
- **Regression Tests**: Maintain a robust suite of regression tests to ensure that new features or bug fixes do not inadvertently break existing functionality.
- **Fuzz Testing**: Potentially use fuzz testing to generate random game states and sequences of actions to uncover unexpected interactions.

### 6.3. Technology Stack (Assumed C# based on project structure)

- **Language**: C#
- **Data Structures**: Utilize custom classes for representing game entities such as `Card`, `Permanent`, `Player`, `Zone`, `Ability`, `Effect`, `Cost`, `Target`, `Counter`, `Designation`, `ManaPool`, `Library`, `Hand`, `Graveyard`, `Stack`, `Exile`, `CommandZone`.
- **Collections**: Leverage .NET's built-in collections like `List<T>`, `Dictionary<TKey, TValue>`, `Stack<T>`, `Queue<T>` for efficient management of game objects within zones and event queues.
- **Event Handling**: Implement a custom event bus or utilize C# events and delegates for the event system, allowing for decoupled communication between game components.
- **Serialization**: Use .NET's built-in JSON or XML serialization capabilities, or a third-party library like Newtonsoft.Json, for saving and loading game states.

## 7. Challenges and Considerations

- **Complexity**: The MTG Comprehensive Rules are incredibly detailed, with numerous interactions, edge cases, and exceptions. Thorough understanding and meticulous implementation of each rule are essential.
- **Performance**: Simulating complex game states and interactions, especially with many objects or long chains of triggers/effects, will require careful optimization to ensure acceptable performance.
- **Extensibility**: Magic: The Gathering frequently introduces new keywords, mechanics, and card types. The engine's architecture must be designed to easily incorporate these future additions without requiring major refactoring.
- **Rule Updates**: The Comprehensive Rules are updated multiple times a year. The engine must have a clear process for adapting to these changes, potentially through modular rule definitions.
- **Non-Deterministic Choices**: Many rules involve player choices (e.g., "choose a target," "divide damage," "choose a mode"). The engine must provide clear interfaces for these choices to be made by an external agent (the user, an AI, or a test harness).
- **Hidden Information**: Correctly handling hidden zones (hand, library, face-down cards) while maintaining game integrity and preventing illegal information access is critical.
- **Copy Effects (Rule 707)**: These are notoriously difficult to implement correctly due to how they interact with copiable values, timestamps, and other continuous effects. Special attention will be required here.
- **Linked Abilities (Rule 607)**: Ensuring correct tracking and application of linked abilities, where one ability refers to actions or objects affected by another.
- **Rollbacks/Reversals (Rule 732)**: Implementing the ability to reverse illegal actions or partial spell/ability castings is complex but necessary for a robust engine.
