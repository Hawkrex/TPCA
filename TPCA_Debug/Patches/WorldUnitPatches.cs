using HarmonyLib;
using SpaceCraft;

namespace TPCA_Debug.Patches
{
    [HarmonyPatch(typeof(WorldUnit))]
    internal class WorldUnitPatches
    {
        [HarmonyPatch(nameof(WorldUnit.Compute))]
        [HarmonyPrefix]
        public static bool Compute_Prefix(DataConfig.WorldUnitType ____unitType)
        {
            switch (____unitType)
            {
                case DataConfig.WorldUnitType.Terraformation:
                    if (Debug.FreezeCurrentTerraformation)
                    {
                        return false;
                    }
                    break;
                case DataConfig.WorldUnitType.Oxygen:
                    if (Debug.FreezeCurrentOxygen)
                    {
                        return false;
                    }
                    break;
                case DataConfig.WorldUnitType.Heat:
                    if (Debug.FreezeCurrentHeat)
                    {
                        return false;
                    }
                    break;
                case DataConfig.WorldUnitType.Pressure:
                    if (Debug.FreezeCurrentPressure)
                    {
                        return false;
                    }
                    break;
                case DataConfig.WorldUnitType.Biomass:
                    if (Debug.FreezeCurrentBiomass)
                    {
                        return false;
                    }
                    break;
                case DataConfig.WorldUnitType.Plants:
                    if (Debug.FreezeCurrentPlants)
                    {
                        return false;
                    }
                    break;
                case DataConfig.WorldUnitType.Insects:
                    if (Debug.FreezeCurrentInsects)
                    {
                        return false;
                    }
                    break;
                case DataConfig.WorldUnitType.Animals:
                    if (Debug.FreezeCurrentAnimals)
                    {
                        return false;
                    }
                    break;
            }

            return true;
        }

        [HarmonyPatch(nameof(WorldUnit.Compute))]
        [HarmonyPostfix]
        public static void Compute_Postfix(float ____currentTotalValue, DataConfig.WorldUnitType ____unitType)
        {
            switch (____unitType)
            {
                case DataConfig.WorldUnitType.Terraformation:
                    Debug.CurrentTerraformationValue = ____currentTotalValue;
                    break;
                case DataConfig.WorldUnitType.Oxygen:
                    Debug.CurrentOxygenValue = ____currentTotalValue;
                    break;
                case DataConfig.WorldUnitType.Heat:
                    Debug.CurrentHeatValue = ____currentTotalValue;
                    break;
                case DataConfig.WorldUnitType.Pressure:
                    Debug.CurrentPressureValue = ____currentTotalValue;
                    break;
                case DataConfig.WorldUnitType.Biomass:
                    Debug.CurrentBiomassValue = ____currentTotalValue;
                    break;
                case DataConfig.WorldUnitType.Plants:
                    Debug.CurrentPlantsValue = ____currentTotalValue;
                    break;
                case DataConfig.WorldUnitType.Insects:
                    Debug.CurrentInsectsValue = ____currentTotalValue;
                    break;
                case DataConfig.WorldUnitType.Animals:
                    Debug.CurrentAnimalsValue = ____currentTotalValue;
                    break;
            }
        }
    }
}
