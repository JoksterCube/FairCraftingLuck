namespace JoksterCube.FairCraftingLuck.Domain;

// Outcome chances of an upgrade at an upgrader station (the ancient upgrade station), 0-1 each, adding up to 1.
// The game rolls one number r (0-1): r <= upgrade chance upgrades the item, otherwise r >= 1 - break chance
// destroys it, and anything else drops it a level.
internal readonly struct UpgradeOdds(float upgrade, float destroy, float downgrade)
{
    internal float Upgrade { get; } = upgrade;
    internal float Destroy { get; } = destroy;
    internal float Downgrade { get; } = downgrade;

    // upgradeChance and breakChance: the upgrader item's m_upgradeChance and m_breakChance.
    internal static UpgradeOdds From(float upgradeChance, float breakChance)
    {
        float upgrade = Clamp01(upgradeChance);
        float destroy = Clamp01(1f - System.Math.Max(upgrade, 1f - Clamp01(breakChance)));
        float downgrade = Clamp01(1f - upgrade - destroy);
        return new UpgradeOdds(upgrade, destroy, downgrade);
    }

    private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
}
