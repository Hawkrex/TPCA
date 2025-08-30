using SpaceCraft;
using System;
using System.Collections.Generic;
using System.Text;
using static SpaceCraft.DataConfig;

namespace TPCA_Debug.JsonModels
{
    internal class GroupDataJson
    {
        public string Id { get; private set; }
        public string RecipeIngredients { get; private set; }
        public bool HideInCrafter { get; private set; }
        public string UnlockingWorldUnit { get; private set; }
        public float UnlockingValue { get; private set; }
        public TerraformStageJson TerraformStage { get; private set; }
        public string UnlockInPlanets { get; private set; }
        public GroupPlanetUsageType PlanetUsageType { get; private set; }
        public bool LootRecipeOnDeconstruct { get; private set; }
        public string TradeCategory { get; private set; }
        public int TradeValue { get; private set; }
        public int InventorySize { get; private set; }
        public string SecondaryInventoriesSize { get; private set; }

        public GroupDataJson(GroupData groupData)
        {
            try
            {
                Id = groupData.id;
                RecipeIngredients = GetRecipeIngredientsString(groupData.recipeIngredients);
                HideInCrafter = groupData.hideInCrafter;
                UnlockingWorldUnit = groupData.unlockingWorldUnit.ToString();
                UnlockingValue = groupData.unlockingValue;
                if(groupData.terraformStageUnlock != null)
                {
                    TerraformStage = new TerraformStageJson(groupData.terraformStageUnlock);
                }
                UnlockInPlanets = GetUnlockInPlanetsString(groupData.unlockInPlanets);
                PlanetUsageType = groupData.planetUsageType;
                LootRecipeOnDeconstruct = groupData.lootRecipeOnDeconstruct;
                TradeCategory = groupData.tradeCategory.ToString();
                TradeValue = groupData.tradeValue;
                InventorySize = groupData.inventorySize;
                SecondaryInventoriesSize = GetIntegersString(groupData.secondaryInventoriesSize);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo($"ex {ex}");
            }
        }

        private string GetRecipeIngredientsString(List<GroupDataItem> recipeIngredients)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in recipeIngredients)
            {
                stringBuilder.Append($"{item.name} + ");
            }

            if (stringBuilder.Length > 3)
            {
                stringBuilder.Remove(stringBuilder.Length - 3, 3);
            }

            return stringBuilder.ToString();
        }

        private string GetUnlockInPlanetsString(List<PlanetData> unlockInPlanets)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in unlockInPlanets)
            {
                stringBuilder.Append($"{Readable.GetPlanetLabel(item)} + ");
            }

            if (stringBuilder.Length > 3)
            {
                stringBuilder.Remove(stringBuilder.Length - 3, 3);
            }

            return stringBuilder.ToString();
        }

        private string GetIntegersString(List<int> integers)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in integers)
            {
                stringBuilder.Append($"{item} + ");
            }

            if (stringBuilder.Length > 3)
            {
                stringBuilder.Remove(stringBuilder.Length - 3, 3);
            }

            return stringBuilder.ToString();
        }
    }
}
