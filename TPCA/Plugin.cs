using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TPCA.Archipelago;
using TPCA.Patches;
using UnityEngine;

namespace TPCA
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static ArchipelagoClient ArchipelagoClient;
        internal static ArchipelagoGameState State;

        internal static bool ArchipelagoModeDeactivated;
        internal static bool DontPrefixUnlockGroupGlobally;

        private static readonly Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

        private static float update = 0;

        private void Awake()
        {
            Log = base.Logger;

            Log.LogInfo($"PluginName: {MyPluginInfo.PLUGIN_NAME}, VersionString: {MyPluginInfo.PLUGIN_VERSION} is loading...");

            harmony.PatchAll();

            State = new("localhost:38281", "Player1", string.Empty);
            ArchipelagoClient = new();

            Log.LogInfo($"PluginName: {MyPluginInfo.PLUGIN_NAME}, VersionString: {MyPluginInfo.PLUGIN_VERSION} is loaded.");
        }

        private void Update()
        {
            if (ArchipelagoModeDeactivated)
            {
                return;
            }

            update += Time.deltaTime;
            if (update > 1.0f) // update every second
            {
                update = 0.0f;
                GameManager.Update();
            }
        }

        private void OnGUI()
        {
            if (SaveFilesCreateNewPatches.ShowArchipelagoSettingsGUI)
            {
                ArchipelagoSettingsGUI.OnGUI();
            }
        }
    }
}
