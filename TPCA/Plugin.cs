using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TPCA.Archipelago;
using UnityEngine;

namespace TPCA
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        internal static ArchipelagoClient ArchipelagoClient;

        internal static ArchipelagoGameState State;

        internal static bool DontPrefix;

        private static readonly Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

        private static ConfigEntry<string> configUri;
        private static ConfigEntry<string> configSlotName;
        private static ConfigEntry<string> configPassword;

        private static float update = 0;

        private void Awake()
        {
            Log = base.Logger;

            Log.LogInfo($"PluginName: {MyPluginInfo.PLUGIN_NAME}, VersionString: {MyPluginInfo.PLUGIN_VERSION} is loading...");

            configUri = Config.Bind("Archipelago", "uri", "archipelago.gg:38281", "The address and port of the archipelago server to connect to");
            configSlotName = Config.Bind("Archipelago", "slotName", "Player1", "The slot name of the player you are connecting as");
            configPassword = Config.Bind("Archipelago", "password", string.Empty, "The password for the player you are connecting as");

            harmony.PatchAll();

            State = new("localhost:38281", "Hawkrex", string.Empty);
            ArchipelagoClient = new();

            Log.LogInfo($"PluginName: {MyPluginInfo.PLUGIN_NAME}, VersionString: {MyPluginInfo.PLUGIN_VERSION} is loaded.");
        }

        private void Update()
        {
            update += Time.deltaTime;
            if (update > 1.0f) // update every second
            {
                update = 0.0f;
                GameManager.Update();
            }
        }

        private void OnGUI()
        {
            if (ArchipelagoClient.IsConnected)
            {
                GUI.BeginGroup(new(Screen.width - 304, 10, 304, 48));
                GUI.Box(new(0, 0, 300, 48), "");

                GUI.Label(new(4, 0, 300, 20), $"TPCA (F1 for Debug)");
                GUI.Label(new(4, 24, 300, 20), $"Status: Connected");

                GUI.EndGroup();
                return;
            }

            GUI.BeginGroup(new(Screen.width - 308, 10, 308, 128));

            GUI.Box(new(0, 0, 300, 124), "");

            GUI.Label(new(4, 0, 300, 20), "TPCA");
            GUI.Label(new(4, 20, 300, 20), $"Status: Disconnected");
            GUI.Label(new(4, 40, 150, 20), "Host: ");
            GUI.Label(new(4, 60, 150, 20), "Player Name: ");
            GUI.Label(new(4, 80, 150, 20), "Password: ");

            var e = Event.current;
            string control = GUI.GetNameOfFocusedControl();
            bool pressedEnter = e.type == EventType.KeyUp &&
                e.keyCode is KeyCode.KeypadEnter or KeyCode.Return &&
                control is "uri" or "slotName" or "password";

            GUI.SetNextControlName("uri");
            string uri = GUI.TextField(new(134, 40, 150, 20), State.Uri);
            GUI.SetNextControlName("slotName");
            string slotName = GUI.TextField(new(134, 60, 150, 20), State.PlayerName);
            GUI.SetNextControlName("password");
            string password = GUI.PasswordField(new(134, 80, 150, 20), State.Password, "*"[0]);
            UpdateConnectionInfo(uri, slotName, password);

            bool pressedButton = GUI.Button(new(4, 100, 100, 20), "Connect");
            if (pressedEnter || pressedButton)
            {
                State = new(configUri.Value, configSlotName.Value, configPassword.Value);
                ArchipelagoClient.Connect();
            }

            GUI.EndGroup();
        }

        public static void UpdateConnectionInfo()
        {
            State.Uri = configUri.Value;
            State.PlayerName = configSlotName.Value;
            State.Password = configPassword.Value;
        }

        public static void UpdateConnectionInfo(string uri, string slotName, string password)
        {
            configUri.Value = uri;
            configSlotName.Value = slotName;
            configPassword.Value = password;
            UpdateConnectionInfo();
        }
    }
}
