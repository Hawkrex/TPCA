using HarmonyLib;
using SpaceCraft;
using System;
using TPCA.Core;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(PopupsHandler))]
    internal static class PopupsHandlerPatches
    {
        /// <summary>
        /// Executes before or override the method
        /// Called when a popup is created
        /// Modifies the message to be shown to the player in the popup
        /// </summary>
        /// <param name="__instance">Instance of the popup</param>
        /// <param name="_group">Group unlocked</param>
        /// <returns>true if original method is executed after</returns>
        [HarmonyPatch(nameof(PopupsHandler.PopNewUnlockable))]
        [HarmonyPrefix]
        public static bool PopNewUnlockable_Prefix(PopupsHandler __instance, Group _group)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return true;
            }

            if (Environment.StackTrace.Contains(nameof(GameManager.UnlockItem))) // Item received from AP server
            {
                Plugin.Log.LogDebug($"{nameof(PopNewUnlockable_Prefix)} => Received item from AP server, modifying the popup ...");

                string message;
                if (!Plugin.State.ItemByLocations.ContainsKey(_group.id))
                {
                    message = $"Received {_group.id}";
                }
                else
                {
                    message = $"Received {_group.id} from {Plugin.State.ItemByLocations[_group.id].PlayerName}";
                }

                Plugin.Log.LogDebug($"{nameof(PopNewUnlockable_Prefix)} => Message : {message}");

                var popupData = new PopupData(_group.GetImage(), message, 5f);
                AccessTools.Method(typeof(PopupsHandler), "AddToList").Invoke(__instance, [popupData]);

                return false;
            }

            //Plugin.Log.LogDebug($"{nameof(PopNewUnlockable_Prefix)} => StackTrace : {Environment.StackTrace}");
            return true;
        }
    }
}
