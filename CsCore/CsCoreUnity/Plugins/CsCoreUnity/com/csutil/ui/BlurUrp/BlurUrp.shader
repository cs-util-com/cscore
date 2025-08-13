Shader "Custom/BlurURP"
{
    Properties{
        _Size           ("Blur Radius"           , Range(0,40)) = 1.0
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
            Tags{ "LightMode" = "SRPDefaultUnlit" } // recognised by URP :contentReference[oaicite:2]{index=2}

            HLSLPROGRAM
            #pragma vertex   vert            // <-- NEW :contentReference[oaicite:3]{index=3}
            #pragma fragment frag
            #pragma target   3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // -------- uniforms
            sampler2D _MainTex;               float4 _MainTex_ST;
            sampler2D _CameraOpaqueTexture;   float4 _CameraOpaqueTexture_TexelSize;
            float    _Size;
            float4   _MultiplyColor, _AdditiveColor;

            // -------- vertex
            struct appdata { float4 pos:POSITION; float2 uv:TEXCOORD0; };
            struct v2f     { float4 pos:SV_POSITION; float2 uvMask:TEXCOORD0; float2 uvScreen:TEXCOORD1; };

            v2f vert (appdata v){
                v2f o;
                o.pos       = mul(UNITY_MATRIX_MVP, v.pos);
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
                float2 t = _CameraOpaqueTexture_TexelSize.xy;
                half  w[5] = { 0.18h, 0.15h, 0.12h, 0.09h, 0.05h };   // weights 0..4

                // Horizontal + vertical blur (9 × 2 taps)
                half3 sumH = 0, sumV = 0;
                [unroll] for(int k=-4;k<=4;++k){
                    int idx = abs(k);
                    sumH += tex2D(_CameraOpaqueTexture, i.uvScreen + float2(k*_Size*t.x,0)).rgb * w[idx];
                    sumV += tex2D(_CameraOpaqueTexture, i.uvScreen + float2(0,k*_Size*t.y)).rgb * w[idx];
                }
                half3 blur    = (sumH + sumV)*0.5;
                half3 tinted  = blur * _MultiplyColor.rgb + _AdditiveColor.rgb;
                half  alpha   = tex2D(_MainTex, i.uvMask).a;
                return half4(tinted, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
