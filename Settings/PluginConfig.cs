using BepInEx.Configuration;
using JoksterCube.FairCraftingLuck.Common;
using JoksterCube.FairCraftingLuck.Domain;
using ServerSync;
using static JoksterCube.FairCraftingLuck.Settings.Constants.Groups;

namespace JoksterCube.FairCraftingLuck.Settings;

internal static class PluginConfig
{
    internal static ConfigEntry<CraftingLuckMode> Mode = null!;
    internal static ConfigEntry<Toggle> UseVanillaLuck = null!;
    internal static ConfigEntry<float> BonusChance = null!;
    internal static ConfigEntry<Toggle> StackMultiCraftBonus = null!;
    internal static ConfigEntry<string> ExcludedItems = null!;

    internal static ConfigEntry<Toggle> ShowLuckOdds = null!;
    internal static ConfigEntry<Toggle> ShowLuckyCraftMessage = null!;

    internal static void Build(ConfigFile config, ConfigSync configSync)
    {
        ConfigOptions.Initialize(config, configSync);

        Mode = ConfigOptions.Config(General.Mode);

        UseVanillaLuck = ConfigOptions.Config(Luck.UseVanillaLuck);
        BonusChance = ConfigOptions.Config(Luck.BonusChance);
        StackMultiCraftBonus = ConfigOptions.Config(Luck.StackMultiCraftBonus);
        ExcludedItems = ConfigOptions.Config(Luck.ExcludedItems);

        ShowLuckOdds = ConfigOptions.Config(Display.ShowLuckOdds);
        ShowLuckyCraftMessage = ConfigOptions.Config(Display.ShowLuckyCraftMessage);
    }
}
