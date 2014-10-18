// Created by Charles Greivelding

Shader "Hidden/Antonov Suit/SSR" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"

	#define ANTONOV_SSR
	#define ANTONOV_VIEW_DEPENDENT_ROUGHNESS
	#include "../AntonovSuitInput.cginc"
	#include "../AntonovSuitLib.cginc"
	#include "../AntonovSuitBRDF.cginc"

	struct appdata 
	{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float3 texcoord : TEXCOORD;
	};

	struct v2f 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0; 
	};

	v2f vert( appdata_tan v ) 
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord;
		return o;
	}

	 float4 RayMarch(float3 R,int NumSteps, float4 viewPos, float3 screenPos, float2 coord, float3 stepOffset)
     {
  		
  		
  		float4 rayPos = viewPos + float4(R,1);
		float4 rayUV = mul (_ProjectionMatrix, rayPos);
		rayUV.xyz /= rayUV.w;
					
		float3 rayDir = normalize( rayUV - screenPos );
		rayDir.xy *= 0.5;

		float sampleDepth;
		float sampleMask = 0;

	    float3 rayStart = float3(coord,screenPos.z);
                    
 		float stepSize = 1 / ( (float)NumSteps + 1);
		rayDir  *= stepOffset * stepSize + stepSize;
                                  
		float3 samplePos = rayStart + rayDir;
	
		float4 SpecularLighting = 0;

		for (int steps = 1;  steps< NumSteps; ++steps)
		{
			sampleDepth  = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dlod (_CameraDepthTexture, float4(samplePos.xy,0,0))));
 
			if ( sampleDepth < LinearEyeDepth(samplePos.z) )  
			{  
			
				if (abs(sampleDepth - LinearEyeDepth(samplePos.z) ) < _reflectionBias)
				{
	                sampleMask = 1;
	                steps = NumSteps+8;
	                break;
				}
				else
				{
					rayDir *= 0.5;
					samplePos = rayStart + rayDir; 
				} 

					                 
			}
			else
			{
		        rayStart = samplePos;
		        rayDir *= 1.1;
		        samplePos += rayDir;
			}
		}

		//#if UNITY_UV_STARTS_AT_TOP
		//if (_MainTex_TexelSize.y < 0)
		//samplePos.y = 1-samplePos.y;
		//#endif

		return float4(samplePos, sampleMask);

	}

	float4 fragSSR (v2f i) : COLOR
	{	
	
		
		//float4 bbb = tex2D(_MainTex, i.uv);
			
		float4 frag = float4(0,0,0,0);
		
		//DIFFUSE
		//float3 diffuse = tex2D(_Diffuse_GBUFFER, i.uv);
					
		//SPECULAR AND ROUGHNESS
		float4 specular = tex2D(_Specular_GBUFFER,i.uv);
		
		float roughness = specular.a; 
        roughness = min(_maxRoughness*_maxRoughness,roughness); 
        
        float alpha =  tex2D(_WorldNormal_GBUFFER,i.uv).a;
        
        //WORLD AND VIEW NORMAL                                    
		float3 worldNormal =  tex2D(_WorldNormal_GBUFFER,i.uv) * 2.0 - 1.0;
		float3 viewNormal =  mul(_WorldViewInverseMatrix, float4(worldNormal.rgb,1));
		
		viewNormal = normalize(viewNormal);
		worldNormal = normalize(worldNormal);
		
		float z = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv)); // we don't want linear Z here
                  	
		float3 scrPos = float3(i.uv*2-1, z);
                    
		float4 worldPos = mul(_ViewProjectionInverseMatrix, float4(scrPos,1)); // World position reconstruct by depth
		worldPos = worldPos / worldPos.w;
			     	
		float4 viewPos = mul(_WorldViewMatrix, worldPos); // World position to view position
			     	   	
		float3 viewDir = normalize(worldPos.rgb-_WorldSpaceCameraPos);
		
		float2 coord = i.uv;
		
		float4 reflectionColor = float4(0,0,0,0);
		float4 bounceColor = float4(0,0,0,0);
		
		int NumRays = 5;
		int NumSteps = 80;

			for( int i = 0; i < NumRays; i++ )
			{

				float3 stepOffset = tex2D( _Jitter, coord * _ScreenParams.xy / 128 );
				

				float2 Xi = Hammersley(i, NumRays);
				//float3 Xi = tex2D( _Jitter, coord * _ScreenParams.xy / 128 );

				//float3 L = ImportanceSampleHemisphereCosine( Xi, viewNormal); // GI Test
				float3 H = ImportanceSampleGGX( Xi,roughness, viewNormal);
				
				float3 R = 2 * dot(-viewPos.rgb,H) * H - (-viewPos.rgb);
				//float3 R = reflect(viewPos,viewNormal);
			
				float4 ray = RayMarch(R, NumSteps, viewPos, scrPos, coord, stepOffset);
				ray.xyz = lerp(-viewPos, ray.xyz, alpha);
				
				if( ray.w <= 1 )
				{
					float borderDist = min(1-max(ray.x, ray.y), min(ray.x, ray.y));
					float borderAtten = saturate(borderDist > _edgeFactor ? 1 : borderDist / _edgeFactor);
					float dirAtten = 1-saturate( dot( -viewDir,worldNormal ));

					float NdotV = saturate(dot( worldNormal ,-viewDir ));   
					float3 F = F_LazarovApprox( specular.rgb,specular.a, NdotV);				

					reflectionColor.rgb += tex2D(_MainTex, ray.xy) * ray.w * borderAtten * dirAtten * F;
					reflectionColor.a = borderAtten;
					//frag.rgb = ray.rgb;
					//bounceColor.rgb += tex2D(_MainTex, ray.xy) * ray.w * borderAtten * dirAtten;
				}
				
			}
			reflectionColor /= NumRays;
			//bounceColor /= NumRays;
			
			//half3 specularIBL = ApproximateSpecularIBL( specular.rgb, roughness, worldNormal, -viewDir, worldPos, float3(0,0,1) );
		
			//float3 final = lerp(bbb,reflectionColor.rgb* _reflectionStrength, saturate(reflectionColor.a *100));			

		//frag.rgb = lerp(bbb,reflectionColor.rgb * _reflectionStrength, saturate(reflectionColor.a *1000)) * alpha;
		frag.rgb = (bounceColor.rgb + reflectionColor.rgb * _reflectionStrength);
		
		return  HDRtoRGBM(frag); 

	}

	float4 fragCompose( v2f i ) : COLOR
	{	 
		float4 frag = tex2D(_MainTex, i.uv);

		// SSR pass texture
		float3 reflection = DecodeRGBMLinear(tex2Dlod(_Reflection_Pass, float4(i.uv,0,0)));

		frag.rgb += reflection.rgb;

		return frag;
	}
	
	float4 fragBlur(v2f i) : COLOR
	{	

		float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).x);

		int uBlurSize = 4; // use size of noise texture

		float2 texelSize = _reflectionBlur / _ScreenParams.xy;
		   				
		float4 result = float4(0,0,0,1);
		   				
		float hlim = float(float(-uBlurSize) * 0.5 + 0.5);
		   				
		for (int x = 0; x < uBlurSize; ++x)
		{
			for (int y = 0; y < uBlurSize; ++y)
			{
				float2 offset = (hlim + float2(float(x), float(y))) * texelSize;
		         				
				result.rgb += DecodeRGBMLinear(tex2D(_Reflection_Pass, i.uv + offset));
			}
		}
	 
		return HDRtoRGBM(result / float(uBlurSize * uBlurSize));
		
	}
	
	float4 fragBlurX(v2f i) : COLOR
	{	
		float roughness = tex2D(_Specular_GBUFFER,i.uv).a;
		roughness = roughness*roughness;
		return gaussianBlurX( _Reflection_Pass,i.uv, _ScreenParams.xy, _reflectionBlur *roughness );
	}
	
	float4 fragBlurY(v2f i) : COLOR
	{	
		float roughness = tex2D(_Specular_GBUFFER,i.uv).a;
		roughness = roughness*roughness;
		return gaussianBlurY( _Reflection_Pass,i.uv, _ScreenParams.xy, _reflectionBlur * roughness );
	}

	float4 fragBase( v2f i ) : COLOR
	{	
		float4 frag = tex2D(_MainTex, i.uv);

		return frag;
	}


	ENDCG 
	
	Subshader 
	{
		//0
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0
			
			#ifdef SHADER_API_OPENGL
       			#pragma glsl
    		#endif

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment fragSSR
			ENDCG
		}
		//1
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0
			
			#ifdef SHADER_API_OPENGL
       			#pragma glsl
    		#endif

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment fragCompose
			ENDCG
		}
		//2
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0 

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment fragBase
			ENDCG
		}
		//3
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment fragBlur
			ENDCG
		}
		//4
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment fragBlurX
			ENDCG
		}
		//5
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment fragBlurY
			ENDCG
		}
	}
	Fallback Off
}
