using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TPCA.Archipelago;
using TPCA.Patches;
using UnityEngine;

namespace TPCA
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static ArchipelagoClient ArchipelagoClient;
        internal static ArchipelagoGameState State;

        internal static bool ArchipelagoModeDeactivated;

        private static readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        private static float update = 0;

        private void Awake()
        {
            Log = base.Logger;

            Log.LogInfo($"Game version: {Application.version}");
            Log.LogInfo($"PluginName: {PluginInfo.PLUGIN_NAME}, VersionString: {PluginInfo.PLUGIN_VERSION} is loading...");

            harmony.PatchAll();

            State = new("localhost:38281", "Player1", string.Empty);
            ArchipelagoClient = new();

            Log.LogInfo($"PluginName: {PluginInfo.PLUGIN_NAME}, VersionString: {PluginInfo.PLUGIN_VERSION} is loaded.");
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

            if (UiWindowPausePatches.ShowArchipelagoSettingsGUI)
            {
                ArchipelagoReconnectionGUI.OnGUI();
            }
        }
    }
}
