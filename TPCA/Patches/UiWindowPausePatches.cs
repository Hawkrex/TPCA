using HarmonyLib;
using SpaceCraft;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(UiWindowPause))]
    internal static class UiWindowPausePatches
    {
        [HarmonyPatch(nameof(UiWindowPause.OnQuit))]
        [HarmonyPrefix]
        public static bool OnQuit_Prefix()
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(OnQuit_Prefix)} => Archipelago mode deactivated");
            }
            else
            {
                Plugin.ArchipelagoClient.Disconnect();
            }

            return true;
        }
    }
}
