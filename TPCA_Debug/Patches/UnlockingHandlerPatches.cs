using HarmonyLib;
using SpaceCraft;
using System;
using System.Text;

namespace TPCA_Debug.Patches
{
    [HarmonyPatch(typeof(UnlockingHandler))]
    internal class UnlockingHandlerPatches
    {
        [HarmonyPatch(nameof(UnlockingHandler.GetUnlockableGroupsUnderUnit))]
        [HarmonyPrefix]
        public static bool GetUnlockableGroupsUnderUnit_Prefix(DataConfig.WorldUnitType unitType, bool onlySpecificToCurrentPlanet)
        {
            if (!Environment.StackTrace.Contains("AlertUnlockables"))
            {
                Plugin.Log.LogInfo($"GetUnlockableGroupsUnderUnit > StackTrace {Environment.StackTrace}");
                return true;
            }

            Plugin.Log.LogInfo($"GetUnlockableGroupsUnderUnit > unitType {unitType} / onlySpecificToCurrentPlanet {onlySpecificToCurrentPlanet}");

            var allGroups = GroupsHandler.GetAllGroups();

            var stringBuilder = new StringBuilder();
            foreach (var group in allGroups)
            {
                bool isUnlocked = group.GetUnlockingInfos().GetIsUnlocked(false);
                if (isUnlocked)
                {
                    stringBuilder.Append($"{Readable.GetGroupName(group)} + ");
                }

                Plugin.Log.LogInfo($"GetUnlockableGroupsUnderUnit > group {group.id} /  GetIsUnlocked {isUnlocked}");
            }
            Plugin.Log.LogInfo($"GetUnlockableGroupsUnderUnit > Location {stringBuilder.ToString()}");
            Debug.Location = stringBuilder.ToString();
            return true;
        }
    }
}
