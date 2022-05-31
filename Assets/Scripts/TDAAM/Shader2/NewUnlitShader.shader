// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Diffuse ("Diffuse", Color) = (1, 1, 1, 1)
		_Specular ("Specular", Color) = (1, 1, 1, 1)
		_Gloss ("Gloss", Range(1.0, 500)) = 20

        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",COLOR) = (1,1,1,1)
        _CenterColor("CenterColor",COLOR) = (1,1,1,1)
        _LineSize("Slider",float) = 0


        _DstPoint("DstPoint",vector) = (0,0,0,0)
        _OriginPoint("OriginPoint",vector) = (0,0,0,0)

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
		    Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos :TEXCOORD1;
				float3 worldNormal : TEXCOORD2;

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _CenterColor;
            float _LineSize;

            fixed4 _Diffuse;
			fixed4 _Specular;
			float _Gloss;

            float3 _DstPoint;
            float3 _OriginPoint;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //π‚’’ Unity Shaders Book/Chapter 6/Blinn-Phong Use Built-in Functions
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * max(0, dot(worldNormal, worldLightDir));
                fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 halfDir = normalize(worldLightDir + viewDir);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(worldNormal, halfDir)), _Gloss);

                //---------------------------------------------------------------------
                float maxX = max(_DstPoint.x,_OriginPoint.x);
                float minX = min(_DstPoint.x,_OriginPoint.x);

                float maxZ = max(_DstPoint.z,_OriginPoint.z);
                float minZ = min(_DstPoint.z,_OriginPoint.z);

                float y = (_DstPoint.z - _OriginPoint.z)/(_DstPoint.x - _OriginPoint.x) * (i.worldPos.x-_OriginPoint.x)+_OriginPoint.z;
                float lineSegment = step(y,i.worldPos.z + _LineSize) * step(i.worldPos.z + -_LineSize,y);
                float limit = step(i.worldPos.x,maxX) * step(minX,i.worldPos.x) * step(i.worldPos.z,maxZ) * step(minZ,i.worldPos.z);
                float DrawLine = lineSegment * limit;

                fixed4 col = tex2D(_MainTex, i.uv);
                return fixed4(ambient + diffuse + specular, 1.0) * col;
                return  col;
                //return DrawLine * _Color * col;
            }
            ENDCG
        }

    }
	FallBack "Specular"

}
