using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JoksterCube.FairCraftingLuck.Domain;

namespace JoksterCube.FairCraftingLuck.Patches;

// The game's bonus loop in DoCrafting, roughly:
//   for (i < crafts) if (Random.value < skillFactor * m_craftBonusChance) { bonus += m_craftBonusAmount; amount += bonus; }
//   if (m_craftUpgradeItem == null && !inventory.CanAddItem(item, amount)) return;
// The patch reads the chance through CraftBonus.GameBonusChance and calls CraftBonus.Apply right after the loop.
// If the game code changed and the pattern is not found, nothing is patched and crafting stays vanilla.
[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
internal static class InventoryGuiDoCraftingPatch
{
    private static readonly FieldInfo BonusChanceField = AccessTools.Field(typeof(InventoryGui), nameof(InventoryGui.m_craftBonusChance));
    private static readonly FieldInfo BonusAmountField = AccessTools.Field(typeof(InventoryGui), nameof(InventoryGui.m_craftBonusAmount));
    private static readonly FieldInfo UpgradeItemField = AccessTools.Field(typeof(InventoryGui), nameof(InventoryGui.m_craftUpgradeItem));
    private static readonly MethodInfo CanAddItemMethod = AccessTools.Method(typeof(Inventory), nameof(Inventory.CanAddItem), [typeof(UnityEngine.GameObject), typeof(int)]);
    private static readonly MethodInfo GameBonusChanceMethod = AccessTools.Method(typeof(CraftBonus), nameof(CraftBonus.GameBonusChance));
    private static readonly MethodInfo ApplyMethod = AccessTools.Method(typeof(CraftBonus), nameof(CraftBonus.Apply));

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = instructions.ToList();

        int chanceIndex = code.FindIndex(i => i.LoadsField(BonusChanceField));
        // bonus += m_craftBonusAmount  ->  ldloc bonus; ldarg.0; ldfld m_craftBonusAmount; add; stloc bonus
        int bonusAmountIndex = code.FindIndex(i => i.LoadsField(BonusAmountField));
        int canAddIndex = code.FindIndex(i => i.Calls(CanAddItemMethod));
        if (chanceIndex < 0 || bonusAmountIndex < 2 || canAddIndex < 0 || bonusAmountIndex + 6 >= code.Count)
            return Fail(code, "bonus loop not found");

        CodeInstruction bonusStore = code[bonusAmountIndex + 2];
        // amount += bonus  ->  ldloc amount; ldloc bonus; add; stloc amount
        CodeInstruction amountStore = code[bonusAmountIndex + 6];
        if (!bonusStore.IsStloc() || !amountStore.IsStloc() || code[bonusAmountIndex + 1].opcode != OpCodes.Add)
            return Fail(code, "bonus locals not found");

        // The "if (m_craftUpgradeItem == null && !CanAddItem(...))" check starts right after the loop.
        int afterLoopIndex = code.FindLastIndex(canAddIndex, i => i.LoadsField(UpgradeItemField)) - 1;
        if (afterLoopIndex < 0 || code[afterLoopIndex].opcode != OpCodes.Ldarg_0)
            return Fail(code, "inventory check not found");

        CodeInstruction afterLoop = code[afterLoopIndex];
        CodeInstruction[] applyCall =
        [
            new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(afterLoop),
            new CodeInstruction(OpCodes.Ldarg_1),
            LoadLocalAddress(amountStore),
            LoadLocalAddress(bonusStore),
            new CodeInstruction(OpCodes.Call, ApplyMethod)
        ];
        code.InsertRange(afterLoopIndex, applyCall);

        // ldarg.0; ldfld m_craftBonusChance  ->  ldarg.0; call GameBonusChance(gui)
        code[chanceIndex] = new CodeInstruction(OpCodes.Call, GameBonusChanceMethod).MoveLabelsFrom(code[chanceIndex]);

        LogOnce(() => Plugin.ModLogger.LogInfo("Patched InventoryGui.DoCrafting: crafting luck is ready."));
        return code;
    }

    // Harmony reruns transpilers each time another patch is added to the same method.
    private static bool _logged;

    private static void LogOnce(System.Action log)
    {
        if (_logged) return;
        _logged = true;
        log();
    }

    private static CodeInstruction LoadLocalAddress(CodeInstruction store)
    {
        if (store.operand is LocalBuilder local) return new CodeInstruction(OpCodes.Ldloca, local);
        int index = StoredLocalIndex(store);
        return index < 256
            ? new CodeInstruction(OpCodes.Ldloca_S, (byte)index)
            : new CodeInstruction(OpCodes.Ldloca, (short)index);
    }

    private static int StoredLocalIndex(CodeInstruction store)
    {
        if (store.opcode == OpCodes.Stloc_0) return 0;
        if (store.opcode == OpCodes.Stloc_1) return 1;
        if (store.opcode == OpCodes.Stloc_2) return 2;
        if (store.opcode == OpCodes.Stloc_3) return 3;
        return System.Convert.ToInt32(store.operand);
    }

    private static IEnumerable<CodeInstruction> Fail(List<CodeInstruction> code, string reason)
    {
        LogOnce(() => Plugin.ModLogger.LogError($"Could not patch crafting ({reason}). Crafting luck stays vanilla. Please report it with the game version."));
        return code;
    }
}
