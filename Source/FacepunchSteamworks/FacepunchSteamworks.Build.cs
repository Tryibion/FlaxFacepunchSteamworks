using System;
using System.IO;
using Flax.Build;
using Flax.Build.NativeCpp;

public class FacepunchSteamworks : GameModule
{
    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);

        // Here you can modify the build options for your game module
        // To reference another module use: options.PublicDependencies.Add("Audio");
        // To add C++ define use: options.PublicDefinitions.Add("COMPILE_WITH_FLAX");
        // To learn more see scripting documentation.
        BuildNativeCode = false;
        
        options.ScriptingAPI.IgnoreMissingDocumentationWarnings = true;

        var facepunchLibsPath = Path.Combine(FolderPath, "..", "..", "Content", "FacepunchLibs");
        var redistPath = Path.Combine(facepunchLibsPath, "redistributable_bin");
        switch (options.Platform.Target)
        {
            case TargetPlatform.Windows:
                options.DependencyFiles.Add(Path.Combine(redistPath, "win64", "steam_api64.dll"));
                options.DelayLoadLibraries.Add("steam_api64.dll");
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.xml"));
                //options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.pdb"));
                break;
            case TargetPlatform.Linux:
                options.DependencyFiles.Add(Path.Combine(redistPath, "linux64", "libsteam_api.so"));
                options.DelayLoadLibraries.Add("libsteam_api.so");
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.xml"));
                break;
            case TargetPlatform.Mac:
                options.DependencyFiles.Add(Path.Combine(redistPath, "osx", "libsteam_api.dylib"));
                options.DelayLoadLibraries.Add("libsteam_api.dylib");
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.xml"));
                //options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.pdb"));
                break;
            default: /*throw new InvalidPlatformException(options.Platform.Target)*/ break;
        }
        
    }
}
