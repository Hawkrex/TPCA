using SpaceCraft;
using System.Collections.Generic;
using System.Linq;
using TPCA_Debug.Exporters;
using UnityEngine;

namespace TPCA_Debug
{
    internal static class Debug
    {
        internal static string Location;

        internal static List<Group> AllGroups;

        internal static bool DontPrefix;

        internal static void OnGUI()
        {
            GUI.BeginGroup(new(Screen.width - 304, 4, 304, 204));
            GUI.Box(new(0, 0, 300, 200), string.Empty);
            GUI.Label(new(4, 4, 300, 20), "Archipelago debug menu");

            GUI.Label(new(4, 28, 300, 20), $"Received item : {Location}");

            var pressedButton = GUI.Button(new(4, 52, 300, 20), "Send PortalGenerator1");
            if (pressedButton)
            {
                SendItem("PortalGenerator1");
            }

            var pressedButton2 = GUI.Button(new(4, 76, 300, 20), "Send FlowerPot2");
            if (pressedButton2)
            {
                SendItem("FlowerPot2");
            }

            var exportGroupDataButton = GUI.Button(new(4, 100, 300, 20), "Export Group Datas to json");
            if (exportGroupDataButton)
            {
                JsonExporter.ExportGroupsDatas(AllGroups);
            }

            var exportArchipelagoDatasButton = GUI.Button(new(4, 124, 300, 20), "Export Group Datas to Archipelago");
            if (exportArchipelagoDatasButton)
            {
                ArchipelagoExporter.ExportGroupsDatas(AllGroups);
            }

            var exportArchipelagoDataNamesButton = GUI.Button(new(4, 148, 300, 20), "Export Group Data names to plain text");
            if (exportArchipelagoDataNamesButton)
            {
                PlainTextExporter.ExportGroupsDataNames(AllGroups);
            }

            GUI.EndGroup();
        }

        private static void SendItem(string groupId)
        {
            var groupToSend = AllGroups.FirstOrDefault(x => x.id == groupId);

            if (groupToSend != null)
            {
                // Boolean passed to the prefix patch to not skip the real method
                DontPrefix = true;
                UnlockedGroupsHandler.Instance.UnlockGroupGlobally(groupToSend);
                DontPrefix = false;
            }
        }
    }
}
