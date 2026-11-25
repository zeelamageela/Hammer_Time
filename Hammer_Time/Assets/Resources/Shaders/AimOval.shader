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
                return fixed4(_Color.rgb * a, a);
            }
            ENDCG
        }
    }
}
