using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Task : MonoBehaviour
{
    public static float CalculatePureDamage(Player attacker, Player defender)
    {
        return attacker.AttackPower + attacker.WeaponStrength;
    }

    public static float CalculateBaseDamage(Player attacker, Player defender)
    {
        float baseDamage = attacker.AttackPower + attacker.WeaponStrength;

        float totalBuffMultiplier = 1f;
        foreach (var buff in attacker.ActiveBuffs)
            totalBuffMultiplier *= buff.DamageMultiplier;

        float totalDebuffMultiplier = 1f;
        foreach (var debuff in attacker.ActiveDebuffs)
            totalDebuffMultiplier *= debuff.DamageMultiplier;

        return baseDamage * totalBuffMultiplier * totalDebuffMultiplier;
    }

    public static float CalculateEnvironmentalDamage(Player attacker, Player defender, Environment environment)
    {
        float attackPower = attacker.AttackPower;
        float weaponStrength = attacker.WeaponStrength;

        float buffMultiplier = 1f;
        float debuffMultiplier = 1f;

        switch (environment)
        {
            case Environment.Forest:
                weaponStrength *= 0.82f;
                goto default;

            case Environment.Hills:
                foreach (var buff in attacker.ActiveBuffs)
                    buffMultiplier *= (buff.DamageMultiplier * 2f);
                break;

            default:
                foreach (var buff in attacker.ActiveBuffs)
                    buffMultiplier *= buff.DamageMultiplier;
                foreach (var debuff in attacker.ActiveDebuffs)
                    debuffMultiplier *= debuff.DamageMultiplier;
                break;
        }

        float damage = (attackPower + weaponStrength) * buffMultiplier * debuffMultiplier;

        if (environment == Environment.Desert)
            damage += 20f;

        return damage;
    }

    public static float CalculateFinalDamage(Player attacker, Player defender, Environment environment, DamageType originalDamageType)
    {
        float attackPower = attacker.AttackPower;
        float weaponStrength = attacker.WeaponStrength;

        float buffMultiplier = 1f;
        float debuffMultiplier = 1f;

        if (environment == Environment.Hills)
        {
            foreach (var buff in attacker.ActiveBuffs)
                buffMultiplier *= (buff.DamageMultiplier * 2f);
        }
        else
        {
            foreach (var buff in attacker.ActiveBuffs)
                buffMultiplier *= buff.DamageMultiplier;

            foreach (var debuff in attacker.ActiveDebuffs)
                debuffMultiplier *= debuff.DamageMultiplier;
        }

        if (environment == Environment.Forest)
            weaponStrength *= 0.82f;

        float totalDamage = (attackPower + weaponStrength) * buffMultiplier * debuffMultiplier;

        float flatBonus = 0f;

        if (environment == Environment.Desert)
        {
            flatBonus += 20f;
            float firePart = totalDamage * 0.15f;
            float originalPart = totalDamage * 0.85f;

            totalDamage = ApplyResistance(originalPart, defender, originalDamageType, environment);
            totalDamage += ApplyResistance(firePart, defender, DamageType.Fire, environment);
        }
        else if (environment == Environment.Mountains && originalDamageType == DamageType.Ice)
        {
            totalDamage = 0f;
        }
        else
        {
            totalDamage = ApplyResistance(totalDamage, defender, originalDamageType, environment);
        }

        return totalDamage + flatBonus;
    }

    private static float ApplyResistance(float damage, Player defender, DamageType damageType, Environment environment)
    {
        float resistance = 0f;
        defender.Resistances.TryGetValue(damageType, out resistance);

        if (environment == Environment.Mountains)
        {
            resistance += 50f;
        }

        if (environment == Environment.Desert && damageType == DamageType.Physical)
        {
            resistance -= 12f;
            if (resistance < 0f) resistance = 0f;
        }

        if (environment == Environment.Forest && damageType == DamageType.Fire)
        {
            resistance = 0f;
        }

        float mitigation = resistance / (resistance + 100f);
        return damage * (1f - mitigation);
    }
    [Button]
    // Optional test method
    public static void Test()
    {
        var attacker = new Player
        {
            AttackPower = 121,
            WeaponStrength = 54
        };

        attacker.ActiveBuffs.Add(new StatusEffect("Determined", 1.4f));
        attacker.ActiveDebuffs.Add(new StatusEffect("Exhausted", 0.85f));
        attacker.ActiveDebuffs.Add(new StatusEffect("Dizzy", 0.7f));

        var defender = new Player();
        defender.Resistances.Add(DamageType.Fire, 56f);
        defender.Resistances.Add(DamageType.Ice, 78f);

        Debug.Log("Final Damage (Desert, Ice): " + CalculateFinalDamage(attacker, defender, Environment.Desert, DamageType.Ice));
        Debug.Log("Environmental Damage (Desert): " + CalculateEnvironmentalDamage(attacker, defender, Environment.Desert));
        Debug.Log("Base Damage: " + CalculateBaseDamage(attacker, defender));
        Debug.Log("Pure Damage: " + CalculatePureDamage(attacker, defender));
    }
}

// === Required Types ===

public enum DamageType { Physical, Fire, Ice, Poison }
public enum Environment { Desert, Mountains, Forest, Hills }

public class Player
{
    public float AttackPower;
    public float WeaponStrength;
    public Dictionary<DamageType, float> Resistances;
    public List<StatusEffect> ActiveBuffs;
    public List<StatusEffect> ActiveDebuffs;

    public Player()
    {
        Resistances = new Dictionary<DamageType, float>();
        ActiveBuffs = new List<StatusEffect>();
        ActiveDebuffs = new List<StatusEffect>();
    }
}

public class StatusEffect
{
    public string Name;
    public float DamageMultiplier;

    public StatusEffect(string name, float damageMultiplier)
    {
        Name = name;
        DamageMultiplier = damageMultiplier;
    }
}


/*           ***********        WITHOUT ENVIRONMENT         ************** */

//using System.Collections.Generic;
//using UnityEngine;
//using Sirenix.OdinInspector;

//public class Task : MonoBehaviour
//{
//    public static float CalculatePureDamage(Player attacker, Player defender)
//    {
//        return attacker.AttackPower + attacker.WeaponStrength;
//    }

//    public static float CalculateBaseDamage(Player attacker, Player defender)
//    {
//        float baseDamage = attacker.AttackPower + attacker.WeaponStrength;

//        float totalBuffMultiplier = 1f;
//        foreach (var buff in attacker.ActiveBuffs)
//            totalBuffMultiplier *= buff.DamageMultiplier;

//        float totalDebuffMultiplier = 1f;
//        foreach (var debuff in attacker.ActiveDebuffs)
//            totalDebuffMultiplier *= debuff.DamageMultiplier;

//        return baseDamage * totalBuffMultiplier * totalDebuffMultiplier;
//    }

//    public static float CalculateFinalDamage(Player attacker, Player defender, DamageType originalDamageType)
//    {
//        float attackPower = attacker.AttackPower;
//        float weaponStrength = attacker.WeaponStrength;

//        float buffMultiplier = 1f;
//        float debuffMultiplier = 1f;

//        foreach (var buff in attacker.ActiveBuffs)
//            buffMultiplier *= buff.DamageMultiplier;

//        foreach (var debuff in attacker.ActiveDebuffs)
//            debuffMultiplier *= debuff.DamageMultiplier;

//        float totalDamage = (attackPower + weaponStrength) * buffMultiplier * debuffMultiplier;

//        return ApplyResistance(totalDamage, defender, originalDamageType);
//    }

//    private static float ApplyResistance(float damage, Player defender, DamageType damageType)
//    {
//        float resistance = 0f;
//        defender.Resistances.TryGetValue(damageType, out resistance);

//        float mitigation = resistance / (resistance + 100f);
//        float finalDamage = damage * (1f - mitigation);
//        Debug.Log($"[Resistance] Mitigation: {mitigation:P0}, Final Damage: {finalDamage}");

//        return finalDamage;
//    }

//    [Button]
//    public static void Test()
//    {
//        var attacker = new Player
//        {
//            AttackPower = 121,
//            WeaponStrength = 54
//        };

//        attacker.ActiveBuffs.Add(new StatusEffect("Determined", 1.4f));
//        attacker.ActiveDebuffs.Add(new StatusEffect("Exhausted", 0.85f));
//        attacker.ActiveDebuffs.Add(new StatusEffect("Dizzy", 0.7f));

//        var defender = new Player();
//        defender.Resistances.Add(DamageType.Fire, 56f);
//        defender.Resistances.Add(DamageType.Ice, 78f);

//        Debug.Log("Final Damage (Fire): " + CalculateFinalDamage(attacker, defender, DamageType.Fire));
//        Debug.Log("Base Damage: " + CalculateBaseDamage(attacker, defender));
//        Debug.Log("Pure Damage: " + CalculatePureDamage(attacker, defender));
//    }
//}

//// === Required Types ===

//public enum DamageType { Physical, Fire, Ice, Poison }

//public class Player
//{
//    public float AttackPower;
//    public float WeaponStrength;
//    public Dictionary<DamageType, float> Resistances;
//    public List<StatusEffect> ActiveBuffs;
//    public List<StatusEffect> ActiveDebuffs;

//    public Player()
//    {
//        Resistances = new Dictionary<DamageType, float>();
//        ActiveBuffs = new List<StatusEffect>();
//        ActiveDebuffs = new List<StatusEffect>();
//    }
//}

//public class StatusEffect
//{
//    public string Name;
//    public float DamageMultiplier;

//    public StatusEffect(string name, float damageMultiplier)
//    {
//        Name = name;
//        DamageMultiplier = damageMultiplier;
//    }
//}
