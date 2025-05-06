
using System;

namespace Z2Randomizer.RandomizerCore;

public static class ToolTips
{
    public static String AttackEffectiveness =
"""
Determines how much damage you do to enemies. Each level is randomized individually, but it will never decrease as you level up.

Vanilla: Same attack damage as in the original game.

Low Attack: 50% of vanilla damage (with at least one damage per attack level).

Randomize (Low): Damage for each attack level is randomized between 50% and 100% of vanilla damage. Level 2 is always at least 2 damage.

Randomize: Damage for each attack level is randomized from 66.7% to 150% of vanilla damage. Level 1 is always at least 2 damage.

Randomize (High): Damage for each attack level is randomized between Vanilla and High Attack.

High Attack: 150% of vanilla damage.

Instant Kill: Every enemy dies in one hit.
""";
}
