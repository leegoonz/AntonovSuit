﻿// Sub Surface Scattering Shader based on Eric Penner - Siggraph 2011 – Advances in Real-Time Rendering
// Created by Charles Greivelding

Shader "Antonov Suit/Skin/PreIntegrated Skin" 
{
	Properties 
	{
		_Color ("Base Color", Color) = (1, 1, 1, 1)  
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
		_tuneSkinCoeffX ("Skin Coeffient R", Range (0,1)) = 1.0
		_tuneSkinCoeffY ("Skin Coeffient G", Range (0,1)) = 0.5
		_tuneSkinCoeffZ ("Skin Coeffient B", Range (0,1)) = 0.25
		_BumpLod ("Skin Softness", Range (0,1)) = 1.0
		_tuneCurvature ( "Skin Scattering", Range (0,1)) = 0.2
		//_SKIN_LUT ("Skin BRDF LUT", 2D) = "" {}
		
		_backScatteringOuterColor("Back Scattering Outer Color", Color) = (1, 0, 0, 1)
		_backScatteringInnerColor("Back Scattering Inner Color", Color) = (1, .4, .25, 1)  
		_backScatteringAmount("Back Scattering Amount", Range (0.0,1)) = 1.0
		_backScatteringSize("Back Scattering Roughness", Range (0.01,1)) = 0.5
				
		_Shininess("Roughness", Range (0.01,1)) = 1.0
			
		_occlusionAmount ("Occlusion Amount", Range (0,1)) = 1.0
		
		_RGBTex ("Roughness (G), Occlusion (B)", 2D) = "white" {}
			
		_cavityAmount ("Cavity Amount", Range (0,1)) = 1.0
		
		_RGBSkinTex ("Cavity (R), Scattering Deepness (G), Thickness (B)", 2D) = "white" {}
		
		
		_BumpMap ("Normal", 2D) = "bump" {}
		
		_microScale("Micro Scale", Float) = 8.0
		_microCavityAmount("Micro Cavity Amount", Float) = 1.0
		_CavityMicroTex ("Micro Cavity", 2D) = "white" {}

		_microBumpAmount ("Micro Bump Amount", Float) = 1.0
		_BumpMicroTex ("Micro Bump", 2D) 	= "bump" {}

		_DiffCubeIBL ("Diffuse Cube", Cube) = "black" {}

		_SpecCubeIBL ("Specular Cube", Cube) = "black" {}
		
		//_ENV_SKIN_LUT ("Env BRDF LUT", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGINCLUDE
		#pragma target 3.0
		
		#ifdef SHADER_API_OPENGL	
			#pragma glsl
		#endif
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma only_renderers d3d9 opengl d3d11
		
		#include "UnityShaderVariables.cginc"

		//ANTONOV SUIT STUFF
		
		// Workflow
		#define ANTONOV_SKIN
		#define ANTONOV_BACKSCATTERING
		
		// Optional features
		//#define ANTONOV_TOKSVIG
		//#define ANTONOV_VIEW_DEPENDENT_ROUGHNESS
		//#define ANTONOV_HORYZON_OCCLUSION

		#include "AntonovSuitInput.cginc"
		#include "AntonovSuitLib.cginc"
		#include "AntonovSuitBRDF.cginc"

		ENDCG
		
		Pass 
		{
			Name "FORWARD" 
			Tags { "LightMode" = "ForwardBase" }
			
			CGPROGRAM

			//UNITY STUFF
			#pragma multi_compile_fwdbase 
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			//ANTONOV SUIT STUFF		
			#define ANTONOV_FWDBASE
			
			#include "AntonovSuitFrag.cginc"
			
			ENDCG
		}
		Pass 
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend One One

			CGPROGRAM
			
			//UNITY STUFF
			#pragma multi_compile_fwdadd_fullshadows
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			//ANTONOV SUIT STUFF		
			
			#include "AntonovSuitFrag.cginc"
			
			ENDCG
		}
	
	}
	FallBack "Antonov Suit/Metallic Workflow/Dielectric"
}
