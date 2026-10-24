using BepInEx.Configuration;
using JoksterCube.FairCraftingLuck.Common;
using JoksterCube.FairCraftingLuck.Domain;

namespace JoksterCube.FairCraftingLuck.Settings;

internal static class Constants
{
    internal static class Plugin
    {
        internal const string ModName = "FairCraftingLuck";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "JoksterCube";
        internal const string ModGUID = $"{Author}.{ModName}";
        internal const string Description = "Lucky crafts give a whole extra batch: 10 extra nails instead of 1. Choose Fair, Level Based, Vanilla or no bonus; servers can enforce it.";
        internal const string Copyright = "Copyright ©  2026";
        internal const string Guid = "c4d2f7a9-3e18-4b6c-a5d0-8f71e2b94c36";

        internal const string ConfigFileName = $"{ModGUID}.cfg";
        internal const string VersionCheckRpc = $"{ModName}_VersionCheck";
    }

    internal static class DebugMessages
    {
        internal const string ReadConfigCalled = "ReadConfigCalled called";
        internal const string ErrorLoadingConfig = $"There was an issue loading your {Plugin.ConfigFileName}";
        internal const string RequestCheckConfig = "Please check your config entries for spelling and format!";
    }

    internal static class Groups
    {
        internal static class General
        {
            internal const string Group = "1 - General";

            internal static readonly ConfigInfo<CraftingLuckMode> Mode = new(
                Group,
                "Crafting Luck Mode",
                "How big a lucky craft is. " +
                "Fair: one extra batch of the recipe (10 nails give 10 extra nails, 1 mead base gives 1 extra). " +
                "LevelBased: a part of the batch that grows with skill level, rounded up (a 20 item recipe gives 1 extra at level 1-5 and 20 at level 100). " +
                "Vanilla: unchanged game behavior, +1 per lucky roll. " +
                "NoBonus: lucky crafts are turned off.",
                CraftingLuckMode.Fair,
                true);
        }

        internal static class Luck
        {
            internal const string Group = "2 - Luck";

            internal static readonly ConfigInfo<float> BonusChance = new(
                Group,
                "Bonus Chance",
                new ConfigDescription(
                    "Chance of a lucky craft at skill level 100, scaled down by your skill level (at level 50 it is half). The game uses 0.25. Not used in Vanilla mode.",
                    new AcceptableValueRange<float>(0f, 1f)),
                0.25f,
                true);

            internal static readonly ConfigInfo<Toggle> StackMultiCraftBonus = new(
                Group,
                "Stack Multi Craft Bonus",
                "Vanilla behavior when crafting several at once (Shift + Craft): every further lucky roll in the same craft gives one more bonus than the previous one (+1, +2, +3...), " +
                "so crafting 5 at once is luckier than crafting one by one. Turn off to make every lucky roll give the same bonus. Not used in Vanilla mode.",
                Toggle.On,
                true);

            internal static readonly ConfigInfo<string> ExcludedItems = new(
                Group,
                "Excluded Items",
                "Comma-separated item prefab names (for example: ArrowCarapace, BombOoze) whose lucky crafts stay fully vanilla (+1, game chance). Empty by default.",
                string.Empty,
                true);
        }

        internal static class Display
        {
            internal const string Group = "3 - Display";

            internal static readonly ConfigInfo<Toggle> ShowLuckOdds = new(
                Group,
                "Show Lucky Craft Odds",
                "Show your lucky craft chance and bonus under the recipe description in the crafting panel.",
                Toggle.On,
                false);

            internal static readonly ConfigInfo<Toggle> ShowLuckyCraftMessage = new(
                Group,
                "Show Lucky Craft Message",
                "Announce a lucky craft on screen, for example \"Lucky craft! +10 Nails\". If off, only the game's small +N popup shows.",
                Toggle.On,
                false);
        }
    }
}
