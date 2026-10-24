using System;
using System.Collections.Generic;
using JoksterCube.FairCraftingLuck.Domain;
using Xunit;

namespace JoksterCube.FairCraftingLuck.Tests;

public class BonusPerLuckyRollTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 10)]
    [InlineData(20, 20)]
    public void Fair_gives_a_whole_batch(int batch, int expected) =>
        Assert.Equal(expected, CraftBonusCalculator.BonusPerLuckyRoll(CraftingLuckMode.Fair, batch, 0.3f));

    [Theory]
    [InlineData(0.01f, 1)]
    [InlineData(0.05f, 1)]
    [InlineData(0.06f, 2)]
    [InlineData(0.5f, 10)]
    [InlineData(1f, 20)]
    public void Level_based_scales_a_20_item_recipe(float skillFactor, int expected) =>
        Assert.Equal(expected, CraftBonusCalculator.BonusPerLuckyRoll(CraftingLuckMode.LevelBased, 20, skillFactor));

    [Fact]
    public void Level_based_gives_at_least_one_item() =>
        Assert.Equal(1, CraftBonusCalculator.BonusPerLuckyRoll(CraftingLuckMode.LevelBased, 1, 0f));

    [Theory]
    [InlineData(3)] // NoBonus
    [InlineData(2)] // Vanilla is handled by the game itself.
    public void Other_modes_add_nothing_here(int mode) =>
        Assert.Equal(0, CraftBonusCalculator.BonusPerLuckyRoll((CraftingLuckMode)mode, 10, 1f));
}

public class TotalBonusTests
{
    private static Func<float> Rolls(params float[] values)
    {
        var queue = new Queue<float>(values);
        return () => queue.Dequeue();
    }

    [Fact]
    public void Stacking_matches_the_game_loop()
    {
        // Lucky, unlucky, lucky, lucky: +1, +2, +3 batches.
        int bonus = CraftBonusCalculator.TotalBonus(10, 4, 0.25f, stack: true, Rolls(0.1f, 0.9f, 0.2f, 0f));
        Assert.Equal(60, bonus);
    }

    [Fact]
    public void Without_stacking_every_lucky_roll_counts_the_same()
    {
        int bonus = CraftBonusCalculator.TotalBonus(10, 4, 0.25f, stack: false, Rolls(0.1f, 0.9f, 0.2f, 0f));
        Assert.Equal(30, bonus);
    }

    [Fact]
    public void Zero_chance_never_rolls() =>
        Assert.Equal(0, CraftBonusCalculator.TotalBonus(10, 5, 0f, true, () => throw new InvalidOperationException()));

    [Fact]
    public void Average_at_level_100_matches_vanilla_ratios()
    {
        var random = new Random(1234);
        const int trials = 200_000;
        long single = 0, multi = 0;
        for (int i = 0; i < trials; i++)
        {
            single += CraftBonusCalculator.TotalBonus(1, 1, 0.25f, true, () => (float)random.NextDouble());
            multi += CraftBonusCalculator.TotalBonus(1, 5, 0.25f, true, () => (float)random.NextDouble());
        }
        // One by one: +0.25 per craft. Five at once with stacking: +1.875 per 5 crafts.
        Assert.InRange(single / (double)trials, 0.24, 0.26);
        Assert.InRange(multi / (double)trials, 1.84, 1.91);
    }
}

public class FitBonusTests
{
    [Fact]
    public void Keeps_the_full_bonus_when_it_fits() =>
        Assert.Equal(20, CraftBonusCalculator.FitBonus(10, 20, total => total <= 100));

    [Fact]
    public void Trims_the_bonus_to_the_free_space() =>
        Assert.Equal(7, CraftBonusCalculator.FitBonus(10, 20, total => total <= 17));

    [Fact]
    public void Drops_the_bonus_when_only_the_craft_fits() =>
        Assert.Equal(0, CraftBonusCalculator.FitBonus(10, 20, total => total <= 10));
}

public class EligibilityTests
{
    [Theory]
    [InlineData(100, false, false, true)]  // nails, arrows, meads
    [InlineData(1, false, true, false)]    // weapons, tools, armor
    [InlineData(1, false, false, false)]   // other single items
    [InlineData(50, true, false, false)]   // upgrades
    [InlineData(50, false, true, false)]   // modded stackable with durability
    public void Only_new_stackable_items_can_be_lucky(int maxStack, bool upgrade, bool durability, bool expected) =>
        Assert.Equal(expected, CraftBonusCalculator.CanBeLucky(maxStack, upgrade, durability));
}

public class ItemListTests
{
    [Fact]
    public void Empty_list_excludes_nothing() =>
        Assert.Empty(CraftBonusCalculator.ParseItemList(""));

    [Fact]
    public void Parses_names_ignoring_spaces_and_case()
    {
        var items = CraftBonusCalculator.ParseItemList(" ArrowCarapace, bombooze ,,");
        Assert.Equal(2, items.Count);
        Assert.Contains("arrowcarapace", items);
        Assert.Contains("BombOoze", items);
    }
}

public class LuckOddsTextTests
{
    [Fact]
    public void Single_craft_shows_chance_and_bonus() =>
        Assert.Equal("\n\n<color=#E8BE60>Lucky craft: 18.5% chance, +10</color>", LuckOddsText.Format(0.185f, 10, 1, true));

    [Fact]
    public void Multi_craft_explains_stacking() =>
        Assert.Contains("Crafting x5: 5 rolls, each lucky roll gives +10 more than the last", LuckOddsText.Format(0.25f, 10, 5, true));

    [Fact]
    public void Multi_craft_without_stacking_only_shows_rolls() =>
        Assert.EndsWith("Crafting x5: 5 rolls</color>", LuckOddsText.Format(0.25f, 10, 5, false));
}

public class LuckyCraftMessageTests
{
    [Fact]
    public void Message_names_the_bonus_and_item() =>
        Assert.Equal("<color=#E8BE60>Lucky craft!</color> +10 Nails", LuckOddsText.LuckyCraft("+10", "Nails"));
}
