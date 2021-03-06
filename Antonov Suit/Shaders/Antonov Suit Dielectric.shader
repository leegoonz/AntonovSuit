﻿// Created by Charles Greivelding
Shader "Antonov Suit/Metallic Workflow/Dielectric" 
{
	Properties 
	{
		_Color ("Base Color", Color) = (1, 1, 1, 1)  
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		 	
		_Shininess("Roughness", Range (0.01,1)) = 0.0
		_viewDpdtRoughness("View Dependent Roughness", Range (0.0,1)) = 0.0
		//_toksvigFactor("Toksvig Factor", Range (0.0,1)) = 0.0	
		
		_occlusionAmount ("Occlusion Amount", Range (0,1)) = 1.0
		//_horyzonOcclusion("Horyzon Occlusion Amount", Range (0,1)) = 1.0
		
		_RGBTex ("Metallic (R), Roughness (G), Occlusion (B)", 2D) = "white" {}
			
		_BumpMap ("Normal", 2D) = "bump" {}

		_DiffCubeIBL ("Diffuse Cube", Cube) = "black" {}

		_SpecCubeIBL ("Specular Cube", Cube) = "black" {}
		
		//_ReflectionTex ("ReflectionTex", 2D) = "white"
		
		//_ENV_LUT ("Env BRDF LUT", 2D) = "white" {}
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
		#define ANTONOV_WORKFLOW_METALLIC
		
		// Metallic workflow only, this define the type of the surface
		#define ANTONOV_DIELECTRIC
		
		// Direct diffuse lighting model
		#define ANTONOV_DIFFUSE_LAMBERT
		
		// Lightmap
		//#define DUALLIGHTMAP_ON // Enable dual lightmaps support in forward rendering
		
		// Optional features
		//#define ANTONOV_PLANAR_REFLECTION
		//#define ANTONOV_TOKSVIG
		#define ANTONOV_VIEW_DEPENDENT_ROUGHNESS
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
	FallBack "Diffuse"
}
