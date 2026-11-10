using HarmonyLib;
using Newtonsoft.Json;
using SpaceCraft;
using System;
using System.IO;
using System.Linq;
using TPCA.Archipelago;
using TPCA.Core;
using UnityEngine;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(JSONExport))]
    internal static class JSONExportPatches
    {
        /// <summary>
        /// Executes before the method
        /// Called when creating a new game
        /// Modify the game settings to skip the intro landing
        /// </summary>
        /// <param name="gameSettings">Game settings</param>
        [HarmonyPatch(nameof(JSONExport.CreateNewSaveFile))]
        [HarmonyPrefix]
        public static void CreateNewSaveFile_Prefix(JsonableGameState gameSettings)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            gameSettings.hasPlayedIntro = true;
        }

        /// <summary>
        /// Executes after the method
        /// Called when creating a new game
        /// Save Archipelago informations to the end of the game save file
        /// </summary>
        /// <param name="saveFileName">Save file name</param>
        [HarmonyPatch(nameof(JSONExport.CreateNewSaveFile))]
        [HarmonyPostfix]
        public static void CreateNewSaveFile_Postfix(string saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            SaveArchipelagoSaveDatas(saveFileName);
        }

        /// <summary>
        /// Executes after the method
        /// Called when game is saved
        /// Save Archipelago informations to the end of the game save file
        /// </summary>
        /// <param name="_saveFileName">Save file name</param>
        [HarmonyPatch(nameof(JSONExport.SaveToJson))]
        [HarmonyPostfix]
        public static void SaveToJson_Postfix(string _saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            SaveArchipelagoSaveDatas(_saveFileName);
        }

        /// <summary>
        /// Saves the Archipelago informations at the end of the save file
        /// </summary>
        /// <param name="saveFileName">Save file name</param>
        private static void SaveArchipelagoSaveDatas(string saveFileName)
        {
            Plugin.Log.LogDebug($"{nameof(SaveArchipelagoSaveDatas)} => Saving file <{saveFileName}> ...");

            string archipelagoDataJson = string.Empty;
            try
            {
                archipelagoDataJson = JsonConvert.SerializeObject(GameManager.State);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogFatal($"{nameof(SaveArchipelagoSaveDatas)} => Exception while trying to serialize Archipelago save datas : <{ex}>");
            }

            string filePath = GetFullSaveFilePath(saveFileName);
            try
            {
                File.AppendAllText(filePath, $"\r\n{archipelagoDataJson}\r\n@");

                Plugin.Log.LogDebug($"{nameof(SaveArchipelagoSaveDatas)} => Saved file <{saveFileName}>");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"{nameof(SaveArchipelagoSaveDatas)} => Error saving archipelago datas into save <{saveFileName}> : <{ex}>");
            }
        }

        /// <summary>
        /// Executes after the method
        /// Called when save is loaded
        /// Load Archipelago informations at the end of the game save file
        /// </summary>
        /// <param name="_saveFileName">Save file name</param>
        [HarmonyPatch(nameof(JSONExport.LoadFromJson))]
        [HarmonyPostfix]
        public static void LoadFromJson_Postfix(string _saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            Plugin.Log.LogDebug($"{nameof(LoadFromJson_Postfix)} => Loading file <{_saveFileName}> ...");

            if (Plugin.ArchipelagoDatasBySaveNames.ContainsKey(_saveFileName))
            {
                Plugin.Log.LogDebug($"{nameof(LoadFromJson_Postfix)} => Infos already loaded for save <{_saveFileName}>");
                return;
            }

            try
            {
                string filePath = GetFullSaveFilePath(_saveFileName);
                var fileContent = File.ReadLines(filePath);
                string archipelagoDataJson = fileContent.ElementAt(fileContent.Count() - 2);
                
                if(archipelagoDataJson.StartsWith("{\"ModName\":\"TPCA\""))
                {
                    Plugin.ArchipelagoDatasBySaveNames[_saveFileName] = JsonConvert.DeserializeObject<ArchipelagoData>(archipelagoDataJson);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogFatal($"{nameof(LoadFromJson_Postfix)} => Exception <{ex}>");
            }

            Plugin.Log.LogDebug($"{nameof(LoadFromJson_Postfix)} => Loaded file <{_saveFileName}>");
        }

        /// <summary>
        /// Get full save file path
        /// </summary>
        /// <param name="saveFileName">Save file name</param>
        /// <returns>The full save file path</returns>
        private static string GetFullSaveFilePath(string saveFileName)
        {
            return $"{Application.persistentDataPath}/{saveFileName}.json";
        }

        /// <summary>
        /// Executes after the method
        /// Called when save is deleted
        /// Delete Archipelago informations internaly
        /// </summary>
        /// <param name="_saveFileName">Save file name</param>
        [HarmonyPatch(nameof(JSONExport.DeleteSaveFile))]
        [HarmonyPostfix]
        public static void DeleteSaveFile_Postfix(string _saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            Plugin.ArchipelagoDatasBySaveNames.Remove(_saveFileName);
        }
    }
}
