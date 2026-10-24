using System;
using HarmonyLib;
using JoksterCube.FairCraftingLuck.Common;
using JoksterCube.FairCraftingLuck.Domain;
using JoksterCube.FairCraftingLuck.Settings;
using UnityEngine;
using static MessageHud;

namespace JoksterCube.FairCraftingLuck.Patches;

// DoCrafting shows a "+N" bonus popup only after a lucky craft was added to the inventory.
// While DoCrafting runs, that popup is caught here and announced as a message too.
[HarmonyPatch]
internal static class LuckyCraftMessagePatch
{
    private const string Feature = "Lucky craft message";

    private static InventoryGui? _crafting;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    [HarmonyPrefix]
    private static void BeginCrafting(InventoryGui __instance) => _crafting = __instance;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    [HarmonyFinalizer]
    private static void EndCrafting() => _crafting = null;

    [HarmonyPatch(typeof(DamageText), nameof(DamageText.ShowText), [typeof(DamageText.TextType), typeof(Vector3), typeof(string), typeof(bool)])]
    [HarmonyPostfix]
    private static void AnnounceBonus(DamageText.TextType type, string text)
    {
        if (type != DamageText.TextType.Bonus || _crafting == null) return;
        if (!PluginConfig.ShowLuckyCraftMessage.IsOn() || PatchGuard.IsDisabled(Feature)) return;
        try
        {
            Recipe recipe = _crafting.m_craftRecipe;
            if (!recipe || !recipe.m_item) return;
            ItemDrop.ItemData item = recipe.m_item.m_itemData;
            string name = Localization.instance.Localize(item.m_shared.m_name);
            PlayerExtensions.Message(
                MessageType.Center,
                LuckOddsText.LuckyCraft(text, name),
                item.GetIcon());
        }
        catch (Exception exception)
        {
            PatchGuard.Fail(Feature, exception);
        }
    }
}
