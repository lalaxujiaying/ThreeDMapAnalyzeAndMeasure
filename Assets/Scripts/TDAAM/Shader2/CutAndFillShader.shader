Shader "Unlit/CutAndFillShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Overlay"}

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            //模板测试对标 CutAndFill_childShader 这个shader
            Stencil 
            {
                Ref 1
                Comp NotEqual
                pass Zero
            }
            ZWrite off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature ProgramRun_On
            #pragma shader_feature AnalysisCompleted_On
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                //float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;

            };
            float4 _OriginPoints[1023];
            float4 _DstPoints[1023];
            int _LineCount;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            int DrawLineFunc1(v2f IN,float4 _OriginPoint, float4 _DstPoint,float _LineSize)
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
                    y1 = IN.worldPos.z;

                }
                //直线水平
                else if (lineHorizontal == 1)
                {
                    x1 = IN.worldPos.x;
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
                    float b1 = IN.worldPos.z - k1 * IN.worldPos.x;
                    //计算出上面两条直线相交点
                    x1 = ((b1 - b)/(k-k1));
                    y1 = (k1 * x1 + b1);
                }
                //交点到当前shader顶点的距离与线粗大小进行对比，求出直线加线粗范围内的点
                float dis = distance(float2(x1,y1),float2(IN.worldPos.x,IN.worldPos.z));
                int InRange = dis <= _LineSize ? 1 : 0;

                int limit = 0;
                limit = step(x1,maxX) * step(minX,x1) * step(y1,maxZ) * step(minZ,y1);
                return InRange * limit;
            }
            //输出0当前点水平射线与输入线段没有交点，1有交点，当前点在左边，2有交点，当前点在右边
            int PointIsInSidePolygon(v2f IN,float4 _OriginPoint, float4 _DstPoint)
            {
                float maxX = max(_DstPoint.x,_OriginPoint.x);
                float minX = min(_DstPoint.x,_OriginPoint.x);
                float maxZ = max(_DstPoint.z,_OriginPoint.z);
                float minZ = min(_DstPoint.z,_OriginPoint.z);

                if(IN.worldPos.x >= maxX || IN.worldPos.x <= minX)return 0;


                float offsetX = abs(_DstPoint.x - _OriginPoint.x);
                float offsetZ = abs(_DstPoint.z - _OriginPoint.z);

                int lineVertical = step(offsetX,0.001) * step(-0.001,offsetX);

                //直线垂直
                if(lineVertical == 1)
                {
                    if(IN.worldPos.x >= _OriginPoint.x)return 2;
                    else return 1;

                }
                //两条直线斜率有意义
                else 
                {
                    //计算两点直线的k和b
                    float k = (_DstPoint.z - _OriginPoint.z ) / ( _DstPoint.x - _OriginPoint.x);
                    float b = _OriginPoint.z - k * _OriginPoint.x;
                    if(IN.worldPos.x * k + b >= IN.worldPos.z)return 2;
                    else return 1;
                }
            }
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                int drawLine = 0;
                int drawPolygon = 0;
                int index = 0;
                int leftIndex = 0;
                int rightIndex = 0;
                int tempIndex = 0;
                //fixed4 col = tex2D(_MainTex, i.uv);
            #if ProgramRun_On

                for(index = 0; index < _LineCount ; index ++)
                {
                    tempIndex = 0;
                    drawLine = DrawLineFunc1(i,_OriginPoints[index],_DstPoints[index],0.06);
                    if(drawLine == 1)break;
                    #if AnalysisCompleted_On
                    //循环判断当前点是否在多边形内 使用引射线法 https://www.cnblogs.com/luxiaoxun/p/3722358.html
                    tempIndex = PointIsInSidePolygon(i,_OriginPoints[index],_DstPoints[index]);
                    if(tempIndex == 1)
                    {
                        leftIndex = leftIndex + 1;
                    }
                    else if(tempIndex == 2)
                    {
                        rightIndex = rightIndex + 1;
                    }
                    #endif
                }
                #if AnalysisCompleted_On
                if(leftIndex % 2 != 0)
                {
                    drawPolygon = 1;

                }
                #endif

            #endif
                if(drawLine == 0 && drawPolygon == 0)discard;
                return fixed4(1,1,0,1) * drawLine + fixed4(0.6,0.6,0,0.5) * (1 - drawLine) * drawPolygon;
            }
            ENDCG
        }
    }
}
