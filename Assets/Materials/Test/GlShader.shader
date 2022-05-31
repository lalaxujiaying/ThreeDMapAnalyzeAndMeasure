Shader "Unlit/GLColorShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Color("Color",COLOR) = (1,1,1,1)
    }
    SubShader
    {
        Tags {  "Queue" = "Transparent"}

        Pass
        {
            //Blend SrcAlpha OneMinusSrcAlpha

            //Cull off
            //ZWrite off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            struct appdata_t {
                float4 vertex : POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f {
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            //float4 _Color;
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color; 
            }
            ENDCG
        }
    }
}
