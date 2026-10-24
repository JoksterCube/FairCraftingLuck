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
    internal static string Format(float chance, int perRoll, int rolls, bool stack)
    {
        string percent = (chance * 100f).ToString("0.#", CultureInfo.InvariantCulture);
        StringBuilder text = new();
        text.Append("\n\n<color=").Append(Color).Append(">Lucky craft: ").Append(percent).Append("% chance, +").Append(perRoll);
        if (rolls > 1)
        {
            text.Append("\nCrafting x").Append(rolls).Append(": ").Append(rolls).Append(" rolls");
            if (stack) text.Append(", each lucky roll gives +").Append(perRoll).Append(" more than the last");
        }
        return text.Append("</color>").ToString();
    }
}
