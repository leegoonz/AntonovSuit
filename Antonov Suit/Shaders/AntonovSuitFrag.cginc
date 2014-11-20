// Created by Charles Greivelding
			#ifdef LIGHTMAP_ON
				inline void DecodeDirLightmap(half3 normal, float4 colorLM, float4 scaleLM, out half3 lightColor, out half3 lightDir)
				{
					UNITY_DIRBASIS
					half3 scalePerBasisVector;

					lightColor = DirLightmapDiffuse (unity_DirBasis, colorLM, scaleLM, normal,true,scalePerBasisVector);
					lightDir = normalize (scalePerBasisVector.x * unity_DirBasis[0] + scalePerBasisVector.y * unity_DirBasis[1] + scalePerBasisVector.z * unity_DirBasis[2]);
				}
				
				float ComputeFadeDistance(float3 wpos, float z)
				{
					float sphereDist = distance(wpos, unity_ShadowFadeCenterAndType.xyz);
					return lerp(z, sphereDist, unity_ShadowFadeCenterAndType.w);
				}
				
			#endif
			
			struct v2f 
			{
			    float4	pos 		: POSITION;
			    float4	lightDir	: TEXCOORD0;
			    #ifdef ANTONOV_PLANAR_REFLECTION
			    	float4  screenPos	: TEXCOORD10;
			    #endif
			    float3	normal		: TEXCOORD1;
			    float4	worldPos	: TEXCOORD2;
			    float2	uv			: TEXCOORD3;
			    #ifdef LIGHTMAP_OFF
				#else
			   		float3	lightmap	: TEXCOORD4;
			    #endif
				LIGHTING_COORDS(5,6)
				float3 	TtoW0		: TEXCOORD7;
				float3 	TtoW1		: TEXCOORD8;
				float3 	TtoW2		: TEXCOORD9;
			};
			
			v2f vert(appdata_full v)
			{
			    v2f o;
			    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			   	o.worldPos = mul(_Object2World, v.vertex);
			   	o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			   	o.lightDir = float4(WorldSpaceLightDir(v.vertex), 0);
			   	#ifdef ANTONOV_PLANAR_REFLECTION
			  	 	o.screenPos = ComputeScreenPos(o.pos);
			  	#endif
			    o.normal =  mul(_Object2World, float4(v.normal, 0)).xyz;
			    TANGENT_SPACE_ROTATION;
				o.TtoW0 = mul(rotation, _Object2World[0].xyz * unity_Scale.w);
				o.TtoW1 = mul(rotation, _Object2World[1].xyz * unity_Scale.w);
				o.TtoW2 = mul(rotation, _Object2World[2].xyz * unity_Scale.w);	 
				//SHADOW
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				#ifdef LIGHTMAP_OFF
				#else
					o.lightmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					o.lightmap.z = 0;
				#endif
				return o;
			}
			
			half4 frag( v2f i ) : COLOR
			{
				
				#define uv_metallic i.uv
				#define uv_base i.uv
				#define uv_diff i.uv
				#define uv_spec i.uv
				#define uv_bump i.uv
				#define uv_occlusion i.uv
			
				//Basic stuff
				half3 white = half3(1.0,1.0,1.0);
				half3 black = half3(0.0,0.0,0.0);
				
				#ifdef ANTONOV_PLANAR_REFLECTION
			   		float2 screenCoord = i.screenPos.xy / i.screenPos.w;
			    #endif
			    
			   // #if UNITY_UV_STARTS_AT_TOP
				//if (_MainTex_TexelSize.y < 0)
				       // screenCoord.y = 1-screenCoord.y;

				//#endif

			    //METALLIC
				#ifdef ANTONOV_WORKFLOW_METALLIC
					half metallic = tex2D(_RGBTex, uv_metallic).x;
					metallic = metallic*metallic; // To sRGB
			    #endif
			    
			    //OCCLUSION
				half occlusion = half(1.0f);
			
				occlusion = tex2D( _RGBTex, uv_occlusion ).z;
				occlusion = occlusion*occlusion; // To sRGB
				
				occlusion = lerp(white,occlusion,_occlusionAmount);
			
			    float4 worldPos = i.worldPos;
			    
			    half3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
			  
			    half3 lightColor = _LightColor0.rgb * 2;
			    
				half3 lightDir = normalize(i.lightDir);
				half atten = LIGHT_ATTENUATION(i);
				
			    //BASE COLOR
				half4 baseColor = tex2D(_MainTex, uv_base);
				baseColor.rgb *= _Color.rgb;
				
				//ALPHA
				half alpha = baseColor.a * _Color.a;
				
				//ROUGHNESS
				half roughness = tex2D(_RGBTex, uv_metallic).y;
				//roughness = roughness*roughness;
				roughness *= _Shininess;
				
				//CAVITY
				#ifdef ANTONOV_SKIN
					float cavity =  tex2D( _RGBSkinTex, uv_base ).x;
					cavity = lerp(1.0f, cavity, _cavityAmount);
				#endif
				
				//NORMAL
				float3 normal = UnpackNormal(tex2D(_BumpMap, uv_bump));
				
				#ifdef ANTONOV_SKIN
					float3 vertexNormal = float3(0,0,1); // Z-Up Vertex tangent space normal

					float3 microNormal = UnpackNormal(tex2D(_BumpMicroTex,uv_bump*_microScale));
					microNormal = lerp(vertexNormal,microNormal,_microBumpAmount);
											
					normal.xy += microNormal.xy;
				#endif
				
				float3 worldNormal = float3(0,0,0);
				worldNormal.x = dot(i.TtoW0, normal);
				worldNormal.y = dot(i.TtoW1, normal);
				worldNormal.z = dot(i.TtoW2, normal);
				
				#ifdef ANTONOV_GBUFFER_WORLDNORMAL
					worldNormal = worldNormal * 0.5 + 0.5;
					return float4(worldNormal,1);
				#else
					worldNormal = normalize(worldNormal);
				#endif

				//VERTEX NORMAL
				#ifdef ANTONOV_SKIN
					float3 worldVertexNormal = i.normal;

					worldVertexNormal = normalize(worldVertexNormal);
					
					float3 blurredNormal = lerp(worldNormal,worldVertexNormal,_BumpLod);
					
					//MICRO CAVITY
					float microCavity = tex2D( _CavityMicroTex, uv_base*_microScale).x;
					microCavity = saturate(lerp(1.0f,microCavity,_microCavityAmount));

					// CURVATURE
					float curvature = tex2D(_RGBSkinTex,uv_base).y * _tuneCurvature;
				#endif
				
				//AMBIENT
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT;
				//half3 topLighting = saturate( dot( worldNormal, float3(0,1,0) ) * 0.5 + 0.5 );
				//half3 ambient = lerp(UNITY_LIGHTMODEL_AMBIENT, _skyColor,topLighting);
			
				//half3 ambientProbe = ShadeSH9(float4(worldNormal,1));
				
				//LIGHTMAP
				half3 attenuatedLight = 1;
				#ifdef LIGHTMAP_OFF
					attenuatedLight = lightColor * atten;
				#else
					float4 lightmapColor = tex2D(unity_Lightmap, i.lightmap.xy);
					#ifdef DIRLIGHTMAP_OFF
						ambient = DecodeLightmap(lightmapColor); //Single Lightmaps replace the ambient light
					#endif
					#ifdef DUALLIGHTMAP_ON
						float4 lightmapColorNear = tex2D(unity_LightmapInd, i.lightmap.xy);
						
						float3 viewPos = mul(UNITY_MATRIX_P, worldPos);
						float fadeDist = ComputeFadeDistance(worldPos, viewPos.z);
						float fade = fadeDist * unity_LightmapFade.z + unity_LightmapFade.w;
				
						half3 lightmap = lerp(DecodeLightmap(lightmapColor),DecodeLightmap(lightmapColorNear),saturate(1-fade));
						
						ambient = lightmap; //Far and near lightmaps replace the ambient light

					#endif								
					#ifdef DIRLIGHTMAP_ON
						float4 lightmapScale = tex2D(unity_LightmapInd, i.lightmap.xy);

						half3 lightDirTangent;
						DecodeDirLightmap (normal, lightmapColor, lightmapScale, lightColor, lightDirTangent); // We are in Tangent here
						// Back to World
						lightDir.x = dot(i.TtoW0,  lightDirTangent);
						lightDir.y = dot(i.TtoW1,  lightDirTangent);
						lightDir.z = dot(i.TtoW2,  lightDirTangent);

						lightDir = normalize(lightDir);																																														
					#endif
					attenuatedLight = max(min(lightColor, atten * lightmapColor.rgb), atten * lightColor);	
				#endif
				
				//VECTORS
				#ifdef ANTONOV_SKIN
					float3 h = -viewDir + lightDir;	
					half3 halfVector = normalize(h);
					half HalfLambert = saturate(dot( worldNormal, lightDir) * 0.5 + 0.5 );
				#else
					half3 halfVector = normalize(-viewDir + lightDir);
				#endif
				
				half NdotL = saturate(dot(worldNormal, lightDir));
				half NdotV = saturate(dot(worldNormal, -viewDir));
				half NdotH = saturate(dot(worldNormal, halfVector));
				half LdotH = saturate(dot(lightDir, halfVector));
				half VdotH = saturate(dot(-viewDir, halfVector));
							
				//SHADOWS
				#ifdef ANTONOV_SKIN
					float3 skinShadow = atten; //TO FIX !!!
					#ifdef ANTONOV_FWDBASE
						skinShadow = tex2D(_SKIN_LUT,float2(atten,.9999));
					#endif
				#endif
				
				//VIEW DEPENDENT ROUGHNESS
				#ifdef ANTONOV_VIEW_DEPENDENT_ROUGHNESS		
					roughness = lerp(0.0, roughness, NdotV) * _viewDpdtRoughness + roughness * (1 - _viewDpdtRoughness);
				#endif
				
				//ROUGHNESS AA
				float roughnessAA = max(roughness,0.05);
				
				#ifdef ANTONOV_TOKSVIG
					float normalMapLen = length(tex2D(_BumpMap, uv_bump)*2-1);
					float s = RoughnessToSpecPower(roughness);
					
					float ft = normalMapLen / lerp(s, 1.0f, normalMapLen);
	                ft = max(ft, 0.01f);
	                
	                roughnessAA = SpecPowerToRoughness(ft * s) * _toksvigFactor + roughness * (1 - _toksvigFactor);
                #endif
				
				float4 frag = float4(0, 0, 0, alpha);
				
				#ifdef SHADER_API_D3D11
					clip(alpha - _Cutoff);
				#endif

				//DIFFUSE
				half4 diffuse = baseColor;
				
				#ifdef ANTONOV_METALLIC
					diffuse = half4(0,0,0,1); //No diffuse color as it is pure metal
				#endif
				
				#ifdef ANTONOV_METALLIC_DIELECTRIC
					half4 diffuseMetallic = half4(0.0,0.0,0.0,1);
					diffuse = lerp(diffuse,diffuseMetallic,metallic);
				#endif

				half3 diffuseDirect = 0;	
		
				#ifdef ANTONOV_METALLIC
					diffuseDirect = 0; // Make sure we don't have any diffuse light with metal
				#endif
				
				#ifdef ANTONOV_DIFFUSE_LAMBERT
					diffuseDirect = NdotL;
					#ifdef LIGHTMAP_ON
						#ifdef DIRLIGHTMAP_OFF
							diffuseDirect = 0;  // With Single Lightmaps we make sure that no more diffuse light is coming as ambient is replaced by lightmaps
						#endif
						#ifdef DUALLIGHTMAP_ON
							diffuseDirect = lerp(0,NdotL,saturate(1-fade)); // Dual Lightmaps support, allow to blend between far and near lightmaps
						#endif
						#ifdef DIRLIGHTMAP_ON
							diffuseDirect = 1; // With Directionnal Lightmaps we make sure that no more diffuse light is coming as it is calculated directly in DirLightmapDiffuse and output in LightColor
						#endif
					#endif
				#endif

				#ifdef ANTONOV_DIFFUSE_BURLEY
					diffuseDirect = Burley(NdotL, NdotV, VdotH, roughness);
					#ifdef LIGHTMAP_ON
						#ifdef DIRLIGHTMAP_OFF
							diffuseDirect = 0;  // With Single Lightmaps we make sure that no more diffuse light is coming as ambient is replaced by lightmaps
						#endif
						#ifdef DUALLIGHTMAP_ON
							diffuseDirect = lerp(0,NdotL,saturate(1-fade)); // Dual Lightmaps support, allow to blend between far and near lightmaps
						#endif
						#ifdef DIRLIGHTMAP_ON
							diffuseDirect = 1; // With Directionnal Lightmaps we make sure that no more diffuse light is coming as it is calculated directly in DirLightmapDiffuse and output in LightColor
						#endif
					#endif
				#endif
				  
				#ifdef ANTONOV_SKIN
					diffuseDirect = PennerSkin(float3(_tuneSkinCoeffX,_tuneSkinCoeffY,_tuneSkinCoeffZ ), worldNormal,lightDir, blurredNormal, curvature, _SKIN_LUT, atten);
				#endif
				
				#ifdef ANTONOV_SKIN
					diffuseDirect *=  lightColor * skinShadow;
				#else				
					diffuseDirect *= attenuatedLight;	
				#endif	
				
				//SPECULAR	
				half4 specular = half4(0,0,0,0);
				
				#ifdef ANTONOV_WORKFLOW_SPECULAR
					specular = tex2D(_SpecTex, uv_spec);
					specular.rgb *= _SpecColor.rgb;
				#endif
					
				#ifdef ANTONOV_WORKFLOW_METALLIC
			   		specular = baseColor;
				#endif
				
				#ifdef ANTONOV_DIELECTRIC
					specular =  half4(0.04,0.04,0.04,1);
				#endif
				
				#ifdef ANTONOV_SKIN
					specular = half4(0.028,0.028,0.028,1) * cavity * microCavity;
				#endif

				#ifdef ANTONOV_METALLIC_DIELECTRIC	
					half4 specularDielectric = half4(0.04,0.04,0.04,1);
					specular = lerp(specularDielectric,specular,metallic);
				#endif
				
				#ifdef ANTONOV_GBUFFER_SPECULAR
					return float4(specular.rgb,roughnessAA);
				#endif

				#ifdef ANTONOV_SKIN
					half D = D_Beckmann(roughness, NdotH);
					half G = 1;
				#endif
				
				#if defined (ANTONOV_WORKFLOW_METALLIC) || defined(ANTONOV_WORKFLOW_SPECULAR)
					half D = D_GGX(roughnessAA, NdotH);
					half G = G_GGX(roughnessAA, NdotL, NdotV);
				#endif
				
				#if defined (ANTONOV_WORKFLOW_METALLIC) || defined(ANTONOV_SKIN) || defined(ANTONOV_WORKFLOW_SPECULAR)
					half3 F = F_Schlick(specular, LdotH);
				#endif
				
				#ifdef LIGHTMAP_ON
					#if defined  (DIRLIGHTMAP_OFF) || defined (DUALLIGHTMAP_ON)
						#ifdef ANTONOV_DIELECTRIC
							attenuatedLight = Luminance(ambient);
						#endif
						#ifdef ANTONOV_METALLIC_DIELECTRIC
							attenuatedLight = lerp(Luminance(ambient), ambient, metallic);
						#endif
					#endif
					#ifdef DIRLIGHTMAP_ON
						#ifdef ANTONOV_DIELECTRIC
							attenuatedLight = Luminance(attenuatedLight);
						#endif
						#ifdef ANTONOV_METALLIC_DIELECTRIC
							attenuatedLight = lerp(Luminance(attenuatedLight), attenuatedLight, metallic);
						#endif
					#endif	
				#endif
				
				half3 specularDirect = half3(0,0,0);
				#ifdef ANTONOV_SKIN
					specularDirect = max(D * F / dot(h, h), 0) * NdotL * attenuatedLight;
				#endif
					
				#if defined (ANTONOV_WORKFLOW_METALLIC) || defined(ANTONOV_WORKFLOW_SPECULAR)
					specularDirect = D * G * F * NdotL * attenuatedLight;
				#endif
				
				#ifdef LIGHTMAP_ON
					#ifdef DIRLIGHTMAP_OFF
						specularDirect = 0; // With Single Lightmaps only IBL contribution is allowed
					#endif
					#ifdef DUALLIGHTMAP_ON
						specularDirect = lerp(0,specularDirect,saturate(1-fade));
					#endif
				#endif				

				//SPECULAR IBL
				#if defined (ANTONOV_WORKFLOW_METALLIC) || defined(ANTONOV_SKIN) || defined(ANTONOV_WORKFLOW_SPECULAR)
					half3 specularIBL = ApproximateSpecularIBL(specular.rgb, roughness, worldNormal, -viewDir, i.worldPos, i.normal) * _exposureIBL.x;
				#endif
				
				#ifdef ANTONOV_SKIN
					specularIBL *= specularOcclusion(worldNormal, -viewDir, occlusion);
				#endif
				
				#ifdef ANTONOV_GBUFFER_REFLECTION
					return HDRtoRGBM(float4(specularIBL*occlusion,1));
				#endif
				
				//DIFFUSE IBL
				#ifdef ANTONOV_SKIN
					half3 diffuseIBL = diffuseSkinIBL(float3(_tuneSkinCoeffX, _tuneSkinCoeffY, _tuneSkinCoeffZ), ApproximateDiffuseIBL(worldNormal,i.worldPos).rgb, ApproximateDiffuseIBL(blurredNormal,i.worldPos).rgb) * _exposureIBL.y;
				#endif
				
				#if defined (ANTONOV_WORKFLOW_METALLIC) || defined(ANTONOV_WORKFLOW_SPECULAR)
					half3 diffuseIBL =ApproximateDiffuseIBL(worldNormal, i.worldPos)* _exposureIBL.y;
				#endif

				float3 backScattering = float3(0,0,0);
				#ifdef ANTONOV_BACKSCATTERING
					float translucency =  tex2D(_RGBSkinTex, uv_base).z;
					translucency = translucency * translucency; // To sRGB
					
					half backRoughness = exp2( 8 * _backScatteringSize + 1); 
					
					float3 viewScattering = exp2( saturate(dot(-viewDir, -(lightDir + (  worldNormal * 0.01)))) * backRoughness - backRoughness) * ((backRoughness / 8) / 4);

					float3 lightScattering = saturate( dot( -lightDir, worldNormal )) * saturate( dot( lightDir, worldNormal ) *0.5+0.5);

					float3 IBLScattering = diffuseIBL;

					backScattering = ((viewScattering + lightScattering) * attenuatedLight + IBLScattering) * translucency;
					
					float3 profile = (_backScatteringInnerColor) * backScattering*backScattering*backScattering + (_backScatteringOuterColor) * backScattering;

					backScattering = profile * _backScatteringAmount;

					/*
					backScattering = -backScattering * backScattering;
						
					float3 profile = 
					float3(0.233, 0.455, 0.649) * exp(backScattering / 0.0064) +
                    float3(0.1,   0.336, 0.344) * exp(backScattering / 0.0484) +
                    float3(0.118, 0.198, 0.0)   * exp(backScattering / 0.187)  +
                    float3(0.113, 0.007, 0.007) * exp(backScattering / 0.567)  +
                    float3(0.358, 0.004, 0.0)   * exp(backScattering / 1.99)   +
                    float3(0.078, 0.0,   0.0)   * exp(backScattering / 7.41);
                    backScattering = profile * diffuse * _backScatteringAmount;
					*/

				#endif
											
				#ifdef LIGHTMAP_ON
						// We normalize the cubemap with lightmap here for infinite projection.
						if (ANTONOV_PROJECTION == 0)
							diffuseIBL *= attenuatedLight;
						if (ANTONOV_PROJECTION == 0)
							specularIBL *= attenuatedLight;																																					
				#endif
				
				float3 reflection = float3(0,0,0);
				
				#ifdef ANTONOV_PLANAR_REFLECTION
				float3 fresnel = F_LagardeSchlick(specular.rgb,roughness, NdotV);
				reflection = tex2D(_ReflectionTex,(screenCoord+normal.xy*0.1)) * (1-roughness) * fresnel;
				//reflection = tex2Dproj(_ReflectionTex,UNITY_PROJ_COORD(i.screenPos + float4(normal.xy*0.5,0,0))) * fresnel;

				specularIBL = 0;
				#endif
				
				#ifdef ANTONOV_METALLIC
					diffuseIBL = half3(0,0,0);
					ambient = half3(0,0,0);
				#endif
				
				half diffuseOcclusion = occlusion;
				
				half specularOcclusion = specularOcclusionRoughness(NdotV, occlusion, roughness);
				
				#ifdef ANTONOV_WORKFLOW_SPECULAR
					diffuse.rgb *= saturate(1.0f - specular); // We balance the diffuse with specular intensity, The Order 1886
				#endif
				
				#if defined (ANTONOV_WORKFLOW_METALLIC) || defined(ANTONOV_SKIN) || defined(ANTONOV_WORKFLOW_SPECULAR)
					#ifdef ANTONOV_FWDBASE
						frag.rgb = (diffuseDirect + (diffuseIBL + ambient) * diffuseOcclusion) * diffuse;
						
						frag.rgb += (specularDirect + specularIBL + reflection) * specularOcclusion;
						
						frag.rgb += backScattering;
					#else	
						frag.rgb = diffuseDirect * diffuse;
						
						frag.rgb += specularDirect * specularOcclusion;
						
						frag.rgb += backScattering;
					#endif
				#endif
				#ifdef ANTONOV_ILLUM
				//ILLUM
					half3 illum = tex2D(_Illum, uv_base);
					half3 illumColor = half3(_illumColorR, _illumColorG, _illumColorB);
					illum = lerp(half3(0.0, 0.0, 0.0), illum * illumColor, _illumStrength);
					
					frag.rgb += illum;
				#endif
				
				//frag.rgb = diffuseIBL;

				return frag;
			}