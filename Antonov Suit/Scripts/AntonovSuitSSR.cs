using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
[AddComponentMenu("Antonov Suit/Rendering/Screen Space Reflection")]


public class AntonovSuitSSR : MonoBehaviour
{
	public bool isDebug = false;
	
	private Camera gBufferCamera = null;
	private Color backGroundColorCamera = new Color(0.5f,0.5f,0.5f,0);
	
	//public LayerMask Layer;
	//public LayerMask gBufferLayer;
	//public Texture lut;
	private Texture Jitter;

	// SSR
	public bool isMetallic = true;
	public bool useSSR = true;
	public bool doBlur = true;

	//public GameObject probe;

	//public int blurIteration = 1;
	public  float reflectionBlur = 1.0f;
	public float reflectionIntensity = 1.0f;
	public float maxRoughness = 1.0f;
	public float reflectionBias = 0.06f;
	private float reflectionEdgeFactor = 0.25f;


	
	private int screenWidth;
	private int screenHeight;
	
	// G-Buffer
	private RenderTexture rtNormalGBuffer;
	private Material m_NormalGBuffer;

	private RenderTexture rtSpecularGBuffer;
	private Material m_SpecularGBuffer;

	private RenderTexture rtSSR;
	private Material m_SSR;

	//public Shader rendererShader = null;
	static Material m_rendererMaterial = null;
	protected Material rendererMaterial
	{
		get 
		{
			if (m_rendererMaterial == null) 
			{
				m_rendererMaterial = new Material(Shader.Find("Hidden/Antonov Suit/SSR"));
				m_rendererMaterial.hideFlags = HideFlags.DontSave;
			}
			return m_rendererMaterial;
		} 
	}

	void goVariable()
	{
		if(useSSR == true )
		{
			Jitter = Resources.Load("NOISE_128X128_JITTER",typeof(Texture)) as Texture;

			rendererMaterial.SetTexture("_Jitter",Jitter);

			rendererMaterial.SetFloat ("_reflectionStrength", reflectionIntensity);

			rendererMaterial.SetFloat ("_maxRoughness", maxRoughness);

			rendererMaterial.SetFloat("_edgeFactor",reflectionEdgeFactor);

			rendererMaterial.SetFloat("_reflectionBlur",reflectionBlur);

			rendererMaterial.SetFloat ("_reflectionBias", reflectionBias);

			//rendererMaterial.SetTexture ("_SpecCubeIBL", probe.GetComponent<AntonovSuitProbe>().specularCube);
			//rendererMaterial.SetInt ("_lodSpecCubeIBL", probe.GetComponent<AntonovSuitProbe>().specularExponent);
			//rendererMaterial.SetTexture ("_ENV_LUT", lut);
		}
	}

	void Start ()
	{
		InitCamera();
	}
	
	void LateUpdate ()
	{
		UpdateCamera();
	}

	public void ResetBuffer()
  	{
		DestroyImmediate(rtNormalGBuffer);
		DestroyImmediate(rtSpecularGBuffer);
		DestroyImmediate(rtSSR);

		rtNormalGBuffer = RenderTexture.GetTemporary(screenWidth,screenHeight,16,RenderTextureFormat.ARGBHalf);
		rtNormalGBuffer.Create();

		rtSpecularGBuffer = RenderTexture.GetTemporary(screenWidth,screenHeight,16,RenderTextureFormat.ARGB32);
		rtSpecularGBuffer.Create();

		rtSSR = RenderTexture.GetTemporary(screenWidth/2,screenHeight/2,16,RenderTextureFormat.ARGB32);
		rtSSR.Create();
  	}

	
	void InitCamera()
	{
		camera.depthTextureMode = DepthTextureMode.Depth;	
		
		if(gBufferCamera == null)
		{
			GameObject go = new GameObject ("G-Buffer_Camera", typeof(Camera));
			gBufferCamera = go.camera;
			gBufferCamera.enabled = false;
			gBufferCamera.hdr = true;
			gBufferCamera.clearFlags = CameraClearFlags.SolidColor;
			gBufferCamera.depth = -1;
			gBufferCamera.backgroundColor = new Vector4( backGroundColorCamera.r, backGroundColorCamera.r, backGroundColorCamera.b, backGroundColorCamera.a) ;
		}	
	}

	void UpdateCamera()
	{	

		//camera.cullingMask = Layer;
		//gBufferCamera.cullingMask = gBufferLayer;
		gBufferCamera.transform.parent = this.gameObject.camera.transform;
	   	gBufferCamera.transform.position = this.gameObject.camera.transform.position;
		gBufferCamera.transform.rotation = this.gameObject.camera.transform.rotation;
		gBufferCamera.transform.localScale = this.gameObject.camera.transform.localScale;
		gBufferCamera.fieldOfView = this.gameObject.camera.fieldOfView;
		gBufferCamera.farClipPlane = this.gameObject.camera.farClipPlane;
		gBufferCamera.nearClipPlane = this.gameObject.camera.nearClipPlane;	
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination) 
	{

		screenWidth = Screen.width;
		screenHeight = Screen.height;

		InitMaterialsAndBuffers();

		goMatrix();
		goVariable();
		
		// save current tagret texture
    	var texTmp = gBufferCamera.targetTexture;
		
		// save current rendering path
    	var pathTmp = gBufferCamera.renderingPath;

		//GL.ClearWithSkybox(false,gBufferCamera);
		
		// generate buffer always in forward
   		gBufferCamera.renderingPath = RenderingPath.Forward;

		// Replacement shader
		gBufferCamera.targetTexture = rtNormalGBuffer;
		gBufferCamera.RenderWithShader(m_NormalGBuffer.shader, "RenderType");

		gBufferCamera.targetTexture = rtSpecularGBuffer;
		gBufferCamera.RenderWithShader(m_SpecularGBuffer.shader, "RenderType");

		// G-Buffer
		rendererMaterial.SetTexture ("_WorldNormal_GBUFFER", rtNormalGBuffer);
		rendererMaterial.SetTexture ("_Specular_GBUFFER", rtSpecularGBuffer);

		if(useSSR == true )
		{

			rendererMaterial.SetTexture ("_Reflection_Pass", rtSSR);

			Graphics.Blit (source,rtSSR, rendererMaterial,0);

			if(doBlur == true)
			{

				RenderTexture blur = RenderTexture.GetTemporary( screenWidth/2, screenHeight/2,16,RenderTextureFormat.ARGB32);

				Graphics.Blit (rtSSR, blur, rendererMaterial, 3);
				rendererMaterial.SetTexture ("_Reflection_Pass", blur);

				Graphics.Blit (source,destination, rendererMaterial,1); // FragCompose

				RenderTexture.ReleaseTemporary(blur);

				/*
				RenderTexture blurX = RenderTexture.GetTemporary( screenWidth, screenHeight,16,RenderTextureFormat.ARGB32);
				RenderTexture blurY = RenderTexture.GetTemporary( screenWidth, screenHeight,16,RenderTextureFormat.ARGB32);

				for(int i=0;i<blurIteration;i++)
				{
					Graphics.Blit (rtSSR, blurX, rendererMaterial, 4);
					rendererMaterial.SetTexture ("_Reflection_Pass", blurX);
					Graphics.Blit (blurX, blurY, rendererMaterial, 5);
					rendererMaterial.SetTexture ("_Reflection_Pass", blurY);
				}

				//Graphics.Blit (blurY,rtSSR, rendererMaterial,6);
				//rendererMaterial.SetTexture ("_Reflection_Pass", rtSSR);

				Graphics.Blit (source,destination, rendererMaterial,1); // FragCompose
				
				RenderTexture.ReleaseTemporary(blurX);
				RenderTexture.ReleaseTemporary(blurY);
				*/

			}
			else
			{
				Graphics.Blit (source,destination, rendererMaterial,1); // FragCompose
			}

			RenderTexture.ReleaseTemporary(rtSSR);

		}
		else
		{
			Graphics.Blit (source,destination, rendererMaterial,2); // FragBase
		}


		RenderTexture.ReleaseTemporary(rtNormalGBuffer);
		RenderTexture.ReleaseTemporary(rtSpecularGBuffer);


		// restore rendering path
    	gBufferCamera.renderingPath = pathTmp;
		
		// restore previous target texture
   		gBufferCamera.targetTexture = texTmp;
		RenderTexture.active = null;

	}

	void goMatrix() 
	{
		rendererMaterial.SetMatrix("_WorldViewMatrix", camera.worldToCameraMatrix);
		rendererMaterial.SetMatrix("_ViewWorldMatrix", camera.cameraToWorldMatrix);
		rendererMaterial.SetMatrix("_WorldViewInverseMatrix", camera.worldToCameraMatrix.inverse.transpose);
		rendererMaterial.SetMatrix("_ViewProjectionMatrix", camera.projectionMatrix * camera.worldToCameraMatrix);
		rendererMaterial.SetMatrix("_ViewProjectionInverseMatrix", (camera.projectionMatrix*camera.worldToCameraMatrix).inverse );

		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
		if (d3d) 
		{
			// Scale and bias from OpenGL -> D3D depth range
			for (int i = 0; i < 4; i++) 
			{
				projectionMatrix[2,i] = projectionMatrix[2,i]*0.5f + projectionMatrix[3,i]*0.5f;
			}
		}

		rendererMaterial.SetMatrix("_ProjectionMatrix", projectionMatrix);
		rendererMaterial.SetMatrix("_ProjectionInverseMatrix", camera.projectionMatrix.inverse);

	}
	
	void InitMaterialsAndBuffers()
	{	
		// init render buffer
		ResetBuffer();

		m_NormalGBuffer = new Material(Shader.Find("Hidden/G-Buffer/WorldNormal"));
		if (m_NormalGBuffer == null)
	    {
			Debug.LogError("Unable to find shader Hidden/G-Buffer/WorldNormal");
	    }

		if(isMetallic == true)
		{
			m_SpecularGBuffer = new Material(Shader.Find("Hidden/G-Buffer/Metallic Specular"));
			if (m_SpecularGBuffer == null)
		    {
				Debug.LogError("Unable to find shader Hidden/G-Buffer/Metallic Specular");
		    }
		}
		else
		{
			m_SpecularGBuffer = new Material(Shader.Find("Hidden/G-Buffer/Specular"));
			if (m_SpecularGBuffer == null)
			{
				Debug.LogError("Unable to find shader Hidden/G-Buffer/Specular");
			}
		}
	}

	void OnGUI()
	{
		if (isDebug)
		{
			GUI.DrawTexture(new Rect(16, 16, 256, 256),rtNormalGBuffer);
			GUI.DrawTexture(new Rect(16, 272, 256, 256),rtSpecularGBuffer);
		}
	}
}
