Shader "Custom/NewSurfaceShader"
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
        //这是啥啊，好像是写数组的时候自动加给我，搞的shader整个不工作
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

        float4 _OriginPointArray[1024];
        float4 _DstPointArray[1024];

        float _CustomTime;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here

        UNITY_INSTANCING_BUFFER_END(Props)
        //y = tanθ * x
        // k1 * k2 = -1
        fixed DrawLineFunc1(float3 _OriginPoint, float3 _DstPoint , Input IN)
        {
            float3 centerPoint = unity_ObjectToWorld._14_24_34;

            float maxX = max(_DstPoint.x,_OriginPoint.x);
            float minX = min(_DstPoint.x,_OriginPoint.x);
            float maxZ = max(_DstPoint.z,_OriginPoint.z);
            float minZ = min(_DstPoint.z,_OriginPoint.z);

            float offsetX = abs(_DstPoint.x - _OriginPoint.x);
            float offsetZ = abs(_DstPoint.z - _OriginPoint.z);



            //判断直线是否是垂直或者平行(在下面相应地方做出特殊处理)
            fixed lineVertical = step(offsetX,0) * step(0,offsetX);
            fixed lineHorizontal = step(offsetZ,0) * step(0,offsetZ);

            //计算两点直线的k和b
            float k = (_DstPoint.z - _OriginPoint.z ) / ( _DstPoint.x - _OriginPoint.x + lineVertical) *  (1 - lineVertical) * (1 - lineHorizontal);
            float b = _OriginPoint.z - k * _OriginPoint.x;
            
            //计算出垂直与两点的直线k和b
            float worldPosK = (-1/(k + lineVertical + lineHorizontal)) * (1 - lineVertical) * (1-lineHorizontal);
            float b1 = IN.worldPos.z - worldPosK * IN.worldPos.x;
            
            //计算出上面两条直线相交点
            float x1 = ((b1 - b)/(k-worldPosK + lineVertical + lineHorizontal) * (1 - lineVertical) * (1 - lineHorizontal) + lineVertical * _OriginPoint.x) * (1 - lineHorizontal);
            x1 = lineHorizontal * IN.worldPos.x + x1;
            float y1 = (worldPosK * x1 + b1) * (1 - lineHorizontal);
            y1 = lineHorizontal * _OriginPoint.z + y1;

            //交点到当前点的距离与线粗大小进行对比，求出在范围内的点
            float dis = distance(fixed2(x1,y1),fixed2(IN.worldPos.x,IN.worldPos.z));
            fixed InRange = step(dis,_LineSize);

            //限制直线为线段
            fixed limit = step(x1,maxX) * step(minX,x1) * step(y1,maxZ) * step(minZ,y1);
            //画线段两个端点
            //fixed limit2 = (1 - limit) * (step(distance(float2(IN.worldPos.x,IN.worldPos.z),_DstPoint.xz),_LineSize) || step(distance(float2(IN.worldPos.x,IN.worldPos.z),_OriginPoint.xz),_LineSize));
            return InRange;
        }

        int DrawLineFunc2(float3 _OriginPoint, float3 _DstPoint , Input IN)
        {
            float3 centerPoint = unity_ObjectToWorld._14_24_34;
            //_DstPoint = centerPoint + _DstPoint;
            //_OriginPoint = centerPoint + _OriginPoint;

            

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
            
            //if(_DstPoint.x >= _OriginPoint.x)
            //{
            //    if(_DstPoint.z >= _OriginPoint.z)
            //    {
                    
            //        float tempX = minX + _Time.y - _CustomTime;
            //        float tempZ = minZ + _Time.y - _CustomTime;


            //        tempX = clamp(tempX,minX,maxX);
            //        tempZ = clamp(tempZ,minZ,maxZ);
            //        //限制直线为线段
            //        limit = step(x1,tempX) * step(minX,x1) * step(y1,tempZ) * step(minZ,y1);
            //    }
            //    else
            //    {
                    
            //        float tempX = minX + _Time.y - _CustomTime;
            //        float tempZ = maxZ - _Time.y + _CustomTime;

            //        tempX = clamp(tempX,minX,maxX);
            //        tempZ = clamp(tempZ,minZ,maxZ);
            //        //限制直线为线段
            //        limit = step(x1,tempX) * step(minX,x1) * step(y1,maxZ) * step(tempZ,y1);
            //    }
            //}
            //else 
            //{
            //    if(_DstPoint.z >= _OriginPoint.z)
            //    {
            //        float tempX = maxX - _Time.y + _CustomTime;
            //        float tempZ = minZ + _Time.y - _CustomTime;

            //        tempX = clamp(tempX,minX,maxX);
            //        tempZ = clamp(tempZ,minZ,maxZ);
            //        //限制直线为线段
            //        limit = step(x1,maxX) * step(tempX,x1) * step(y1,tempZ) * step(minZ,y1);
            //    }
            //    else
            //    {
            //        float tempX = maxX - _Time.y + _CustomTime;
            //        float tempZ = maxZ - _Time.y + _CustomTime;

            //        tempX = clamp(tempX,minX,maxX);
            //        tempZ = clamp(tempZ,minZ,maxZ);
            //        //限制直线为线段
            //        limit = step(x1,maxX) * step(tempX,x1) * step(y1,maxZ) * step(tempZ,y1);
            //    }
            //}


            //fixed limit2 = (1 - limit) * (step(distance(float2(IN.worldPos.x,IN.worldPos.z),_DstPoint.xz),_LineSize) || step(distance(float2(IN.worldPos.x,IN.worldPos.z),_OriginPoint.xz),_LineSize));
            return InRange * limit;
        }
        //正常使用step性能没问题，如果因为替代if而使用，可能会负优化
        //if该用就用
        //三目运算符也有不错的性能
        //因为画线段两个端点非常消耗性能，所以取消掉
        //方法2比方法1实际测试后是有小幅度性能提升，且可读性更好
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            int i = 0;
            int DrawRedLine = 0;
            int DrawGreenLine = 0;
            //DrawLine = DrawLineFunc2(_OriginPoint,_DstPoint,IN) || DrawLine;
            #if for_On
            for(i = 0;i<_lineCount;i++)
            {
                if(_DstPointArray[i].w == 1)
                {
                    DrawGreenLine = DrawLineFunc2(_OriginPointArray[i].xyz,_DstPointArray[i].xyz,IN);
                }
                else if(_DstPointArray[i].w == 0)
                {
                    DrawRedLine = DrawLineFunc2(_OriginPointArray[i].xyz,_DstPointArray[i].xyz,IN);
                }
                if((DrawGreenLine || DrawRedLine) == 1)
                {
                    break;
                }

            }
            #endif



            //输出显示
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
