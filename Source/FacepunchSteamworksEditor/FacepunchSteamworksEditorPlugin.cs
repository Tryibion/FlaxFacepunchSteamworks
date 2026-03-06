using System;
using System.Collections.Generic;
using System.IO;
using FacepunchSteamworks;
using FlaxEditor;
using FlaxEditor.Content;
using FlaxEditor.Content.Settings;
using FlaxEngine;

namespace FacepunchSteamworksEditor;

/// <summary>
/// FacepunchSteamworksEditorPlugin Script.
/// </summary>
public class FacepunchSteamworksEditorPlugin : EditorPlugin
{
    private AssetProxy _assetProxy;
    
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
        
        Editor.ContentDatabase.Rebuild(true);
    }

    public override void Deinitialize()
    {
        Editor.ContentDatabase.Proxy.Remove(_assetProxy);
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
