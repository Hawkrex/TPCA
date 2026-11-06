using HarmonyLib;
using SpaceCraft;
using TPCA.Core;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(SaveFilesSelector))]
    internal static class SaveFilesSelectorPatches
    {
        /// <summary>
        /// Executes after the method
        /// Called when the save file is selected
        /// Retrieve the Archipelago informations and connect to the server
        /// </summary>
        /// <param name="fileName">Save file name</param>
        [HarmonyPatch(nameof(SaveFilesSelector.SelectedSaveFile))]
        [HarmonyPostfix]
        public static void SelectedSaveFile_Postfix(string fileName)
        {
            Plugin.Log.LogDebug($"{nameof(SelectedSaveFile_Postfix)} => Selected save <{fileName}>");

            Plugin.ArchipelagoModeDeactivated = !Plugin.ArchipelagoDatasBySaveNames.ContainsKey(fileName);

            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            GameManager.LoadSave(fileName);
        }

        /// <summary>
        /// Executes before the method
        /// Called when the Create New File menu is closed
        /// Hide the AP settings GUI
        /// </summary>
        [HarmonyPatch(nameof(SaveFilesSelector.AbortCreateNewFile))]
        [HarmonyPrefix]
        public static void AbortCreateNewFile_Prefix()
        {
            Plugin.ArchipelagoClient.Disconnect();
            SaveFilesCreateNewPatches.ShowArchipelagoSettingsGUI = false;
        }
    }
}
