Shader "ScreenShader/ScreenStencil"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Stencil
            {
                Ref 1
                comp Always
                pass Replace
            }
            ZWrite Off
            ColorMask 0
          
        }
    }
}
