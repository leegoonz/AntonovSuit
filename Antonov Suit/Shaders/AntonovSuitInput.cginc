// Created by Charles Greivelding

#ifndef ANTONOV_SUIT_INPUT_CGINC
#define ANTONOV_SUIT_INPUT_CGINC

half4		_exposureIBL;
float		_timeCube;
#ifdef SHADER_API_D3D11
half		_Cutoff;
#endif

#ifdef ANTONOV_PLANAR_REFLECTION
sampler2D	_ReflectionTex;
#endif

//Define the max amount of probe used
int _NumIndex;

// 0 Bake cubemap
// 1 Importance Sampling
int ANTONOV_IMPORTANCE_SAMPLING;

// 0 Infinite
// 1 Sphere
// 2 Box
int ANTONOV_PROJECTION;

float4x4	_WorldToCube;
float4x4	_WorldToCubeInverse;

float4		_cubemapPosScale;
float4		_cubemapBoxSize;

//LIGHTMAP
#ifndef LIGHTMAP_OFF
float4 unity_LightmapFade;
float4 unity_LightmapST;
sampler2D unity_Lightmap;
	#ifndef DIRLIGHTMAP_OFF
		sampler2D unity_LightmapInd;
	#endif
	#ifdef DUALLIGHTMAP_ON
		sampler2D unity_LightmapInd;
	#endif
#endif	

//SKYLIGHT
//samplerCUBE	_AmbCubeIBL;
float4		_skyColor;
//float4		_groundColor;

//SKIN
#ifdef ANTONOV_SKIN
sampler2D	_ENV_SKIN_LUT;
sampler2D	_SKIN_LUT;
sampler2D	_RGBSkinTex;
sampler2D	_BumpMicroTex;
sampler2D	_CavityMicroTex;
half		_tuneCurvature;
half		_cavityAmount;						
half		_BumpLod;																					

half		_microBumpAmount;
half		_microBumpLod;
half		_microCavityAmount;
half		_microScale;
half		_tuneSkinCoeffX;
half		_tuneSkinCoeffY;
half		_tuneSkinCoeffZ;
#endif

#ifdef ANTONOV_BACKSCATTERING
half4		_backScatteringOuterColor;
half4		_backScatteringInnerColor;
half		_backScatteringAmount;
half		_backScatteringSize;	
#endif

//SELF ILLUM
#ifdef ANTONOV_ILLUM
sampler2D	_Illum;
half		_illumStrength;
half		_illumColorR;
half		_illumColorG;
half		_illumColorB;
#endif

//SPECULAR
#ifdef ANTONOV_WORKFLOW_SPECULAR
sampler2D	_SpecTex;
//fixed4		_SpecColor; Built in
#endif

//METALLIC SPECULAR ROUGHNESS AND OCCLUSION
sampler2D	_RGBTex;

half		_Shininess;
half		_occlusionAmount;
			
//BASE COLOR AND DIFFUSE
sampler2D	_MainTex;
float4		_MainTex_ST;
fixed4		_Color;

//NORMAL
sampler2D	_BumpMap;

#ifdef ANTONOV_IMPORTANCE_DIFFUSE
int		_diffSamples;
#endif

#ifdef ANTONOV_IMPORTANCE_SPECULAR
int		_specSamples;
#endif

//DIFFUSE IBL
samplerCUBE _DiffCubeIBL;
int 		_diffuseSize;
		
//SPECULAR IBL
samplerCUBE _SpecCubeIBL;

sampler2D	_ENV_LUT;
int 		_specularSize;
int 		_lodSpecCubeIBL;

//HORYZON OCCLUSION
#ifdef ANTONOV_TOKSVIG
half		_toksvigFactor;
#endif

//VIEW DEPENDENT ROUGHNESS
#ifdef ANTONOV_VIEW_DEPENDENT_ROUGHNESS	
half		_viewDpdtRoughness;
#endif

#ifdef ANTONOV_HORYZON_OCCLUSION
half		_horyzonOcclusion;
#endif

//SSR
#ifdef ANTONOV_SSR
//G-Buffer
sampler2D	_Specular_GBUFFER;
sampler2D	_WorldNormal_GBUFFER;
sampler2D	_Reflection_GBUFFER; 
sampler2D	_Reflection_Pass; 

sampler2D	_Jitter;
sampler2D	_Dither;
sampler2D	_CameraDepthTexture;

float4		_CameraDepthTexture_ST;
float4		_MainTex_TexelSize;

float		_fresnelIntensity;
float		_reflectionBlur;   
float		_reflectionStrength;
float		_reflectionBias;
float		_edgeFactor; 
float		_maxRoughness;
float		_bounceStrength;

uniform float4x4	_ProjectionMatrix;
uniform float4x4	_ProjectionInverseMatrix;
uniform float4x4	_ProjectionPreviousMatrix;
uniform float4x4	_ViewProjectionInverseMatrix;
uniform float4x4	_ViewProjectionMatrix;
uniform float4x4	_WorldViewMatrix;
uniform float4x4	_WorldViewInverseMatrix;
uniform float4x4	_ViewWorldMatrix;
uniform float4x4	_ViewProjectionPreviousMatrix; 
#endif

#endif