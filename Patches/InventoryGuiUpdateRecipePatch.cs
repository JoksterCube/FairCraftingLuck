using System;
using HarmonyLib;
using JoksterCube.FairCraftingLuck.Common;
using JoksterCube.FairCraftingLuck.Domain;
using JoksterCube.FairCraftingLuck.Settings;

namespace JoksterCube.FairCraftingLuck.Patches;

// The game rebuilds the recipe description every frame in UpdateRecipe, so the odds line is appended
// after it each time and never piles up.
[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipe))]
internal static class InventoryGuiUpdateRecipePatch
{
    private const string Feature = "Lucky craft odds";

    private static void Postfix(InventoryGui __instance, Player player)
    {
        if (!PluginConfig.ShowLuckOdds.IsOn() || PatchGuard.IsDisabled(Feature)) return;
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
        Recipe? recipe = gui.m_selectedRecipe.Recipe;
        if (recipe == null || !recipe) return null;
        bool isUpgrade = gui.m_selectedRecipe.ItemData != null;
        if (!CraftBonus.CanBeLucky(recipe, player, isUpgrade, out CraftingStation? station)) return null;

        // Same Shift check the game uses for the craft button.
        bool multi = ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyLStick") || gui.m_touchMultiCrafting;
        int rolls = multi ? gui.m_multiCraftAmount : 1;
        float skillFactor = player.GetSkillFactor(station!.m_craftingSkill);

        if (!CraftBonus.IsActiveFor(recipe))
            return LuckOddsText.Format(skillFactor * gui.m_craftBonusChance, gui.m_craftBonusAmount, rolls, true);

        CraftingLuckMode mode = PluginConfig.Mode.Value;
        if (mode == CraftingLuckMode.NoBonus) return null;
        int perRoll = CraftBonusCalculator.BonusPerLuckyRoll(mode, recipe.m_amount, skillFactor);
        return LuckOddsText.Format(
            skillFactor * CraftBonus.LuckChance(gui),
            perRoll,
            rolls,
            CraftBonus.StackLuckyRolls());
    }
}
