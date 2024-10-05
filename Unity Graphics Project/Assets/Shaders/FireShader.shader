Shader "Custom/FireShader"
{
    Properties
    {
        _Colour ("Colour", Color) = (1,1,1,1)
        _NoiseTex("Noise Texture", 2D) = "white" {} // noise texture used to sample movement from
		_MainTex("Main Texture", 2D) = "white" {} // gradient texture used to sample colour from
        _NoiseScale("Noise Scale", Range(0.1, 1)) = 0.5
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 200
        ZWrite Off
        Blend SrcAlpha One
        pass{
        CGPROGRAM

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _NoiseTex;

        fixed4 _Colour;
        float _NoiseScale;

        struct appdata {
            float4 vertex : POSITION;
            float2 uv     : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex : POSITION;
            float2 uv     : TEXCOORD0;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
		    o.uv = v.uv;
            return o;
        }

        float4 frag(v2f i) : SV_Target{
            // randomness from noise texture and time
            float noise = tex2D(_NoiseTex, i.uv * _NoiseScale).r;
            float time = sin(_Time);
            float2 clampedCoords = float2(clamp(i.uv.x * time, 0.3, 0.7), clamp(i.uv.y * time, 0.3, 1)); // clamp so stays within of flame not in the whitespace
            fixed4 mainTexColour = tex2D(_MainTex, clampedCoords * noise);

            fixed4 finalColour = mainTexColour * _Colour;
                        
            return finalColour;
        }

        ENDCG
        }
    }
    FallBack "Diffuse"
}
