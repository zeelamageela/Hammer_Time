using System.Collections.Generic;
using UnityEngine;

public static class OvalLook
{
    // renders the oval + cone at a few skills, side by side, on an ice-grey background: skill 10, 40, 80
    public static string Run(string outPath)
    {
        var root = new GameObject("OvalLookTemp");
        try
        {
            var camGo = new GameObject("cam");
            camGo.transform.SetParent(root.transform);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.70f, 0.74f, 0.77f);
            cam.transform.position = new Vector3(0f, -1.5f, -10f);
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 50f;

            float[] skills = { 10f, 40f, 80f };
            for (int s = 0; s < skills.Length; s++)
            {
                float cx = (s - 1) * 3.0f;
                var holder = new GameObject("bands" + s);
                holder.transform.SetParent(root.transform);
                var b = holder.AddComponent<AimFuzzBands>();
                b.tint = Color.Lerp(new Color(0.2f, 0.7f, 0.3f), Color.white, 0.6f);
                var end = new Vector2(cx + 0.25f, 0.5f);
                b.ShowPatch(end, AimFuzzBands.AimHalfWidth(skills[s]), AimFuzzBands.WeightHalfHeight(skills[s]), skills[s], skills[s]);
                var path = new List<Vector3>();
                for (int i = 0; i <= 40; i++)
                {
                    float t = i / 40f;
                    path.Add(new Vector3(cx + 0.25f * t * t, Mathf.Lerp(-5.5f, 0.5f, t), 0f));
                }
                b.ShowCone(path, end, skills[s]);
            }

            int w = 720, h = 720;
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
        finally { Object.DestroyImmediate(root); }
    }
}
