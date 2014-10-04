// Created by Charles Greivelding
Shader "Hidden/G-Buffer/WorldNormal" 
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

			#include "../../AntonovSuitInput.cginc"
			#include "../../AntonovSuitLib.cginc"
			#include "../../AntonovSuitBRDF.cginc"
			
			#define ANTONOV_GBUFFER_WORLDNORMAL

			#include "../../AntonovSuitFrag.cginc"

			ENDCG
		}
	}
Fallback Off	
}


