using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;
using System.Linq;
public static class Probe
{
    public static string Scene()
    {
        var s = SceneManager.GetActiveScene();
        return s.name + " | " + s.path + " | dirty=" + s.isDirty;
    }

    public static string Dump(string path)
    {
        var sb = new StringBuilder();
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var rocks = Object.FindObjectsByType<Rock_Force>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine(path + ": Rock_Force count=" + rocks.Length + ", GameManager=" + (Object.FindFirstObjectByType<GameManager>() != null));
        foreach (var r in rocks.Take(2))
        {
            var rb = r.GetComponent<Rigidbody2D>();
            var col = r.GetComponent<Collider2D>();
            sb.AppendLine("  " + r.name + " pos=" + r.transform.position + " scale=" + r.transform.localScale
              + " mass=" + rb.mass + " linDamp=" + rb.linearDamping + " angDamp=" + rb.angularDamping + " gravScale=" + rb.gravityScale
              + " inertia=" + rb.inertia + " collMode=" + rb.collisionDetectionMode + " interp=" + rb.interpolation + " bodyType=" + rb.bodyType
              + " scaleFactor=" + r.scaleFactor + " curlMult=" + r.curlForceMultiplier + " springMult=" + r.springTensionMultiplier);
            if (col != null)
            {
                var m = col.sharedMaterial;
                sb.AppendLine("    collider=" + col.GetType().Name + " bounds=" + col.bounds.size + " material=" + (m != null ? m.name + " bounce=" + m.bounciness + " fric=" + m.friction : "none"));
            }
        }
        foreach (var n in new[] { "Hog_Line", "Far_Hog_Line", "FarHog", "Launcher", "Hack", "InPlay", "Boards" })
        {
            var go = GameObject.Find(n);
            if (go != null)
            {
                var c = go.GetComponent<Collider2D>();
                sb.AppendLine("  " + n + " pos=" + go.transform.position + (c != null ? " bounds=" + c.bounds.center + "/" + c.bounds.size + " trigger=" + c.isTrigger : ""));
            }
        }
        // anything hog-ish / ice-ish
        foreach (var go in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                 .Select(t => t.gameObject).Where(g => { var l = g.name.ToLower(); return l.Contains("hog") || l.Contains("hack") || l.Contains("ice") || l.Contains("inplay") || l.Contains("launch"); }).Take(20))
        {
            var c = go.GetComponent<Collider2D>();
            sb.AppendLine("  [obj] " + go.name + " y=" + go.transform.position.y + (c != null ? " bounds=" + c.bounds.center.y + " h=" + c.bounds.size.y : ""));
        }
        return sb.ToString();
    }

    public static string Back() { EditorSceneManager.OpenScene("Assets/Scenes/SplashMenu.unity", OpenSceneMode.Single); return Scene(); }
    public static string DumpTourny() { return Dump("Assets/Scenes/TournyGame.unity"); }
    public static string DumpTest() { return Dump("Assets/Scenes/Game_Test.unity"); }
}
