using Archipelago.MultiClient.Net;
using HarmonyLib;
using SpaceCraft;

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

            Plugin.ArchipelagoModeDeactivated = !JSONExportPatches.ArchipelagoDatasBySaveNames.ContainsKey(fileName);

            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            Plugin.State.SaveDatas = JSONExportPatches.ArchipelagoDatasBySaveNames[fileName];

            Plugin.State.Host = Plugin.State.SaveDatas.Host;
            Plugin.State.PlayerName = Plugin.State.SaveDatas.PlayerName;
            Plugin.State.Password = Plugin.State.SaveDatas.Password;

            var loginResult = Plugin.ArchipelagoClient.TryConnect();
            if (loginResult is LoginSuccessful loginSuccessful)
            {
                if(Plugin.State.SaveDatas.Guid == Plugin.ArchipelagoClient.GetServerGuid())
                {
                    Plugin.Log.LogWarning($"{nameof(SelectedSaveFile_Postfix)} => Loading a save where the GUID doesn't match with the server automatically connected to, please check the informations of the server you are connected to!");
                }

                Plugin.ArchipelagoClient.OnConnect(loginSuccessful);
                Plugin.Log.LogInfo($"{nameof(SelectedSaveFile_Postfix)} => Automatic connection on save loading successful");
            }
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
