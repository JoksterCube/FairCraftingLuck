using HarmonyLib;
using JoksterCube.FairCraftingLuck.Domain;

namespace JoksterCube.FairCraftingLuck.Patches;

// Notes the game's own lucky craft chance as the crafting panel is made, so a chance raised later by another mod (a
// blessing, say) is shown as extra luck and also raises the Bonus Chance setting.
[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
internal static class InventoryGuiAwakePatch
{
    private static void Postfix(InventoryGui __instance) => CraftBonus.PanelMade(__instance);
}
