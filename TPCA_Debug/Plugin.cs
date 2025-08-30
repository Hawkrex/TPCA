using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TPCA_Debug
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Log.LogInfo($"PluginName: {MyPluginInfo.PLUGIN_NAME}, VersionString: {MyPluginInfo.PLUGIN_VERSION} is loading...");

            Harmony.PatchAll();

            Log.LogInfo($"PluginName: {MyPluginInfo.PLUGIN_NAME}, VersionString: {MyPluginInfo.PLUGIN_VERSION} is loaded.");
        }

        private void OnGUI()
        {
            Debug.OnGUI();
        }
    }
}
