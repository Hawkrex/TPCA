using HarmonyLib;
using SpaceCraft;
using System;
using Unity.Netcode;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(UnlockedGroupsHandler))]
    internal class UnlockedGroupsHandlerPatches
    {
        /// <summary>
        /// Executes before or override the method
        /// Called when the 
        /// Blocks microchip unlocks and send location unlocked to the server instead
        /// </summary>
        /// <param name="group">Group location to unlock</param>
        /// <param name="____unlockedGroups">List of unlocked groups</param>
        /// <returns>true if original method is executed after</returns>
        [HarmonyPatch(nameof(UnlockedGroupsHandler.UnlockGroupGlobally))]
        [HarmonyPrefix]
        public static bool UnlockGroupGlobally_Prefix(Group group, NetworkList<int> ____unlockedGroups)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return true;
            }

            if (Environment.StackTrace.Contains(nameof(Archipelago.GameManager.UnlockItem))) // Execute the original method when called from the plugin
            {
                Plugin.Log.LogDebug($"{nameof(UnlockGroupGlobally_Prefix)} => Method called from within the plugin (received item from AP server)");
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
