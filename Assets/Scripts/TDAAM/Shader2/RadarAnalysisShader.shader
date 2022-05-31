Shader "Unlit/RadarAnalysisShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            //Blend SrcAlpha OneMinusSrcAlpha
            cull off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            half4 _OriginPointIndex[360];
            half4 _DstPointIndex[360];
            half4 _OriginPoints[1023];
            half4 _DstPoints[1023];
            half _Radius;
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 worldPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float3 center = unity_ObjectToWorld._14_24_34;
                if(i.worldPos.y - center.y < 0)
                {
                    discard;
                }
                float dis = distance(float2(i.worldPos.x,i.worldPos.z),float2(center.x,center.z));
                int circleIndex = 0;
                float _Radius100 = 0.08333;
                for(circleIndex = 1;circleIndex < 360;circleIndex ++)
                {
                    if(step(dis , (_Radius100 * circleIndex + 0.01f)) * step((_Radius100 * circleIndex - 0.01f),  dis))
                    {
                        return fixed4(0,1,0,1);
                    }
                }
                //discard;
                return 0;
            }
            ENDCG
        }
    }
}
