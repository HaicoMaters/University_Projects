Shader "Custom/SmokeShader"
{
    Properties
    {
        _Colour ("Colour", Color) = (1,1,1,1)
        _NoiseTex("Noise Texture", 2D) = "white" {} // noise texture used to sample movement from
		_MainTex("Main Texture", 2D) = "white" {} // gradient texture used to sample colour from
        _NoiseScale("Noise Scale", Range(0.1, 1)) = 0.5
        _Speed("Speed", Range(0.1, 1)) = 0.5
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 200
        ZWrite Off
        Blend One One

        Pass{
        CGPROGRAM

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_ST;
        sampler2D _NoiseTex;

        fixed4 _Colour;
        float _NoiseScale;
        float _Speed;

        struct appdata {
            float4 vertex : POSITION;
            float2 uv     : TEXCOORD0;
            float4 color  : COLOR; // color from particle system
        };

        struct v2f
        {
            float4 vertex : POSITION;
            float2 uv     : TEXCOORD0;
            float4 color  : COLOR;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            // Simulate brownian motion using the perlin noise texture
		    float2 NoiseUV = v.uv.xy * _NoiseScale +  float2(_Time.y, 0) * _Speed;
            o.uv = (v.uv + NoiseUV) * _MainTex_ST.xy * 0.1;  // _MainTex_ST.xy is the tiling
            o.color = v.color;
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
             fixed4 mainTexColour = tex2D(_MainTex, i.uv);

             // Use the red channel as a mask to blend with the base color only where the main smoke texture is not black
             fixed4 maskedColor = lerp(_Colour, mainTexColour, mainTexColour.a);

             return lerp(mainTexColour,  i.color, _Colour);
        }
        ENDCG
        }
    }
    FallBack "Diffuse"
}
