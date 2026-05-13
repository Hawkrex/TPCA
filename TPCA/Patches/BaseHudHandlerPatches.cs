using HarmonyLib;
using SpaceCraft;
using TMPro;
using UnityEngine;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(BaseHudHandler))]
    internal static class BaseHudHandlerPatches
    {
        private static TextMeshProUGUI text;

        /// <summary>
        /// Executes after the method
        /// Called when the HUD is created
        /// Create the Archipelago connection status text component
        /// </summary>
        /// <param name="___textBottomRight">Text component at the bottom right of the screen</param>
        [HarmonyPatch(nameof(BaseHudHandler.Start))]
        [HarmonyPostfix]
        public static void Start_Postfix(TextMeshProUGUI ___textBottomRight)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            CreateArchipelagoIdTextComponent(___textBottomRight);

            text.GetComponent<RectTransform>().localPosition += new Vector3(310, 30);
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(370, 50);
        }

        /// <summary>
        /// Executes after the method
        /// Called when the HUD is updated
        /// Updates the Archipelago connection status text
        /// </summary>
        [HarmonyPatch(nameof(BaseHudHandler.UpdateHud))]
        [HarmonyPostfix]
        public static void UpdateHud_Postfix()
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            string apStatus = Plugin.ArchipelagoClient.IsConnected ? "Connected" : "Disconnected";

            text.color = Plugin.ArchipelagoClient.IsConnected ? Color.green : Color.red;

            text.text = $"Archipelago status: {apStatus}";
        }

        /// <summary>
        /// Create a new text component above the bottom right text component
        /// </summary>
        /// <param name="___textBottomRight">Text component at the bottom right of the screen</param>
        private static void CreateArchipelagoIdTextComponent(TextMeshProUGUI ___textBottomRight)
        {
            var archipelagoConnectionStatus = new GameObject("ArchipelagoConnectionStatus");
            archipelagoConnectionStatus.transform.SetParent(___textBottomRight.transform, false);

            text = archipelagoConnectionStatus.AddComponent<TextMeshProUGUI>();
            text.font = ___textBottomRight.font;
            text.fontSize = ___textBottomRight.fontSize;
            text.overflowMode = TextOverflowModes.Ellipsis;

            archipelagoConnectionStatus.SetActive(true);
        }
    }
}
