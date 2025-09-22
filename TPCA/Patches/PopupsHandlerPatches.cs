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

            if (Environment.StackTrace.Contains("AlertUnlockables"))
            {
                Plugin.Log.LogDebug($"{nameof(PopNewUnlockable_Prefix)} => Location unlocked, modify the popup");

                var popupData = new PopupData(_group.GetImage(), $"Send location to AP server : {_group.id}", 5f);
                AccessTools.Method(typeof(PopupsHandler), "AddToList").Invoke(__instance, [popupData]);

                return false;
            }
            else
            {
                Plugin.Log.LogDebug($"{nameof(PopNewUnlockable_Prefix)} => TPC object unlocked, do not modify the popup");
                return true;
            }
        }
    }
}
