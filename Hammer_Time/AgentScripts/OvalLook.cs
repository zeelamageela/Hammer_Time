using System.Collections.Generic;
using UnityEngine;

public static class OvalLook
{
    // renders the oval + cone at one skill in several tints, side by side, on an ice-grey background (isolated from the open scene)
    public static string Run(string outPath, float skill)
    {
        var tints = new[]
        {
            Color.white,
            new Color(1.00f, 0.80f, 0.20f),               // gold
            new Color(1.00f, 0.80f, 0.20f),               // gold, stronger
            new Color(0.20f, 0.60f, 1.00f),               // blue, stronger
        };
        var gain = new[] { 3f, 2f, 4f, 4f };
        var root = new GameObject("OvalLookTemp");
        try
        {
            var camGo = new GameObject("cam");
            camGo.transform.SetParent(root.transform);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.70f, 0.74f, 0.77f);
            cam.cullingMask = 1 << 31;
            cam.transform.position = new Vector3(0f, -1.0f, -10f);
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 50f;

            for (int s = 0; s < tints.Length; s++)
            {
                float cx = (s - 1.5f) * 2.6f;
                var holder = new GameObject("bands" + s);
                holder.transform.SetParent(root.transform);
                var b = holder.AddComponent<AimFuzzBands>();
                AimFuzzBands.ConeStrength = 0.2f * gain[s]; AimFuzzBands.OvalFill = 0.1f * gain[s]; AimFuzzBands.OvalOutline = 0.2f * gain[s];
                b.tint = tints[s];
                var end = new Vector2(cx + 0.15f, 0.5f);
                b.ShowPatch(end, AimFuzzBands.AimHalfWidth(skill), AimFuzzBands.WeightHalfHeight(skill), skill, skill);
                var path = new List<Vector3>();
                for (int i = 0; i <= 40; i++)
                {
                    float t = i / 40f;
                    path.Add(new Vector3(cx + 0.15f * t * t, Mathf.Lerp(-4.5f, 0.5f, t), 0f));
                }
                b.ShowCone(path, end, skill);
                foreach (var tr in holder.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = 31;
            }

            int w = 1040, h = 520;
            var rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.aspect = w / (float)h;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes(outPath, tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            return "wrote " + outPath;
        }
        finally { AimFuzzBands.ConeStrength = 0.2f; AimFuzzBands.OvalFill = 0.1f; AimFuzzBands.OvalOutline = 0.2f; Object.DestroyImmediate(root); }
    }
}
