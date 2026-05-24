using HarmonyLib;
using SpaceCraft;

namespace TPCA_Debug.Patches
{
    [HarmonyPatch(typeof(WorldUnitsPlanet))]
    internal class WorldUnitsPlanetPatches
    {
        private static WorldUnitsPlanet instance;

        [HarmonyPatch(nameof(WorldUnitsPlanet.CreateUnits))]
        [HarmonyPostfix]
        public static void CreateUnits_Postfix(WorldUnitsPlanet __instance)
        {
            instance = __instance;
        }

        public static void SendNewValue(DataConfig.WorldUnitType unitType, float newValue)
        {
            instance.GetUnit(unitType).SetCurrentTotalValue(newValue);
        }
    }
}
