#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class XcodePostProcess
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS)
            return;

        string projPath = PBXProject.GetPBXProjectPath(buildPath);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string mainTarget = proj.GetUnityMainTargetGuid();
        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();

        // Unity-generated PluginBase headers use double-quoted includes (e.g. #include "LifeCycleListener.h")
        // which Xcode 15+ treats as an error in framework targets.
        proj.SetBuildProperty(frameworkTarget, "CLANG_WARN_QUOTED_INCLUDE_IN_FRAMEWORK_HEADER", "NO");

        // The module verifier runs an isolated pass that ignores target-level warning overrides and
        // rejects Unity's PluginBase headers. Unity's generated framework isn't designed to pass it.
        proj.SetBuildProperty(frameworkTarget, "ENABLE_MODULE_VERIFIER", "NO");

        // Xcode 15+ enables script sandboxing by default, which blocks the IL2CPP compiler from
        // loading libhostfxr.dylib (its .NET runtime). Without it, il2cpp.a never builds and the
        // linker fails. Disable sandboxing on all targets AND at the project level — project-level
        // YES can persist even when targets are set to NO and still causes sandbox violations.
        proj.SetBuildProperty(mainTarget, "ENABLE_USER_SCRIPT_SANDBOXING", "NO");
        proj.SetBuildProperty(frameworkTarget, "ENABLE_USER_SCRIPT_SANDBOXING", "NO");
        proj.SetBuildProperty(proj.ProjectGuid(), "ENABLE_USER_SCRIPT_SANDBOXING", "NO");

        proj.WriteToFile(projPath);
    }
}
#endif
