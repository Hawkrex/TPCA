using HarmonyLib;
using SpaceCraft;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(UiWindowPause))]
    internal static class UiWindowPausePatches
    {
        public static bool ShowArchipelagoSettingsGUI;

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

            ShowArchipelagoSettingsGUI = false;
            return true;
        }

        /// <summary>
        /// Executes after the method
        /// Called when the pause menu is opened
        /// Show the ArchipelagoSettingsGUI
        /// </summary>
        [HarmonyPatch(nameof(UiWindowPause.OnOpen))]
        [HarmonyPostfix]
        public static void OnOpen_Postfix()
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            ShowArchipelagoSettingsGUI = true;
        }

        /// <summary>
        /// Executes after the method
        /// Called when the pause menu is closed
        /// Hide the ArchipelagoSettingsGUI
        /// </summary>
        [HarmonyPatch(nameof(UiWindowPause.OnClose))]
        [HarmonyPostfix]
        public static void OnClose_Postfix()
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            ShowArchipelagoSettingsGUI = false;
        }
    }
}
