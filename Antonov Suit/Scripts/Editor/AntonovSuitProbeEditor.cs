// Created by Charles Greivelding

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(AntonovSuitProbe))]
public class AntonovSuitProbeEditor : Editor
{

	AntonovSuitProbe m_target;

	private List<GameObject> Meshes = new List<GameObject>();

	//private GameObject Meshes = new GameObject();
	private GameObject m_Meshes = null;

	private Object diffuseCubeObject;
	private Object specularCubeObject;

	static bool c_showMeshes = false;
	static bool c_showCube = true;
	static bool c_showSmoothEdge = true;

	static bool c_showSphereProjection = false;
	static bool c_showBoxProjection = false;
	
	void OnEnable()
	{
		m_target = (AntonovSuitProbe)target;
	}

	public override void OnInspectorGUI()
	{

		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
		buttonStyle.margin = new RectOffset(4,4,8,8);
		buttonStyle.padding = new RectOffset(8, 8, 8, 8);

		Texture2D logo = Resources.Load("logo", typeof(Texture2D))as Texture2D;

		EditorGUILayout.Space();
		GUILayout.Label( logo,GUILayout.Width(128),GUILayout.Height(128));
		
		EditorGUILayout.Space();
		GUILayout.Label("Baking Settings", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		EditorGUI.indentLevel += 1;
			
			m_target.cubemapFolder = EditorGUILayout.TextField("Output Path", m_target.cubemapFolder);
			m_target.cubemapName = EditorGUILayout.TextField("Cubemap Name", m_target.cubemapName);

			c_showCube = EditorGUILayout.Foldout(c_showCube, "Cubemap Settings" );
			if(c_showCube)
			{
				EditorGUI.indentLevel += 1;
					m_target.diffuseSize = (AntonovSuitProbe.facesSize)EditorGUILayout.EnumPopup ("Diffuse Face Size", m_target.diffuseSize);
					m_target.specularSize = (AntonovSuitProbe.facesSize)EditorGUILayout.EnumPopup ("Specular Face Size",m_target.specularSize);
				EditorGUI.indentLevel -= 1;
			}
		if(m_target.isDX11 == false)
		{
			c_showSmoothEdge = EditorGUILayout.Foldout(c_showSmoothEdge, "Smooth Edge Settings (DX9)" );	
			if(c_showSmoothEdge)
			{
				EditorGUI.indentLevel += 1;
					m_target.smoothEdge = EditorGUILayout.IntField("Edge Width", m_target.smoothEdge);
				EditorGUI.indentLevel -= 1;
			}
		}

		EditorGUI.indentLevel -= 1;

		EditorGUILayout.BeginVertical();
		if (GUILayout.Button("Bake Probe", buttonStyle))
		{
			m_target.bakeDirectAndIBL = false;
			m_target.InitCreateCube();
			
		}
		if (GUILayout.Button("Bake Probe With IBL", buttonStyle))
		{
			m_target.bakeDirectAndIBL = true;
			m_target.InitCreateCube();	
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();
		GUILayout.Label("Convolution Settings", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		GUIStyle convolveStyle = new GUIStyle(GUI.skin.button);
		convolveStyle.fixedWidth = 128;
		convolveStyle.margin = new RectOffset(8,0,0,0);
		convolveStyle.padding = new RectOffset(8, 8, 16, 16);

		EditorGUILayout.BeginHorizontal();

			EditorGUILayout.BeginVertical();
				EditorGUI.indentLevel += 1;
					m_target.diffuseSamples = (AntonovSuitProbe.qualitySamples)EditorGUILayout.EnumPopup ("Diffuse Quality", m_target.diffuseSamples);
					m_target.irradianceModel = (AntonovSuitProbe.irradianceEnum)EditorGUILayout.EnumPopup ("Diffuse Model :",m_target.irradianceModel);
				EditorGUI.indentLevel -= 1;
			EditorGUILayout.EndVertical();
	
		if (GUILayout.Button("Convolve Diffuse", convolveStyle))
			m_target.InitConvolveIrradianceCube();

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.BeginVertical();
		EditorGUI.indentLevel += 1;
			m_target.specularSamples = (AntonovSuitProbe.qualitySamples)EditorGUILayout.EnumPopup ("Specular Quality", m_target.specularSamples);
			m_target.radianceModel = (AntonovSuitProbe.radianceEnum)EditorGUILayout.EnumPopup ("Specular Model :", m_target.radianceModel);
		EditorGUI.indentLevel -= 1;
		EditorGUILayout.EndVertical();

		if (GUILayout.Button("Convolve Specular", convolveStyle))
			m_target.InitConvolveRadianceCube();

		EditorGUILayout.EndHorizontal();
	
		EditorGUILayout.Space();
		GUILayout.Label("Probe Settings", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical();
		EditorGUI.indentLevel += 1;
		c_showMeshes = EditorGUILayout.Foldout(c_showMeshes, "Objects" );
		if(c_showMeshes == true)
		{
		EditorGUILayout.BeginHorizontal();

		for (int i = 0; i < m_target.Meshes.Count; i++)
		{
			m_target.Meshes[i] = (GameObject)EditorGUILayout.ObjectField( m_target.Meshes[i], typeof(GameObject), true);

			if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(50)) ) 
			{
				m_target.Meshes.RemoveAt(i);
			}

		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		m_Meshes = (GameObject)EditorGUILayout.ObjectField( m_Meshes, typeof(GameObject), true);
		if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(50)) ) 
		{
			if (m_Meshes) 
			{
				m_target.Meshes.Add(m_Meshes);
				m_Meshes = null;
			}
		}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUI.indentLevel -= 1;
		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();
		m_target.typeOfProjection = (AntonovSuitProbe.ProjectionType)EditorGUILayout.EnumPopup ("Cubemap Projection", m_target.typeOfProjection);

		if(m_target.typeOfProjection == AntonovSuitProbe.ProjectionType.SphereProjection)
		{
			c_showSphereProjection = true;

			if(c_showSphereProjection == true)
			{
				EditorGUI.indentLevel += 1;
				m_target.probeRadius = EditorGUILayout.FloatField("Cubemap Radius", m_target.probeRadius);
			}
		}
		if(m_target.typeOfProjection == AntonovSuitProbe.ProjectionType.BoxProjection)
		{
			c_showBoxProjection = true;

			if(c_showBoxProjection == true)
			{
				EditorGUI.indentLevel += 1;
				m_target.probeBoxSize = EditorGUILayout.Vector3Field("Cubemap Box Size", m_target.probeBoxSize);
			}
		}
		if(m_target.typeOfProjection == AntonovSuitProbe.ProjectionType.InfiniteProjection)
		{
			c_showSphereProjection = false;
			c_showBoxProjection = false;
		}
		
		EditorGUILayout.Space();

		GUILayout.Label("Diffuse Cubemap");
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();	
			diffuseCubeObject = EditorGUILayout.ObjectField(m_target.diffuseCube, typeof(Cubemap), false);
			m_target.diffuseCube = (Cubemap)diffuseCubeObject;

			m_target.diffuseExposure = EditorGUILayout.FloatField("Diffuse Exposure", m_target.diffuseExposure);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		GUILayout.Label("Specular Cubemap");
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
			specularCubeObject = EditorGUILayout.ObjectField(m_target.specularCube, typeof(Cubemap), false);
			m_target.specularCube = (Cubemap)specularCubeObject;
			
			m_target.specularExposure = EditorGUILayout.FloatField("Specular Exposure", m_target.specularExposure);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
	}
}
#endif
