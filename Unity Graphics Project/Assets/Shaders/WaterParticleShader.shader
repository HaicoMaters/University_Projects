Shader "Custom/WaterParticleShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ColorStrength ("Color Strength", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {

            CGPROGRAM
            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _ColorStrength;
            fixed4 _Color;
            float3 lightDirection;
            float4 lightColour;
            float lightIntensity;
            
            float4 ambientColour;
            float ambientIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR; // get color from particle to 
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float3 normal   : NORMAL;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = mul((float3x3) UNITY_MATRIX_IT_MV, v.normal);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // reflect the lighting change from time of day
                float3 incident = normalize(lightDirection);
                float3 normal = normalize(i.normal);
                float diffuseAmount = saturate(dot(incident, normal));
                float4 diffuseColour = lightColour * lightIntensity * diffuseAmount;
                float4 finalColor = (diffuseColour) + ambientColour * ambientIntensity + _Color * _ColorStrength;

                finalColor =  finalColor * i.color; // apply the particle system color
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Particles/Alpha Blended"
}