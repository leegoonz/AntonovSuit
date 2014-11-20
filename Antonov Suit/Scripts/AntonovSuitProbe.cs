// Cubemap capture script based on http://alastaira.wordpress.com/2013/11/12/oooh-shiny-fun-with-cube-mapping/ and on LUX http://forum.unity3d.com/threads/lux-an-open-source-physically-based-shading-framework.235027/
// Created by Charles Greivelding

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.IO;

[ExecuteInEditMode]
[System.Serializable]
public class AntonovSuitProbe : MonoBehaviour 
{

	private Shader HDRtoRGBM;

	public string cubemapFolder = "Assets/Antonov Suit/Textures/Cubemaps/";
	public string cubemapName = "Cube";

	public int probeIndex = 0;

	public enum facesSize
	{
		_64, 
		_128, 
		_256, 
		_512,
	};

	public facesSize diffuseSize = facesSize._64;

	public facesSize specularSize = facesSize._128;

	public enum qualitySamples
	{
		Low,
		Medium,
		High
	};

	public qualitySamples diffuseSamples = qualitySamples.High;

	public qualitySamples specularSamples = qualitySamples.High;

	public int smoothEdge = 4;

	private Material[] m_materials;

	public List<GameObject> Meshes = new List<GameObject>();

	private Cubemap emptyCube;
	public Cubemap diffuseCube;
	public Cubemap specularCube;

	private Camera cubeCamera; 

	public enum ProjectionType
	{	
		InfiniteProjection,
		SphereProjection,
		BoxProjection,
	};

	public ProjectionType typeOfProjection = ProjectionType.InfiniteProjection;

	private Matrix4x4 probeMatrix;

	public float probeRadius;

	//Custom exposure control
	public float diffuseExposure = 1;
	public float specularExposure = 1;
	
	public Vector3 probeBoxSize;
	public Vector3 attenBoxSize;

	public Matrix4x4 probeMatrixTranspose;
	public Matrix4x4 probeMatrixInverse;
	public Vector3 cubePos;

	//Importance sampled material for skybox
	private Material convolveDiffuseSkybox = null;
	private Material convolveSpecularSkybox = null;

	public int specularExponent = 1;

	public bool hasPro;
	public bool isDX11;

	//PREVIEW
	private GameObject previewProbe;
	private Material previewMaterial;

	//Always true
	private bool RGBM = true;

	public bool bakeDirectAndIBL = false;

	public enum radianceEnum
	{
		GGX,
		BlinnPhong
	};

	public radianceEnum radianceModel = radianceEnum.GGX;

	public enum irradianceEnum
	{
		SphereUniform,
		HemisphereUniform,
		HemisphereCosine,
	};
	
	public irradianceEnum irradianceModel = irradianceEnum.HemisphereCosine;

	public bool goConvolveIrradiance = false;
	public bool goConvolveRadiance = false;

	public bool goBake = false;
	public bool done = false;

	#if UNITY_EDITOR
	void SetLinearSpace(ref SerializedObject obj, bool linear)
	{
		if (obj == null) return;
		
		SerializedProperty prop = obj.FindProperty("m_ColorSpace");
		if (prop != null)
		{
			prop.intValue = linear ? (int)ColorSpace.Gamma : (int)ColorSpace.Linear;
			obj.ApplyModifiedProperties();
		}
	}

	public string GetOutPutPath(Cubemap cubemap, bool diffuse)
	{
		if(diffuse)
		{
			cubemap.name = cubemapFolder + cubemapName + "_" + this.gameObject.name + "_DIFF.cubemap";
		}
		else
		{
			cubemap.name = cubemapFolder + cubemapName + "_" + this.gameObject.name + "_SPEC.cubemap";
		}
		return cubemap.name;
	}
	#endif

	int CubeSizeSetup(bool isDiffuse)
	{
		int result= 0;
		
		if( isDiffuse == true )
		{
			if(diffuseSize == facesSize._64)
			{
				result = 64;
			}
			if(diffuseSize == facesSize._128)
			{
				result  = 128;
			}
			if(diffuseSize == facesSize._256)
			{
				result  = 256;
			}
			if(diffuseSize == facesSize._512)
			{
				result  = 512;
			}
		}
		else
		{
			if(specularSize == facesSize._64)
			{
				result = 64;
			}
			if(specularSize == facesSize._128)
			{
				result  = 128;
			}
			if(specularSize == facesSize._256)
			{
				result  = 256;
			}
			if(specularSize == facesSize._512)
			{
				result  = 512;
			}
		}
		return result;
	}

	public int CubeLodSetup()
	{
		int result= 0;

		if(specularSize == facesSize._64)
		{
			result = 6;
		}
		if(specularSize == facesSize._128)
		{
			result  = 7;
		}
		if(specularSize == facesSize._256)
		{
			result  = 8;
		}
		if(specularSize == facesSize._512)
		{
			result  = 9;
		}

		return result;
	}

	int qualitySetup(bool isDiffuse)
	{
		int result= 0;
		
		if( isDiffuse == true )
		{
			if(diffuseSamples == qualitySamples.Low)
			{
				result = 64;
			}
			if(diffuseSamples == qualitySamples.Medium)
			{
				result  = 128;
			}
			if(diffuseSamples == qualitySamples.High)
			{
				result  = 256;
			}
		}
		else
		{
			if(specularSamples == qualitySamples.Low)
			{
				result = 64;
			}
			if(specularSamples == qualitySamples.Medium)
			{
				result  = 128;
			}
			if(specularSamples == qualitySamples.High)
			{
				result  = 256;
			}
		}
		return result;
	}

	// Resize a Texture2D
	// http://docs-jp.unity3d.com/Documentation/ScriptReference/Texture2D.GetPixelBilinear.html
	Texture2D Resize(Texture2D sourceTex, int Width, int Height, bool flipY) 
	{
		Texture2D destTex = new Texture2D(Width, Height, sourceTex.format, false);
		Color[] destPix = new Color[Width * Height];
		int y = 0;
		while (y < Height) 
		{
			int x = 0;
			while (x < Width) 
			{
				float xFrac = x * 1.0F / (Width );
				float yFrac = y * 1.0F / (Height);
				if(flipY == true)
					yFrac = (1 - y - 2) * 1.0F / (Height);
				destPix[y * Width + x] = sourceTex.GetPixelBilinear(xFrac, yFrac);
				x++;
			}
			y++;
		}
		destTex.SetPixels(destPix);
		destTex.Apply();
		return destTex;
	}
	
	IEnumerator Capture(Cubemap cubemap,CubemapFace face,Camera cam)
	{
		var width = Screen.width;
		var height = Screen.height;

		Texture2D tex = new Texture2D(height, height, TextureFormat.ARGB32, false);
		int cubeSize = cubemap.height;
		
		cam.transform.localRotation = Rotation(face);
		
		yield return new WaitForEndOfFrame();

		tex.ReadPixels(new Rect((width-height)/2, 0, height, height), 0, 0);
		tex.Apply();
		tex = Resize(tex, cubeSize,cubeSize,false);

		Color cubeCol;
		for (int y = 0; y < cubeSize; y++)
		{
			for (int x = 0; x < cubeSize; x++)
			{
				cubeCol = tex.GetPixel(cubeSize + x, (cubeSize - 1) - y);

				cubemap.SetPixel(face, x, y, cubeCol);
			}
		}

		cubemap.Apply();

		DestroyImmediate(tex);
	}

	Quaternion RotationInv(CubemapFace face)
	{
		Quaternion result;
		switch(face)
		{
		case CubemapFace.PositiveX:
			result = Quaternion.Euler(0, 90, -180);
			break;
		case CubemapFace.NegativeX:
			result = Quaternion.Euler(0, -90, 180);
			break;
		case CubemapFace.PositiveY:
			result = Quaternion.Euler(-90, 0, 0);
			break;
		case CubemapFace.NegativeY:
			result = Quaternion.Euler(90, 0, 0);
			break;
		case CubemapFace.NegativeZ:
			result = Quaternion.Euler(-180, 0, 0);
			break;
		default:
			result = Quaternion.Euler(0, 0, -180);
			break;
		}
		return result;
	}

	IEnumerator CaptureImportanceSample(Cubemap cubemap,CubemapFace face,Camera cam, int mip)
	{
		var width = Screen.width;
		var height = Screen.height;
		Texture2D tex = new Texture2D(height, height, TextureFormat.ARGB32, false);

		cam.transform.localRotation = Rotation(face);
		
		yield return new WaitForEndOfFrame();
		
		tex.ReadPixels(new Rect((width-height)/2, 0, height, height), 0, 0);
		tex.Apply();

		int cubeSize = Mathf.Max(1, cubemap.width >> mip );
	
		tex = Resize(tex, cubeSize,cubeSize,true);

		Color[] tempCol = tex.GetPixels();

		cubemap.SetPixels(tempCol,face,mip);

		cubemap.Apply(false);

		DestroyImmediate(tex);
	}

	Quaternion Rotation(CubemapFace face)
	{
		Quaternion result;
		switch(face)
		{
		case CubemapFace.PositiveX:
			result = Quaternion.Euler(0, 90, 0);
			break;
		case CubemapFace.NegativeX:
			result = Quaternion.Euler(0, -90, 0);
			break;
		case CubemapFace.PositiveY:
			result = Quaternion.Euler(-90, 0, 0);
			break;
		case CubemapFace.NegativeY:
			result = Quaternion.Euler(90, 0, 0);
			break;
		case CubemapFace.NegativeZ:
			result = Quaternion.Euler(0, 180, 0);
			break;
		default:
			result = Quaternion.Euler(0, 0, 0);
			break;
		}
		return result;
	}

	void DoSetup()
	{
		if (SystemInfo.graphicsShaderLevel >= 30)
			isDX11 = true;
	}

	void DoUpdatePreview()
	{
		previewMaterial = new Material( Shader.Find("Hidden/Antonov Suit/Probe" ));
		previewMaterial.hideFlags = HideFlags.HideAndDontSave;

		previewMaterial.SetTexture("_DiffCubeIBL", diffuseCube);
		previewMaterial.SetTexture("_SpecCubeIBL", specularCube);
		previewMaterial.SetVector("_exposureIBL", new Vector4(specularExposure,diffuseExposure,0,0));
		
		if (previewProbe != null && previewProbe.GetComponent<MeshRenderer>())
		{
			previewProbe.GetComponent<MeshRenderer>().enabled = false;
		}
		if (previewProbe == null)
		{
			previewProbe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			previewProbe.name = this.name + "_Debug";
			DestroyImmediate(previewProbe.GetComponent<SphereCollider>(), false);
		}

		MeshRenderer targetRenderer = previewProbe.GetComponent<MeshRenderer>();
		targetRenderer.enabled = true;
		targetRenderer.material = previewMaterial;
		
		targetRenderer.castShadows = false;
		targetRenderer.receiveShadows = false;

		previewProbe.transform.parent = this.transform;
		previewProbe.transform.position = transform.position;
		previewProbe.transform.localScale = transform.localScale * 0.5f;
		previewProbe.hideFlags = HideFlags.HideInHierarchy;
	}

	// Use this for initialization
	void Start () 
	{

	}

	#if UNITY_EDITOR
	public void InitCreateCube()
	{
		EditorApplication.isPlaying = true;
		StartCoroutine(GoBake());  
	}

	public void InitConvolveIrradianceCube()
	{
		EditorApplication.isPlaying = true;
		StartCoroutine(GoConvolveIrradiance());  
	}
	public void InitConvolveRadianceCube()
	{
		EditorApplication.isPlaying = true;
		StartCoroutine(GoConvolveRadiance());  
	}

	IEnumerator GoBake()
	{
		StartCoroutine(Bake());
		goBake = true;
		yield return null;
	}
	IEnumerator GoConvolveIrradiance()
	{	
		StartCoroutine(ConvolveIrradiance());
		goConvolveIrradiance = true;
		yield return null;
	}
	IEnumerator GoConvolveRadiance()
	{	
		StartCoroutine(ConvolveRadiance());
		goConvolveRadiance = true;
		yield return null;
	}

	void OnEnable()
	{
		DoSetup(); //While in game or build
	}

	void OnDisable()
	{
		DestroyImmediate(previewMaterial, true);
		//DestroyImmediate(previewProbe);
	}

	IEnumerator Bake()
	{
		if (goBake && EditorApplication.isPlaying)
		{
			CameraSetup(false);
			
			StartCoroutine(CreateCubeMap(true));
			StartCoroutine(CreateCubeMap(false));  
		}		
		
		Resources.UnloadUnusedAssets();
		AssetDatabase.Refresh();
		yield return new WaitForEndOfFrame();
	}

	void CameraSetup(bool convolve)
	{
		// Create a camera that will be used to render the faces
		GameObject go = new GameObject("CubemapCamera", typeof(Camera));
		
		cubeCamera = go.GetComponent<Camera>();
		
		// Place the camera on this object
		cubeCamera.transform.position = transform.position;
		
		// Initialise the rotation - this will be changed for each texture grab
		cubeCamera.transform.rotation = Quaternion.identity;

		cubeCamera.nearClipPlane = 0.001f;
		cubeCamera.fieldOfView = 90;

		// Ensure this camera renders above all others
		cubeCamera.depth = float.MaxValue;

		if( goConvolveIrradiance == true || goConvolveRadiance == true)
		{
			cubeCamera.clearFlags = CameraClearFlags.Skybox;
		
			//Show sky only, previous attempt were using a inverted sphere but using a skybox is much better
			cubeCamera.cullingMask = 0;
		}

		// HDR TO RGBM
		if(RGBM == true )
		{
			hasPro = UnityEditorInternal.InternalEditorUtility.HasPro();

			if( hasPro == true && convolve == false )
			{	
				cubeCamera.hdr = true;	
				go.AddComponent<HDRtoRGBM>();
			}
			if( hasPro == false && convolve == false )
			{
				cubeCamera.hdr = false;
				go.AddComponent<RGBM>();
			}
			if( hasPro == true && convolve == true )
			{
				cubeCamera.hdr = true;
			}
			if( hasPro == false  && convolve == true )
			{
				cubeCamera.hdr = false;
			}
		}
	}


	IEnumerator ConvolveIrradiance()
	{
		if (goConvolveIrradiance && EditorApplication.isPlaying)
		{
			CameraSetup(true);

			StartCoroutine(ConvolveDiffuseCubeMap()); 
		}

		Resources.UnloadUnusedAssets();
		AssetDatabase.Refresh();
		yield return new WaitForEndOfFrame();	
	}

	IEnumerator ConvolveRadiance()
	{
		if (goConvolveRadiance && EditorApplication.isPlaying)
		{
			CameraSetup(true);

			StartCoroutine(ConvolveSpecularCubeMap()); 
		}

		Resources.UnloadUnusedAssets();
		AssetDatabase.Refresh();
		yield return new WaitForEndOfFrame();
	}

	IEnumerator ConvolveDiffuseCubeMap()
	{
		int size = 0;
		int samples = 0;
		size = CubeSizeSetup(true);
		samples = qualitySetup(true);

		if(irradianceModel == irradianceEnum.SphereUniform)
		{
			convolveDiffuseSkybox = new Material(Shader.Find("Hidden/Antonov Suit/Irradiance/Sphere"));
		}
		if(irradianceModel == irradianceEnum.HemisphereUniform)
		{
			convolveDiffuseSkybox = new Material(Shader.Find("Hidden/Antonov Suit/Irradiance/Hemisphere"));
		}
		if(irradianceModel == irradianceEnum.HemisphereCosine)
		{
			convolveDiffuseSkybox = new Material(Shader.Find("Hidden/Antonov Suit/Irradiance/Cosine"));
		}

		convolveDiffuseSkybox.SetInt("_diffSamples",samples);
		convolveDiffuseSkybox.SetInt("_diffuseSize", size);
		convolveDiffuseSkybox.SetTexture("_DiffCubeIBL", diffuseCube);

		UnityEngine.RenderSettings.skybox = convolveDiffuseSkybox;
			
		Cubemap tempCube = new Cubemap(size, TextureFormat.ARGB32, false);

		if( hasPro == true )
		{		
			cubeCamera.RenderToCubemap(tempCube);
		}
		else
		{
			yield return StartCoroutine(Capture(tempCube, CubemapFace.PositiveZ, cubeCamera));
			yield return StartCoroutine(Capture(tempCube, CubemapFace.PositiveX, cubeCamera));
			yield return StartCoroutine(Capture(tempCube, CubemapFace.NegativeX, cubeCamera));
			yield return StartCoroutine(Capture(tempCube, CubemapFace.NegativeZ, cubeCamera));
			yield return StartCoroutine(Capture(tempCube, CubemapFace.PositiveY, cubeCamera));
			yield return StartCoroutine(Capture(tempCube, CubemapFace.NegativeY, cubeCamera));
		}

		tempCube.Apply();

		diffuseCube = tempCube;

		string convolvedDiffusePath = GetOutPutPath(diffuseCube,true);
			
		AssetDatabase.CreateAsset(diffuseCube, convolvedDiffusePath);
		SerializedObject serializedCubemap = new SerializedObject(diffuseCube);
		SetLinearSpace(ref serializedCubemap, false);

		yield return StartCoroutine(CaptureFinished());
	}
	
	IEnumerator ConvolveSpecularCubeMap()
	{
		int size = 0;
		int samples = 0;
		size = CubeSizeSetup(false);
		samples = qualitySetup(false);

		if(radianceModel == radianceEnum.BlinnPhong)
		{
			convolveSpecularSkybox = new Material(Shader.Find("Hidden/Antonov Suit/Radiance/Blinn"));
		}

		if(radianceModel == radianceEnum.GGX)
		{
			convolveSpecularSkybox = new Material(Shader.Find("Hidden/Antonov Suit/Radiance/GGX"));
		}

		convolveSpecularSkybox.SetInt("_specSamples",samples);
		convolveSpecularSkybox.SetInt("_specularSize", size);
		convolveSpecularSkybox.SetTexture("_SpecCubeIBL", specularCube);

		UnityEngine.RenderSettings.skybox = convolveSpecularSkybox;

		Cubemap tempCube = new Cubemap(size, TextureFormat.ARGB32, true);

		for(int mip = 0; (size >> mip) > 0; mip++)
		{

			// v0.049
			float minExponent = 0.1f;
			float stepExp = 1 / (float)specularExponent * (float)mip + 0.01f;
			float exponent = Mathf.Max( stepExp, minExponent );

			convolveSpecularSkybox.SetFloat("_Shininess", exponent);

			int cubeSize = Mathf.Max(1, tempCube.width >> mip );

			Cubemap mipCube = new Cubemap(cubeSize, TextureFormat.ARGB32, false);

			if( hasPro == true )
			{		
				cubeCamera.RenderToCubemap(mipCube);

				for(int f=0; f<6; ++f) 
				{
					CubemapFace face = (CubemapFace)f;
					tempCube.SetPixels(mipCube.GetPixels(face), face, mip);
				}

			}
			else
			{
				yield return StartCoroutine(CaptureImportanceSample(tempCube, CubemapFace.PositiveZ, cubeCamera,mip));
				yield return StartCoroutine(CaptureImportanceSample(tempCube, CubemapFace.PositiveX, cubeCamera,mip));
				yield return StartCoroutine(CaptureImportanceSample(tempCube, CubemapFace.NegativeX, cubeCamera,mip));
				yield return StartCoroutine(CaptureImportanceSample(tempCube, CubemapFace.NegativeZ, cubeCamera,mip));
				yield return StartCoroutine(CaptureImportanceSample(tempCube, CubemapFace.PositiveY, cubeCamera,mip));
				yield return StartCoroutine(CaptureImportanceSample(tempCube, CubemapFace.NegativeY, cubeCamera,mip));
			}
		}

		// v0.035 this fix the ugly mipmap transition
		tempCube.filterMode = FilterMode.Trilinear;
		tempCube.wrapMode = TextureWrapMode.Clamp;

		if (SystemInfo.graphicsShaderLevel != 50)
		{
			tempCube.SmoothEdges(smoothEdge);
		}

		tempCube.Apply(false);

		specularCube = tempCube;

		string convolvedSpecularPath = GetOutPutPath(specularCube,false);
	
		AssetDatabase.CreateAsset(specularCube, convolvedSpecularPath);
		SerializedObject serializedCubemap = new SerializedObject(specularCube);
		SetLinearSpace(ref serializedCubemap, false);

		yield return StartCoroutine(CaptureFinished());
	}

	// This is the coroutine that creates the cubemap images
	IEnumerator CreateCubeMap(bool diffuse)
	{

		int size;
		if(diffuse == true)
		{
			size = CubeSizeSetup(true);
		}
		else
		{
			size = CubeSizeSetup(false);
		}

		Cubemap tempCube = new Cubemap(size, TextureFormat.ARGB32, true);

		yield return StartCoroutine(Capture(tempCube, CubemapFace.PositiveZ, cubeCamera));
		yield return StartCoroutine(Capture(tempCube, CubemapFace.PositiveX, cubeCamera));
		yield return StartCoroutine(Capture(tempCube, CubemapFace.NegativeX, cubeCamera));
		yield return StartCoroutine(Capture(tempCube, CubemapFace.NegativeZ, cubeCamera));
		yield return StartCoroutine(Capture(tempCube, CubemapFace.PositiveY, cubeCamera));
		yield return StartCoroutine(Capture(tempCube, CubemapFace.NegativeY, cubeCamera));

		// v0.035 this fix the ugly mipmap transition
		tempCube.filterMode = FilterMode.Trilinear;
		tempCube.wrapMode = TextureWrapMode.Clamp;

		if (SystemInfo.graphicsShaderLevel != 50)
		{
			tempCube.SmoothEdges(smoothEdge);
		}
		
		tempCube.Apply();

		if(diffuse == true)
		{
			diffuseCube = tempCube;

			string diffusePath = GetOutPutPath(diffuseCube,true);

			AssetDatabase.CreateAsset(diffuseCube, diffusePath);
			SerializedObject serializedCubemap = new SerializedObject(diffuseCube);
			SetLinearSpace(ref serializedCubemap, false);
		}
		else
		{
			specularCube = tempCube;

			string specularPath = GetOutPutPath(specularCube,false);

			AssetDatabase.CreateAsset(specularCube, specularPath);
			SerializedObject serializedCubemap = new SerializedObject(specularCube);
			SetLinearSpace(ref serializedCubemap, false);
		}

		yield return StartCoroutine(CaptureFinished());
	}
	
	IEnumerator CaptureFinished()
	{
		done = true;
		yield return null;
	}
	#endif

	void DoSpecularLod()
	{
		int lod = CubeLodSetup();
		
		specularExponent = lod;
	}

	void DoShaderUpdate()
	{
		if(Meshes != null)
		{
			foreach (GameObject cubeMeshes  in Meshes )
			{
				
				Renderer[] renderers = cubeMeshes.GetComponentsInChildren<Renderer>();
				
				foreach (Renderer mr in renderers) 
				{	
					this.m_materials = mr.renderer.sharedMaterials;
					foreach( Material mat in this.m_materials ) 
					{
						switch(typeOfProjection)
						{
						case ProjectionType.InfiniteProjection:
							mat.SetInt("ANTONOV_PROJECTION", 0);
							
							break;
						case ProjectionType.SphereProjection:
							mat.SetInt("ANTONOV_PROJECTION", 1);
							mat.SetVector("_cubemapPosScale", new Vector4(this.transform.position.x, this.transform.position.y, this.transform.position.z, probeRadius));
							
							break;
						case ProjectionType.BoxProjection:
							mat.SetInt("ANTONOV_PROJECTION", 2);
							
							probeMatrix.SetTRS(this.transform.position, this.transform.rotation, Vector3.one);
							probeMatrixTranspose = probeMatrix.transpose;
							probeMatrixInverse = probeMatrix.inverse;
							
							mat.SetMatrix("_WorldToCube", probeMatrixTranspose);
							mat.SetMatrix("_WorldToCubeInverse", probeMatrixInverse);
							mat.SetVector("_cubemapBoxSize", probeBoxSize);
							
							break;
						}

						mat.SetInt("ANTONOV_IMPORTANCE_SAMPLING",0);
						
						mat.SetInt("_lodSpecCubeIBL", specularExponent);
						
						if(diffuseCube != null)
							mat.SetTexture("_DiffCubeIBL", diffuseCube);
						
						if(specularCube != null)
							mat.SetTexture("_SpecCubeIBL", specularCube);
						
						
						mat.SetVector("_exposureIBL", new Vector4(specularExposure,diffuseExposure,1,1));
					}
				}
			}
		}
	}

	void DoReset()
	{
		goBake = false;
		goConvolveIrradiance = false;
		goConvolveRadiance = false;
	}

	void LateUpdate() 
	{

		DoSpecularLod(); //While in game or build

		#if UNITY_EDITOR
		emptyCube = Resources.Load("emptyCube", typeof( Cubemap ) ) as Cubemap;

		if(goBake && EditorApplication.isPlaying == true)
		{
			if(bakeDirectAndIBL == false)
			{
				diffuseCube = emptyCube;
				specularCube = emptyCube;
			}
		}
		if(UnityEditor.EditorApplication.isPlaying && goBake == true)
		{
			StartCoroutine(Bake());
			goBake = false;
		}
		if(UnityEditor.EditorApplication.isPlaying && goConvolveIrradiance == true )
		{
			StartCoroutine(ConvolveIrradiance());
			goConvolveIrradiance = false;
		}
		if(UnityEditor.EditorApplication.isPlaying && goConvolveRadiance == true )
		{
			StartCoroutine(ConvolveRadiance());
			goConvolveRadiance = false;
		}
		if(UnityEditor.EditorApplication.isPlaying == false)
		{
			goBake = false;
			goConvolveIrradiance = false;
			goConvolveRadiance = false;
		}
		if(done == true)
		{
			goBake = false;
			goConvolveIrradiance = false;
			goConvolveRadiance = false;


			CleanUp();
			UnityEditor.EditorApplication.isPlaying = false;    
		}
		#endif

		DoShaderUpdate();
	}

	void CleanUp()
	{
		foreach (GameObject cubeCamera in GameObject.FindObjectsOfType<GameObject>())
		{
			if (cubeCamera.name == "CubemapCamera")
			{
				DestroyImmediate(cubeCamera);
			}
		}
	}

	void OnDestroy() 
	{
		DestroyImmediate(previewProbe);
		DestroyImmediate(previewMaterial);
		DestroyImmediate(convolveDiffuseSkybox);
		DestroyImmediate(convolveSpecularSkybox);
	}

	void OnDrawGizmosSelected()
	{
		if (typeOfProjection == ProjectionType.SphereProjection) 
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(this.transform.position, probeRadius );
		}
		if (typeOfProjection == ProjectionType.BoxProjection) 
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(this.transform.position, probeBoxSize );
		}

		#if UNITY_EDITOR
			DoUpdatePreview(); 
		#endif
		
	}

	void OnDrawGizmos()
	{
		DestroyImmediate(previewProbe,true);
		DestroyImmediate(previewMaterial,true);

		Gizmos.DrawIcon(transform.position, "../Antonov Suit/Resources/probe.tga", true);
	}
}
