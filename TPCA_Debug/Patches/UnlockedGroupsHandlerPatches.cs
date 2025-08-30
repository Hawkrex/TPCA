using HarmonyLib;
using SpaceCraft;

namespace TPCA_Debug.Patches
{
    [HarmonyPatch(typeof(UnlockedGroupsHandler))]
    internal class UnlockedGroupsHandlerPatches
    {
        // Blocks blueprint unlocks
        /*[HarmonyPatch(nameof(UnlockedGroupsHandler.UnlockGroupGlobally))]
        [HarmonyPrefix]
        public static bool UnlockGroupGlobally_Prefix(Group group)
        {
            if(Debug.DontPrefix)
            {
                // Don't skip the original method when called from the plugin
                return true;
            }

            Plugin.Log.LogInfo($"UnlockedGroupsHandler > Unlock item : {Readable.GetGroupName(group)}");
            Debug.Location = Readable.GetGroupName(group);
            return false;
        }*/
    }
}
