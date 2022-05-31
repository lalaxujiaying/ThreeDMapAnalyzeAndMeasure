
Shader "ScreenShader/VisibleRangeScreenShader"
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
        Tags { "Queue" = "Geometry"}
        Pass
        {
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature  Texture_On 
            #pragma shader_feature  drawCircle_On
            #pragma shader_feature  drawMouseLine_On
            #pragma shader_feature  staticLine_On
            //#pragma multi_compile extraCircleLine_On
            
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
                //float4 worldPos : TEXCOORD1;
            };
            float4 _Color;
            float _LineSize;
            float4 _LineRedColor;
            float4 _LineGreenColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _lineCount;


            //half4 maxAndmin[2048];
            //half4 verticalAndHorizontalAndKB[2048];

            StructuredBuffer<float4> maxAndmin;
            StructuredBuffer<float4> verticalAndHorizontalAndKB;


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

            sampler2D _StencilBufferToColor;
            float4 _StencilBufferToColor_TexelSize;

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
            int DrawLineFunc1(float4 worldPos,float4 _OriginPoint, float4 _DstPoint,float _LineSize)
            {
                float maxX = max(_DstPoint.x,_OriginPoint.x);
                float minX = min(_DstPoint.x,_OriginPoint.x);
                float maxZ = max(_DstPoint.z,_OriginPoint.z);
                float minZ = min(_DstPoint.z,_OriginPoint.z);

                float offsetX = abs(_DstPoint.x - _OriginPoint.x);
                float offsetZ = abs(_DstPoint.z - _OriginPoint.z);



                //判断直线是否是垂直或者平行(在下面相应地方做出特殊处理)
                //int lineVertical = offsetX == 0 ? 1 : 0;
                //int lineHorizontal = offsetZ == 0 ? 1 : 0;       
            
                int lineVertical = step(offsetX,0.001) * step(-0.001,offsetX);
                int lineHorizontal = step(offsetZ,0.001) * step(-0.001,offsetZ);



                //输入两点坐标形成的直线与过当前shader顶点做垂直直线，两直线交点（x1,y1）
                float x1 = 0;
                float y1 = 0;
                //直线垂直
                if(lineVertical == 1)
                {
                    x1 = _OriginPoint.x;
                    y1 = worldPos.z;

                }
                //直线水平
                else if (lineHorizontal == 1)
                {
                    x1 = worldPos.x;
                    y1 = _OriginPoint.z;
                }
                //两条直线斜率有意义
                else 
                {
                    //计算两点直线的k和b
                    float k = (_DstPoint.z - _OriginPoint.z ) / ( _DstPoint.x - _OriginPoint.x);
                    float b = _OriginPoint.z - k * _OriginPoint.x;
                    //计算出垂直于两点直线的k和b
                    float k1 = (-1/k);
                    float b1 = worldPos.z - k1 * worldPos.x;
                    //计算出上面两条直线相交点
                    x1 = ((b1 - b)/(k-k1));
                    y1 = (k1 * x1 + b1);
                }
                //交点到当前shader顶点的距离与线粗大小进行对比，求出直线加线粗范围内的点
                float dis = distance(float2(x1,y1),float2(worldPos.x,worldPos.z));
                int InRange = dis <= _LineSize ? 1 : 0;

                int limit = 0;
                limit = step(x1,maxX) * step(minX,x1) * step(y1,maxZ) * step(minZ,y1);
                return InRange * limit;
            }
            int DrawLineFunc2(float4 worldPos,half4 maxAndmin,half4 verticalAndHorizontalAndKB)
            {

                //输入两点坐标形成的直线与过当前shader顶点做垂直直线，两直线交点（x1,y1）
                half x1 = 0;
                half y1 = 0;
                //直线垂直
                [flatten]
                //[branch]
                if(verticalAndHorizontalAndKB.w == 0)
                {
                    x1 = maxAndmin.y;
                    y1 = worldPos.z;

                }
                //直线水平
                else if (verticalAndHorizontalAndKB.w == 1)
                {
                    x1 = worldPos.x;
                    y1 = maxAndmin.w;
                }
                //两条直线斜率有意义
                else 
                {
                    //计算两点直线的k和b
                    //half k = (_DstPoint.z - _OriginPoint.z ) / ( _DstPoint.x - _OriginPoint.x);
                    //half b = _OriginPoint.z - k * _OriginPoint.x;
                    ////计算出垂直于两点直线的k和b
                    half k1 = (-1/verticalAndHorizontalAndKB.x);

                    half b1 = worldPos.z - k1 * worldPos.x;
                    //计算出上面两条直线相交点
                    x1 = ((b1 - verticalAndHorizontalAndKB.y)/(verticalAndHorizontalAndKB.x-k1));
                    y1 = (k1 * x1 + b1);
                }
                int limit = 0;
                limit = step(x1,maxAndmin.x) * step(maxAndmin.y,x1) * step(y1,maxAndmin.z) * step(maxAndmin.w,y1);
                //交点到当前shader顶点的距离与线粗大小进行对比，求出直线加线粗范围内的点
                half dis = distance(half2(x1,y1),half2(worldPos.x,worldPos.z));
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
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 stencilCol = tex2D(_StencilBufferToColor, i.uv);
                
                int index = 0;
                int DrawRedLine = 0;
                int DrawGreenLine = 0;
                int DrawBlueLine = 0;
                int DrawCircle = 0;
                int DrawLineCircle = 0;
                int area = 0;

                
                float rawDepth =  SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv);
                float linearDepth = Linear01Depth(rawDepth);
                float4 worldPos = GetWorldPositionFromDepthValue( i.uv, linearDepth );

                float dis = distance(float2(worldPos.x,worldPos.z),float2(_centerPoint.x,_centerPoint.z));
                #if Texture_On || staticLine_On || drawMouseLine_On || drawCircle_On
                if(step(dis,_radius + _LineSize))
                {
                    area = GetCurrentPointArea2(worldPos);
                    #if Texture_On
                        for(index = (int)areaFirstCount[area];index <= (int)areaLastCount[area];index++)
                        {
                            if(verticalAndHorizontalAndKB[index].z == 1)
                            {
                                DrawGreenLine = DrawLineFunc2(worldPos,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            else
                            {
                                DrawRedLine = DrawLineFunc2(worldPos,maxAndmin[index],verticalAndHorizontalAndKB[index]);
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
                                DrawGreenLine = DrawLineFunc2(worldPos,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            else
                            {
                                DrawRedLine = DrawLineFunc2(worldPos,maxAndmin[index],verticalAndHorizontalAndKB[index]);
                            }
                            if((DrawGreenLine || DrawRedLine) == 1)
                            {
                                break;
                            }
                        }
                    #endif 

                    #if drawCircle_On
                        if(step(dis,_radius + _LineSize * 6) * step(_radius - _LineSize * 6,dis))
                        {
                            DrawCircle = 1;
                        }
                    #endif

                    #if drawMouseLine_On
                        DrawBlueLine = DrawLineFunc1(worldPos,_centerPoint,_mouseWorldPoint,_LineSize * 2);
                    #endif

                }
                #endif


               
                if((DrawRedLine || DrawGreenLine || DrawCircle || DrawBlueLine || DrawLineCircle) == 0)
                    return col;
                fixed4 effectCol = DrawRedLine * _LineRedColor + DrawGreenLine * _LineGreenColor + DrawBlueLine * _LineBlueColor + DrawCircle * _CircleColor + DrawLineCircle * _lineCircleColor;
                return lerp(col,effectCol,stencilCol.x);
            }
            ENDCG
        }
    }
}
