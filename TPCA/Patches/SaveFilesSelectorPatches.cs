using HarmonyLib;
using SpaceCraft;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(SaveFilesSelector))]
    internal static class SaveFilesSelectorPatches
    {
        [HarmonyPatch(nameof(SaveFilesSelector.SelectedSaveFile))]
        [HarmonyPostfix]
        public static void SelectedSaveFile_Postfix(string fileName)
        {
            Plugin.Log.LogDebug($"{nameof(SelectedSaveFile_Postfix)} => Selected save <{fileName}>");

            Plugin.ArchipelagoModeDeactivated = !JSONExportPatches.ArchipelagoInfosByNames.ContainsKey(fileName);
            
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(SelectedSaveFile_Postfix)} => Archipelago mode deactivated");
                return;
            }

            Plugin.State.SaveInfos = JSONExportPatches.ArchipelagoInfosByNames[fileName];

            Plugin.State.Host = Plugin.State.SaveInfos.Host;
            Plugin.State.PlayerName = Plugin.State.SaveInfos.PlayerName;
            Plugin.State.Password = Plugin.State.SaveInfos.Password;

            if (Plugin.ArchipelagoClient.Connect())
            {
                Plugin.Log.LogInfo("Automatic connection on save loading successful");
            }

            // TODO Check GUID
        }
    }
}
