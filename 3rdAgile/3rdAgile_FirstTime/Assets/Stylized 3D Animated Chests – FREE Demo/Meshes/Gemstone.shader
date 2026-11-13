Shader "ShaderProgramming/Chapter5/Gemstone"
{
    Properties
    {
        [MainColor] _GemColor("Gem Color", Color) = (0.05, 0.35, 1, 1)
        _Opacity("Opacity", Range(0, 1)) = 0.75

        _SpecularPower("Specular Power", Range(8, 256)) = 128
        _SpecularIntensity("Specular Intensity", Range(0, 5)) = 2

        _FresnelColor("Fresnel Color", Color) = (0.3, 0.7, 1, 1)
        _FresnelPower("Fresnel Power", Range(0.5, 8)) = 3
        _FresnelIntensity("Fresnel Intensity", Range(0, 5)) = 2

        _SparkleIntensity("Sparkle Intensity", Range(0, 5)) = 1

        _Ambient("Ambient", Color) = (0.1, 0.1, 0.15, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)

                half4 _GemColor;
                half4 _FresnelColor;
                half4 _Ambient;

                half _Opacity;
                half _SpecularPower;
                half _SpecularIntensity;
                half _FresnelPower;
                half _FresnelIntensity;
                half _SparkleIntensity;

            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionWS =
                    TransformObjectToWorld(IN.positionOS.xyz);

                OUT.positionHCS =
                    TransformWorldToHClip(OUT.positionWS);

                OUT.normalWS =
                    TransformObjectToWorldNormal(IN.normalOS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 N = normalize(IN.normalWS);

                half3 V = normalize(
                    GetCameraPositionWS() - IN.positionWS
                );

                // メインライト
                Light mainLight = GetMainLight();

                half3 L = normalize(mainLight.direction);
                half3 H = normalize(L + V);

                half NdotL = saturate(dot(N, L));
                half NdotH = saturate(dot(N, H));

                // 拡散反射
                half3 diffuse =
                    _GemColor.rgb
                    * mainLight.color
                    * NdotL;

                // 鏡面反射
                half specular =
                    pow(NdotH, _SpecularPower)
                    * _SpecularIntensity;

                half3 specularColor =
                    mainLight.color * specular;

                // Fresnel（視線角度による輪郭の輝き）
                half NdotV = saturate(dot(N, V));

                half fresnel =
                    pow(1.0h - NdotV, _FresnelPower);

                half3 fresnelColor =
                    _FresnelColor.rgb
                    * fresnel
                    * _FresnelIntensity;

                // 簡易的なファセットの輝き
                half sparkle =
                    pow(saturate(NdotH), 64.0h)
                    * _SparkleIntensity;

                // 最終カラー
                half3 color =
                    _Ambient.rgb * _GemColor.rgb
                    + diffuse
                    + specularColor
                    + fresnelColor
                    + sparkle * mainLight.color;

                return half4(color, _Opacity);
            }

            ENDHLSL
        }
    }
}