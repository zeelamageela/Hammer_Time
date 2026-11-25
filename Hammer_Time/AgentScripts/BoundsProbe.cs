using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;
using System.Linq;
public static class BoundsProbe
{
    public static string Go()
    {
        var sb = new StringBuilder();
        EditorSceneManager.OpenScene("Assets/Scenes/TournyGame.unity", OpenSceneMode.Single);
        sb.AppendLine("== every Joint2D in the scene (other than the rocks' own spring to the launcher):");
        foreach (var j in Object.FindObjectsByType<Joint2D>(FindObjectsInactive.Include))
        {
            var rb = j.attachedRigidbody;
            sb.AppendLine("   " + j.GetType().Name + " on '" + j.gameObject.name + "' enabled=" + j.enabled + " active=" + j.gameObject.activeInHierarchy
                + " | body mass=" + (rb != null ? rb.mass.ToString("F2") + " " + rb.bodyType + " gravityScale=" + rb.gravityScale + " damping=" + rb.linearDamping : "none")
                + " | connected=" + (j.connectedBody != null ? j.connectedBody.name + " mass=" + j.connectedBody.mass.ToString("F2") + " " + j.connectedBody.bodyType : "world")
                + " | breakForce=" + j.breakForce + " collideConnected=" + j.enableCollision);
        }
        sb.AppendLine("== every non-rock Rigidbody2D (dynamic or kinematic) in the scene:");
        foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include))
        {
            if (rb.GetComponent<Rock_Force>() != null) continue;
            sb.AppendLine("   '" + rb.gameObject.name + "' " + rb.bodyType + " mass=" + rb.mass.ToString("F2") + " gravityScale=" + rb.gravityScale + " active=" + rb.gameObject.activeInHierarchy + " pos=" + rb.transform.position);
        }
        sb.AppendLine("== ShooterAnim components (the sliding curler character): fields that point at the rock or a joint");
        foreach (var sa in Object.FindObjectsByType<ShooterAnim>(FindObjectsInactive.Include))
        {
            var t = sa.GetType();
            sb.Append("   ShooterAnim on '" + sa.gameObject.name + "':");
            foreach (var f in t.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                var v = f.GetValue(sa);
                if (v is Object o && o != null) sb.Append(" " + f.Name + "=" + o.name + "(" + o.GetType().Name + ")");
            }
            sb.AppendLine();
        }
        EditorSceneManager.OpenScene("Assets/Scenes/SplashMenu.unity", OpenSceneMode.Single);
        return sb.ToString();
    }
}
