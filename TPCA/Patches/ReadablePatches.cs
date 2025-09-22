using HarmonyLib;
using SpaceCraft;
using TPCA.Datas;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(Readable))]
    internal static class ReadablePatches
    {
        [HarmonyPatch(nameof(Readable.GetGroupName))]
        [HarmonyPrefix]
        public static bool GetGroupName_Prefix(Group _group, ref string __result)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(GetGroupName_Prefix)} => Archipelago mode deactivated");
                return true;
            }

            if (_group is GroupLocation)
            {
                var apItem = Plugin.State.ItemByLocations[_group.id];

                __result = $"{apItem.PlayerName}'s {GetGroupName(apItem.Name)}";
                return false;
            }

            return true;
        }

        public static string GetGroupName(string groupId)
        {
            string localizedString = Localization.GetLocalizedString(GameConfig.localizationGroupNameId + groupId);

            return string.IsNullOrEmpty(localizedString) ? groupId : localizedString;
        }

        [HarmonyPatch(nameof(Readable.GetGroupDescription))]
        [HarmonyPrefix]
        public static bool GetGroupDescription_Prefix(Group _group, ref string __result)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(GetGroupDescription_Prefix)} => Archipelago mode deactivated");
                return true;
            }

            if (_group is GroupLocation)
            {
                __result = GetGroupDescription(Plugin.State.ItemByLocations[_group.id].Name);
                return false;
            }

            return true;
        }

        public static string GetGroupDescription(string groupId)
        {
            return Localization.GetLocalizedString(GameConfig.localizationGroupDescriptionId + groupId);
        }
    }
}
