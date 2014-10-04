// Created by Charles Greivelding
Shader "Hidden/G-Buffer/Specular" 
{
	SubShader 
	{
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		Fog	{ Mode Off }
	    Pass 
	    {
			CGPROGRAM
			#pragma target 2.0
			
			#ifdef SHADER_API_OPENGL	
				#pragma glsl
			#endif
		
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			
			#define ANTONOV_VIEW_DEPENDENT_ROUGHNESS

			#include "../../AntonovSuitInput.cginc"
			#include "../../AntonovSuitLib.cginc"
			#include "../../AntonovSuitBRDF.cginc"
			
			
			#define ANTONOV_GBUFFER_SPECULAR

			#include "../../AntonovSuitFrag.cginc"

			ENDCG
		}
	}
Fallback Off
} 