using FlaxEngine;

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
    /// If true, the steam library will be initialized in the editor.
    /// </summary>
    public bool InitializeInEditor = true;
    
    /// <summary>
    /// The steam app id. 480 is SpaceWars.
    /// </summary>
    public uint AppId = 480;
}