using System;
using FlaxEngine;
using FlaxEngine.Networking;
using Steamworks;
using SettingsBase = FlaxEngine.SettingsBase;

namespace FacepunchSteamworks;

/// <summary>
/// Facepunch steam settings.
/// </summary>
public class FacepunchSteamSettings : SettingsBase
{
    /// <summary>
    /// If true, initializes steam during the `FacepunchSteamworksPlugin` initialization.
    /// </summary>
    public bool InitializeSteamAutomatically = true;
    
    /// <summary>
    /// If true, while not in the editor, the app will reboot if steam is required to start it.
    /// </summary>
    public bool RestartAppIfSteamRequires = true;
    
    /// <summary>
    /// If true, while the build is not in release mode, the steam library will log callbacks.
    /// </summary>
    public bool UseSteamDebugCallback = true;
    
    /// <summary>
    /// The steam app id. 480 is SpaceWars.
    /// </summary>
    public uint AppId = 480;
}

/// <summary>
/// The FacepunchSteamworksPlugin used to interface with the Facepunch.Steamworks library.
/// </summary>
/// <seealso cref="FlaxEngine.GamePlugin" />
public class FacepunchSteamworksPlugin : GamePlugin
{
    /// <inheritdoc />
    public FacepunchSteamworksPlugin()
    {
        _description = new PluginDescription
        {
            Name = "FacepunchSteamworks",
            Category = "Other",
            Author = "",
            AuthorUrl = null,
            HomepageUrl = null,
            RepositoryUrl = "https://github.com/Tryibion/FlaxFacepunchSteamworks",
            Description = "This is an implementation of Facepunch.Steamworks library for the Flax Engine.",
            Version = new Version("0.1.0"),
            IsAlpha = false,
            IsBeta = false,
        };
    }
    
    private FacepunchSteamSettings _settings;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _settings = Engine.GetCustomSettings("Steam").Instance as FacepunchSteamSettings;
        if (_settings == null)
        {
            Debug.LogError("No steam settings found.");
            Engine.RequestExit();
        }
        else
        {
            Debug.Write(LogType.Info, $"Steam settings found. AppId = {_settings.AppId}.");
        }

#if !BUILD_RELEASE
        if (_settings.UseSteamDebugCallback)
            Dispatch.OnDebugCallback += OnDebugCallback;
#endif

#if !FLAX_EDITOR
        if (SteamClient.RestartAppIfNecessary(_settings.AppId) && _settings.InitializeSteamAutomatically)
        {
            Engine.RequestExit();
        }
#endif

        if (_settings.InitializeSteamAutomatically)
            InitializeSteam();

        Scripting.Update += OnUpdate;
    }

    /// <summary>
    /// Initializes the steam client. Use if not initializing steam automatically.
    /// </summary>
    public void InitializeSteam()
    {
        if (_settings == null || SteamClient.IsValid)
            return;

        try
        {
            SteamClient.Init(_settings.AppId, false);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Steamworks failed to init: {e}");
        }
    }

    /// <summary>
    /// Starts the networking host.
    /// </summary>
    public void StartHost()
    {
        if (NetworkManager.Peer.NetworkDriver is FacepunchNetworkDriver)
            NetworkManager.StartHost();
        else
            Debug.LogWarning($"Failed to start host due to `NetworkDriver` not being set to `FacepunchNetworkDriver`.");
    }
    
    /// <summary>
    /// Starts a client to the target steam id.
    /// </summary>
    /// <param name="targetSteamID">The target steam id to connect to.</param>
    public void StartClient(ulong targetSteamID)
    {
        if (NetworkManager.Peer.NetworkDriver is FacepunchNetworkDriver networkDriver)
        {
            networkDriver.TargetSteamId = targetSteamID;
            NetworkManager.StartClient();
        }
        else
        {
            Debug.LogWarning($"Failed to start client due to `NetworkDriver` not being set to `FacepunchNetworkDriver`.");
        }
    }

    private void OnDebugCallback(CallbackType type, string message, bool server)
    {
        Debug.Write(LogType.Info, $"Type: {type}, Server: {server}, Message: {message}");
    }

    private void OnUpdate()
    {
        if (!SteamClient.IsValid)
            return;
        SteamClient.RunCallbacks();
    }

    /// <inheritdoc />
    public override void Deinitialize()
    {
        if (SteamClient.IsValid)
            SteamClient.Shutdown();
        Scripting.Update -= OnUpdate;

#if !BUILD_RELEASE
        if (_settings.UseSteamDebugCallback)
            Dispatch.OnDebugCallback -= OnDebugCallback;
#endif
        base.Deinitialize();
    }
}

