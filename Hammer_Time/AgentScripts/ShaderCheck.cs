using UnityEngine;
using UnityEditor;

public static class ShaderCheck
{
    public static string Run()
    {
        AssetDatabase.Refresh();
        var sb = new System.Text.StringBuilder();
        foreach (var name in new[] { "Hammer/AimLight", "Hammer/AimOval" })
        {
            var sh = Shader.Find(name);
            if (sh == null) { sb.AppendLine(name + ": NOT FOUND"); continue; }
            sb.AppendLine(name + ": found, supported=" + sh.isSupported + " messages=" + ShaderUtil.GetShaderMessageCount(sh));
            for (int i = 0; i < ShaderUtil.GetShaderMessageCount(sh); i++)
            {
                var m = ShaderUtil.GetShaderMessages(sh)[i];
                sb.AppendLine("   " + m.severity + ": " + m.message + " (line " + m.line + ")");
            }
        }
        return sb.ToString();
    }
}
