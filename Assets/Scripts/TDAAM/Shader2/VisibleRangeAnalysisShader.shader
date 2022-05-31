// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/VisibleRangeAnalysisShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineSize("LineSize",float) = 0
        _LineRedColor("LineRedColor",Color) = (1,1,1,1)
        _LineGreenColor("LineGreenColor",Color) = (1,1,1,1)
        _CircleColor("CircleColor",COLOR) = (1,1,1,1)
        _Color("Color",Color) = (1,1,1,1)
        _LineBlueColor("LineBlueColor",COLOR)= (1,1,1,1)
        _lineCircleColor("LineCircleColor",COLOR) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent"}

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma shader_feature for_On 
            #pragma shader_feature Texture_On 
            #pragma shader_feature drawCircle_On
            #pragma shader_feature drawMouseLine_On
            #pragma shader_feature staticLine_On
            #pragma shader_feature extraCircleLine_On
            
            #include "UnityCG.cginc"
            #include "HLSLSupport.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };
            float4 _Color;
            float _LineSize;
            float4 _LineRedColor;
            float4 _LineGreenColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _lineCount;
            half4 maxAndmin[1023];
            half4 verticalAndHorizontalAndKB[1023];
            float _radius;
            float4 _centerPoint;
            float4 _CircleColor;
            float areaLastCount[360];
            float areaFirstCount[360];
            float4 _mouseWorldPoint;
            float4 _LineBlueColor;
            float areaK[90];
            int areaCount;
            float4 _lineCircleColor;
            int DrawLineFunc1(v2f IN,float4 _OriginPoint, float4 _DstPoint,float _LineSize)
            {
                float maxX = max(_DstPoint.x,_OriginPoint.x);
                float minX = min(_DstPoint.x,_OriginPoint.x);
                float maxZ = max(_DstPoint.z,_OriginPoint.z);
                float minZ = min(_DstPoint.z,_OriginPoint.z);

                float offsetX = abs(_DstPoint.x - _OriginPoint.x);
                float offsetZ = abs(_DstPoint.z - _OriginPoint.z);



                //�ж�ֱ���Ƿ��Ǵ�ֱ����ƽ��(��������Ӧ�ط��������⴦��)
                //int lineVertical = offsetX == 0 ? 1 : 0;
                //int lineHorizontal = offsetZ == 0 ? 1 : 0;       
            
                int lineVertical = step(offsetX,0.001) * step(-0.001,offsetX);
                int lineHorizontal = step(offsetZ,0.001) * step(-0.001,offsetZ);



                //�������������γɵ�ֱ�������ǰshader��������ֱֱ�ߣ���ֱ�߽��㣨x1,y1��
                float x1 = 0;
                float y1 = 0;
                //ֱ�ߴ�ֱ
                if(lineVertical == 1)
                {
                    x1 = _OriginPoint.x;
                    y1 = IN.worldPos.z;

                }
                //ֱ��ˮƽ
                else if (lineHorizontal == 1)
                {
                    x1 = IN.worldPos.x;
                    y1 = _OriginPoint.z;
                }
                //����ֱ��б��������
                else 
                {
                    //��������ֱ�ߵ�k��b
                    float k = (_DstPoint.z - _OriginPoint.z ) / ( _DstPoint.x - _OriginPoint.x);
                    float b = _OriginPoint.z - k * _OriginPoint.x;
                    //�������ֱ������ֱ�ߵ�k��b
                    float k1 = (-1/k);
                    float b1 = IN.worldPos.z - k1 * IN.worldPos.x;
                    //�������������ֱ���ཻ��
                    x1 = ((b1 - b)/(k-k1));
                    y1 = (k1 * x1 + b1);
                }
                //���㵽��ǰshader����ľ������ߴִ�С���жԱȣ����ֱ�߼��ߴַ�Χ�ڵĵ�
                float dis = distance(float2(x1,y1),float2(IN.worldPos.x,IN.worldPos.z));
                int InRange = dis <= _LineSize ? 1 : 0;

                int limit = 0;
                limit = step(x1,maxX) * step(minX,x1) * step(y1,maxZ) * step(minZ,y1);
                return InRange * limit;
            }
            int DrawLineFunc2(v2f IN,half4 maxAndmin,half4 verticalAndHorizontalAndKB)
            {

                //�������������γɵ�ֱ�������ǰshader��������ֱֱ�ߣ���ֱ�߽��㣨x1,y1��
                half x1 = 0;
                half y1 = 0;
                //ֱ�ߴ�ֱ
                [flatten]
                //[branch]
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
                int limit = 0;
                limit = step(x1,maxAndmin.x) * step(maxAndmin.y,x1) * step(y1,maxAndmin.z) * step(maxAndmin.w,y1);
                //���㵽��ǰshader����ľ������ߴִ�С���жԱȣ����ֱ�߼��ߴַ�Χ�ڵĵ�
                half dis = distance(half2(x1,y1),half2(IN.worldPos.x,IN.worldPos.z));
                int InRange = dis <= _LineSize ? 1 : 0;
                return InRange * limit;
               
            }
           
            int GetCurrentPointArea2(float4 worldPos)
            {
                //int areaCount = 360;
                int area = 0;
                int index = 0;
                if(worldPos.x == _centerPoint.x)
                    {
                        area = areaCount / 4 - 1;
                    }
                    else 
                    {
                        float k0 = (worldPos.z - _centerPoint.z)/(worldPos.x - _centerPoint.x);
                        //0.01745
                        for(index = 0;index < areaCount / 4 - 1;index ++)
                        {
                            if(k0 >= areaK[index] && k0 <= areaK[index + 1])
                            {
                                area = index;
                                break;
                            }
                        }
                        if (k0 >= areaK[areaCount / 4 - 1])
                        {
                            area = areaCount / 4 - 1;
                        }
                        else if (k0 <= -areaK[areaCount / 4 - 1])
						{
                            area = areaCount / 4;
                        }

                         for(index = areaCount / 4 - 1;index >= 1;index --)
                        {
                            if(k0 >= -areaK[index] && k0 <= -areaK[index - 1])
                            {
                                area = areaCount / 2 - index;
                                break;
                            }
                        }
                    }
                    if(worldPos.z - _centerPoint.z < 0)
                    {
                        area = area + areaCount / 2;
                    }
                return area;
            }
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                int index = 0;
                int DrawRedLine = 0;
                int DrawGreenLine = 0;
                int DrawBlueLine = 0;
                int DrawCircle = 0;
                int DrawLineCircle = 0;
                int area = 0;

                float dis = distance(float2(i.worldPos.x,i.worldPos.z),float2(_centerPoint.x,_centerPoint.z));
                #if for_On || Texture_On || staticLine_On || drawMouseLine_On || drawCircle_On
                if(step(dis,_radius + _LineSize))
                {
                    area = GetCurrentPointArea2(i.worldPos);
                    #if for_On
                        int initIndex = (area - 1) == -1?0:areaLastCount[area - 1];
                        for(index = initIndex;index <= (int)areaLastCount[area];index++)
                        {
                            if(verticalAndHorizontalAndKB[index].z == 1)
                            {
                                DrawGreenLine = DrawLineFunc2(i,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            else
                            {
                                DrawRedLine = DrawLineFunc2(i,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            if((DrawGreenLine || DrawRedLine) == 1)
                            {
                                break;
                            }
                        }
                    #endif
                    #if Texture_On
                        for(index = (int)areaFirstCount[area];index <= (int)areaLastCount[area];index++)
                        {
                            //if((int)areaLastCount[area] == 0)
                            //{
                            //    break;
                            //}
                            if(verticalAndHorizontalAndKB[index].z == 1)
                            {
                                DrawGreenLine = DrawLineFunc2(i,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            else
                            {
                                DrawRedLine = DrawLineFunc2(i,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            if((DrawGreenLine || DrawRedLine) == 1)
                            {
                                break;
                            }
                        }
                    #endif
                    #if staticLine_On
                        for(index = (int)areaFirstCount[area];index <= (int)areaLastCount[area];index++)
                        {
                            if(verticalAndHorizontalAndKB[index].z == 1)
                            {
                                DrawGreenLine = DrawLineFunc2(i,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            else
                            {
                                DrawRedLine = DrawLineFunc2(i,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            if((DrawGreenLine || DrawRedLine) == 1)
                            {
                                break;
                            }
                        }
                    #endif 

                    #if extraCircleLine_On
                    //int circleIndex = 0;
                    //float _radius100 = _radius / 100;
                    //for(circleIndex = 0;circleIndex < 100;circleIndex ++)
                    //{
                    //    if(step(dis , (_radius100 * circleIndex + _LineSize)) * step((_radius100 * circleIndex - _LineSize),  dis))
                    //    {
                    //        if(DrawRedLine == 1)
                    //        {
                    //            break;
                    //        }
                    //        else 
                    //        {
                    //            DrawLineCircle = 1;
                    //            break;
                    //        }
                    //    }
                    //}
                    #endif
                    
                    #if drawCircle_On
                        if(step(dis,_radius + _LineSize * 2) * step(_radius - _LineSize * 2,dis))
                        {
                            DrawCircle = 1;
                        }
                    #endif

                    #if drawMouseLine_On
                        DrawBlueLine = DrawLineFunc1(i,_centerPoint,_mouseWorldPoint,_LineSize * 2);
                    #endif

                }
                #endif


               

                //return _Color * col * (1 - (DrawRedLine || DrawGreenLine || DrawCircle || DrawBlueLine)) + DrawRedLine * _LineRedColor + DrawGreenLine * _LineGreenColor + DrawBlueLine * _LineBlueColor + DrawCircle * _CircleColor;
                if((DrawRedLine || DrawGreenLine || DrawCircle || DrawBlueLine || DrawLineCircle) == 0)
                    discard;
                //if(i.worldPos.y < 0)
                //{
                //    discard;
                //}
                return DrawRedLine * _LineRedColor + DrawGreenLine * _LineGreenColor + DrawBlueLine * _LineBlueColor + DrawCircle * _CircleColor + DrawLineCircle * _lineCircleColor;
            }
            ENDCG
        }
        //Pass
        //{
        //    CGPROGRAM
        //    #pragma vertex vert
        //    #pragma fragment frag
            
        //    #include "UnityCG.cginc"
        //    #include "HLSLSupport.cginc"
        //    struct appdata
        //    {
        //        float4 vertex : POSITION;
        //        float2 uv : TEXCOORD0;
        //    };

        //    struct v2f
        //    {
        //        float2 uv : TEXCOORD0;
        //        float4 vertex : SV_POSITION;
        //        float4 worldPos : TEXCOORD1;
        //    };
        //    float4 _Color;
        //    float _LineSize;
        //    float4 _LineRedColor;
        //    sampler2D _MainTex;
        //    float4 _MainTex_ST;
        //    v2f vert (appdata v)
        //    {
        //        v2f o;
        //        o.vertex = UnityObjectToClipPos(v.vertex);
        //        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        //        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        //        return o;
        //    }

        //    fixed4 frag (v2f i) : SV_Target
        //    {
        //        return fixed4(1,1,1,1);
        //    }
        //    ENDCG
        //}
    }
}