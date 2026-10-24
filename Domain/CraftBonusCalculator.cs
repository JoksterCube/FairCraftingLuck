using System;
using System.Collections.Generic;

namespace JoksterCube.FairCraftingLuck.Domain;

// Pure bonus math, kept free of Unity so it can be unit tested.
internal static class CraftBonusCalculator
{
    // Guards against float noise such as 20 * 0.05 = 1.0000001 rounding up to 2.
    private const float RoundingTolerance = 0.0001f;

    // Only new stackable items can be lucky, like in the game. Weapons, armor, tools and other
    // single items never are, and neither are upgrades.
    internal static bool CanBeLucky(int maxStackSize, bool isUpgrade, bool usesDurability) =>
        maxStackSize > 1 && !isUpgrade && !usesDurability;

    internal static HashSet<string> ParseItemList(string? text)
    {
        HashSet<string> items = new(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text)) return items;
        foreach (string part in text!.Split(',', ';'))
        {
            string name = part.Trim();
            if (name.Length > 0) items.Add(name);
        }
        return items;
    }

    // Extra items one lucky roll gives, before multi craft stacking.
    // batchAmount: items one craft of the recipe makes (10 for nails).
    // skillFactor: crafting skill level / 100.
    internal static int BonusPerLuckyRoll(CraftingLuckMode mode, int batchAmount, float skillFactor)
    {
        if (batchAmount <= 0) return 0;
        return mode switch
        {
            CraftingLuckMode.Fair => batchAmount,
            CraftingLuckMode.LevelBased => Math.Min(batchAmount, Math.Max(1, (int)Math.Ceiling(batchAmount * Clamp01(skillFactor) - RoundingTolerance))),
            _ => 0
        };
    }

    // Rolls once per crafted batch, like the game. With stacking, the n-th lucky roll in one craft gives n times the bonus (game behavior).
    internal static int TotalBonus(int bonusPerLuckyRoll, int rolls, float chance, bool stack, Func<float> random)
    {
        if (bonusPerLuckyRoll <= 0 || chance <= 0f) return 0;
        int lucky = 0;
        int total = 0;
        for (int i = 0; i < rolls; i++)
        {
            if (random() >= chance) continue;
            lucky++;
            total += stack ? lucky * bonusPerLuckyRoll : bonusPerLuckyRoll;
        }
        return total;
    }

    // Largest bonus (up to the rolled one) that still fits, so a lucky roll never blocks a craft the inventory has room for.
    internal static int FitBonus(int baseAmount, int bonus, Func<int, bool> canAdd)
    {
        if (bonus <= 0 || canAdd(baseAmount + bonus)) return Math.Max(0, bonus);
        int low = 0;
        int high = bonus - 1;
        while (low < high)
        {
            int mid = (low + high + 1) / 2;
            if (canAdd(baseAmount + mid)) low = mid;
            else high = mid - 1;
        }
        return low;
    }

    private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
}
