using BepInEx;
using HarmonyLib;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.IO;
using TPCA.Archipelago;
using UnityEngine;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(StaticDataHandler))]
    internal class StaticDataHandlerPatches
    {
        [HarmonyPatch(nameof(StaticDataHandler.LoadStaticData))]
        [HarmonyPrefix]
        public static bool LoadStaticData_Prefix(StaticDataHandler __instance)
        {
            var groupsData = __instance.staticAvailableObjects.groupsData;

            GameManager.AllGroups = CreateGroups(groupsData);

            RewriteGroups(groupsData);

            return false;
        }

        private static List<Group> CreateGroups(List<GroupData> groupsData)
        {
            List<Group> list = new List<Group>();
            foreach (GroupData groupData in groupsData)
            {
                if (!(groupData == null))
                {
                    if (groupData is GroupDataConstructible)
                    {
                        list.Add(new GroupConstructible((GroupDataConstructible)groupData));
                    }
                    else if (groupData is GroupDataItem)
                    {
                        list.Add(new GroupItem((GroupDataItem)groupData));
                    }
                }
            }
            return list;
        }

        private static void RewriteGroups(List<GroupData> groupsData)
        {
            Plugin.Log.LogInfo("RewriteGroups ...");

            var list = new List<Group>();

            foreach (var groupData in groupsData)
            {
                if (groupData is GroupDataConstructible constructible)
                {
                    if (ShouldReplace(groupData))
                    {
                        list.Add(new GroupConstructible(CreateAPLocationConstructible(CloneGroupDataConstructible(constructible))));
                        list.Add(new GroupConstructible(CreateAPItemConstructible(CloneGroupDataConstructible(constructible))));
                    }
                    else
                    {
                        list.Add(new GroupConstructible(constructible));
                    }
                }
                else if (groupData is GroupDataItem item)
                {
                    if (ShouldReplace(groupData))
                    {
                        list.Add(new GroupItem(CreateAPLocationItem(CloneGroupDataItem(item))));
                        list.Add(new GroupItem(CreateAPItemItem(CloneGroupDataItem(item))));
                    }
                    else
                    {
                        list.Add(new GroupItem(item));
                    }
                }
            }

            Plugin.Log.LogInfo("SetAllGroups ...");

            try
            {
                GroupsHandler.SetAllGroups(list);

                foreach (var group in GroupsHandler.GetAllGroups())
                {
                    foreach (var groupData2 in groupsData)
                    {
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

            Plugin.Log.LogInfo("SetAllGroups done");
        }

        private static bool ShouldReplace(GroupData groupData)
        {
            if (groupData.unlockingValue != 0)
            {
                return true;
            }

            return false;
        }

        private static GroupDataConstructible CreateAPLocationConstructible(GroupDataConstructible gd)
        {
            var texture = LoadTexture("ArchipelagoItem.png");

            gd.id = $"{gd.unlockingWorldUnit}_{gd.unlockingValue}";
            gd.icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
            gd.groupCategory = DataConfig.GroupCategory.Null;

            return gd;
        }

        private static GroupDataItem CreateAPLocationItem(GroupDataItem gd)
        {
            var texture = LoadTexture("ArchipelagoItem.png");

            gd.id = $"{gd.unlockingWorldUnit}_{gd.unlockingValue}";
            gd.icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);

            return gd;
        }

        private static GroupDataConstructible CreateAPItemConstructible(GroupDataConstructible gd)
        {
            gd.unlockingValue = 0;
            gd.unlockingWorldUnit = DataConfig.WorldUnitType.Null;
            
            return gd;
        }

        private static GroupDataItem CreateAPItemItem(GroupDataItem gd)
        {
            gd.unlockingValue = 0;
            gd.unlockingWorldUnit = DataConfig.WorldUnitType.Null;

            return gd;
        }

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
            newGroupData.terraStageRequirement = groupData.terraStageRequirement;
            newGroupData.notAllowedPlanetsRequirement = groupData.notAllowedPlanetsRequirement;
            newGroupData.notAllowedPlanetsRequirementTextId = groupData.notAllowedPlanetsRequirementTextId;
            newGroupData.groupCategory = groupData.groupCategory;
            newGroupData.worlUnitMultiplied = groupData.worlUnitMultiplied;

            return newGroupData;
        }

        // From https://github.com/jotunnlib/jotunnlib/blob/90b20cb85a1c981324891246375b6460c87f76db/JotunnLib/Utils/AssetUtils.cs/#L18
        public static Texture2D LoadTexture(string texturePath)
        {
            string path = Path.Combine(Paths.PluginPath, "TPCA", texturePath); // TODO : To change

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
