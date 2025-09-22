using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using SpaceCraft;
using TPCA_Debug.JsonModels;

namespace TPCA_Debug.Patches
{
    [HarmonyPatch(typeof(GroupsHandler))]
    internal class GroupsHandlerPatches
    {
        [HarmonyPatch(nameof(GroupsHandler.SetAllGroups))]
        [HarmonyPostfix]
        public static void SetAllGroups_Postfix(List<Group> groups)
        {
            Debug.AllGroups = groups;
        }

        private static void ExportInLog(List<Group> groups)
        {
            foreach (var group in groups)
            {
                Plugin.Log.LogInfo($"name = {Readable.GetGroupName(group)}");
                Plugin.Log.LogInfo($"description = {Readable.GetGroupDescription(group)}");
                Plugin.Log.LogInfo($"stableHashCode = {group.stableHashCode}");
                Plugin.Log.LogInfo($"id = {group.id}");
                Plugin.Log.LogInfo($"GetRecipe = {GetRecipeString(group.GetRecipe())}");
                Plugin.Log.LogInfo($"GetUnlockingInfos = {group.GetUnlockingInfos()}");
                Plugin.Log.LogInfo($"GetInventorySize = {group.GetInventorySize()}");
                Plugin.Log.LogInfo($"GetSecondaryInventoriesSize = {GetIntegersString(group.GetSecondaryInventoriesSize())}");
                Plugin.Log.LogInfo($"GetLogisticInterplanetaryType = {group.GetLogisticInterplanetaryType()}");
                Plugin.Log.LogInfo($"GetIsGloballyUnlocked = {group.GetIsGloballyUnlocked()}");
                Plugin.Log.LogInfo($"GetHideInCrafter = {group.GetHideInCrafter()}");
                Plugin.Log.LogInfo($"GetTradeCategory = {group.GetTradeCategory()}");
                Plugin.Log.LogInfo($"GetTradeValue = {group.GetTradeValue()}");
                Plugin.Log.LogInfo($"GetLootRecipeOnDeconstruct = {group.GetLootRecipeOnDeconstruct()}");
                Plugin.Log.LogInfo($"GetCanConsume {group.GetCanConsume()}");
                Plugin.Log.LogInfo($"GetCanUse = {group.GetCanUse()}");

                // group.GetImage()

                Plugin.Log.LogInfo($"---------------------------");

                var groupData = group.GetGroupData();
                Plugin.Log.LogInfo($"id = {groupData.id}");
                Plugin.Log.LogInfo($"recipeIngredients = {GetRecipeIngredientsString(groupData.recipeIngredients)}");
                Plugin.Log.LogInfo($"hideInCrafter = {groupData.hideInCrafter}");
                Plugin.Log.LogInfo($"unlockingWorldUnit = {groupData.unlockingWorldUnit}");
                Plugin.Log.LogInfo($"unlockingValue = {groupData.unlockingValue}");
                //Plugin.Log.LogInfo($"terraformStageUnlock = {Readable.GetTerraformStageName(groupData.terraformStageUnlock)}");
                Plugin.Log.LogInfo($"unlockInPlanets = {GetUnlockInPlanetsString(groupData.unlockInPlanets)}");
                Plugin.Log.LogInfo($"planetUsageType = {groupData.planetUsageType}");
                Plugin.Log.LogInfo($"lootRecipeOnDeconstruct = {groupData.lootRecipeOnDeconstruct}");
                Plugin.Log.LogInfo($"tradeCategory = {groupData.tradeCategory}");
                Plugin.Log.LogInfo($"tradeValue = {groupData.tradeValue}");
                Plugin.Log.LogInfo($"inventorySize = {groupData.inventorySize}");
                Plugin.Log.LogInfo($"secondaryInventoriesSize = {GetIntegersString(groupData.secondaryInventoriesSize)}");

                // groupData.icon

                Plugin.Log.LogInfo($"_______________________________________________________");
            }
        }

        private static string GetRecipeString(Recipe recipe)
        {
            /*var stringBuilder = new StringBuilder();

            foreach (var item in recipe.GetIngredientsGroupInRecipe())
            {
                stringBuilder.Append($"{Readable.GetGroupName(item)} + ");
            }
            stringBuilder.Remove(stringBuilder.Length - 3, 3);

            return stringBuilder.ToString();*/
            return string.Empty;
        }

        private static string GetRecipeIngredientsString(List<GroupDataItem> recipeIngredients)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in recipeIngredients)
            {
                stringBuilder.Append($"{item.name} + ");
            }
            stringBuilder.Remove(stringBuilder.Length - 3, 3);

            return stringBuilder.ToString();
        }

        private static string GetUnlockInPlanetsString(List<PlanetData> unlockInPlanets)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in unlockInPlanets)
            {
                stringBuilder.Append($"{Readable.GetPlanetLabel(item)} + ");
            }
            stringBuilder.Remove(stringBuilder.Length - 3, 3);

            return stringBuilder.ToString();
        }

        private static string GetIntegersString(List<int> integers)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in integers)
            {
                stringBuilder.Append($"{item} + ");
            }
            stringBuilder.Remove(stringBuilder.Length - 3, 3);

            return stringBuilder.ToString();
        }
    }
}
