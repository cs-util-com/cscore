Shader "Custom/BlurURP"
{
    Properties{
        _Size           ("Blur Radius"           , Range(0,50)) = 5.0
        _Samples        ("Sample Count"          , Range(4,32)) = 16
        _MainTex        ("Mask Texture (Alpha)" , 2D)          = "white" {}
        _MultiplyColor  ("Multiply Tint Color"  , Color)       = (1,1,1,1)
        _AdditiveColor  ("Additive Tint Color"  , Color)       = (0,0,0,0)
    }

    SubShader{
        Tags{ "Queue"="Transparent" "RenderType"="Transparent"
              "RenderPipeline"="UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass{
            Name "ForwardBlur"
            Tags{ "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // -------- uniforms
            sampler2D _MainTex;               float4 _MainTex_ST;
            sampler2D _CameraOpaqueTexture;   float4 _CameraOpaqueTexture_TexelSize;
            float    _Size;
            int      _Samples;
            float4   _MultiplyColor, _AdditiveColor;

            // -------- vertex
            struct appdata { float4 pos:POSITION; float2 uv:TEXCOORD0; };
            struct v2f     { float4 pos:SV_POSITION; float2 uvMask:TEXCOORD0; float2 uvScreen:TEXCOORD1; };

            v2f vert (appdata v){
                v2f o;
                o.pos       = TransformObjectToHClip(v.pos.xyz);
                o.uvMask    = TRANSFORM_TEX(v.uv, _MainTex);

                float2 uv   = o.pos.xy / o.pos.w;
            #if UNITY_UV_STARTS_AT_TOP
                uv.y = -uv.y;
            #endif
                o.uvScreen  = uv * 0.5 + 0.5;
                return o;
            }

            // -------- fragment
            half4 frag (v2f i) : SV_Target{
                float2 texelSize = _CameraOpaqueTexture_TexelSize.xy;
                half3 result = 0;
                float totalWeight = 0;
                
                // Much stronger blur using circular sampling pattern
                for(int x = -_Samples/2; x <= _Samples/2; x++) {
                    for(int y = -_Samples/2; y <= _Samples/2; y++) {
                        float2 offset = float2(x, y) * _Size * texelSize;
                        float distance = length(offset);
                        
                        // Gaussian weight based on distance
                        float weight = exp(-distance * distance * 0.5);
                        
                        half3 sample = tex2D(_CameraOpaqueTexture, i.uvScreen + offset).rgb;
                        result += sample * weight;
                        totalWeight += weight;
                    }
                }
                
                // Normalize by total weight
                half3 blur = result / totalWeight;
                half3 tinted = blur * _MultiplyColor.rgb + _AdditiveColor.rgb;
                half alpha = tex2D(_MainTex, i.uvMask).a;
                return half4(tinted, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
