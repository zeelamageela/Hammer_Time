using UnityEngine;

/// <summary>
/// One switch for every tester-only option (the physics-pace button in the pause menu, and anything else added to it).
/// On in the Editor and in Development builds; off in a release build, so shipping needs no menu surgery.
/// Set ForceOff to true to hide them even in a Development build (e.g. for a store screenshot build).
/// </summary>
public static class DevOptions
{
    public static bool ForceOff = false;

    public static bool Enabled { get { return !ForceOff && Debug.isDebugBuild; } }
}
