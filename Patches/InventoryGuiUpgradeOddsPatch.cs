using System;
using HarmonyLib;
using JoksterCube.FairCraftingLuck.Common;
using JoksterCube.FairCraftingLuck.Domain;
using JoksterCube.FairCraftingLuck.Settings;

namespace JoksterCube.FairCraftingLuck.Patches;

// At an upgrader station (the ancient upgrade station), appends the chance that the selected item is upgraded,
// drops a level or is destroyed. Like the lucky craft odds, it is added after the game rebuilds the description each frame.
// Only the odds are shown, never the roll itself: the game picks the roll when the upgrade starts.
[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipe))]
internal static class InventoryGuiUpgradeOddsPatch
{
    private const string Feature = "Upgrade odds";

    private static void Postfix(InventoryGui __instance, Player player)
    {
        if (!PluginConfig.ShowUpgradeOdds.IsOn() || PatchGuard.IsDisabled(Feature)) return;
        try
        {
            string? odds = BuildOdds(__instance, player);
            if (odds != null) __instance.m_recipeDecription.text += odds;
        }
        catch (Exception exception)
        {
            PatchGuard.Fail(Feature, exception);
        }
    }

    private static string? BuildOdds(InventoryGui gui, Player player)
    {
        if (!player) return null;
        CraftingStation station = player.GetCurrentCraftingStation();
        if (!station || !station.m_upgrader) return null;

        Recipe? recipe = gui.m_selectedRecipe.Recipe;
        ItemDrop.ItemData? item = gui.m_selectedRecipe.ItemData;
        if (recipe == null || !recipe || item == null) return null;

        // The same upgrader item the game reads the chances from in DoCrafting.
        ItemDrop.ItemData.SharedData? upgrader = null;
        foreach (Piece.Requirement requirement in recipe.m_resources)
        {
            if (!requirement.m_upgraderResource || !requirement.m_resItem) continue;
            upgrader = requirement.m_resItem.m_itemData.m_shared;
            break;
        }
        if (upgrader == null) return null;

        UpgradeOdds odds = UpgradeOdds.From(upgrader.m_upgradeChance, upgrader.m_breakChance);
        return LuckOddsText.Upgrade(odds, item.m_quality, upgrader.m_breakReturnIngreientsAmount);
    }
}
