using System;
using System.Collections.Generic;

using ACE.Database.Models.Shard;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.Network.Structure;
using ACE.Server.WorldObjects;
using ACE.Server.WorldObjects.Entity;

namespace ACE.Server.Managers
{
    public class EnchantmentManagerWithCaching : EnchantmentManager
    {
        /// <summary>
        /// Constructs a new EnchantmentManager for a WorldObject
        /// </summary>
        public EnchantmentManagerWithCaching(WorldObject obj) : base(obj)
        {
        }

        /// <summary>
        /// Add/update an enchantment in this object's registry
        /// </summary>
        public override (StackType stackType, Spell surpass) Add(Enchantment enchantment, WorldObject caster)
        {
            var result = base.Add(enchantment, caster);

            ClearCache();

            return result;
        }

        /// <summary>
        /// Removes a spell from the enchantment registry, and
        /// sends the relevant network messages for spell removal
        /// </summary>
        public override void Remove(BiotaPropertiesEnchantmentRegistry entry, bool sound = true)
        {
            base.Remove(entry, sound);

            ClearCache();
        }

        /// <summary>
        /// Removes all enchantments except for vitae
        /// Called on player death
        /// </summary>
        public override void RemoveAllEnchantments()
        {
            base.RemoveAllEnchantments();

            ClearCache();
        }


        /// <summary>
        /// Called on player death
        /// </summary>
        public override float UpdateVitae()
        {
            var result = base.UpdateVitae();

            ClearCache();

            return result;
        }

        /// <summary>
        /// Called when player crosses the VitaeCPPool threshold
        /// </summary>
        public override float ReduceVitae()
        {
            var result = base.ReduceVitae();

            ClearCache();

            return result;
        }


        /// <summary>
        /// Silently removes a spell from the enchantment registry, and sends the relevant network message for dispel
        /// </summary>
        public override void Dispel(BiotaPropertiesEnchantmentRegistry entry)
        {
            base.Dispel(entry);

            ClearCache();
        }

        /// <summary>
        /// Silently removes multiple spells from the enchantment registry, and sends the relevent network messages for dispel
        /// </summary>
        public override void Dispel(List<BiotaPropertiesEnchantmentRegistry> entries)
        {
            base.Dispel(entries);

            ClearCache();
        }


        private void ClearCache()
        {
            attributeModCache.Clear();
            vitalModCache.Clear();
            skillModCache.Clear();

            bodyArmorModCache = null;
            resistanceModCache.Clear();
            protectionResistanceModCache.Clear();
            vulnerabilityResistanceModCache.Clear();
            regenerationModCache.Clear();

            damageModCache = null;
            damageModifierCache = null;
            attackModCache = null;
            weaponSpeedModCache = null;
            defenseModCache = null;
            varianceModCache = null;
            armorModCache = null;
            armorModVsTypeModCache.Clear();

            damageRatingCache = null;
            damageResistRatingCache = null;
            healingResistRatingModCache = null;
        }


        private readonly Dictionary<PropertyAttribute, int> attributeModCache = new Dictionary<PropertyAttribute, int>();

        /// <summary>
        /// Returns the bonus to an attribute from enchantments
        /// </summary>
        public override int GetAttributeMod(PropertyAttribute attribute)
        {
            if (attributeModCache.TryGetValue(attribute, out var value))
                return value;

            value = base.GetAttributeMod(attribute);

            attributeModCache[attribute] = value;

            return value;
        }

        private readonly Dictionary<CreatureVital, float> vitalModCache = new Dictionary<CreatureVital, float>();

        /// <summary>
        /// Gets the direct modifiers to a vital / secondary attribute
        /// </summary>
        public override float GetVitalMod(CreatureVital vital)
        {
            if (vitalModCache.TryGetValue(vital, out var value))
                return value;

            value = base.GetVitalMod(vital);

            vitalModCache[vital] = value;

            return value;
        }

        private readonly Dictionary<Skill, int> skillModCache = new Dictionary<Skill, int>();

        /// <summary>
        /// Returns the bonus to a skill from enchantments
        /// </summary>
        public override int GetSkillMod(Skill skill)
        {
            if (skillModCache.TryGetValue(skill, out var value))
                return value;

            value = base.GetSkillMod(skill);

            skillModCache[skill] = value;

            return value;
        }


        private int? bodyArmorModCache;

        /// <summary>
        /// Returns the base armor modifier from enchantments
        /// </summary>
        public override int GetBodyArmorMod()
        {
            if (bodyArmorModCache.HasValue)
                return bodyArmorModCache.Value;

            bodyArmorModCache = base.GetBodyArmorMod();

            return bodyArmorModCache.Value;
        }

        private readonly Dictionary<DamageType, float> resistanceModCache = new Dictionary<DamageType, float>();

        /// <summary>
        /// Gets the resistance modifier for a damage type
        /// </summary>
        public override float GetResistanceMod(DamageType damageType)
        {
            if (resistanceModCache.TryGetValue(damageType, out var value))
                return value;

            value = base.GetResistanceMod(damageType);

            resistanceModCache[damageType] = value;

            return value;
        }

        private readonly Dictionary<DamageType, float> protectionResistanceModCache = new Dictionary<DamageType, float>();

        /// <summary>
        /// Gets the resistance modifier for a damage type
        /// </summary>
        public override float GetProtectionResistanceMod(DamageType damageType)
        {
            if (protectionResistanceModCache.TryGetValue(damageType, out var value))
                return value;

            value = base.GetProtectionResistanceMod(damageType);

            protectionResistanceModCache[damageType] = value;

            return value;
        }

        private readonly Dictionary<DamageType, float> vulnerabilityResistanceModCache = new Dictionary<DamageType, float>();

        /// <summary>
        /// Gets the resistance modifier for a damage type
        /// </summary>
        public override float GetVulnerabilityResistanceMod(DamageType damageType)
        {
            if (vulnerabilityResistanceModCache.TryGetValue(damageType, out var value))
                return value;

            value = base.GetVulnerabilityResistanceMod(damageType);

            vulnerabilityResistanceModCache[damageType] = value;

            return value;
        }

        private readonly Dictionary<CreatureVital, float> regenerationModCache = new Dictionary<CreatureVital, float>();

        /// <summary>
        /// Gets the regeneration modifier for a vital type
        /// (regeneration / rejuvenation / mana renewal)
        /// </summary>
        public override float GetRegenerationMod(CreatureVital vital)
        {
            if (regenerationModCache.TryGetValue(vital, out var value))
                return value;

            value = base.GetRegenerationMod(vital);

            regenerationModCache[vital] = value;

            return value;
        }


        private int? damageModCache;

        /// <summary>
        /// Returns the weapon damage modifier, ie. Blood Drinker
        /// </summary>
        public override int GetDamageMod()
        {
            if (damageModCache.HasValue)
                return damageModCache.Value;

            damageModCache = base.GetDamageMod();

            return damageModCache.Value;
        }

        private float? damageModifierCache;

        /// <summary>
        /// Returns the DamageMod for bow / crossbow
        /// </summary>
        public override float GetDamageModifier()
        {
            if (damageModifierCache.HasValue)
                return damageModifierCache.Value;

            damageModifierCache = base.GetDamageModifier();

            return damageModifierCache.Value;
        }

        private float? attackModCache;

        /// <summary>
        /// Returns the attack skill modifier, ie. Heart Seeker
        /// </summary>
        public override float GetAttackMod()
        {
            if (attackModCache.HasValue)
                return attackModCache.Value;

            attackModCache = base.GetAttackMod();

            return attackModCache.Value;
        }

        private int? weaponSpeedModCache;

        /// <summary>
        /// Returns the weapon speed modifier, ie. Swift Killer
        /// </summary>
        public override int GetWeaponSpeedMod()
        {
            if (weaponSpeedModCache.HasValue)
                return weaponSpeedModCache.Value;

            weaponSpeedModCache = base.GetWeaponSpeedMod();

            return weaponSpeedModCache.Value;
        }

        private float? defenseModCache;

        /// <summary>
        /// Returns the defense skill modifier, ie. Defender
        /// </summary>
        public override float GetDefenseMod()
        {
            if (defenseModCache.HasValue)
                return defenseModCache.Value;

            defenseModCache = base.GetDefenseMod();

            return defenseModCache.Value;
        }

        private float? varianceModCache;

        /// <summary>
        /// Returns the weapon damage variance modifier
        /// </summary>
        /// 
        public override float GetVarianceMod()
        {
            if (varianceModCache.HasValue)
                return varianceModCache.Value;

            varianceModCache = base.GetVarianceMod();

            return varianceModCache.Value;
        }

        private int? armorModCache;

        /// <summary>
        /// Returns the additive armor level modifier, ie. Impenetrability
        /// </summary>
        public override int GetArmorMod()
        {
            if (armorModCache.HasValue)
                return armorModCache.Value;

            armorModCache = base.GetArmorMod();

            return armorModCache.Value;
        }

        private readonly Dictionary<DamageType, float> armorModVsTypeModCache = new Dictionary<DamageType, float>();

        /// <summary>
        /// Gets the additive armor level vs type modifier, ie. banes
        /// </summary>
        public override float GetArmorModVsType(DamageType damageType)
        {
            if (armorModVsTypeModCache.TryGetValue(damageType, out var value))
                return value;

            value = base.GetArmorModVsType(damageType);

            armorModVsTypeModCache[damageType] = value;

            return value;
        }


        private int? damageRatingCache;

        /// <summary>
        /// Returns the damage rating modifier from enchantments as an int rating (additive)
        /// </summary>
        public override int GetDamageRating()
        {
            if (damageRatingCache.HasValue)
                return damageRatingCache.Value;

            damageRatingCache = base.GetDamageRating();

            return damageRatingCache.Value;
        }

        private int? damageResistRatingCache;

        public override int GetDamageResistRating()
        {
            if (damageResistRatingCache.HasValue)
                return damageResistRatingCache.Value;

            damageResistRatingCache = base.GetDamageResistRating();

            return damageResistRatingCache.Value;
        }

        private float? healingResistRatingModCache;

        /// <summary>
        /// Returns the healing resistance rating enchantment modifier
        /// </summary>
        public override float GetHealingResistRatingMod()
        {
            if (healingResistRatingModCache.HasValue)
                return healingResistRatingModCache.Value;

            healingResistRatingModCache = base.GetHealingResistRatingMod();

            return healingResistRatingModCache.Value;
        }
    }
}