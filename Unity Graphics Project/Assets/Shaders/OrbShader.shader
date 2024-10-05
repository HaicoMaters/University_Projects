Shader "Custom/OrbShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (.5, .5, .5, 1)
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            fixed4 color : COLOR;
        };

        fixed4 _Color;
        fixed4 _GlowColor;

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = _Color * IN.color;
            o.Albedo = c.rgb;
            o.Emission = _GlowColor.rgb * IN.color;
        }
        ENDCG
    }

    FallBack "Diffuse"
}