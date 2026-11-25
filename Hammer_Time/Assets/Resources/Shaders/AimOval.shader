// The landing oval at the end of the shot cone, drawn on a quad as a screened-on light. The shape is worked out per pixel in metres, so
// it is a true ellipse of any width and height, and its soft glowing outline keeps the same thickness all the way round however stretched it is.
//   _Ellipse = (width, height, outline half-thickness, strength), all in metres except strength
//   _Quad    = (width, height) of the quad itself (a little bigger than the ellipse so the glow has room)
Shader "Hammer/AimOval"
{
    Properties
    {
        _Color ("Colour", Color) = (1, 1, 1, 1)
        _Ellipse ("Ellipse (w, h, outline, strength)", Vector) = (1, 1, 0.08, 1)
        _Quad ("Quad (w, h)", Vector) = (1.4, 1.4, 0, 0)
        _Fill ("Fill", Float) = 0.35
        _Outline ("Outline", Float) = 0.7
        _Cross ("Crosshairs (gap x, gap y, half thickness, strength)", Vector) = (0.28, 0.28, 0.012, 0)
        _Pulse ("Pulse phase 0-1", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Cull Off
        ZWrite Off
        Blend OneMinusDstColor One   // screen

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            fixed4 _Color;
            float4 _Ellipse;
            float4 _Quad;
            float _Fill;
            float _Outline;
            float4 _Cross;
            float _Pulse;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = (i.uv - 0.5) * _Quad.xy;                 // metres from the centre
                float2 ab = max(_Ellipse.xy * 0.5, 0.001);          // the two radii
                float r = length(p / ab);                           // 1 on the edge
                float2 g = p / (ab * ab);
                float slope = max(length(g) / max(r, 0.0001), 0.0001);
                float sd = (r - 1.0) / slope;                       // distance from the edge in metres: negative inside

                float rim = _Ellipse.z;
                float glow = exp(-(sd * sd) / (rim * rim));         // the gradient outline, same thickness everywhere
                float fill = 1.0 - smoothstep(-rim * 0.5, rim * 0.5, sd);
                float outside = 1.0 - smoothstep(rim * 1.5, rim * 3.0, sd);   // lets the glow die away before the quad's edge

                float a = saturate(fill * _Fill + glow * _Outline) * outside * _Ellipse.w;

                // crosshairs: four arms from a gap around the centre out to the edge of the oval, with a wave of light running outward along them
                float ax = abs(p.x), ay = abs(p.y);
                float th = _Cross.z;
                float2 gap = _Cross.xy;
                float hArm = (1.0 - smoothstep(th * 0.4, th, ay)) * smoothstep(gap.x - 0.01, gap.x, ax) * (1.0 - smoothstep(ab.x - 0.03, ab.x, ax));
                float vArm = (1.0 - smoothstep(th * 0.4, th, ax)) * smoothstep(gap.y - 0.01, gap.y, ay) * (1.0 - smoothstep(ab.y - 0.03, ab.y, ay));
                float sh = saturate((ax - gap.x) / max(ab.x - gap.x, 0.001));
                float sv = saturate((ay - gap.y) / max(ab.y - gap.y, 0.001));
                float wh = pow(0.5 + 0.5 * cos(6.2832 * (sh - _Pulse)), 2.0);
                float wv = pow(0.5 + 0.5 * cos(6.2832 * (sv - _Pulse)), 2.0);
                float cross = max(hArm * (0.45 + 0.55 * wh), vArm * (0.45 + 0.55 * wv)) * _Cross.w;
                a = saturate(a + cross);
                return fixed4(_Color.rgb * a, a);
            }
            ENDCG
        }
    }
}
