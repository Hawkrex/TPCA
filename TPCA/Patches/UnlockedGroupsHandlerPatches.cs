using HarmonyLib;
using SpaceCraft;
using System;
using Unity.Netcode;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(UnlockedGroupsHandler))]
    internal class UnlockedGroupsHandlerPatches
    {
        // Blocks blueprint unlocks
        [HarmonyPatch(nameof(UnlockedGroupsHandler.UnlockGroupGlobally))]
        [HarmonyPrefix]
        public static bool UnlockGroupGlobally_Prefix(Group group, NetworkList<int> ____unlockedGroups)
        {
            if (Plugin.ArchipelagoModeDeactivated
                || Environment.StackTrace.Contains(nameof(Archipelago.GameManager.UnlockItem))) // Execute the original method when called from the plugin
            {
                Plugin.Log.LogDebug($"{nameof(UnlockGroupGlobally_Prefix)} => Archipelago mode deactivated or method called from within the plugin (received item from AP server)");
                return true;
            }

            Plugin.Log.LogInfo($"{nameof(UnlockGroupGlobally_Prefix)} => Unlocked microchip location {group.GetGroupData().id}");
            Plugin.ArchipelagoClient.SendLocation(group.GetGroupData().id);

            // Code from UnlockGroupGloballyServerRpc
            // Fixes microchip blueprints not working after Tier1
            if (!____unlockedGroups.Contains(group.stableHashCode))
            {
                ____unlockedGroups.Add(group.stableHashCode);
            }

            return false;
        }
    }
}
