using HarmonyLib;
using SpaceCraft;
using System;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(PopupsHandler))]
    internal static class PopupsHandlerPatches
    {
        [HarmonyPatch(nameof(PopupsHandler.PopNewUnlockable))]
        [HarmonyPrefix]
        public static bool PopNewUnlockable_Prefix(PopupsHandler __instance, Group _group)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(PopNewUnlockable_Prefix)} => Archipelago mode deactivated");
                return true;
            }

            if (Environment.StackTrace.Contains(nameof(Archipelago.GameManager.UnlockItem))) // Item received from AP server
            {
                Plugin.Log.LogDebug($"{nameof(PopNewUnlockable_Prefix)} => Received item from AP server, modify the popup");

                string message;
                if (!Plugin.State.ItemByLocations.ContainsKey(_group.id))
                {
                    message = $"Received {_group.id}";
                }
                else
                {
                    message = $"Received {_group.id} from {Plugin.State.ItemByLocations[_group.id].PlayerName}";
                }

                var popupData = new PopupData(_group.GetImage(), message, 5f);
                AccessTools.Method(typeof(PopupsHandler), "AddToList").Invoke(__instance, [popupData]);

                return false;
            }
            else if (Environment.StackTrace.Contains("AlertUnlockables")) // Sending location
            {
                if (!Plugin.State.ItemByLocations.ContainsKey(_group.id))
                {
                    Plugin.Log.LogWarning($"{nameof(PopNewUnlockable_Prefix)} => Couldn't find location <{_group.id}> in ItemByLocations dictionary");
                    return false;
                }
                else
                {
                    var itemInfos = Plugin.State.ItemByLocations[_group.id];
                    if (!itemInfos.IsLocal) // Send item to someone
                    {
                        Plugin.Log.LogDebug($"{nameof(PopNewUnlockable_Prefix)} => Location unlocked, modify the popup");

                        var popupData = new PopupData(_group.GetImage(), $"Send item {itemInfos.Name} to {itemInfos.PlayerName}", 5f);
                        AccessTools.Method(typeof(PopupsHandler), "AddToList").Invoke(__instance, [popupData]);
                    }
                    // Else, you'll get notified when receiving the item from the AP server
                    // In both case, no need to execute the original code
                    return false;
                }
            }

            Plugin.Log.LogDebug($"PopNewUnlockable_Prefix => {Environment.StackTrace}");
            return true;
        }
    }
}
