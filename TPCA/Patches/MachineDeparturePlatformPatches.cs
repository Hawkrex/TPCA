using HarmonyLib;
using SpaceCraft;
using TPCA.Core;
using static SpaceCraft.MachineDeparturePlatform;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(MachineDeparturePlatform))]
    internal class MachineDeparturePlatformPatches
    {
        /// <summary>
        /// Executes beofre the method
        /// Called when player selects a departure on the departure platform's screen
        /// Send the completion flag to the server if the goal matches to the correct departure method
        /// </summary>
        /// <param name="endingType"></param>
        [HarmonyPatch(nameof(MachineDeparturePlatform.StartDepartureSequence))]
        [HarmonyPrefix]
        public static void StartDepartureSequence_Prefix(DepartureType endingType)
        {
            GameManager.CheckCompletion(endingType);
        }
    }
}
