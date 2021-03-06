﻿Shader "Instanced/InstancedSurfaceShader" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model
#pragma surface surf Standard addshadow fullforwardshadows
#pragma multi_compile_instancing
#pragma instancing_options procedural:setup

		sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
	};

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	StructuredBuffer<float3> positionBuffer;
	StructuredBuffer<float> touchedBuffer;
	StructuredBuffer<float3> followPoint;
	StructuredBuffer<float> musicBuffer;
#endif
	void rotate2D(inout float2 v, float r)
	{
		float s, c;
		sincos(r, s, c);
		v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
	}
	void setup()
	{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		float3 data = positionBuffer[unity_InstanceID];

		unity_ObjectToWorld._11_21_31_41 = float4(1, 0, 0, 0);
		unity_ObjectToWorld._12_22_32_42 = float4(0, 1, 0, 0);
		unity_ObjectToWorld._13_23_33_43 = float4(0, 0, 1, 0);

		unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
		unity_WorldToObject = unity_ObjectToWorld;
		unity_WorldToObject._14_24_34 *= -1;
		unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
#endif
	}
	float Random(float u, float v)
	{
		float f = dot(float2(12.9898, 78.233), float2(u, v));
		return frac(43758.5453 * sin(f));
	}
	float3 RandomPoint(float id)
	{
		float u = Random(id * 0.01334, 0.3728) * UNITY_PI * 2;
		float z = Random(0.8372, id * 0.01197) * 2 - 1;
		float l = Random(4.438, id * 0.01938 - 4.378);
		return float3(float2(cos(u), sin(u)) * sqrt(1 - z * z), z) * sqrt(l);
	}
	half _Glossiness;
	half _Metallic;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		//fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		fixed4 c = float4(0,1,0,1);
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		c.rgb = RandomPoint(unity_InstanceID);
		if(touchedBuffer[unity_InstanceID]==1)
			c = float4(1, 0, 0,1);
		//c.rgb = float3(0,0,0);
#endif
		o.Emission = c.rgb*0.8;
		o.Albedo = c;
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = c.a;
	}
	ENDCG
	}
		FallBack "Diffuse"
}