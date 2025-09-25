using HarmonyLib;
using SpaceCraft;
using System;

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
                || Environment.StackTrace.Contains(nameof(Archipelago.GameManager.UnlockItem))) // Execute the original method when called from the plugin
            {
                Plugin.Log.LogDebug($"{nameof(UnlockGroupGlobally_Prefix)} => Archipelago mode deactivated or method called from within the plugin (received item from AP server)");
                return true;
            }

            Plugin.Log.LogInfo($"{nameof(UnlockGroupGlobally_Prefix)} => Unlocked microchip location {group.GetGroupData().id}");
            Plugin.ArchipelagoClient.SendLocation(group.GetGroupData().id);

            return false;
        }
    }
}
