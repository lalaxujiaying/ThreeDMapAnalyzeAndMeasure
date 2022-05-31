Shader "DistanceSurfaceShader"
{
    Properties
    {
                _MainTex ("Texture", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            StructuredBuffer<float4> _OriginPoints;
            StructuredBuffer<float4> _DstPoints;
            int _LineCount;

            sampler2D _CameraDepthTexture;
            float4 GetWorldPositionFromDepthValue( float2 uv, float linearDepth ) 
            {
                float camPosZ = _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * linearDepth;
                float height = 2 * camPosZ / unity_CameraProjection._m11;
                float width = _ScreenParams.x / _ScreenParams.y * height;

                float camPosX = width * uv.x - width / 2;
                float camPosY = height * uv.y - height / 2;
                float4 camPos = float4(camPosX, camPosY, camPosZ, 1.0);
                return mul(unity_CameraToWorld, camPos);
            }
            int PosOnLine(float3 pos, float3 originLine, float3 dstLine, float lineWidth)
            {
                float3 ap = pos - originLine;
                float3 ab = dstLine - originLine;
                float dotValue = ap.x * ab.x + ap.z * ab.z;
                if(dotValue > 0)
                {
                    float abSize = length(ab);
                    if(dotValue <= abSize * abSize)
                    {
                        float crossValue = abs(ap.x * ab.z - ab.x * ap.z);
                        if(crossValue <= abSize * lineWidth)
                        {
                            return 1;
                        }
                    }
                }
                return 0;
            }
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float rawDepth =  SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv);
                float linearDepth = Linear01Depth(rawDepth);
                float4 worldPos = GetWorldPositionFromDepthValue( i.uv, linearDepth );

                //if(PosOnLine(worldPos,fixed3(0,0,0),fixed3(10,0,20),0.2) == 1)
                //{
                //    return fixed4(1,0,0,1);
                //}
                for (int i = 0; i < _LineCount; i++)
                {
                    if(PosOnLine(worldPos,_OriginPoints[i].xyz,_DstPoints[i].xyz,0.2) == 1)
                    {
                        if(_OriginPoints[i].w == 1)
                        {
                            return fixed4(0,1,0,1);
                        }
                        else 
                        {
                            return fixed4(1,0,0,1);
                        }
                    }

                }
                return col;
            }
            ENDCG
        }
    }
}
