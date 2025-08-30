using HarmonyLib;
using SpaceCraft;
using System;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(UnlockingHandler))]
    internal class UnlockingHandlerPatches
    {
        /*[HarmonyPatch(nameof(UnlockingHandler.GetUnlockableGroupsUnderUnit))]
        [HarmonyPrefix]
        public static bool GetUnlockableGroupsUnderUnit_Prefix(DataConfig.WorldUnitType unitType, bool onlySpecificToCurrentPlanet)
        {
            return !Environment.StackTrace.Contains("AlertUnlockables"); // We block popup when unlocking something
        }*/
    }
}
