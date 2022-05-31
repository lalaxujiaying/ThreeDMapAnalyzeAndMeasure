Shader "Custom/NewSurfaceShader111"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _LineSize("LineSize",float) = 0

        _LineRedColor("LineRedColor",Color) = (1,1,1,1)
        _LineGreenColor("LineGreenColor",Color) = (1,1,1,1)

        //_DstPoint("DstPoint",Vector) = (0,0,0,0)
        //_OriginPoint("OriginPoint",Vector) = (0,0,0,0)



    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        //����ɶ����������д�����ʱ���Զ��Ӹ��ң����shader����������
//////// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
////////#pragma exclude_renderers d3d11 gles
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows keepalpha
        #pragma shader_feature for_On
        #pragma shader_feature Textrue_On
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        //float4 _DstPoint;
        //float4 _OriginPoint;
        float _LineSize;
        float4 _LineRedColor;
        float4 _LineGreenColor;
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;

        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        int _lineCount;

        //half4 _OriginPointArray[1024];
        //half4 _DstPointArray[1024];
        half4 maxAndmin[1024];
        half4 verticalAndHorizontalAndKB[1024];



        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)

        UNITY_INSTANCING_BUFFER_END(Props)
        //y = tan�� * x
        // k1 * k2 = -1
        int DrawLineFunc2(Input IN,half4 maxAndmin,half4 verticalAndHorizontalAndKB)
        {
            //half maxX = max(_DstPoint.x,_OriginPoint.x);
            //half minX = min(_DstPoint.x,_OriginPoint.x);
            //half maxZ = max(_DstPoint.z,_OriginPoint.z);
            //half minZ = min(_DstPoint.z,_OriginPoint.z);

            //half offsetX = abs(_DstPoint.x - _OriginPoint.x);
            //half offsetZ = abs(_DstPoint.z - _OriginPoint.z);

            //int lineVertical = step(offsetX,0.001) * step(-0.001,offsetX);
            //int lineHorizontal = step(offsetZ,0.001) * step(-0.001,offsetZ);


            //�������������γɵ�ֱ�������ǰshader��������ֱֱ�ߣ���ֱ�߽��㣨x1,y1��
            half x1 = 0;
            half y1 = 0;
            //ֱ�ߴ�ֱ
            if(verticalAndHorizontalAndKB.w == 0)
            {
                x1 = maxAndmin.y;
                y1 = IN.worldPos.z;

            }
            //ֱ��ˮƽ
            else if (verticalAndHorizontalAndKB.w == 1)
            {
                x1 = IN.worldPos.x;
                y1 = maxAndmin.w;
            }
            //����ֱ��б��������
            else 
            {
                //��������ֱ�ߵ�k��b
                //half k = (_DstPoint.z - _OriginPoint.z ) / ( _DstPoint.x - _OriginPoint.x);
                //half b = _OriginPoint.z - k * _OriginPoint.x;
                ////�������ֱ������ֱ�ߵ�k��b
                half k1 = (-1/verticalAndHorizontalAndKB.x);

                half b1 = IN.worldPos.z - k1 * IN.worldPos.x;
                //�������������ֱ���ཻ��
                x1 = ((b1 - verticalAndHorizontalAndKB.y)/(verticalAndHorizontalAndKB.x-k1));
                y1 = (k1 * x1 + b1);
            }
            //���㵽��ǰshader����ľ������ߴִ�С���жԱȣ����ֱ�߼��ߴַ�Χ�ڵĵ�
            half dis = distance(half2(x1,y1),half2(IN.worldPos.x,IN.worldPos.z));
            int InRange = dis <= _LineSize ? 1 : 0;

            int limit = 0;
            limit = step(x1,maxAndmin.x) * step(maxAndmin.y,x1) * step(y1,maxAndmin.z) * step(maxAndmin.w,y1);
            
            return InRange * limit;
        }
        //����ʹ��step����û���⣬�����Ϊ���if��ʹ�ã����ܻḺ�Ż�
        //if���þ���
        //��Ŀ�����Ҳ�в��������
        //��Ϊ���߶������˵�ǳ��������ܣ�����ȡ����
        //����2�ȷ���1ʵ�ʲ��Ժ�����С���������������ҿɶ��Ը���
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            int i = 0;
            int DrawRedLine = 0;
            int DrawGreenLine = 0;
            //DrawLine = DrawLineFunc2(_OriginPoint,_DstPoint,IN) || DrawLine;
            #if for_On
            for(i = 0;i<_lineCount;i++)
            {

                if(verticalAndHorizontalAndKB[i].z == 1)
                {
                    DrawGreenLine = DrawLineFunc2(IN,maxAndmin[i],verticalAndHorizontalAndKB[i]);
                }
                else if(verticalAndHorizontalAndKB[i].z == 0)
                {
                    DrawRedLine = DrawLineFunc2(IN,maxAndmin[i],verticalAndHorizontalAndKB[i]);
                }
                if((DrawGreenLine || DrawRedLine) == 1)
                {
                    break;
                }

            }
            #endif



            //�����ʾ
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb * (1 - DrawRedLine) * (1 - DrawGreenLine) + DrawRedLine * _LineRedColor + DrawGreenLine * _LineGreenColor;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a * (1 - DrawRedLine) * (1 - DrawGreenLine) + (DrawGreenLine || DrawRedLine);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
