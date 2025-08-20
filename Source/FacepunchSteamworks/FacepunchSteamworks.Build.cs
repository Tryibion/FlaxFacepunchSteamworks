using System;
using System.IO;
using Flax.Build;
using Flax.Build.NativeCpp;

public class FacepunchSteamworks : GameModule
{
    public override void Init()
    {
        base.Init();
        
        BuildNativeCode = true;
    }

    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);

        
        Tags["Network"] = string.Empty;
        options.PublicDependencies.Add("Networking");
        options.ScriptingAPI.SystemReferences.Add("System.Runtime");
        
        options.ScriptingAPI.IgnoreMissingDocumentationWarnings = true;

        var facepunchLibsPath = Path.Combine(FolderPath, "..", "..", "Content", "FacepunchLibs");
        var redistPath = Path.Combine(facepunchLibsPath, "redistributable_bin");
        switch (options.Platform.Target)
        {
            case TargetPlatform.Windows:
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.xml"));
                //options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.pdb"));
                options.Libraries.Add(Path.Combine(redistPath, "win64", "steam_api64.lib"));
                options.DependencyFiles.Add(Path.Combine(redistPath, "win64", "steam_api64.dll"));
                break;
            case TargetPlatform.Linux:
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.xml"));
                //options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.pdb"));
                options.Libraries.Add(Path.Combine(redistPath, "linux64", "libsteam_api.so"));
                options.DependencyFiles.Add(Path.Combine(redistPath, "linux64", "libsteam_api.so"));
                break;
            case TargetPlatform.Mac:
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.xml"));
                //options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.pdb"));
                options.Libraries.Add(Path.Combine(redistPath, "osx", "libsteam_api.dylib"));
                options.DependencyFiles.Add(Path.Combine(redistPath, "osx", "libsteam_api.dylib"));
                break;
            default: /*throw new InvalidPlatformException(options.Platform.Target)*/ break;
        }
        
    }
}
