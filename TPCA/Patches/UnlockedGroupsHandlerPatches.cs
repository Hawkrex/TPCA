using HarmonyLib;
using SpaceCraft;
using TPCA.Archipelago;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(UnlockedGroupsHandler))]
    internal class UnlockedGroupsHandlerPatches
    {
        // Blocks blueprint unlocks
        [HarmonyPatch(nameof(UnlockedGroupsHandler.UnlockGroupGlobally))]
        [HarmonyPrefix]
        public static bool UnlockGroupGlobally_Prefix(Group group)
        {
            if(Plugin.DontPrefix)
            {
                // Don't skip the original method when called from the plugin
                return true;
            }

            Plugin.Log.LogInfo($"Unlocked location {group.GetGroupData().id}");

            Plugin.ArchipelagoClient.SendLocation(group.GetGroupData().id);

            return false;
        }
    }
}
