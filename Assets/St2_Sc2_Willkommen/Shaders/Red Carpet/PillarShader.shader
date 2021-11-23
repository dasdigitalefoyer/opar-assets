Shader "OpAR/ReflectionShader" {
   Properties {
      _MainTex("Diffuse", 2D) = "white" {}
      _TintColor("Tint", Color) = (0,0,0,1)
      _Cube("Reflection Map", Cube) = "" {}
      _ReflectionAmount("Reflection Amount", Range(0.0,1.0)) = 0.5
   }
   
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    ENDHLSL
   
   SubShader {
              Tags { "RenderType"="Opaque" "RenderQueue"="Opaque" "RenderPipeline" = "UniversalPipeline" }
      
      Pass {   
            HLSLPROGRAM
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment
         
         uniform samplerCUBE _Cube;

         struct Attributes
         {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 color		: COLOR;
            float2 uv			: TEXCOORD2;
             UNITY_VERTEX_INPUT_INSTANCE_ID
         };

         struct Varyings
         {
            float4 pos : SV_POSITION;
            float3 normalDir : TEXCOORD0;
            float3 viewDir : TEXCOORD1;
            float4 color		: COLOR;
            float2 uv			: TEXCOORD2;
             UNITY_VERTEX_OUTPUT_STEREO
         };

         CBUFFER_START(UnityPerMaterial)
         TEXTURE2D(_MainTex);
         SAMPLER(sampler_MainTex);
         float4 _MainTex_ST;
         half4 _TintColor;
         half _ReflectionAmount;
         CBUFFER_END
            
         Varyings UnlitVertex(Attributes attributes) {
             Varyings o = (Varyings)0;
             UNITY_SETUP_INSTANCE_ID(attributes);
             UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
 
            o.viewDir = TransformObjectToWorld(attributes.vertex).xyz 
               - _WorldSpaceCameraPos;
            o.normalDir = normalize(
               TransformWorldToObject(attributes.normal).xyz);
            o.pos = TransformObjectToHClip(attributes.vertex);
            o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
            o.uv = attributes.uv;
            o.color = _TintColor;
            
            return o;
         }
 
            float4 UnlitFragment(Varyings i) : SV_Target
         {
            float3 reflectedDir = 
               reflect(i.viewDir, normalize(i.normalDir));
            float3 diffuseReflection = texCUBE(_Cube, reflectedDir);
            float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
            mainTex.rgb = lerp(mainTex, diffuseReflection, _ReflectionAmount);
            return mainTex;
         }
         ENDHLSL
      }
   }
}