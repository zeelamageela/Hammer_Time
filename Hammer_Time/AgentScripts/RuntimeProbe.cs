using UnityEngine;
using System.Text;
public static class RuntimeProbe
{
    public static string Go()
    {
        var sb = new StringBuilder();
        sb.AppendLine("time=" + Time.time.ToString("F1") + " frame=" + Time.frameCount);
        sb.AppendLine("== every Joint2D now:");
        foreach (var j in Object.FindObjectsByType<Joint2D>(FindObjectsInactive.Include))
        {
            var rb = j.attachedRigidbody;
            sb.AppendLine("   " + j.GetType().Name + " on '" + j.gameObject.name + "' enabled=" + j.enabled + " active=" + j.gameObject.activeInHierarchy
                + " | body mass=" + (rb != null ? rb.mass.ToString("F2") + " " + rb.bodyType : "none")
                + " | connected=" + (j.connectedBody != null ? j.connectedBody.name + " mass=" + j.connectedBody.mass.ToString("F2") + " " + j.connectedBody.bodyType : "world"));
        }
        sb.AppendLine("== every non-rock Rigidbody2D now:");
        foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include))
        {
            if (rb.GetComponent<Rock_Force>() != null) continue;
            sb.AppendLine("   '" + rb.gameObject.name + "' " + rb.bodyType + " mass=" + rb.mass.ToString("F2") + " gravityScale=" + rb.gravityScale + " damping=" + rb.linearDamping + " active=" + rb.gameObject.activeInHierarchy + " pos=" + rb.position);
        }
        sb.AppendLine("== objects with ShooterAnim:");
        foreach (var sa in Object.FindObjectsByType<ShooterAnim>(FindObjectsInactive.Include))
        {
            sb.Append("   '" + sa.gameObject.name + "' active=" + sa.gameObject.activeInHierarchy + " enabled=" + sa.enabled + " fields:");
            foreach (var f in sa.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                var v = f.GetValue(sa);
                if (v is Object o && o != null) sb.Append(" " + f.Name + "=" + o.name + "(" + o.GetType().Name + ")");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
