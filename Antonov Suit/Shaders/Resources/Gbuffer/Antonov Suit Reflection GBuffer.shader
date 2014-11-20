// Created by Charles Greivelding
Shader "Hidden/G-Buffer/Reflection" 
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
			
			//ANTONOV SUIT STUFF
			#pragma multi_compile ANTONOV_INFINITE_PROJECTION ANTONOV_SPHERE_PROJECTION ANTONOV_BOX_PROJECTION
			#pragma multi_compile _ ANTONOV_CUBEMAP_ATTEN
			
					
			// Workflow
			#define ANTONOV_WORKFLOW_SPECULAR
			
			#define ANTONOV_VIEW_DEPENDENT_ROUGHNESS

			#include "../../AntonovSuitInput.cginc"
			#include "../../AntonovSuitLib.cginc"
			#include "../../AntonovSuitBRDF.cginc"
			
			
			#define ANTONOV_GBUFFER_REFLECTION

			#include "../../AntonovSuitFrag.cginc"

			ENDCG
		}
	}
Fallback Off
} 