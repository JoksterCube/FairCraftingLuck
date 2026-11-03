using System;
using System.Globalization;
using System.Text;

namespace JoksterCube.FairCraftingLuck.Domain;

internal static class LuckOddsText
{
    private const string Color = "#E8BE60";

    // bonusText is the game's popup text, such as "+10".
    internal static string LuckyCraft(string bonusText, string itemName) =>
        $"<color={Color}>Lucky craft!</color> {bonusText.Trim()} {itemName}";

    // chance: 0-1 per roll. rolls: crafts in this action (5 when holding Shift).
    // luck: how many times other mods raised the chance (a blessing, say), shown when it isn't 1.
    internal static string Format(float chance, int perRoll, int rolls, bool stack, float luck = 1f)
    {
        string percent = (chance * 100f).ToString("0.#", CultureInfo.InvariantCulture);
        StringBuilder text = new();
        text.Append("\n\n<color=").Append(Color).Append(">Lucky craft: ").Append(percent).Append("% chance");
        if (Math.Abs(luck - 1f) >= 0.05f) text.Append(" (").Append(luck.ToString("0.#", CultureInfo.InvariantCulture)).Append("x luck)");
        text.Append(", +").Append(perRoll);
        if (rolls > 1)
        {
            text.Append("\nCrafting x").Append(rolls).Append(": ").Append(rolls).Append(" rolls");
            if (stack) text.Append(", each lucky roll gives +").Append(perRoll).Append(" more than the last");
        }
        return text.Append("</color>").ToString();
    }

    // level: the item's level now. returned: share of the materials given back when the item is destroyed (0-1).
    internal static string Upgrade(UpgradeOdds odds, int level, float returned)
    {
        StringBuilder text = new();
        text.Append("\n\n<color=").Append(Color).Append(">Upgrade odds:");
        if (odds.Upgrade > 0f) text.Append('\n').Append(Percent(odds.Upgrade)).Append(" upgraded to level ").Append(level + 1);
        if (odds.Downgrade > 0f) text.Append('\n').Append(Percent(odds.Downgrade)).Append(" drops to level ").Append(level - 1);
        if (odds.Destroy > 0f)
        {
            text.Append("\n<color=").Append(DestroyColor).Append('>').Append(Percent(odds.Destroy)).Append(" destroyed");
            if (returned > 0f) text.Append(" (").Append(Percent(returned)).Append(" of materials back)");
            text.Append("</color>");
        }
        return text.Append("</color>").ToString();
    }

    private const string DestroyColor = "#E06C5C";

    private static string Percent(float chance) => (chance * 100f).ToString("0.#", CultureInfo.InvariantCulture) + "%";
}
