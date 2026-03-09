using FacepunchSteamworks;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.Content.Settings;
using FlaxEditor.GUI;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace FacepunchSteamworksEditor;

/// <summary>
/// FacepunchSteamworksEditorPlugin Script.
/// </summary>
public class FacepunchSteamworksEditorPlugin : EditorPlugin
{
    private AssetProxy _assetProxy;
    MainMenuButton _pluginButton;
    ContextMenuButton _openButton;

    public override void InitializeEditor()
    {
        base.InitializeEditor();
        
        GameCooker.DeployFiles += OnDeployFiles;
        _assetProxy = new CustomSettingsProxy(typeof(FacepunchSteamSettings), "Steam");
        Editor.ContentDatabase.Proxy.Add(_assetProxy);
        
        // Auto create and set steam settings
        var settingsPath = Path.Combine(Globals.ProjectContentFolder, "Settings", "Facepunch Steamworks Settings.json");
        if (!File.Exists(settingsPath))
        {
            Editor.SaveJsonAsset(settingsPath, new FacepunchSteamSettings());
        }
        var jsonAsset = Engine.GetCustomSettings("Steam");
        if (jsonAsset == null)
        {
            jsonAsset = Content.LoadAsync<JsonAsset>(settingsPath);
            GameSettings.SetCustomSettings("Steam", jsonAsset);
        }

        _pluginButton = Editor.UI.MainMenu.GetButton("Plugins") ?? Editor.UI.MainMenu.AddButton("Plugins");
        _openButton = _pluginButton.ContextMenu.AddButton("Open Facepunch Steamworks Settings", () =>
        {
            Editor.ContentEditing.Open(jsonAsset);
        });

        Editor.ContentDatabase.Rebuild(true);
    }

    public override void Deinitialize()
    {
        Editor.ContentDatabase.Proxy.Remove(_assetProxy);
        _openButton.Dispose();
        _openButton = null;
        _pluginButton = null;
        _assetProxy = null;
        GameCooker.DeployFiles -= OnDeployFiles;
        
        base.Deinitialize();
    }

    private void OnDeployFiles()
    {
        // Include steam_appid.txt file with a game
        var data = GameCooker.CurrentData;
        var settingsAsset = Engine.GetCustomSettings("Steam");
        var settings = settingsAsset?.CreateInstance<FacepunchSteamSettings>();
        var appId = settings?.AppId ?? 480;
        switch (data.Platform)
        {
            case BuildPlatform.Windows32:
            case BuildPlatform.Windows64:
            case BuildPlatform.LinuxX64:
            case BuildPlatform.MacOSx64:
                File.WriteAllText(Path.Combine(data.NativeCodeOutputPath, "steam_appid.txt"), appId.ToString());
                break;
        }
    }
}
