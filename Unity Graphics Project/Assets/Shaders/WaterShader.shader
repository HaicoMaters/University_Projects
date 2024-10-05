Shader "Custom/WaterShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ColorStrength ("Color Strength", Range(0.1, 2)) = 0.5 // how much weight the original color has for the finalColor
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        
        // properties associated with creating waves in water
        _NoiseTex ("Noise Texture", 2D) = "white" {} // a perlin noise texture used for general of water movement surface
        _NoiseScale ("Noise Scale", Range(0.01, 1)) = 0.8
        _Height ("Height", Range(0.01, 1)) = 0.6 // change in water height due to wave
        _Speed ("Speed", Range(0.01, 0.1)) = 0.025 // speed of change in water height

        // properties associated with normal used to make water texture appear to moves
        _NormalTex("Normal Texture", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0.01, 1)) = 0.5
        _NormalSpeed("Normal Speed", Range(1, 100)) = 8 // speed of the effect of the water moving from the normal texture
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
        CGPROGRAM

        #pragma target 3.0

        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        sampler2D _NormalTex;
        
        float4 _MainTex_ST;
        float4 _NormalTex_ST;

        float _NormalScale;
        float _NoiseScale;
        float _Height;
        float _Speed;
        float _NormalSpeed;
        
        fixed4 _Color;
        float _ColorStrength;

        float3 camPosition;
        float3 lightDirection;
        float4 lightColour;

        float lightIntensity;
        float specularPower;

        float4 ambientColour;
        float ambientIntensity;

        float3 otherLightPositions[20];
        fixed4 otherLightColours[20];
        float otherLightIntensities[20];
        float otherLightRanges[20];
        int numberOfLights;


        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv     : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv_MainTex   : TEXCOORD0;
            float4 vertex       : SV_POSITION;
            float3 normal       : NORMAL;
            float3 worldPos     : POSITIONT;
            float2 uv_NormalTex : TEXCOORD1;
        };

        v2f vert(appdata v) 
        {
            v2f o;
            // Get position in the Noise texture toget added height from
            float2 NoiseUV = float2((v.uv.xy + _Time * _Speed) * _NoiseScale);
            
            // Get added height from wave to increase vertex height by
            float NoiseValue = tex2Dlod(_NoiseTex, float4(NoiseUV, 0, 0)).z * _Height;
            v.vertex = v.vertex + float4(0, NoiseValue, 0, 0);

            // values to pass to frag
            o.worldPos = mul(UNITY_MATRIX_M , v.vertex).xyz;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);
            o.uv_NormalTex = TRANSFORM_TEX(v.uv, _NormalTex);
            float3x3 normalMat = UNITY_MATRIX_M;
            o.normal = mul (normalMat, v.normal);
            return o;  
        }

       float MixedAttenuation (float distance, float lightRange) 
        {
            float distanceSqr = distance * distance ;
            float falloffStart = lightRange * 0.4f ;
            float linearAtten = 1.0f - saturate((distance - falloffStart) / (lightRange - falloffStart));
            return saturate (( lightRange/(lightRange + distanceSqr)) * linearAtten);
        }

        fixed4 frag(v2f i) : SV_Target
        {
            // make the water appear to move using the normal texture with water waves based on sine waves
            float updatedXCoord = i.uv_NormalTex.x + sin(_Time) * _NormalSpeed; // new x coord the water should display
            float updatedYCoord = i.uv_NormalTex.y + sin(_Time + 7) * _NormalSpeed; // make y coord diff keep flows different visually by changing sin val

            // use to move texture along the x/y axis of screen to simulate water flowing in that direction
            float2 normalUVX = float2(updatedXCoord, i.uv_NormalTex.y); 
            float2 normalUVY = float2(i.uv_NormalTex.x, updatedYCoord);

            // Calculate a default waterflow using the normal
            float4 defaultFlow = (tex2D(_MainTex, normalUVX) + tex2D(_MainTex, normalUVY)) * _NormalScale;
            
            // apply normal to surface make water flow along both x and y independently for more effect

            float3 _normal = UnpackNormal(defaultFlow);

            // Custom fragment shader calculations for lighting using method similar to Phong reflection model 
            // sun light and ambient light
            fixed4 textureColour = tex2D(_MainTex, i.uv_MainTex);
            float3 incident = normalize(lightDirection);
            float3 normal = normalize(i.normal + _normal);

            float diffuseAmount = saturate(dot(incident, normal)); 
            float4 diffuseColour = lightColour * lightIntensity * diffuseAmount;

            float3 camVec = normalize(camPosition - i.worldPos);
            float3 halfAngle = normalize(incident + camVec);
            float specAmount = saturate(dot(halfAngle, normal));
            specAmount = pow(specAmount, specularPower);

            float4 specularColour = lightColour * lightIntensity * specAmount;

            float4 finalColour = ((diffuseColour * textureColour + specularColour) + (ambientColour * ambientIntensity) + (_Color * _ColorStrength * defaultFlow));
            
           // Lights coming from other positions (orbs or campfires)
           for (int j = 0; j < numberOfLights; j++)
           {
               float distance = length(otherLightPositions[j] - i.worldPos);
               if (distance < otherLightRanges[j]){
                    incident = normalize(otherLightPositions[j] - i.worldPos);
                    diffuseAmount = saturate(dot(incident, normal));
                    diffuseColour = otherLightColours[j] * otherLightIntensities[j] * diffuseAmount;
                    halfAngle = normalize(incident + camVec);
                    specAmount = saturate(dot(halfAngle, normal));
                    specAmount = pow(specAmount, specularPower);
                    specularColour = otherLightColours[j] * otherLightIntensities[j] * specAmount;
                    float attenuation = MixedAttenuation(distance, otherLightRanges[j]);
                    finalColour *= (diffuseColour * textureColour + specularColour) * attenuation;
               }
           }
           return finalColour;
       }
       ENDCG
     }
    }
 FallBack "Diffuse"
}
