// Created by Charles Greivelding
Shader "Hidden/G-Buffer/Metallic Specular" 
{
	SubShader 
	{
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		Fog	{ Mode Off }
	    Pass 
	    {
			CGPROGRAM
			#pragma target 3.0
			
			#ifdef SHADER_API_OPENGL	
				#pragma glsl
			#endif
		
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			
			#define ANTONOV_WORKFLOW_METALLIC
			#define ANTONOV_METALLIC_DIELECTRIC
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