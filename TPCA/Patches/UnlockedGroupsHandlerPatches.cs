using HarmonyLib;
using SpaceCraft;

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
            if (Plugin.ArchipelagoModeDeactivated
                || Plugin.DontPrefixUnlockGroupGlobally) // Execute the original method when called from the plugin
            {
                Plugin.Log.LogDebug($"{nameof(UnlockGroupGlobally_Prefix)} => Archipelago mode deactivated or method called from within the plugin (received item from AP server)");
                return true;
            }

            Plugin.Log.LogInfo($"Unlocked location {group.GetGroupData().id}");
            Plugin.ArchipelagoClient.SendLocation(group.GetGroupData().id);

            return false;
        }
    }
}
