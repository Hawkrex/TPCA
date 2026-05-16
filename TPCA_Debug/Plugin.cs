using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TPCA_Debug
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Log.LogInfo($"PluginName: {PluginInfo.PLUGIN_NAME}, VersionString: {PluginInfo.PLUGIN_VERSION} is loading...");

            Harmony.PatchAll();

            Log.LogInfo($"PluginName: {PluginInfo.PLUGIN_NAME}, VersionString: {PluginInfo.PLUGIN_VERSION} is loaded.");
        }

        private void OnGUI()
        {
            Debug.OnGUI();
        }
    }
}
