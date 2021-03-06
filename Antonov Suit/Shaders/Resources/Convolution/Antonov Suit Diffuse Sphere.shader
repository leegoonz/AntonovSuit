﻿Shader "Hidden/Antonov Suit/Irradiance/Sphere" 
{
	Properties 
	{
		_diffSamples("Diffuse Samples", float) = 256
		_diffuseSize("Diffuse Cube Size", float) = 256
		_DiffCubeIBL ("Diffuse Cube", Cube) = "black" {}
	}
	SubShader 
	{
		Pass 
		{
			Tags 
			{ 
				"Queue"="Background"
				"RenderType"="Background"
			}
			
			Cull Off 
			ZWrite Off 
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag 

			#ifdef SHADER_API_OPENGL	
				#pragma glsl
			#endif
			
			#pragma target 3.0
			
			#define ANTONOV_SPHERE
			#define ANTONOV_IMPORTANCE_DIFFUSE
			
			#include "UnityCG.cginc"
			#include "../../AntonovSuitInput.cginc"
			#include "../../AntonovSuitLib.cginc"
			#include "../../AntonovSuitBRDF.cginc"
		
			#include "AntonovSuitImportanceFrag.cginc"
			
			ENDCG
		}
	}
FallBack Off
}
