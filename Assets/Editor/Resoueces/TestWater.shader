Shader "Custom/TestWater"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HeightTex("HeightTex", 2D) = "white"{}

        _Power("Power", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="transparent" 
            "Queue" = "AlphaTest"
        }
        LOD 100
        Cull Back
        ZWrite On
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _HeightTex;
            float4 _MainTex_ST;
            float4 _HeightTex_ST;

            float _Power;

            v2f vert (appdata v)
            {
                v2f o;

                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //float4 worldPoint = mul(unity_ObjectToWorld, v.vertex);
                //float4 heightValue = tex2D(_HeightTex, v.uv);
                //worldPoint.y += heightValue.a * _Power;
                //o.vertex = mul(UNITY_MATRIX_VP, worldPoint);

                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                //mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                col.a = 0.5f;
                return col;
            }
            ENDCG
        }
    }
}
