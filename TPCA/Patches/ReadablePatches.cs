using HarmonyLib;
using SpaceCraft;
using TPCA.Datas;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(Readable))]
    internal static class ReadablePatches
    {
        /// <summary>
        /// Executes before or override the method
        /// Called when the game needs to show a group name (translated) to the player
        /// Returns the PlayerName and the readable group name
        /// </summary>
        /// <param name="_group">Group to make readable</param>
        /// <param name="__result">Readable name with its owner</param>
        /// <returns>true if original method is executed after</returns>
        [HarmonyPatch(nameof(Readable.GetGroupName))]
        [HarmonyPrefix]
        public static bool GetGroupName_Prefix(Group _group, ref string __result)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
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

        /// <summary>
        /// Get the readable or translated name of a group
        /// </summary>
        /// <param name="groupId">Group Id to translate</param>
        /// <returns>The readable name of the group</returns>
        public static string GetGroupName(string groupId)
        {
            string localizedString = Localization.GetLocalizedString(GameConfig.localizationGroupNameId + groupId);

            return string.IsNullOrEmpty(localizedString) ? groupId : localizedString;
        }

        /// <summary>
        /// Executes before or override the method
        /// Called when the game needs to show a group description (translated) to the player
        /// TODO
        /// </summary>
        /// <param name="_group">Group to get description readable</param>
        /// <param name="__result">TODO</param>
        /// <returns>true if original method is executed after</returns>
        [HarmonyPatch(nameof(Readable.GetGroupDescription))]
        [HarmonyPrefix]
        public static bool GetGroupDescription_Prefix(Group _group, ref string __result)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return true;
            }

            if (_group is GroupLocation)
            {
                __result = GetGroupDescription(Plugin.State.ItemByLocations[_group.id].Name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the readable or translated description of a group
        /// </summary>
        /// <param name="groupId">Group Id to translate</param>
        /// <returns>The readable description of the group</returns>
        public static string GetGroupDescription(string groupId)
        {
            return Localization.GetLocalizedString(GameConfig.localizationGroupDescriptionId + groupId);
        }
    }
}
