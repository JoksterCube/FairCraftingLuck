# FairCraftingLuck

## A lucky craft should feel lucky, whatever you craft!

In vanilla Valheim, a high crafting skill sometimes gives you a bonus item. Craft one mead base and get lucky: you get **2**, a whole extra craft. Craft 10 nails and get lucky: you get **11**, just one nail more.

FairCraftingLuck makes a lucky craft give a **whole extra batch**: 10 nails become **20**, 20 arrows become **40**, the same way a single item doubles.

---

### 📸 Screenshots

**See your odds before you craft.** The crafting panel shows your chance and bonus, and what changes when crafting 5 at once (hold Shift).

<p align="center">
	<img src="https://raw.githubusercontent.com/JoksterCube/FairCraftingLuck/refs/heads/main/Screenshots/Odds.png" alt="Crafting panel showing a 25% lucky craft chance for +20 wood arrows" width="320">
	<img src="https://raw.githubusercontent.com/JoksterCube/FairCraftingLuck/refs/heads/main/Screenshots/OddsMultiCraft.png" alt="Crafting panel for 5 crafts at once, where each lucky roll gives 20 more arrows than the last" width="320">
</p>

**Cooking too.** At the cauldron, your Cooking skill drives the chance, and recipes that make several give a whole extra batch.

<p align="center">
	<img src="https://raw.githubusercontent.com/JoksterCube/FairCraftingLuck/refs/heads/main/Screenshots/Cooking.png" alt="Cauldron recipe for Queen's Jam x4 with a lucky craft bonus of +4" width="320">
</p>

---

### 🎲 Modes

| Mode | Lucky craft of 10 nails | Notes |
| --- | --- | --- |
| **Fair** (default) | +10 | One extra batch of the recipe. |
| **LevelBased** | +1 to +10 | Part of the batch that grows with skill, rounded up. A 20 item recipe gives +1 at level 1-5 and +20 at level 100. |
| **Vanilla** | +1 | Unchanged game behavior, the mod stays out of crafting. |
| **NoBonus** | +0 | Lucky crafts are turned off. |

By default the chance of a lucky craft is the game's own: up to 25% per craft at skill level 100, lower at lower levels. If a game update changes it, the mod follows. With default settings, single-item recipes (like mead bases) behave like vanilla in every mode except NoBonus.

---

### ⚙️ Settings

| Setting | Default | What it does |
| --- | --- | --- |
| **Crafting Luck Mode** | Fair | Fair, LevelBased, Vanilla or NoBonus. |
| **Use Vanilla Luck** | On | Use the game's own chance and multi craft stacking, so the mode only changes the size of a lucky craft. Follows the game, and other mods, if they change the chance. Overrides the two settings below; turn it off to set your own. |
| **Bonus Chance** | 0.25 | Chance of a lucky craft at skill level 100, scaled down by skill level. Not used while Use Vanilla Luck is on. |
| **Stack Multi Craft Bonus** | On | Vanilla behavior for Shift + Craft (5 at once): each further lucky roll in the same craft gives one more bonus than the last (+1, +2, +3 batches). This is why crafting 5 at once is luckier than one by one. Turn off to make every lucky roll equal. Not used while Use Vanilla Luck is on. |
| **Excluded Items** | *(empty)* | Comma-separated item prefab names (for example `ArrowCarapace, BombOoze`) that act fully vanilla. These are the item IDs used by the `spawn` console command. |
| **Show Lucky Craft Odds** | On | Shows your chance and bonus under the recipe description. Your own choice, not synced. |
| **Show Lucky Craft Message** | On | Announces a lucky craft in the center of the screen, like "Lucky craft! +10 Nails". Your own choice, not synced. |

Use Vanilla Luck, Bonus Chance and Stack Multi Craft Bonus do not apply in Vanilla mode or to excluded items, which act fully vanilla.

Like in vanilla, only stackable items can be lucky. Weapons, armor, tools and upgrades never get a lucky craft, in any mode.

A lucky craft never blocks a craft: if your inventory can't hold the whole bonus, you get as much as fits. (In Vanilla mode and for excluded items, the game's own rule applies.)

---

### 🔍 Lucky craft odds

Select a recipe at a crafting station to see your odds under its description, for example:

> Lucky craft: 18.5% chance, +10
> Crafting x5: 5 rolls, each lucky roll gives +10 more than the last

Hold Shift to see the odds for crafting several at once.

---

### 🌐 Multiplayer

- Crafting happens on your own game, so the mod works for each player who has it installed.
- **Server with the mod:** its settings are synced to players who have the mod and can't be changed in game, not even by admins. Change them in the server's config file. Players without the mod can still join and craft as vanilla.
- **Server without the mod:** players with the mod use their own settings.
- When both the server and a player have the mod, they must have the **same version**.

---

### 📦 Installation

1. Install [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/).
2. Put `FairCraftingLuck.dll` in `BepInEx/plugins`.
3. Optional: install it on the server to enforce the settings.

Settings are in `BepInEx/config/JoksterCube.FairCraftingLuck.cfg`, or in-game with Configuration Manager (F1).

---

### 💬 Feedback & Support

Report bugs or suggest improvements on [GitHub](https://github.com/JoksterCube/FairCraftingLuck/issues).

---

**Author:** JoksterCube<br>
**Version:** 1.1.0<br>
**License:** MIT
