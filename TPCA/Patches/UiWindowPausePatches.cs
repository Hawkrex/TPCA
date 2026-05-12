using HarmonyLib;
using SpaceCraft;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(UiWindowPause))]
    internal static class UiWindowPausePatches
    {
        /// <summary>
        /// Executes before the method
        /// Called when the game is quitted
        /// Disconnect from the Archipelago server when quitting the game
        /// </summary>
        /// <returns>true to execute the original method after</returns>
        [HarmonyPatch(nameof(UiWindowPause.OnQuit))]
        [HarmonyPrefix]
        public static bool OnQuit_Prefix()
        {
            if (!Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.ArchipelagoClient.Disconnect();
            }

            return true;
        }
    }
}
