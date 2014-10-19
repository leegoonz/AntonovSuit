#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(AntonovSuitSky))]
public class AntonovSuitSkyEditor : Editor 
{
	[MenuItem ("Antonov Suit/GameObject/Sky")]
	public static  AntonovSuitSky addAntonovSuitSky() 
	{
		GameObject go = new GameObject("AntonovSuitSky");
		go.AddComponent("AntonovSuitSky");

		Selection.activeGameObject = go;
		AntonovSuitSky s_AntonovSuitSky = go.GetComponent<AntonovSuitSky>();
		
		Undo.RegisterCreatedObjectUndo(go, "Add Sky");
		return s_AntonovSuitSky;
	}

	AntonovSuitSky m_target;

	List<GameObject> probes;

	private Object skyboxObject;
	private Object diffuseCubeObject;
	private Object specularCubeObject;

	void OnEnable()
	{
		probes = (target as AntonovSuitSky).probes;
		m_target = (AntonovSuitSky)target;
	}

	public override void OnInspectorGUI()
	{
		if (m_target != null)
		{
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.margin = new RectOffset(4,4,8,8);
			buttonStyle.padding = new RectOffset(8, 8, 8, 8);

			EditorGUILayout.Space();
			GUILayout.Label("Render Settings", EditorStyles.boldLabel);
			//EditorGUILayout.Space();

			EditorGUI.indentLevel += 1;
			//m_target.ambientSkyColor = EditorGUILayout.ColorField( "Ambient Sky Color", m_target.ambientSkyColor);
			m_target.ambientColor = EditorGUILayout.ColorField( "Ambient Light", m_target.ambientColor);
			skyboxObject = EditorGUILayout.ObjectField("Skybox Material", m_target.skyBoxMaterial, typeof(Material), false);
			m_target.skyBoxMaterial = (Material)skyboxObject;
			EditorGUI.indentLevel -= 1;

			EditorGUILayout.Space();
			GUILayout.Label("Probes", EditorStyles.boldLabel);
			//EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Probe",buttonStyle))
			{
				string probeName = "Probe_" + probes.Count.ToString();
				GameObject newProbes = new GameObject(probeName);
				
				if (Selection.activeTransform != null)
				{
					newProbes.transform.parent = Selection.activeTransform;
					newProbes.transform.position = newProbes.transform.parent.position;
					newProbes.AddComponent<AntonovSuitProbe>();
					probes.Add(newProbes);
					Undo.RegisterCreatedObjectUndo(newProbes, "Add Probe");
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if( GUILayout.Button("Remove Probe",buttonStyle))
			{
				foreach( GameObject probe in probes )
					DestroyImmediate( probe.gameObject, false );
				probes.Clear();
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
			GUILayout.Label("Cubemap Settings", EditorStyles.boldLabel);
			//EditorGUILayout.Space();

			EditorGUI.indentLevel += 1;
			m_target.diffuseExposure = EditorGUILayout.FloatField("Diffuse Exposure",m_target.diffuseExposure);
			EditorGUILayout.BeginHorizontal();	
			diffuseCubeObject = EditorGUILayout.ObjectField("Diffuse Cubemap",m_target.diffuseCube, typeof(Cubemap), false);
			m_target.diffuseCube = (Cubemap)diffuseCubeObject;
			EditorGUILayout.EndHorizontal();

			m_target.specularExposure = EditorGUILayout.FloatField("Specular Exposure",m_target.specularExposure);
			EditorGUILayout.BeginHorizontal();
			specularCubeObject = EditorGUILayout.ObjectField("Specular Cubemap",m_target.specularCube, typeof(Cubemap), false);
			m_target.specularCube = (Cubemap)specularCubeObject;
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel -= 1;
		}
	}
}
#endif