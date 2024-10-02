Shader "Custom/ParticleShader"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,1,1)
        _Radius ("Radius", float) = 0.03
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
		#pragma instancing_options procedural:updatePos

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        float _Radius;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<float3> positions;
        #endif

        void updatePos()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				float3 pos = positions[unity_InstanceID];
				unity_ObjectToWorld._11_21_31_41 = float4(_Radius, 0, 0, 0);
				unity_ObjectToWorld._12_22_32_42 = float4(0, _Radius, 0, 0);
				unity_ObjectToWorld._13_23_33_43 = float4(0, 0, _Radius, 0);
                unity_ObjectToWorld._14_24_34_44 = float4(pos, 1);
				unity_WorldToObject = unity_ObjectToWorld;
				unity_WorldToObject._14_24_34 *= -1;
				unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
			#endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            // Albedo comes from a texture tinted by color
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Alpha = c.a;
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}
