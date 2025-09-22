using Archipelago.MultiClient.Net;
using UnityEngine;

namespace TPCA
{
    internal static class ArchipelagoSettingsGUI
    {
        private const float Margin = 30;

        private const int boxWidth = 500;
        private const int boxHeight = 400;

        private const int titleFontSize = 40;

        private const int labelFontSize = 24;
        private const float labelWidth = 180;
        private const float labelHeight = 30;

        private const float textFieldWidth = 260;
        private const float textFieldHeight = labelHeight + 4;

        private const string hostNameControlName = "HostName";
        private const string playerNameControlName = "PlayerName";
        private const string passwordControlName = "Password";

        public static ArchipelagoSettingsState State;

        internal static void OnGUI()
        {
            var titleStyle = new GUIStyle(GUI.skin.box);
            titleStyle.fontSize = titleFontSize;
            titleStyle.normal.textColor = Color.white;
            titleStyle.alignment = TextAnchor.UpperCenter;

            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = labelFontSize;
            labelStyle.normal.textColor = Color.white;

            var textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.fontSize = labelFontSize;
            textFieldStyle.normal.textColor = Color.white;

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = labelFontSize;
            buttonStyle.normal.textColor = Color.white;

            var e = Event.current;
            string focusedControlName = GUI.GetNameOfFocusedControl();
            bool isEnterKeyPressed = e.type == EventType.KeyUp &&
                e.keyCode is KeyCode.KeypadEnter or KeyCode.Return &&
                focusedControlName is hostNameControlName or playerNameControlName or passwordControlName;

            // Frame
            GUI.BeginGroup(new(800, 550, boxWidth, boxHeight));
            GUI.Box(new(0, 0, boxWidth, boxHeight), "Archipelago Settings", titleStyle);

            // Host name
            float hostY = titleFontSize + 60;
            GUI.Label(new(Margin, hostY, labelWidth, labelHeight), "Host", labelStyle);
            GUI.SetNextControlName(hostNameControlName);
            Plugin.State.Host = GUI.TextField(new(Margin + labelWidth, hostY, textFieldWidth, textFieldHeight), Plugin.State.Host, textFieldStyle);

            // Player name
            float slotNameY = hostY + textFieldHeight + Margin;
            GUI.Label(new(Margin, slotNameY, labelWidth, labelHeight), "Player Name", labelStyle);
            GUI.SetNextControlName(playerNameControlName);
            Plugin.State.PlayerName = GUI.TextField(new(Margin + labelWidth, slotNameY, textFieldWidth, textFieldHeight), Plugin.State.PlayerName, textFieldStyle);

            // Password (optional)
            float passwordY = slotNameY + textFieldHeight + Margin;
            GUI.Label(new(Margin, passwordY, labelWidth, labelHeight), "Password", labelStyle);
            GUI.SetNextControlName(passwordControlName);
            Plugin.State.Password = GUI.PasswordField(new(Margin + labelWidth, passwordY, textFieldWidth, textFieldHeight), Plugin.State.Password, "*"[0], textFieldStyle);

            float connectionStateX = (boxWidth - 450) / 2;
            float connectionStateY = passwordY + textFieldHeight + Margin - 10;
            string connectionState = string.Empty;
            if (State == ArchipelagoSettingsState.NotConnected)
            {
                connectionState = "Please connect to AP server first!";
                labelStyle.normal.textColor = Color.yellow;
            }
            else if (State == ArchipelagoSettingsState.Connecting) // Need to rexrite TryConnectAndLogin in async for this to work
            {
                connectionState = "Connecting to AP server ...";
                labelStyle.normal.textColor = Color.white;
            }
            else if (State == ArchipelagoSettingsState.ConnectionError)
            {
                connectionState = "Error connecting to AP server!";
                labelStyle.normal.textColor = Color.red;
            }
            else if (State == ArchipelagoSettingsState.ConnectedButGuidExists)
            {
                connectionState = "Connected to AP server but a save has already been associated to this AP server and player!\r\nCreate a new save at your own risk!";
                labelStyle.normal.textColor = Color.yellow;
            }
            else if (State == ArchipelagoSettingsState.Connected)
            {
                connectionState = "Connected to AP server.";
                labelStyle.normal.textColor = Color.green;
            }

            labelStyle.fontSize = 16;
            labelStyle.alignment = TextAnchor.LowerCenter;
            GUI.Label(new(connectionStateX, connectionStateY, 450, labelHeight * 2), connectionState, labelStyle);

            float connectionX = (boxWidth - 200) / 2;
            float connectionY = connectionStateY + labelHeight * 2 + 10;
            GUI.enabled = !(State == ArchipelagoSettingsState.Connecting || State == ArchipelagoSettingsState.Connected);
            bool isButtonClicked = GUI.Button(new(connectionX, connectionY, 200, 40), "First Connection", buttonStyle);
            GUI.enabled = true;

            if (isEnterKeyPressed || isButtonClicked)
            {
                State = ArchipelagoSettingsState.Connecting;
                var loginResult = Plugin.ArchipelagoClient.TryConnect();
                if (loginResult is LoginSuccessful)
                {
                    if (Plugin.ArchipelagoClient.DoesGuidExist())
                    {
                        State = ArchipelagoSettingsState.ConnectedButGuidExists;
                    }
                    else
                    {
                        State = ArchipelagoSettingsState.Connected;
                    }
                }
                else
                {
                    State = ArchipelagoSettingsState.ConnectionError;
                }
            }

            GUI.EndGroup();
        }

        internal static void ResetGUI()
        {
            State = ArchipelagoSettingsState.None;
        }
    }
}
