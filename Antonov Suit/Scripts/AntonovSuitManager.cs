using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.IO;

[ExecuteInEditMode]
public class AntonovSuitManager : MonoBehaviour 
{
	public List<GameObject> probes = new List<GameObject>();

	private Texture skinLUT;
	private Texture envSkinLUT;
	private Texture envLUT;

	public Material skyBoxMaterial = null;
	
	public Color ambientColor = new Color(0.5f,0.5f,0.5f,0.0f);


	public Cubemap diffuseCube = null;
	public float diffuseExposure = 1;
	public Cubemap specularCube = null;
	public float specularExposure = 1;

	private int specularSize;
	public int specularExponent = 1;

	public int CubeLodSetup()
	{
		int result= 0;
		
		if(specularSize == 64)
		{
			result = 6;
		}
		if(specularSize == 128)
		{
			result  = 7;
		}
		if(specularSize == 256)
		{
			result  = 8;
		}
		if(specularSize == 512)
		{
			result  = 9;
		}
		
		return result;
	}

	public void GetCubemapSize()
	{
		if(specularCube != null)
			specularSize = specularCube.height;

		int lod = CubeLodSetup();
		
		specularExponent = lod;

	}


	public void GetAntonovSuitTexture()
	{
		skinLUT = Resources.Load("SKIN_LUT",typeof(Texture)) as Texture;
		envSkinLUT = Resources.Load("BLINN_SMITH_LUT",typeof(Texture)) as Texture;
		envLUT = Resources.Load("GGX_SMITH_LUT",typeof(Texture)) as Texture;
	}

	public void DoUpdate()
	{
		if(skyBoxMaterial != null)
			RenderSettings.skybox = skyBoxMaterial;

		GetCubemapSize();

		if(diffuseCube != null)
			Shader.SetGlobalTexture("_SkyDiffCubeIBL", diffuseCube);
		if(specularCube != null)
			Shader.SetGlobalTexture("_SkySpecCubeIBL", specularCube);

		RenderSettings.ambientLight = ambientColor;

		Shader.SetGlobalVector("_exposureIBL", new Vector4(specularExposure,diffuseExposure,1,1));
		Shader.SetGlobalTexture("_SKIN_LUT", skinLUT);
		Shader.SetGlobalTexture("_ENV_LUT", envLUT);
		Shader.SetGlobalTexture("_ENV_SKIN_LUT", envSkinLUT);

	}

	void Start()
	{

	}

	public void Awake()
	{
		GetAntonovSuitTexture();
	}

	public void LateUpdate () 
	{
		DoUpdate();
	}

	public void OnDrawGizmos () 
	{
		Gizmos.DrawIcon(transform.position, "../Antonov Suit/Resources/sky.tga", true);
	}
}
