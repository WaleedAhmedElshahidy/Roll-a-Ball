using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

public class Task1 : MonoBehaviour
{
    [SerializeField] private float attackerDamage = 0;
    [SerializeField] private float attackerWeaponDamage = 0;
    [SerializeField] private Environment currentEnvironment;
    [SerializeField] private DamageType damageTypeByAttacker;
    public List<StatusEffect> attackerBuffsEffect;
    public List<DefenderResistance> defenderResistance;
    public enum DamageType { Physical, Fire, Ice, Poison }
    public enum EffectType { Buff, DeBuff }
    public enum Environment { Desert, Forest, Mountains, Hills }
    private bool hasFlatDamage;
    [Serializable]
    public class DefenderResistance
    {
        public DamageType attackerDamageType;
        public float resistanceAmount = 0;
    }

    [Button]
    public void TestDamageDealed()
    {
        Player attackerPlayer = new Player // Set attacker player values
        {
            playerBasicDamage = attackerDamage,
            playerWeaponDamage = attackerWeaponDamage,
        };

        Player defenderPlayer = new Player(); // No need to set base stats

        // Assign resistances from inspector to defender
        foreach (DefenderResistance resistanceInputed in defenderResistance)
        {
            defenderPlayer.Resistances.Add(resistanceInputed.attackerDamageType, resistanceInputed.resistanceAmount);
        }

        // Assign buffs and debuffs to attacker
        foreach (StatusEffect singleEffects in attackerBuffsEffect)
        {
            if (singleEffects.effectType == EffectType.Buff)
            {
                attackerPlayer.activeBuffs.Add(singleEffects);
            }
            else if (singleEffects.effectType == EffectType.DeBuff)
            {
                attackerPlayer.activeDebuffs.Add(singleEffects);
            }
        }

        // Step 1: Basic + Weapon
        float pureDamage = CalculatePureDamage(attackerPlayer, defenderPlayer);

        // Step 2: Apply buffs/debuffs to get base damage
        float baseDamage = CalculateBaseDamage(attackerPlayer, defenderPlayer, currentEnvironment);
        float preEnvBaseDamage = baseDamage;

        // Step 3: Apply environment impact
        ApplyEnvironmentEffects(ref baseDamage, attackerPlayer, defenderPlayer, currentEnvironment);

        // Save the updated baseDamage as environmental damage
        float baseDamageAfterEnv = baseDamage;


        // Step 4: Apply resistance — skip if Desert already applied inside ApplyEnvironmentEffects
        float finalDamage;
        if (currentEnvironment == Environment.Desert && damageTypeByAttacker == DamageType.Physical)
        {
            // Resistance already applied in ApplyDualResistance
            finalDamage = baseDamage;
        }
        else
        {
            finalDamage = ApplyResistance(defenderPlayer, baseDamage);
        }

        // Step 5: Add +20 flat bonus in Desert
        if (hasFlatDamage)
            finalDamage += 20f;

        Debug.Log("Pure Damage: " + pureDamage);
        Debug.Log("Base Damage (Pre-Environment): " + preEnvBaseDamage);
        Debug.Log("Environmental Damage (After Environment Effects): " + baseDamageAfterEnv);
        Debug.Log("Final Damage Dealt: " + finalDamage);
    }

    public void ApplyEnvironmentEffects(ref float baseDamage, Player attacker, Player defender, Environment env)
    {
        switch (env)
        {
            case Environment.Desert:
                // Add flat +20 after resistance phase
                hasFlatDamage = true;

                // Reduce physical resistance by 12 (flat)
                if (damageTypeByAttacker == DamageType.Physical &&
                    defender.Resistances.ContainsKey(DamageType.Physical))
                {
                    defender.Resistances[DamageType.Physical] -= 12f;
                }

                // Convert 15% of attack into fire
                if (damageTypeByAttacker == DamageType.Physical)
                {
                    float firePortion = baseDamage * 0.15f;
                    float physicalPortion = baseDamage * 0.85f;
                    baseDamage = ApplyDualResistance(defender, physicalPortion, firePortion);
                    return; // return early since resistance is handled
                }
                break;

            case Environment.Mountains:
                // All resistances +50
                foreach (DamageType damageType in defender.Resistances.Keys.ToList())
                    defender.Resistances[damageType] += 50f;

                // Ignore all Ice damage
                if (damageTypeByAttacker == DamageType.Ice)
                    baseDamage = 0f;

                break;

            case Environment.Forest:
                // Reduce weapon damage by 18%
                attacker.playerWeaponDamage *= 0.82f;

                // Ignore fire resistance
                if (damageTypeByAttacker == DamageType.Fire &&
                    defender.Resistances.ContainsKey(DamageType.Fire))
                {
                    defender.Resistances[DamageType.Fire] = 0;
                }
                break;

            case Environment.Hills:
                // Ignore debuffs
                attacker.activeDebuffs.Clear();

                // Double buff effect
                foreach (var buff in attacker.activeBuffs)
                    buff.damageMultiplier *= 2f;
                break;
        }
    }
    public float ApplyDualResistance(Player defender, float physical, float fire)
    {
        float finalPhysical = physical;
        float finalFire = fire;

        if (defender.Resistances.ContainsKey(DamageType.Physical))
        {
            float resist = defender.Resistances[DamageType.Physical];
            float reduction = resist / (resist + 100f);
            finalPhysical *= (1f - reduction);
        }

        if (defender.Resistances.ContainsKey(DamageType.Fire))
        {
            float resist = defender.Resistances[DamageType.Fire];
            float reduction = resist / (resist + 100f);
            finalFire *= (1f - reduction);
        }

        return finalPhysical + finalFire;
    }


    public float CalculateFinalDamage(Player attacker , Player defender)
    {
        float attackPower = attacker.playerBasicDamage;
        float weaponStrength = attacker.playerWeaponDamage;

        float totalDamage = attackerDamage + weaponStrength ;
        Debug.Log("total Damage before Resistance : " + totalDamage);
        totalDamage = ApplyResistance(defender, totalDamage);
        Debug.Log("total Damage after Resistance : " + totalDamage);

        return totalDamage;
    }

    public float ApplyResistance(Player defender , float damageDealed)
    {

        float totalResistance = 0f;


        foreach (DefenderResistance defenderResistance in defenderResistance)
        {
            if(damageTypeByAttacker == defenderResistance.attackerDamageType)
            {
                totalResistance += defenderResistance.resistanceAmount;
            }

        }


        float mitigatedResistance = totalResistance / (totalResistance + 100f);
        float finalDamage = damageDealed * (1f - mitigatedResistance);
        return finalDamage; 
    }

    public float CalculateBaseDamage(Player attacker, Player defender, Environment environment) // Base Damage : basicDamage + Weapon + buff Effects
    {
        float baseDamage = attacker.playerBasicDamage + attacker.playerWeaponDamage;

        return baseDamage * CalculateBuffsEffect(attacker, defender, environment);
    }


    public float CalculateBuffsEffect(Player attacker, Player defender, Environment environment)
    {
        float totalBuffMultiplier = 1f;
        foreach (var buff in attacker.activeBuffs)
            totalBuffMultiplier *= buff.damageMultiplier;

        float totalDebuffMultiplier = 1f;
        foreach (var debuff in attacker.activeDebuffs)
            totalDebuffMultiplier *= debuff.damageMultiplier;

        float totalBuffsMulipliers = totalBuffMultiplier * totalDebuffMultiplier;
        return totalBuffsMulipliers;
    }
    public float CalculatePureDamage(Player attacker, Player defender)
    {
        return attacker.playerBasicDamage + attacker.playerWeaponDamage;
    }
    public class Player
    {
        public float playerBasicDamage;
        public float playerWeaponDamage;
        public Dictionary<DamageType, float> Resistances;
        public List<StatusEffect> activeBuffs;
        public List<StatusEffect> activeDebuffs;

        public Player()
        {
            Resistances = new Dictionary<DamageType, float>();
            activeBuffs = new List<StatusEffect>();
            activeDebuffs = new List<StatusEffect>();
        }
    }
    [Serializable]
    public class StatusEffect
    {
        public string effectName;
        public EffectType effectType;
        public float damageMultiplier;

        public StatusEffect(string name,EffectType type, float DamageMultiplier)
        {
            effectName = name;
            effectType = type;
            damageMultiplier = DamageMultiplier;
        }
    }
}
