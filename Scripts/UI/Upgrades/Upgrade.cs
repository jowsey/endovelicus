using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class Upgrade
{
    public readonly string name;
    public readonly string description;
    public readonly Sprite icon;
    public readonly Action onRedeemEffect;
    public readonly Upgrade[] requirements;

    public Upgrade(string name, string description, Sprite icon, Action onRedeemEffect, Upgrade[] requirements)
    {
        this.name = name;
        this.description = description;
        this.icon = icon;
        this.onRedeemEffect = onRedeemEffect;
        this.requirements = requirements;
    }
}

public static class Upgrades
{
    public static readonly List<Upgrade> redeemedUpgrades = new List<Upgrade>();

    // TODO find a way to strongly type or auto-generate the upgrade stats

    #region SummonSpell

    private static readonly Upgrade spellSummon2 = new Upgrade(
        "Summon II",
        "Unlocks level 2 of the Summon spell, allowing you to summon an additional 2 units at once",
        Icons.ResolveIcon(Icons.SwordBrandish),
        () => SpellUI.instance.AddSpell<Summon>(2),
        null
    );

    private static readonly Upgrade spellSummon3 = new Upgrade(
        "Summon III",
        "Unlocks level 3 of the Summon spell, allowing you to summon an additional 4 units at once",
        Icons.ResolveIcon(Icons.SwordBrandish),
        () => SpellUI.instance.AddSpell<Summon>(3),
        new[] { spellSummon2 }
    );

    [UsedImplicitly]
    private static readonly Upgrade spellSummon4 = new Upgrade(
        "Summon IV",
        "Unlocks the final level of the Summon spell, allowing you to summon an additional 4 units at once",
        Icons.ResolveIcon(Icons.SwordBrandish),
        () => SpellUI.instance.AddSpell<Summon>(4),
        new[] { spellSummon3 }
    );

    #endregion

    #region BribeSpell

    private static readonly Upgrade spellBribe2 = new Upgrade(
        "Bribe II",
        "Unlocks level 2 of the Bribe spell, allowing you to trade 60 gold for 20 devotion",
        Icons.ResolveIcon(Icons.TwoCoins),
        () => SpellUI.instance.AddSpell<Bribe>(2),
        null
    );

    private static readonly Upgrade spellBribe3 = new Upgrade(
        "Bribe III",
        "Unlocks level 3 of the Bribe spell, allowing you to trade 85 gold for 30 devotion",
        Icons.ResolveIcon(Icons.TwoCoins),
        () => SpellUI.instance.AddSpell<Bribe>(3),
        new[] { spellBribe2 }
    );

    private static readonly Upgrade spellBribe4 = new Upgrade(
        "Bribe IV",
        "Unlocks level 4 of the Bribe spell, allowing you to trade 110 gold for 40 devotion",
        Icons.ResolveIcon(Icons.TwoCoins),
        () => SpellUI.instance.AddSpell<Bribe>(4),
        new[] { spellBribe3 }
    );

    [UsedImplicitly]
    private static readonly Upgrade spellBribe5 = new Upgrade(
        "Bribe V",
        "Unlocks the final level of the Bribe spell, allowing you to trade 120 gold for 50 devotion",
        Icons.ResolveIcon(Icons.TwoCoins),
        () => SpellUI.instance.AddSpell<Bribe>(5),
        new[] { spellBribe4 }
    );

    #endregion

    #region HealingRingSpell

    private static readonly Upgrade spellHealingRing1 = new Upgrade(
        "Healing Ring I",
        "Unlocks the Healing Ring spell, allowing you to heal units in a specified area",
        Icons.ResolveIcon(Icons.Healing),
        () => SpellUI.instance.AddSpell<HealingRing>(1),
        null
    );

    private static readonly Upgrade spellHealingRing2 = new Upgrade(
        "Healing Ring II",
        "Unlocks level 2 of the Healing Ring spell, increasing its duration and scale",
        Icons.ResolveIcon(Icons.Healing),
        () => SpellUI.instance.AddSpell<HealingRing>(2),
        new[] { spellHealingRing1 }
    );

    private static readonly Upgrade spellHealingRing3 = new Upgrade(
        "Healing Ring III",
        "Unlocks level 3 of the Healing Ring spell, increasing its duration, HP/s, and scale",
        Icons.ResolveIcon(Icons.Healing),
        () => SpellUI.instance.AddSpell<HealingRing>(3),
        new[] { spellHealingRing2 }
    );

    private static readonly Upgrade spellHealingRing4 = new Upgrade(
        "Healing Ring IV",
        "Unlocks level 4 of the Healing Ring spell, increasing its duration and scale",
        Icons.ResolveIcon(Icons.Healing),
        () => SpellUI.instance.AddSpell<HealingRing>(4),
        new[] { spellHealingRing3 }
    );

    [UsedImplicitly]
    private static readonly Upgrade spellHealingRing5 = new Upgrade(
        "Healing Ring V",
        "Unlocks the final level of the Healing Ring spell, increasing its duration, HP/s, and scale",
        Icons.ResolveIcon(Icons.Healing),
        () => SpellUI.instance.AddSpell<HealingRing>(5),
        new[] { spellHealingRing4 }
    );

    #endregion

    #region PoisonRingSpell

    private static readonly Upgrade spellPoisonRing1 = new Upgrade(
        "Poison Ring I",
        "Unlocks the Poison Ring spell, allowing you to damage units in a specified area",
        Icons.ResolveIcon(Icons.Bubbles),
        () => SpellUI.instance.AddSpell<PoisonRing>(1),
        null
    );

    private static readonly Upgrade spellPoisonRing2 = new Upgrade(
        "Poison Ring II",
        "Unlocks level 2 of the Poison Ring spell, increasing its duration, DPS, and scale",
        Icons.ResolveIcon(Icons.Bubbles),
        () => SpellUI.instance.AddSpell<PoisonRing>(2),
        new[] { spellPoisonRing1 }
    );

    private static readonly Upgrade spellPoisonRing3 = new Upgrade(
        "Poison Ring III",
        "Unlocks level 3 of the Poison Ring spell, increasing its duration, DPS, and scale",
        Icons.ResolveIcon(Icons.Bubbles),
        () => SpellUI.instance.AddSpell<PoisonRing>(3),
        new[] { spellPoisonRing2 }
    );

    private static readonly Upgrade spellPoisonRing4 = new Upgrade(
        "Poison Ring IV",
        "Unlocks level 4 of the Poison Ring spell, increasing its duration, DPS, and scale",
        Icons.ResolveIcon(Icons.Bubbles),
        () => SpellUI.instance.AddSpell<PoisonRing>(4),
        new[] { spellPoisonRing3 }
    );

    [UsedImplicitly]
    private static readonly Upgrade spellPoisonRing5 = new Upgrade(
        "Poison Ring V",
        "Unlocks the final level of the Poison Ring spell, increasing its duration, DPS, and scale",
        Icons.ResolveIcon(Icons.Bubbles),
        () => SpellUI.instance.AddSpell<PoisonRing>(5),
        new[] { spellPoisonRing4 }
    );

    #endregion

    #region MagesGuildSpell

    private static readonly Upgrade spellMagesGuild1 = new Upgrade(
        "Mage's Guild I",
        "Unlocks the Mage's Guild spell, allowing you to summon groups of powerful mages",
        Icons.ResolveIcon(Icons.WizardStaff),
        () => SpellUI.instance.AddSpell<MagesGuild>(1),
        null
    );

    private static readonly Upgrade spellMagesGuild2 = new Upgrade(
        "Mage's Guild II",
        "Unlocks level 2 of the Mage's Guild spell, allowing you to summon an additional mage",
        Icons.ResolveIcon(Icons.WizardStaff),
        () => SpellUI.instance.AddSpell<MagesGuild>(2),
        new[] { spellMagesGuild1 }
    );

    [UsedImplicitly]
    private static readonly Upgrade spellMagesGuild3 = new Upgrade(
        "Mage's Guild III",
        "Unlocks the final level of the Mage's Guild spell, allowing you to summon an additional 2 mages",
        Icons.ResolveIcon(Icons.WizardStaff),
        () => SpellUI.instance.AddSpell<MagesGuild>(3),
        new[] { spellMagesGuild2 }
    );

    #endregion

    #region MeleeDamage

    private static readonly Upgrade meleeDamage1 = new Upgrade(
        "Sharpened Blades I",
        "Increase melee damage by 10%",
        Icons.ResolveIcon(Icons.SwordBreak),
        () => UnitStats.modifiers[UnitClass.Melee][UnitStats.UnitStat.AttackDamage] += 0.10f,
        null
    );

    private static readonly Upgrade meleeDamage2 = new Upgrade(
        "Sharpened Blades II",
        "Increase melee damage by an additional 15%",
        Icons.ResolveIcon(Icons.SwordBreak),
        () => UnitStats.modifiers[UnitClass.Melee][UnitStats.UnitStat.AttackDamage] += 0.15f,
        new[] { meleeDamage1 }
    );

    [UsedImplicitly]
    private static readonly Upgrade meleeDamage3 = new Upgrade(
        "Sharpened Blades III",
        "Increase melee damage by an additional 20%",
        Icons.ResolveIcon(Icons.SwordBreak),
        () => UnitStats.modifiers[UnitClass.Melee][UnitStats.UnitStat.AttackDamage] += 0.20f,
        new[] { meleeDamage2 }
    );

    #endregion

    #region ArcherDamage

    private static readonly Upgrade archerDamage1 = new Upgrade(
        "Precision Fletching I",
        "Increase archer damage by 10%",
        Icons.ResolveIcon(Icons.WingedArrow),
        () => UnitStats.modifiers[UnitClass.Archer][UnitStats.UnitStat.AttackDamage] += 0.10f,
        null
    );

    private static readonly Upgrade archerDamage2 = new Upgrade(
        "Precision Fletching II",
        "Increase archer damage by an additional 15%",
        Icons.ResolveIcon(Icons.WingedArrow),
        () => UnitStats.modifiers[UnitClass.Archer][UnitStats.UnitStat.AttackDamage] += 0.15f,
        new[] { archerDamage1 }
    );

    [UsedImplicitly]
    private static readonly Upgrade archerDamage3 = new Upgrade(
        "Precision Fletching III",
        "Increase archer damage by an additional 20%",
        Icons.ResolveIcon(Icons.WingedArrow),
        () => UnitStats.modifiers[UnitClass.Archer][UnitStats.UnitStat.AttackDamage] += 0.20f,
        new[] { archerDamage2 }
    );

    #endregion

    #region MageDamage

    private static readonly Upgrade mageDamage1 = new Upgrade(
        "Infused Wands I",
        "Increase mage damage by 10%",
        Icons.ResolveIcon(Icons.CrystalWand),
        () => UnitStats.modifiers[UnitClass.Mage][UnitStats.UnitStat.AttackDamage] += 0.10f,
        new[] { spellMagesGuild1 }
    );

    private static readonly Upgrade mageDamage2 = new Upgrade(
        "Infused Wands II",
        "Increase mage damage by an additional 10%",
        Icons.ResolveIcon(Icons.CrystalWand),
        () => UnitStats.modifiers[UnitClass.Mage][UnitStats.UnitStat.AttackDamage] += 0.10f,
        new[] { mageDamage1 }
    );

    [UsedImplicitly]
    private static readonly Upgrade mageDamage3 = new Upgrade(
        "Infused Wands III",
        "Increase mage damage by an additional 10%",
        Icons.ResolveIcon(Icons.CrystalWand),
        () => UnitStats.modifiers[UnitClass.Mage][UnitStats.UnitStat.AttackDamage] += 0.10f,
        new[] { mageDamage2 }
    );

    #endregion

    #region MeleeHealth

    private static readonly Upgrade meleeHealth1 = new Upgrade(
        "Improvised Shields I",
        "Increase melee max health by 10%",
        Icons.ResolveIcon(Icons.RoundShield),
        () => UnitStats.modifiers[UnitClass.Melee][UnitStats.UnitStat.MaxHealth] += 0.10f,
        null
    );

    private static readonly Upgrade meleeHealth2 = new Upgrade(
        "Improvised Shields II",
        "Increase melee max health by an additional 15%",
        Icons.ResolveIcon(Icons.RoundShield),
        () => UnitStats.modifiers[UnitClass.Melee][UnitStats.UnitStat.MaxHealth] += 0.15f,
        new[] { meleeHealth1 }
    );

    [UsedImplicitly]
    private static readonly Upgrade meleeHealth3 = new Upgrade(
        "Improvised Shields III",
        "Increase melee max health by an additional 20%",
        Icons.ResolveIcon(Icons.RoundShield),
        () => UnitStats.modifiers[UnitClass.Melee][UnitStats.UnitStat.MaxHealth] += 0.20f,
        new[] { meleeHealth2 }
    );

    #endregion

    #region ArcherHealth

    private static readonly Upgrade archerHealth1 = new Upgrade(
        "Leather Gloves I",
        "Increase archer max health by 10%",
        Icons.ResolveIcon(Icons.Gloves),
        () => UnitStats.modifiers[UnitClass.Archer][UnitStats.UnitStat.MaxHealth] += 0.10f,
        null
    );

    private static readonly Upgrade archerHealth2 = new Upgrade(
        "Leather Gloves II",
        "Increase archer max health by an additional 15%",
        Icons.ResolveIcon(Icons.Gloves),
        () => UnitStats.modifiers[UnitClass.Archer][UnitStats.UnitStat.MaxHealth] += 0.15f,
        new[] { archerHealth1 }
    );

    [UsedImplicitly]
    private static readonly Upgrade archerHealth3 = new Upgrade(
        "Leather Gloves III",
        "Increase archer max health by an additional 20%",
        Icons.ResolveIcon(Icons.Gloves),
        () => UnitStats.modifiers[UnitClass.Archer][UnitStats.UnitStat.MaxHealth] += 0.20f,
        new[] { archerHealth2 }
    );

    #endregion

    #region MageHealth

    private static readonly Upgrade mageHealth1 = new Upgrade(
        "Layered Robes I",
        "Increase mage max health by 10%",
        Icons.ResolveIcon(Icons.Robe),
        () => UnitStats.modifiers[UnitClass.Mage][UnitStats.UnitStat.MaxHealth] += 0.10f,
        new[] { spellMagesGuild1 }
    );

    private static readonly Upgrade mageHealth2 = new Upgrade(
        "Layered Robes II",
        "Increase mage max health by an additional 10%",
        Icons.ResolveIcon(Icons.Robe),
        () => UnitStats.modifiers[UnitClass.Mage][UnitStats.UnitStat.MaxHealth] += 0.10f,
        new[] { mageHealth1 }
    );

    [UsedImplicitly]
    private static readonly Upgrade mageHealth3 = new Upgrade(
        "Layered Robes III",
        "Increase mage max health by an additional 10%",
        Icons.ResolveIcon(Icons.Robe),
        () => UnitStats.modifiers[UnitClass.Mage][UnitStats.UnitStat.MaxHealth] += 0.10f,
        new[] { mageHealth2 }
    );

    #endregion

    private static readonly Upgrade[] allUpgrades = typeof(Upgrades)
        .GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
        .Where(field => field.FieldType == typeof(Upgrade))
        .Select(f => (Upgrade)f.GetValue(null))
        .ToArray();

    private static bool RequirementsMet(Upgrade upgrade) => upgrade.requirements == null || upgrade.requirements.All(r => redeemedUpgrades.Contains(r));
    public static IEnumerable<Upgrade> AvailableUpgrades => allUpgrades.Where(u => RequirementsMet(u) && !redeemedUpgrades.Contains(u));

    public static void RedeemUpgrade(Upgrade upgrade)
    {
        redeemedUpgrades.Add(upgrade);
        upgrade.onRedeemEffect();
    }
}
