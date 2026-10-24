using System;
using System.Collections.Generic;
using JoksterCube.FairCraftingLuck.Common;
using JoksterCube.FairCraftingLuck.Settings;

namespace JoksterCube.FairCraftingLuck.Domain;

// Called from the patched InventoryGui.DoCrafting. Outside Vanilla mode the game's own bonus roll is
// switched off (its chance reads as 0) and this class adds the bonus instead, right before the game
// checks inventory space and adds the crafted items. Excluded items keep the game's own roll.
internal static class CraftBonus
{
    internal const string Feature = "Crafting luck";

    private static string? _excludedRaw;
    private static HashSet<string> _excluded = new(StringComparer.OrdinalIgnoreCase);

    // True when this mod, not the game, decides the lucky craft for the recipe being crafted.
    internal static bool IsActiveFor(Recipe? recipe) =>
        PluginConfig.Mode.Value != CraftingLuckMode.Vanilla &&
        !PatchGuard.IsDisabled(Feature) &&
        !IsExcluded(recipe);

    // Replaces InventoryGui.m_craftBonusChance in the game's bonus loop.
    internal static float GameBonusChance(InventoryGui gui) => IsActiveFor(gui.m_craftRecipe) ? 0f : gui.m_craftBonusChance;

    // amount: items the craft makes so far (recipe amount x crafts). bonus: extra items shown as "+N".
    internal static void Apply(InventoryGui gui, Player player, ref int amount, ref int bonus)
    {
        try
        {
            if (!IsActiveFor(gui.m_craftRecipe)) return;
            int extra = Calculate(gui, player, amount);
            if (extra <= 0) return;
            amount += extra;
            bonus += extra;
        }
        catch (Exception exception)
        {
            PatchGuard.Fail(Feature, exception);
        }
    }

    // Station with a crafting skill, and a new stackable item. Same rules as the game, plus an explicit
    // upgrade and durability check so gear can never become lucky.
    internal static bool CanBeLucky(Recipe? recipe, Player player, bool isUpgrade, out CraftingStation? station)
    {
        station = null;
        if (!recipe || !recipe!.m_item || !player) return false;
        station = player.GetCurrentCraftingStation();
        if (!station || station!.m_craftingSkill == Skills.SkillType.None) return false;
        ItemDrop.ItemData.SharedData shared = recipe.m_item.m_itemData.m_shared;
        return CraftBonusCalculator.CanBeLucky(shared.m_maxStackSize, isUpgrade, shared.m_useDurability);
    }

    internal static bool IsExcluded(Recipe? recipe)
    {
        if (!recipe || !recipe!.m_item) return false;
        string raw = PluginConfig.ExcludedItems.Value ?? string.Empty;
        if (!ReferenceEquals(raw, _excludedRaw))
        {
            _excluded = CraftBonusCalculator.ParseItemList(raw);
            _excludedRaw = raw;
        }
        return _excluded.Count > 0 && _excluded.Contains(recipe.m_item.gameObject.name);
    }

    private static int Calculate(InventoryGui gui, Player player, int amount)
    {
        Recipe recipe = gui.m_craftRecipe;
        bool isUpgrade = gui.m_craftUpgradeItem != null;
        if (!CanBeLucky(recipe, player, isUpgrade, out CraftingStation? station)) return 0;

        int crafts = gui.m_multiCrafting ? gui.m_multiCraftAmount : 1;
        if (crafts <= 0) return 0;
        int batchAmount = amount / crafts;
        float skillFactor = player.GetSkillFactor(station!.m_craftingSkill);

        int perRoll = CraftBonusCalculator.BonusPerLuckyRoll(PluginConfig.Mode.Value, batchAmount, skillFactor);
        int bonus = CraftBonusCalculator.TotalBonus(
            perRoll,
            crafts,
            skillFactor * PluginConfig.BonusChance.Value,
            PluginConfig.StackMultiCraftBonus.IsOn(),
            () => UnityEngine.Random.value);
        if (bonus <= 0) return 0;

        Inventory inventory = player.GetInventory();
        UnityEngine.GameObject prefab = recipe.m_item.gameObject;
        return CraftBonusCalculator.FitBonus(amount, bonus, total => inventory.CanAddItem(prefab, total));
    }
}
