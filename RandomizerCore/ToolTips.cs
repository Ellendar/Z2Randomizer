
using System;

namespace RandomizerCore;

public static class ToolTips
{
    public static String AttackEffectiveness =
"""
Determines how much damage you do to enemies. Each level is randomized individually, but it will never decrease as you level up.

Vanilla: Same attack damage as in the original game.

Low Attack: 50% of vanilla damage (with at least one damage per attack level).

Randomize (Low): Damage for each attack level is randomized between Low Attack and Vanilla.

Randomize: Damage for each attack level is randomized from 66.7% to 150% of vanilla damage.

Randomize (High): Damage for each attack level is randomized between Vanilla and High Attack.

High Attack: 150% of vanilla damage.

Instant Kill: Every enemy dies in one hit.
""";
}
