using BepInEx;
using HarmonyLib;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TPCA.Archipelago;
using TPCA.Datas;
using UnityEngine;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(StaticDataHandler))]
    internal class StaticDataHandlerPatches
    {
        private static Texture2D archipelagoTexture;
        private static Texture2D errorTexture;

        /// <summary>
        /// Executes before or override the method
        /// Called when the game save loads
        /// Rewrite all the groups to make the randomisation (GroupLocations, GroupConstructibles and GroupItems)
        /// </summary>
        /// <param name="__instance">Instance of the data handler</param>
        /// <returns>true if original method is executed after</returns>
        [HarmonyPatch(nameof(StaticDataHandler.LoadStaticData))]
        [HarmonyPrefix]
        public static bool LoadStaticData_Prefix(StaticDataHandler __instance)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return true;
            }

            if (!Plugin.ArchipelagoClient.IsConnected)
            {
                Plugin.Log.LogDebug($"{nameof(LoadStaticData_Prefix)} => Archipelago not connected to the AP server");
                return true;
            }

            var groupsData = __instance.staticAvailableObjects.groupsData;
            archipelagoTexture = LoadTexture("ArchipelagoItem.png");
            errorTexture = LoadTexture("ErrorTexture.png");



            GameManager.AllGroups = CreateGroups(groupsData); // First half of the original method (untouched)

            var list = RewriteGroups(groupsData); // Rewrite all the groups to make the randomisation locations and objects

            GroupsHandler.SetAllGroups(list); // Central line of the original method (untouched)

            SetGroupsRecipes(groupsData); // Second half of the original method (untouched)

            return false;
        }

        /// <summary>
        /// Create the Groups from the GroupDatas
        /// </summary>
        /// <param name="groupsData">Data to convert</param>
        /// <returns>List of groups</returns>
        private static List<Group> CreateGroups(List<GroupData> groupsData)
        {
            var list = new List<Group>();
            foreach (GroupData groupData in groupsData)
            {
                if (groupData != null)
                {
                    if (groupData is GroupDataConstructible constructible)
                    {
                        list.Add(new GroupConstructible(constructible));
                    }
                    else if (groupData is GroupDataItem item)
                    {
                        list.Add(new GroupItem(item));
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// For each archipelago location, create a GroupLocation and Group(Item or Constructible)
        /// For each non location, just create the normal Group(Item or Constructible)
        /// </summary>
        /// <param name="groupDatas">List of groupData to rewrite</param>
        private static List<Group> RewriteGroups(List<GroupData> groupDatas)
        {
            Plugin.Log.LogDebug($"{nameof(RewriteGroups)} ...");

            var list = new List<Group>();

            //Plugin.Log.LogDebug("Creating new objects ...");
            foreach (var groupData in groupDatas)
            {
                //Plugin.Log.LogDebug($"Processing {groupData.id} ...");

                if (groupData is GroupDataConstructible constructible)
                {
                    if (IsAnArchipelagoLocation(groupData))
                    {
                        Plugin.Log.LogDebug($"Add Archipelago Constructible {groupData.id}");
                        list.Add(new GroupLocation(CreateAPLocationConstructible(CloneGroupDataConstructible(constructible))));
                        list.Add(new GroupConstructible(CreateAPItemConstructible(CloneGroupDataConstructible(constructible))));
                    }
                    else
                    {
                        Plugin.Log.LogDebug($"Add normal Constructible {groupData.id}");
                        list.Add(new GroupConstructible(constructible));
                    }
                }
                else if (groupData is GroupDataItem item)
                {
                    if (IsAnArchipelagoLocation(groupData))
                    {
                        Plugin.Log.LogDebug($"Add Archipelago Item {groupData.id}");
                        list.Add(new GroupLocation(CreateAPLocationItem(CloneGroupDataItem(item))));
                        list.Add(new GroupItem(CreateAPItemItem(CloneGroupDataItem(item))));
                    }
                    else
                    {
                        Plugin.Log.LogDebug($"Add normal Item {groupData.id}");
                        list.Add(new GroupItem(item));
                    }
                }
            }

            Plugin.Log.LogDebug($"{nameof(RewriteGroups)} done");

            return list;
        }

        /// <summary>
        /// Call the SetRecipe of the groups
        /// </summary>
        /// <param name="groupsData">GroupDatas from which to make the recipes</param>
        private static void SetGroupsRecipes(List<GroupData> groupsData)
        {
            try
            {
                foreach (var group in GroupsHandler.GetAllGroups())
                {
                    foreach (var groupData2 in groupsData)
                    {
                        //Plugin.Log.LogDebug($"SetRecipe for {groupData2.id} ...");
                        if (groupData2 is not null && groupData2.id == group.id && groupData2.recipeIngredients is not null)
                        {
                            group.SetRecipe(new Recipe(groupData2.recipeIngredients));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error : {ex}");
            }
        }

        /// <summary>
        /// Determines if a groupData is an Archipelago location
        /// </summary>
        /// <param name="groupData">The GroupData to examine</param>
        /// <returns>true if the groupdata is an Archipelago location</returns>
        private static bool IsAnArchipelagoLocation(GroupData groupData)
        {
            return Plugin.ArchipelagoClient.GetLocationsNames().Contains(groupData.id);
        }

        /// <summary>
        /// Modify the GroupDataConstructible to make it an AP location
        /// </summary>
        /// <param name="gd">GroupDataConstructible to customise</param>
        /// <returns>The GroupDataConstructible customised</returns>
        private static GroupDataConstructible CreateAPLocationConstructible(GroupDataConstructible gd)
        {
            gd.icon = ModifyIcon(gd.id);
            gd.groupCategory = DataConfig.GroupCategory.Null;

            return gd;
        }

        /// <summary>
        /// Modify the GroupDataItem to make it an AP location
        /// </summary>
        /// <param name="gd">GroupDataItem to customise</param>
        /// <returns>The GroupDataItem customised</returns>
        private static GroupDataItem CreateAPLocationItem(GroupDataItem gd)
        {
            gd.icon = ModifyIcon(gd.id);

            return gd;
        }

        /// <summary>
        /// Modifies the sprite of a GroupData
        /// If the GroupData is a Planet Crafter object, then retrieves its sprite
        /// Else load the default Archipelago texture
        /// </summary>
        /// <param name="groupDataId">GroupData Id</param>
        /// <returns>A new sprite with either the Planet Crafter object texture or the Archipelago texture</returns>
        private static Sprite ModifyIcon(string groupDataId)
        {
            //Plugin.Log.LogDebug($"Modifying icon for {gd.id} ...");

            Texture2D texture;
            var apItem = Plugin.State.ItemByLocations[groupDataId];
            //Plugin.Log.LogDebug($"apItem.Name {apItem.Name}");

            if (apItem.IsTpcItem)
            {
                //Plugin.Log.LogDebug($"Loading default texture");
                texture = GameManager.AllGroups.FirstOrDefault(x => x.id == apItem.Name)?.GetGroupData().icon.texture;
            }
            else
            {
                //Plugin.Log.LogDebug($"Loading AP texture");
                texture = archipelagoTexture;
            }

            //Plugin.Log.LogDebug($"Icon modified");

            return Sprite.Create(texture ?? errorTexture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
        }

        /// <summary>
        /// Modify the GroupDataConstructible to make it an AP item
        /// </summary>
        /// <param name="gd">GroupDataConstructible to customise</param>
        /// <returns>The GroupDataConstructible customised</returns>
        private static GroupDataConstructible CreateAPItemConstructible(GroupDataConstructible gd)
        {
            gd.unlockingValue = 0;
            gd.unlockingWorldUnit = DataConfig.WorldUnitType.Null;

            return gd;
        }

        /// <summary>
        /// Modify the GroupDataItem to make it an AP item
        /// </summary>
        /// <param name="gd">GroupDataItem to customise</param>
        /// <returns>The GroupDataItem customised</returns>
        private static GroupDataItem CreateAPItemItem(GroupDataItem gd)
        {
            gd.unlockingValue = 0;
            gd.unlockingWorldUnit = DataConfig.WorldUnitType.Null;

            return gd;
        }

        /// <summary>
        /// Deep clone of a GroupDataItem
        /// </summary>
        /// <param name="groupData">GroupDataItem to clone</param>
        /// <returns>A clone of the GroupDataItem</returns>
        private static GroupDataItem CloneGroupDataItem(GroupDataItem groupData)
        {
            var newGroupData = new GroupDataItem();

            newGroupData.id = groupData.id;
            newGroupData.associatedGameObject = groupData.associatedGameObject;
            newGroupData.icon = groupData.icon;
            newGroupData.recipeIngredients = groupData.recipeIngredients;
            newGroupData.hideInCrafter = groupData.hideInCrafter;
            newGroupData.unlockingWorldUnit = groupData.unlockingWorldUnit;
            newGroupData.unlockingValue = groupData.unlockingValue;
            newGroupData.terraformStageUnlock = groupData.terraformStageUnlock;
            newGroupData.unlockInPlanets = groupData.unlockInPlanets;
            newGroupData.planetUsageType = groupData.planetUsageType;
            newGroupData.lootRecipeOnDeconstruct = groupData.lootRecipeOnDeconstruct;
            newGroupData.tradeCategory = groupData.tradeCategory;
            newGroupData.tradeValue = groupData.tradeValue;
            newGroupData.inventorySize = groupData.inventorySize;
            newGroupData.secondaryInventoriesSize = groupData.secondaryInventoriesSize;
            newGroupData.logisticInterplanetaryType = groupData.logisticInterplanetaryType;

            newGroupData.value = groupData.value;
            newGroupData.craftableInList = groupData.craftableInList;
            newGroupData.equipableType = groupData.equipableType;
            newGroupData.usableType = groupData.usableType;
            newGroupData.itemCategory = groupData.itemCategory;
            newGroupData.itemSubCategory = groupData.itemSubCategory;
            newGroupData.growableGroup = groupData.growableGroup;
            newGroupData.unlocksGroup = groupData.unlocksGroup;
            newGroupData.effectOnPlayer = groupData.effectOnPlayer;
            newGroupData.chanceToSpawn = groupData.chanceToSpawn;
            newGroupData.cantBeDestroyed = groupData.cantBeDestroyed;
            newGroupData.cantBeRecycled = groupData.cantBeRecycled;
            newGroupData.craftedInWorld = groupData.craftedInWorld;
            newGroupData.canBePickedUpFromWorldByDrones = groupData.canBePickedUpFromWorldByDrones;
            newGroupData.displayInLogisticType = groupData.displayInLogisticType;
            newGroupData.unitMultiplierOxygen = groupData.unitMultiplierOxygen;
            newGroupData.unitMultiplierPressure = groupData.unitMultiplierPressure;
            newGroupData.unitMultiplierHeat = groupData.unitMultiplierHeat;
            newGroupData.unitMultiplierEnergy = groupData.unitMultiplierEnergy;
            newGroupData.unitMultiplierPlants = groupData.unitMultiplierPlants;
            newGroupData.unitMultiplierInsects = groupData.unitMultiplierInsects;
            newGroupData.unitMultiplierAnimals = groupData.unitMultiplierAnimals;

            return newGroupData;
        }

        /// <summary>
        /// Deep clone of a GroupDataConstructible
        /// </summary>
        /// <param name="groupData">GroupDataConstructible to clone</param>
        /// <returns>A clone of the GroupDataConstructible</returns>
        private static GroupDataConstructible CloneGroupDataConstructible(GroupDataConstructible groupData)
        {
            var newGroupData = new GroupDataConstructible();

            newGroupData.id = groupData.id;
            newGroupData.associatedGameObject = groupData.associatedGameObject;
            newGroupData.icon = groupData.icon;
            newGroupData.recipeIngredients = groupData.recipeIngredients;
            newGroupData.hideInCrafter = groupData.hideInCrafter;
            newGroupData.unlockingWorldUnit = groupData.unlockingWorldUnit;
            newGroupData.unlockingValue = groupData.unlockingValue;
            newGroupData.terraformStageUnlock = groupData.terraformStageUnlock;
            newGroupData.unlockInPlanets = groupData.unlockInPlanets;
            newGroupData.planetUsageType = groupData.planetUsageType;
            newGroupData.lootRecipeOnDeconstruct = groupData.lootRecipeOnDeconstruct;
            newGroupData.tradeCategory = groupData.tradeCategory;
            newGroupData.tradeValue = groupData.tradeValue;
            newGroupData.inventorySize = groupData.inventorySize;
            newGroupData.secondaryInventoriesSize = groupData.secondaryInventoriesSize;
            newGroupData.logisticInterplanetaryType = groupData.logisticInterplanetaryType;

            newGroupData.unitGenerationOxygen = groupData.unitGenerationOxygen;
            newGroupData.unitGenerationPressure = groupData.unitGenerationPressure;
            newGroupData.unitGenerationHeat = groupData.unitGenerationHeat;
            newGroupData.unitGenerationEnergy = groupData.unitGenerationEnergy;
            newGroupData.unitGenerationPlants = groupData.unitGenerationPlants;
            newGroupData.unitGenerationInsects = groupData.unitGenerationInsects;
            newGroupData.unitGenerationAnimals = groupData.unitGenerationAnimals;
            newGroupData.nextTierGroup = groupData.nextTierGroup;
            newGroupData.rotationFixed = groupData.rotationFixed;
            newGroupData.terraStageRequirements = groupData.terraStageRequirements;
            newGroupData.notAllowedPlanetsRequirement = groupData.notAllowedPlanetsRequirement;
            newGroupData.notAllowedPlanetsRequirementTextId = groupData.notAllowedPlanetsRequirementTextId;
            newGroupData.groupCategory = groupData.groupCategory;
            newGroupData.worlUnitMultiplied = groupData.worlUnitMultiplied;

            return newGroupData;
        }

        /// <summary>
        /// Loads a custom texture for the plugin
        /// From https://github.com/jotunnlib/jotunnlib/blob/90b20cb85a1c981324891246375b6460c87f76db/JotunnLib/Utils/AssetUtils.cs/#L18
        /// </summary>
        /// <param name="texturePath">Path of the texture from the plugin folder</param>
        /// <returns>The specified texture loaded</returns>
        public static Texture2D LoadTexture(string texturePath)
        {
            string path = Path.Combine(Paths.PluginPath, "TPCA", texturePath);

            //Plugin.Log.LogDebug($"Loading AP texture at {path}");

            if (!File.Exists(path))
            {
                return null;
            }

            byte[] fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(1, 1);
            texture.LoadImage(fileData);
            return texture;
        }
    }
}
