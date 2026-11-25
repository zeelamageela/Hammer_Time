using UnityEngine;
using System.Text;
using System.Linq;
using System.Collections.Generic;

// Isolate what makes a rock lose 2% of its speed near y = -24: throw it through the real release path with only a chosen subset of the
// scene's scripts enabled.
public static class Repro
{
    static Rock_Force Rock()
    {
        Application.runInBackground = true;
        var all = Object.FindObjectsByType<Rock_Force>(FindObjectsInactive.Include);
        Rock_Force rf = null;
        foreach (var c in all) if (c.name == "YELLERRR 1") rf = c;
        if (rf == null) rf = all[0];
        rf.GetComponent<Rigidbody2D>().simulated = true;
        foreach (var other in Object.FindObjectsByType<Rock_Info>(FindObjectsInactive.Include))
            if (other.gameObject != rf.gameObject) { var orb = other.GetComponent<Rigidbody2D>(); if (orb != null) orb.simulated = false; }
        return rf;
    }

    // every enabled script in the scene that is not on the thrown rock, in a stable order
    static List<MonoBehaviour> Candidates(GameObject rock)
    {
        return Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude)
            .Where(m => m != null && m.gameObject != rock && m.GetType().Namespace == null)           // project scripts (no namespace), not engine/packages
            .OrderBy(m => m.GetType().Name).ThenBy(m => m.gameObject.name).ThenBy(m => m.GetInstanceID()).ToList();
    }

    static Dictionary<int, bool> saved = new Dictionary<int, bool>();

    public static string List()
    {
        var rf = Rock();
        var sb = new StringBuilder();
        int i = 0;
        foreach (var m in Candidates(rf.gameObject)) sb.AppendLine((i++) + ": " + m.GetType().Name + " on " + m.gameObject.name + " enabled=" + m.enabled);
        return sb.ToString();
    }

    /// <summary>Enable only candidates [lo, hi); disable the rest; throw the rock; return what was enabled.</summary>
    public static string Run(int lo, int hi)
    {
        var rf = Rock();
        var go = rf.gameObject;
        var cands = Candidates(go);
        for (int i = 0; i < cands.Count; i++)
        {
            if (!saved.ContainsKey(cands[i].GetInstanceID())) saved[cands[i].GetInstanceID()] = cands[i].enabled;
            cands[i].enabled = saved[cands[i].GetInstanceID()] && i >= lo && i < hi;
        }

        var flick = go.GetComponent<Rock_Flick>();
        var rb = go.GetComponent<Rigidbody2D>();
        go.GetComponent<Collider2D>().enabled = true;
        var rel = go.GetComponent<Rock_Release>(); rel.enabled = true; rel.released = false;
        rf.enabled = true;
        var info = go.GetComponent<Rock_Info>(); info.released = false; info.rest = false; info.stopped = false;
        flick.enabled = false; flick.enabled = true;
        ShotAudit.DropCount = 0;
        flick.isPressedAI = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.position = new Vector2(0.05f, -28.1f);
        go.transform.position = new Vector3(0.05f, -28.1f, 0f);
        Physics2D.SyncTransforms();
        flick.mouseUp = true;
        int on = cands.Count(c => c.enabled);
        return "thrown with " + on + " of " + cands.Count + " scripts enabled (range " + lo + ".." + hi + ")";
    }

    /// <summary>Everything stays enabled except the named script types (comma separated); then throw.</summary>
    public static string RunExcept(string names)
    {
        var skip = new HashSet<string>(names.Split(','));
        var rf = Rock();
        var go = rf.gameObject;
        var cands = Candidates(go);
        foreach (var m in cands)
        {
            if (!saved.ContainsKey(m.GetInstanceID())) saved[m.GetInstanceID()] = m.enabled;
            m.enabled = saved[m.GetInstanceID()] && !skip.Contains(m.GetType().Name);
        }
        var flick = go.GetComponent<Rock_Flick>();
        var rb = go.GetComponent<Rigidbody2D>();
        go.GetComponent<Collider2D>().enabled = true;
        var rel = go.GetComponent<Rock_Release>(); rel.enabled = true; rel.released = false;
        rf.enabled = true;
        var info = go.GetComponent<Rock_Info>(); info.released = false; info.rest = false; info.stopped = false;
        flick.enabled = false; flick.enabled = true;
        ShotAudit.DropCount = 0;
        flick.isPressedAI = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.position = new Vector2(0.05f, -28.1f);
        go.transform.position = new Vector3(0.05f, -28.1f, 0f);
        Physics2D.SyncTransforms();
        flick.mouseUp = true;
        return "thrown with " + cands.Count(c => c.enabled) + " of " + cands.Count + " scripts enabled; disabled types: " + names;
    }

    public static string Result()
    {
        var rf = Rock();
        var rb = rf.GetComponent<Rigidbody2D>();
        return "DROPS=" + ShotAudit.DropCount + " (ratio " + ShotAudit.LastDropRatio.ToString("F4") + " at y " + ShotAudit.LastDropY.ToString("F2") + ") | rock pos=" + rb.position + " speed=" + rb.linearVelocity.magnitude.ToString("F3");
    }

    public static string Restore()
    {
        var rf = Rock();
        foreach (var m in Candidates(rf.gameObject)) if (saved.ContainsKey(m.GetInstanceID())) m.enabled = saved[m.GetInstanceID()];
        return "restored";
    }
}
