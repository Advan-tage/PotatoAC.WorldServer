using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ACE.Database;
using ACE.Database.Models.World;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.Factories;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.WorldObjects;

namespace ACE.Server.Managers
{
    public partial class RecipeManager
    {
        public static Dictionary<uint, Dictionary<uint, uint>> Precursors;

        public static void ReadJSON()
        {
            // read recipeprecursors.json
            // tool -> target -> recipe
            var json = File.ReadAllText(@"json\recipeprecursors.json");
            var precursors = JsonConvert.DeserializeObject<List<RecipePrecursor>>(json);
            Precursors = new Dictionary<uint, Dictionary<uint, uint>>();

            foreach (var precursor in precursors)
            {
                Dictionary<uint, uint> tool = null;
                if (!Precursors.TryGetValue(precursor.Tool, out tool))
                {
                    tool = new Dictionary<uint, uint>();
                    Precursors.Add(precursor.Tool, tool);
                }
                tool[precursor.Target] = precursor.RecipeID;
            }
        }

        public static Recipe GetNewRecipe(Player player, WorldObject source, WorldObject target)
        {
            Recipe recipe = null;

            switch ((WeenieClassName)source.WeenieClassId)
            {
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFBLUE_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFBLACK_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFBOTCHED_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFDARKGREEN_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFDARKRED_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFDARKYELLOW_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFLIGHTBLUE_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFLIGHTGREEN_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFPURPLE_CLASS:
                case WeenieClassName.W_DYERAREETERNALFOOLPROOFSILVER_CLASS:

                    // ensure item is armor/clothing and dyeable
                    if (target.WeenieType != WeenieType.Clothing || !(target.GetProperty(PropertyBool.Dyable) ?? false))
                        return null;

                    // use dye recipe as base, cleared
                    recipe = DatabaseManager.World.GetRecipe(3844);
                    ClearRecipe(recipe);
                    break;

                case WeenieClassName.W_MATERIALIVORY_CLASS:
                case WeenieClassName.W_MATERIALRAREETERNALIVORY_CLASS:

                    // ensure item is ivoryable
                    if (!(target.GetProperty(PropertyBool.Ivoryable) ?? false))
                        return null;

                    // use ivory recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3977);

                    if (source.WeenieClassId == (int)WeenieClassName.W_MATERIALRAREETERNALIVORY_CLASS)
                        ClearRecipe(recipe);

                    break;

                case WeenieClassName.W_MATERIALLEATHER_CLASS:
                case WeenieClassName.W_MATERIALRAREETERNALLEATHER_CLASS:

                    // ensure item is not retained and sellable
                    if (target.Retained || !target.IsSellable)
                        return null;

                    // use leather recipe as base
                    recipe = DatabaseManager.World.GetRecipe(4426);

                    if (source.WeenieClassId == (int)WeenieClassName.W_MATERIALRAREETERNALLEATHER_CLASS)
                        ClearRecipe(recipe);

                    break;

                case WeenieClassName.W_MATERIALSANDSTONE_CLASS:
                case WeenieClassName.W_MATERIALSANDSTONE100_CLASS:

                    // ensure item is retained and sellable
                    if (!target.Retained || !target.IsSellable)
                        return null;

                    // use sandstone recipe as base
                    recipe = DatabaseManager.World.GetRecipe(8003);

                    break;

                case WeenieClassName.W_MATERIALGOLD_CLASS:

                    // ensure item has value and workmanship
                    if ((target.Value ?? 0) == 0 || target.Workmanship == null)
                        return null;

                    // use gold recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3851);
                    break;

                case WeenieClassName.W_MATERIALLINEN_CLASS:

                    // ensure item has burden and workmanship
                    if ((target.EncumbranceVal ?? 0) == 0 || target.Workmanship == null)
                        return null;

                    // use linen recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3854);
                    break;

                case WeenieClassName.W_MATERIALMOONSTONE_CLASS:

                    // ensure item has mana and workmanship
                    if ((target.ItemMaxMana ?? 0) == 0 || target.Workmanship == null)
                        return null;

                    // use moonstone recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3978);
                    break;

                case WeenieClassName.W_MATERIALPINE_CLASS:

                    // ensure item has value and workmanship
                    if ((target.Value ?? 0) == 0 || target.Workmanship == null)
                        return null;

                    // use pine recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3858);
                    break;

                case WeenieClassName.W_MATERIALIRON100_CLASS:
                case WeenieClassName.W_MATERIALIRON_CLASS:
                //case WeenieClassName.W_MATERIALGRANITE50_CLASS:
                case WeenieClassName.W_MATERIALGRANITE100_CLASS:
                case WeenieClassName.W_MATERIALGRANITE_CLASS:
                case WeenieClassName.W_MATERIALGRANITEPATHWARDEN_CLASS:
                case WeenieClassName.W_MATERIALVELVET100_CLASS:
                case WeenieClassName.W_MATERIALVELVET_CLASS:

                    // ensure melee weapon and workmanship
                    if (target.WeenieType != WeenieType.MeleeWeapon || target.Workmanship == null)
                        return null;

                    // grab correct recipe to use as base
                    recipe = DatabaseManager.World.GetRecipe(SourceToRecipe[(WeenieClassName)source.WeenieClassId]);
                    break;

                case WeenieClassName.W_MATERIALMAHOGANY100_CLASS:
                case WeenieClassName.W_MATERIALMAHOGANY_CLASS:

                    // ensure missile weapon and workmanship
                    if (target.WeenieType != WeenieType.MissileLauncher || target.Workmanship == null)
                        return null;

                    // use mahogany recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3855);
                    break;

                case WeenieClassName.W_MATERIALOAK_CLASS:

                    // ensure melee or missile weapon, and workmanship
                    if (target.WeenieType != WeenieType.MeleeWeapon && target.WeenieType != WeenieType.MissileLauncher || target.Workmanship == null)
                        return null;

                    // use oak recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3857);
                    break;

                case WeenieClassName.W_MATERIALOPAL100_CLASS:
                case WeenieClassName.W_MATERIALOPAL_CLASS:

                    // ensure item is caster and has workmanship
                    if (target.WeenieType != WeenieType.Caster || target.Workmanship == null)
                        return null;

                    // use opal recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3979);
                    break;

                case WeenieClassName.W_MATERIALGREENGARNET100_CLASS:
                case WeenieClassName.W_MATERIALGREENGARNET_CLASS:

                    // ensure item is caster and has workmanship
                    if (target.WeenieType != WeenieType.Caster || target.Workmanship == null)
                        return null;

                    // use green garnet recipe as base
                    recipe = DatabaseManager.World.GetRecipe(5202);
                    break;

                case WeenieClassName.W_MATERIALBRASS100_CLASS:
                case WeenieClassName.W_MATERIALBRASS_CLASS:

                    // ensure item has workmanship
                    if (target.Workmanship == null) return null;

                    // use brass recipe as base
                    recipe = DatabaseManager.World.GetRecipe(3848);
                    break;

                case WeenieClassName.W_MATERIALROSEQUARTZ_CLASS:
                case WeenieClassName.W_MATERIALREDJADE_CLASS:
                case WeenieClassName.W_MATERIALMALACHITE_CLASS:
                case WeenieClassName.W_MATERIALLAVENDERJADE_CLASS:
                case WeenieClassName.W_MATERIALHEMATITE_CLASS:
                case WeenieClassName.W_MATERIALBLOODSTONE_CLASS:
                case WeenieClassName.W_MATERIALAZURITE_CLASS:
                case WeenieClassName.W_MATERIALAGATE_CLASS:
                case WeenieClassName.W_MATERIALSMOKYQUARTZ_CLASS:
                case WeenieClassName.W_MATERIALCITRINE_CLASS:
                case WeenieClassName.W_MATERIALCARNELIAN_CLASS:

                    // ensure item is generic (jewelry), and has workmanship
                    if (target.WeenieType != WeenieType.Generic || target.Workmanship == null || target.ValidLocations == EquipMask.TrinketOne)
                        return null;

                    recipe = DatabaseManager.World.GetRecipe(SourceToRecipe[(WeenieClassName)source.WeenieClassId]);
                    break;

                //case WeenieClassName.W_MATERIALSTEEL50_CLASS:
                case WeenieClassName.W_MATERIALSTEEL100_CLASS:
                case WeenieClassName.W_MATERIALSTEEL_CLASS:
                case WeenieClassName.W_MATERIALSTEELPATHWARDEN_CLASS:
                case WeenieClassName.W_MATERIALALABASTER_CLASS:
                case WeenieClassName.W_MATERIALBRONZE_CLASS:
                case WeenieClassName.W_MATERIALMARBLE_CLASS:
                case WeenieClassName.W_MATERIALARMOREDILLOHIDE_CLASS:
                case WeenieClassName.W_MATERIALCERAMIC_CLASS:
                case WeenieClassName.W_MATERIALWOOL_CLASS:
                case WeenieClassName.W_MATERIALREEDSHARKHIDE_CLASS:
                case WeenieClassName.W_MATERIALSILVER_CLASS:
                case WeenieClassName.W_MATERIALCOPPER_CLASS:

                    // ensure armor w/ workmanship
                    if (target.ItemType != ItemType.Armor || (target.ArmorLevel ?? 0) == 0 || target.Workmanship == null)
                        return null;

                    // TODO: replace with PropertyInt.MeleeDefenseImbuedEffectTypeCache == 1 when data is updated
                    if (source.MaterialType == MaterialType.Steel && !target.IsEnchantable)
                        return null;

                    recipe = DatabaseManager.World.GetRecipe(SourceToRecipe[(WeenieClassName)source.WeenieClassId]);
                    break;

                case WeenieClassName.W_MATERIALPERIDOT_CLASS:
                case WeenieClassName.W_MATERIALYELLOWTOPAZ_CLASS:
                case WeenieClassName.W_MATERIALZIRCON_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFPERIDOT_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFYELLOWTOPAZ_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFZIRCON_CLASS:
                case WeenieClassName.W_MATERIALACE36634FOOLPROOFPERIDOT:
                case WeenieClassName.W_MATERIALACE36635FOOLPROOFYELLOWTOPAZ:
                case WeenieClassName.W_MATERIALACE36636FOOLPROOFZIRCON:

                    // ensure clothing/armor w/ AL and workmanship
                    if (target.WeenieType != WeenieType.Clothing || (target.ArmorLevel ?? 0) == 0 || target.Workmanship == null)
                        return null;

                    recipe = DatabaseManager.World.GetRecipe(SourceToRecipe[(WeenieClassName)source.WeenieClassId]);
                    break;

                case WeenieClassName.W_POTDYEDARKGREEN_CLASS:
                case WeenieClassName.W_POTDYEDARKRED_CLASS:
                case WeenieClassName.W_POTDYEDARKYELLOW_CLASS:
                case WeenieClassName.W_POTDYEWINTERBLUE_CLASS:
                case WeenieClassName.W_POTDYEWINTERGREEN_CLASS:
                case WeenieClassName.W_POTDYEWINTERSILVER_CLASS:
                case WeenieClassName.W_POTDYESPRINGBLACK_CLASS:
                case WeenieClassName.W_POTDYESPRINGBLUE_CLASS:
                case WeenieClassName.W_POTDYESPRINGPURPLE_CLASS:

                    // ensure dyeable armor/clothing
                    if (target.WeenieType != WeenieType.Clothing || !(target.GetProperty(PropertyBool.Dyable) ?? false))
                        return null;

                    recipe = DatabaseManager.World.GetRecipe(3844);
                    break;

                // imbues - foolproof handled in regular imbue code
                case WeenieClassName.W_MATERIALRAREFOOLPROOFAQUAMARINE_CLASS:
                case WeenieClassName.W_MATERIALAQUAMARINE100_CLASS:
                case WeenieClassName.W_MATERIALAQUAMARINE_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFBLACKGARNET_CLASS:
                case WeenieClassName.W_MATERIALBLACKGARNET100_CLASS:
                case WeenieClassName.W_MATERIALBLACKGARNET_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFBLACKOPAL_CLASS:
                case WeenieClassName.W_MATERIALBLACKOPAL100_CLASS:
                case WeenieClassName.W_MATERIALBLACKOPAL_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFEMERALD_CLASS:
                case WeenieClassName.W_MATERIALEMERALD100_CLASS:
                case WeenieClassName.W_MATERIALEMERALD_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFFIREOPAL_CLASS:
                case WeenieClassName.W_MATERIALFIREOPAL100_CLASS:
                case WeenieClassName.W_MATERIALFIREOPAL_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFIMPERIALTOPAZ_CLASS:
                case WeenieClassName.W_MATERIALIMPERIALTOPAZ100_CLASS:
                case WeenieClassName.W_MATERIALIMPERIALTOPAZ_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFJET_CLASS:
                case WeenieClassName.W_MATERIALJET100_CLASS:
                case WeenieClassName.W_MATERIALJET_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFREDGARNET_CLASS:
                case WeenieClassName.W_MATERIALREDGARNET100_CLASS:
                case WeenieClassName.W_MATERIALREDGARNET_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFSUNSTONE_CLASS:
                case WeenieClassName.W_MATERIALSUNSTONE100_CLASS:
                case WeenieClassName.W_MATERIALSUNSTONE_CLASS:
                case WeenieClassName.W_MATERIALRAREFOOLPROOFWHITESAPPHIRE_CLASS:
                case WeenieClassName.W_MATERIALWHITESAPPHIRE100_CLASS:
                case WeenieClassName.W_MATERIALWHITESAPPHIRE_CLASS:
                case WeenieClassName.W_LEFTHANDTETHER_CLASS:
                case WeenieClassName.W_LEFTHANDTETHERREMOVER_CLASS:
                case WeenieClassName.W_COREPLATINGINTEGRATOR_CLASS:
                case WeenieClassName.W_COREPLATINGDISINTEGRATOR_CLASS:
                case WeenieClassName.W_MATERIALACE36619FOOLPROOFAQUAMARINE:
                case WeenieClassName.W_MATERIALACE36620FOOLPROOFBLACKGARNET:
                case WeenieClassName.W_MATERIALACE36621FOOLPROOFBLACKOPAL:
                case WeenieClassName.W_MATERIALACE36622FOOLPROOFEMERALD:
                case WeenieClassName.W_MATERIALACE36623FOOLPROOFFIREOPAL:
                case WeenieClassName.W_MATERIALACE36624FOOLPROOFIMPERIALTOPAZ:
                case WeenieClassName.W_MATERIALACE36625FOOLPROOFJET:
                case WeenieClassName.W_MATERIALACE36626FOOLPROOFREDGARNET:
                case WeenieClassName.W_MATERIALACE36627FOOLPROOFSUNSTONE:
                case WeenieClassName.W_MATERIALACE36628FOOLPROOFWHITESAPPHIRE:


                    recipe = DatabaseManager.World.GetRecipe(SourceToRecipe[(WeenieClassName)source.WeenieClassId]);
                    break;

                // Paragon Weapons
                case WeenieClassName.W_LUMINOUSAMBEROFTHE1STTIERPARAGON_CLASS:

                    switch (target.WeenieType)
                    {
                        case WeenieType.Caster:
                            recipe = DatabaseManager.World.GetRecipe(8700);
                            break;

                        case WeenieType.MeleeWeapon:
                            recipe = DatabaseManager.World.GetRecipe(8701);
                            break;

                        case WeenieType.MissileLauncher:
                            recipe = DatabaseManager.World.GetRecipe(8699);
                            break;

                        default:
                            return null;
                    }

                    break;

                case WeenieClassName.W_LUMINOUSAMBEROFTHE2NDTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE3RDTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE4THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE5THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE6THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE7THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE8THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE9THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE10THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE11THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE12THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE13THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE14THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE15THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE16THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE17THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE18THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE19THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE20THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE21STTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE22NDTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE23RDTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE24THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE25THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE26THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE27THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE28THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE29THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE30THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE31STTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE32NDTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE33RDTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE34THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE35THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE36THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE37THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE38THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE39THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE40THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE41STTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE42NDTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE43RDTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE44THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE45THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE46THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE47THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE48THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE49THTIERPARAGON_CLASS:
                case WeenieClassName.W_LUMINOUSAMBEROFTHE50THTIERPARAGON_CLASS:
                    recipe = DatabaseManager.World.GetRecipe(SourceToRecipe[(WeenieClassName)source.WeenieClassId]);
                    break;
            }

            switch (source.MaterialType)
            {
                case MaterialType.LessDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var applyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        applyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        applyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 1.25)
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        applyable = false;
                    }

                    if (!applyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666666);
                    break;

                case MaterialType.LessLessDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var xapplyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        xapplyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        xapplyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 1.15) // if this property is greater than what this gem gives.. dont allow apply and send msg
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        xapplyable = false;
                    }

                    if (!xapplyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666667);
                    break;

                case MaterialType.LessGreaterDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var zapplyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        zapplyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        zapplyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 1.35) // if this property is greater than what this gem gives.. dont allow apply and send msg
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        xapplyable = false;
                    }

                    if (!zapplyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666668);
                    break;

                case MaterialType.ModerateDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var qapplyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        qapplyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        qapplyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 1.60) // if this property is greater than what this gem gives.. dont allow apply and send msg
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        qapplyable = false;
                    }

                    if (!qapplyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666669);
                    break;

                case MaterialType.ModerateLessDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var wapplyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        wapplyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        wapplyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 1.50) // if this property is greater than what this gem gives.. dont allow apply and send msg
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        wapplyable = false;
                    }

                    if (!wapplyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666670);
                    break;

                case MaterialType.ModerateGreaterDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var eapplyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        eapplyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        eapplyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 1.70) // if this property is greater than what this gem gives.. dont allow apply and send msg
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        eapplyable = false;
                    }

                    if (!eapplyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666671);
                    break;

                case MaterialType.GreaterLessDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var rapplyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        rapplyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        rapplyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 1.80) // if this property is greater than what this gem gives.. dont allow apply and send msg
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        rapplyable = false;
                    }

                    if (!rapplyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666672);
                    break;

                case MaterialType.GreaterDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var tapplyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        tapplyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        tapplyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 1.90) // if this property is greater than what this gem gives.. dont allow apply and send msg
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        tapplyable = false;
                    }

                    if (!tapplyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666673);
                    break;

                case MaterialType.GreaterGreaterDrudgeSlayer:

                    // requirements:
                    // - target has workmanship (loot generated)
                    // - itemtype, such as weapon or armor/clothing
                    // - etc.
                    var yapplyable = false;

                    if ((target.ItemType & (ItemType.WeaponOrCaster)) != 0 && target.Workmanship != null)
                        yapplyable = true;

                    if (Aetheria.IsAetheria(target.WeenieClassId) || target.GetProperty(PropertyInt.RareId) != null || (target.ItemType & (ItemType.Jewelry)) != 0
                        || (target.ItemType & (ItemType.Vestements)) != 0)
                        yapplyable = false;

                    if (target.GetProperty(PropertyFloat.SlayerDamageBonus) >= 2) // if this property is greater than what this gem gives.. dont allow apply and send msg
                    {
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You cannot apply a lesser or same value Slayer Gem to this weapon!", ChatMessageType.Broadcast));
                        eapplyable = false;
                    }

                    if (!yapplyable)
                        return null;

                    // replace with your custom recipe id
                    recipe = DatabaseManager.World.GetRecipe(666674);
                    break;

            }
            return recipe;
        }

        public static void ClearRecipe(Recipe recipe)
        {
            recipe.Difficulty = 0;
            recipe.FailAmount = 0;
            recipe.FailDestroySourceAmount = 0;
            recipe.FailDestroySourceChance = 0;
            recipe.SuccessAmount = 0;
            recipe.SuccessDestroySourceChance = 0;
            recipe.SuccessDestroySourceChance = 0;
            recipe.SuccessWCID = 0;
            recipe.FailWCID = 0;
        }

        public static Dictionary<WeenieClassName, uint> SourceToRecipe = new Dictionary<WeenieClassName, uint>()
        {
            { WeenieClassName.W_MATERIALIRON100_CLASS,         3853 },
            { WeenieClassName.W_MATERIALIRON_CLASS,            3853 },
            { WeenieClassName.W_MATERIALGRANITE100_CLASS,      3852 },
            { WeenieClassName.W_MATERIALGRANITE_CLASS,         3852 },
            { WeenieClassName.W_MATERIALGRANITEPATHWARDEN_CLASS, 3852 },

            { WeenieClassName.W_MATERIALVELVET100_CLASS,       3861 },
            { WeenieClassName.W_MATERIALVELVET_CLASS,          3861 },

            { WeenieClassName.W_MATERIALROSEQUARTZ_CLASS,      4446 },
            { WeenieClassName.W_MATERIALREDJADE_CLASS,         4442 },
            { WeenieClassName.W_MATERIALMALACHITE_CLASS,       4438 },
            { WeenieClassName.W_MATERIALLAVENDERJADE_CLASS,    4441 },
            { WeenieClassName.W_MATERIALHEMATITE_CLASS,        4440 },
            { WeenieClassName.W_MATERIALBLOODSTONE_CLASS,      4448 },
            { WeenieClassName.W_MATERIALAZURITE_CLASS,         4437 },
            { WeenieClassName.W_MATERIALAGATE_CLASS,           4445 },
            { WeenieClassName.W_MATERIALSMOKYQUARTZ_CLASS,     4447 },
            { WeenieClassName.W_MATERIALCITRINE_CLASS,         4439 },
            { WeenieClassName.W_MATERIALCARNELIAN_CLASS,       4443 },

            //{ WeenieClassName.W_MATERIALSTEEL50_CLASS,         3860 },
            { WeenieClassName.W_MATERIALSTEEL100_CLASS,        3860 },
            { WeenieClassName.W_MATERIALSTEEL_CLASS,           3860 },
            { WeenieClassName.W_MATERIALSTEELPATHWARDEN_CLASS, 3860 },

            { WeenieClassName. W_MATERIALALABASTER_CLASS,      3846 },
            { WeenieClassName.W_MATERIALBRONZE_CLASS,          3849 },
            { WeenieClassName.W_MATERIALMARBLE_CLASS,          3856 },
            { WeenieClassName.W_MATERIALARMOREDILLOHIDE_CLASS, 3847 },
            { WeenieClassName.W_MATERIALCERAMIC_CLASS,         3850 },
            { WeenieClassName.W_MATERIALWOOL_CLASS,            3862 },
            { WeenieClassName.W_MATERIALREEDSHARKHIDE_CLASS,   3859 },
            { WeenieClassName.W_MATERIALSILVER_CLASS,          4427 },
            { WeenieClassName.W_MATERIALCOPPER_CLASS,          4428 },

            { WeenieClassName.W_MATERIALPERIDOT_CLASS,         4435 },
            { WeenieClassName.W_MATERIALYELLOWTOPAZ_CLASS,     4434 },
            { WeenieClassName.W_MATERIALZIRCON_CLASS,          4433 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFPERIDOT_CLASS,     4435 },
            { WeenieClassName.W_MATERIALACE36634FOOLPROOFPERIDOT,       4435 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFYELLOWTOPAZ_CLASS, 4434 },
            { WeenieClassName.W_MATERIALACE36635FOOLPROOFYELLOWTOPAZ,   4434 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFZIRCON_CLASS,      4433 },
            { WeenieClassName.W_MATERIALACE36636FOOLPROOFZIRCON,        4433 },

            { WeenieClassName.W_MATERIALRAREFOOLPROOFAQUAMARINE_CLASS,    4436 },
            { WeenieClassName.W_MATERIALACE36619FOOLPROOFAQUAMARINE,      4436 },
            { WeenieClassName.W_MATERIALAQUAMARINE100_CLASS,              4436 },
            { WeenieClassName.W_MATERIALAQUAMARINE_CLASS,                 4436 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFBLACKGARNET_CLASS,   4449 },
            { WeenieClassName.W_MATERIALACE36620FOOLPROOFBLACKGARNET,     4449 },
            { WeenieClassName.W_MATERIALBLACKGARNET100_CLASS,             4449 },
            { WeenieClassName.W_MATERIALBLACKGARNET_CLASS,                4449 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFBLACKOPAL_CLASS,     3863 },
            { WeenieClassName.W_MATERIALACE36621FOOLPROOFBLACKOPAL,       3863 },
            { WeenieClassName.W_MATERIALBLACKOPAL100_CLASS,               3863 },
            { WeenieClassName.W_MATERIALBLACKOPAL_CLASS,                  3863 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFEMERALD_CLASS,       4450 },
            { WeenieClassName.W_MATERIALACE36622FOOLPROOFEMERALD,         4450 },
            { WeenieClassName.W_MATERIALEMERALD100_CLASS,                 4450 },
            { WeenieClassName.W_MATERIALEMERALD_CLASS,                    4450 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFFIREOPAL_CLASS,      3864 },
            { WeenieClassName.W_MATERIALACE36623FOOLPROOFFIREOPAL,        3864 },
            { WeenieClassName.W_MATERIALFIREOPAL100_CLASS,                3864 },
            { WeenieClassName.W_MATERIALFIREOPAL_CLASS,                   3864 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFIMPERIALTOPAZ_CLASS, 4454 },
            { WeenieClassName.W_MATERIALACE36624FOOLPROOFIMPERIALTOPAZ,   4454 },
            { WeenieClassName.W_MATERIALIMPERIALTOPAZ100_CLASS,           4454 },
            { WeenieClassName.W_MATERIALIMPERIALTOPAZ_CLASS,              4454 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFJET_CLASS,           4451 },
            { WeenieClassName.W_MATERIALACE36625FOOLPROOFJET,             4451 },
            { WeenieClassName.W_MATERIALJET100_CLASS,                     4451 },
            { WeenieClassName.W_MATERIALJET_CLASS,                        4451 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFREDGARNET_CLASS,     4452 },
            { WeenieClassName.W_MATERIALACE36626FOOLPROOFREDGARNET,       4452 },
            { WeenieClassName.W_MATERIALREDGARNET100_CLASS,               4452 },
            { WeenieClassName.W_MATERIALREDGARNET_CLASS,                  4452 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFSUNSTONE_CLASS,      3865 },
            { WeenieClassName.W_MATERIALACE36627FOOLPROOFSUNSTONE,        3865 },
            { WeenieClassName.W_MATERIALSUNSTONE100_CLASS,                3865 },
            { WeenieClassName.W_MATERIALSUNSTONE_CLASS,                   3865 },
            { WeenieClassName.W_MATERIALRAREFOOLPROOFWHITESAPPHIRE_CLASS, 4453 },
            { WeenieClassName.W_MATERIALACE36628FOOLPROOFWHITESAPPHIRE,   4453 },
            { WeenieClassName.W_MATERIALWHITESAPPHIRE100_CLASS,           4453 },
            { WeenieClassName.W_MATERIALWHITESAPPHIRE_CLASS,              4453 },
            { WeenieClassName.W_LEFTHANDTETHER_CLASS,                     6798 },
            { WeenieClassName.W_LEFTHANDTETHERREMOVER_CLASS,              6799 },
            { WeenieClassName.W_COREPLATINGINTEGRATOR_CLASS,              6800 },
            { WeenieClassName.W_COREPLATINGDISINTEGRATOR_CLASS,           6801 },

            { WeenieClassName.W_LUMINOUSAMBEROFTHE2NDTIERPARAGON_CLASS,    8702 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE3RDTIERPARAGON_CLASS,    8703 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE4THTIERPARAGON_CLASS,    8704 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE5THTIERPARAGON_CLASS,    8705 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE6THTIERPARAGON_CLASS,    8706 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE7THTIERPARAGON_CLASS,    8707 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE8THTIERPARAGON_CLASS,    8708 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE9THTIERPARAGON_CLASS,    8709 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE10THTIERPARAGON_CLASS,   8710 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE11THTIERPARAGON_CLASS,   8711 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE12THTIERPARAGON_CLASS,   8712 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE13THTIERPARAGON_CLASS,   8713 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE14THTIERPARAGON_CLASS,   8714 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE15THTIERPARAGON_CLASS,   8715 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE16THTIERPARAGON_CLASS,   8716 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE17THTIERPARAGON_CLASS,   8717 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE18THTIERPARAGON_CLASS,   8718 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE19THTIERPARAGON_CLASS,   8719 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE20THTIERPARAGON_CLASS,   8720 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE21STTIERPARAGON_CLASS,   8721 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE22NDTIERPARAGON_CLASS,   8722 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE23RDTIERPARAGON_CLASS,   8723 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE24THTIERPARAGON_CLASS,   8724 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE25THTIERPARAGON_CLASS,   8725 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE26THTIERPARAGON_CLASS,   8726 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE27THTIERPARAGON_CLASS,   8727 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE28THTIERPARAGON_CLASS,   8728 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE29THTIERPARAGON_CLASS,   8729 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE30THTIERPARAGON_CLASS,   8730 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE31STTIERPARAGON_CLASS,   8731 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE32NDTIERPARAGON_CLASS,   8732 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE33RDTIERPARAGON_CLASS,   8733 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE34THTIERPARAGON_CLASS,   8734 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE35THTIERPARAGON_CLASS,   8735 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE36THTIERPARAGON_CLASS,   8736 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE37THTIERPARAGON_CLASS,   8737 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE38THTIERPARAGON_CLASS,   8738 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE39THTIERPARAGON_CLASS,   8739 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE40THTIERPARAGON_CLASS,   8740 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE41STTIERPARAGON_CLASS,   8741 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE42NDTIERPARAGON_CLASS,   8742 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE43RDTIERPARAGON_CLASS,   8743 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE44THTIERPARAGON_CLASS,   8744 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE45THTIERPARAGON_CLASS,   8745 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE46THTIERPARAGON_CLASS,   8746 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE47THTIERPARAGON_CLASS,   8747 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE48THTIERPARAGON_CLASS,   8748 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE49THTIERPARAGON_CLASS,   8749 },
            { WeenieClassName.W_LUMINOUSAMBEROFTHE50THTIERPARAGON_CLASS,   8750 },
        };
    }
}
