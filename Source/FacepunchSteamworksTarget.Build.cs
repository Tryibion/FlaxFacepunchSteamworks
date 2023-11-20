using Flax.Build;

public class FacepunchSteamworksTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();
        
        Platforms = new[]
        {
            TargetPlatform.Windows,
            TargetPlatform.Linux,
            TargetPlatform.Mac,
        };

        // Reference the modules for game
        Modules.Add("FacepunchSteamworks");
    }
}
