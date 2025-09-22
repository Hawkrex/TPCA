using HarmonyLib;
using SpaceCraft;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(SaveFilesCreateNew))]
    internal class SaveFilesCreateNewPatches
    {
        public static bool ShowArchipelagoSettingsGUI => archipelagoModeCheckbox.GetStatus() && instance.isActiveAndEnabled;

        private static bool isAlreadyInit;

        private static UiButtonBool archipelagoModeCheckbox;

        private static SaveFilesCreateNew instance;

        [HarmonyPatch(nameof(SaveFilesCreateNew.CreateNewFile))]
        [HarmonyPrefix]
        public static bool CreateNewFile_Prefix(SaveFilesCreateNew __instance, JsonableGameState ____jsonableGameSettings, string ____fileName)
        {
            // If we are not in Archipelago mode, just execute original code
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(CreateNewFile_Prefix)} => Archipelago mode deactivated");
                return true;
            }

            // If we are connected to the AP server, we generate a guid that we store on the AP server and in the game save (original code execution + JSONExportPatches.CreateNewSaveFile_Postfix)
            if ((ArchipelagoSettingsGUI.State == ArchipelagoSettingsState.ConnectedButGuidExists || ArchipelagoSettingsGUI.State == ArchipelagoSettingsState.Connected) && Plugin.ArchipelagoClient.IsConnected)
            {
                if (Plugin.ArchipelagoClient.CreateAndStoreGuid())
                {
                    Plugin.ArchipelagoClient.Disconnect();
                    return true;
                }
                else
                {
                    Plugin.Log.LogError("Trying to communicate with AP server but client is not connected");
                }
            }

            // Else warn user he is not connected to AP server and do nothing else
            ArchipelagoSettingsGUI.State = ArchipelagoSettingsState.NotConnected;
            return false;
        }

        // Add an archipelago mode checkbutton that brings up the ArchipelagoSettingsGUI on the planet save creation menu
        [HarmonyPatch(nameof(SaveFilesCreateNew.Init))]
        [HarmonyPostfix]
        public static void Init_Postfix(SaveFilesCreateNew __instance, Button ___settingsButton)
        {
            ArchipelagoSettingsGUI.ResetGUI();

            if (!isAlreadyInit) // Init can be triggered multiple times but we don't want to add an infinite of checkboxes
            {
                isAlreadyInit = true;
                instance = __instance;

                // Added a chebox to let user create a classic game
                archipelagoModeCheckbox = UnityEngine.Object.Instantiate(__instance.gameSettingsUi.buttonBoolGameObject, ___settingsButton.transform.GetParent().transform).GetComponent<UiButtonBool>();
                archipelagoModeCheckbox.SetState(true);
                archipelagoModeCheckbox.SetButtonBoolLabel("Archipelago Mode");
                archipelagoModeCheckbox.uiTooltip = null;
                archipelagoModeCheckbox.transform.position = new Vector3(618, 442);
                archipelagoModeCheckbox.onChangeButtonBool = (OnChangeButtonBool)Delegate.Combine(archipelagoModeCheckbox.onChangeButtonBool, new OnChangeButtonBool(OnArchipelagoModeChecked));

                var checkbox = archipelagoModeCheckbox.transform.GetChild(1);
                checkbox.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.9f);
                checkbox.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 0);

                // Lower the settings button to make place for the AP button
                ___settingsButton.transform.position = new Vector3(652, 348);
            }

            //CreateArchipelagoMenuFromScratch(___settingsButton.transform.GetParent().gameObject);
        }

        private static void OnArchipelagoModeChecked()
        {
            Plugin.Log.LogInfo($"Archipelago mode checkbox state <{archipelagoModeCheckbox.GetStatus()}>");
            Plugin.ArchipelagoModeDeactivated = !archipelagoModeCheckbox.GetStatus();
        }

        /*private static void CreateArchipelagoMenuFromScratch(GameObject newFileContainer)
        {
            // Create frame
            var archipelagoContainer = new GameObject("ArchipelagoContainer");
            archipelagoContainer.transform.SetParent(newFileContainer.transform, false);
            archipelagoContainer.transform.position = new Vector3(600, 400);

            var archipelagoContainerImage = archipelagoContainer.AddComponent<Image>();
            var newFileContainerImage = newFileContainer.GetComponent<Image>();
            archipelagoContainerImage.sprite = newFileContainerImage.sprite;

            var archipelagoContainerRectTransform = archipelagoContainer.GetComponent<RectTransform>();
            var newFileContainerRectTransform = newFileContainer.GetComponent<RectTransform>();
            archipelagoContainerRectTransform.sizeDelta = new Vector2(newFileContainerRectTransform.sizeDelta.x, newFileContainerRectTransform.sizeDelta.y);

            // Create title
            var archipelagoSettingsTitle = new GameObject("ArchipelagoSettingsTitle");
            archipelagoSettingsTitle.transform.SetParent(newFileContainer.transform, false);
            var text = archipelagoSettingsTitle.AddComponent<TextMeshProUGUI>();
            text.text = "Archipelago Settings";
        }

        private static void CreteMenuFromExisting(GameObject newFileContainer)
        {
            // Copy UI frame
            var archipelagoContainer = UnityEngine.Object.Instantiate<GameObject>(newFileContainer, newFileContainer.transform);

            archipelagoContainer.GetComponent<RectTransform>().position = new Vector3(820, 540);

            // Delete unused child frame elements
            for (int i = 7; i > 1; i--)
            {
                var gameObjectToDestroy = archipelagoContainer.transform.GetChild(i).gameObject;
                UnityEngine.Object.DestroyImmediate(gameObjectToDestroy);
            }

            // Trying to change text (works but gets overriden after)
            archipelagoContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "AP Settings";

            // Modify existing text input and label into Hostname
            var hostnameObject = archipelagoContainer.transform.GetChild(1).gameObject;
            hostnameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Hostname";

            // Add Password text input
            var passwordObject = UnityEngine.Object.Instantiate<GameObject>(hostnameObject, hostnameObject.transform.GetParent().transform);
            passwordObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Password";
            passwordObject.transform.position = new Vector3(1180, 500);

            // Should add password but the rest doesn't work so ...

            archipelagoContainer.SetActive(true);
        }*/
    }
}
