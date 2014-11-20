#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(AntonovSuitManager))]
public class AntonovSuitManagerEditor : Editor 
{
	[MenuItem ("Antonov Suit/Antonov Suit Manager")]
	public static  AntonovSuitManager addAntonovSuitManager() 
	{
		GameObject go = new GameObject("AntonovSuitManager");
		go.AddComponent("AntonovSuitManager");

		Selection.activeGameObject = go;
		AntonovSuitManager s_AntonovSuitManager = go.GetComponent<AntonovSuitManager>();
		
		Undo.RegisterCreatedObjectUndo(go, "Add Sky");
		return s_AntonovSuitManager;
	}
	
	AntonovSuitManager m_target;

	private List<GameObject> probes = new List<GameObject>();

	private Object skyboxObject;
	private Object diffuseCubeObject;
	private Object specularCubeObject;

	private bool c_showProbes = false;

	void OnEnable()
	{
		probes = (target as AntonovSuitManager).probes;

		m_target = (AntonovSuitManager)target;
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

			EditorGUILayout.BeginVertical();
			EditorGUI.indentLevel += 1;
			//m_target.ambientSkyColor = EditorGUILayout.ColorField( "Ambient Sky Color", m_target.ambientSkyColor);
			m_target.ambientColor = EditorGUILayout.ColorField( "Ambient Light", m_target.ambientColor);
			skyboxObject = EditorGUILayout.ObjectField("Skybox Material", m_target.skyBoxMaterial, typeof(Material), false);
			m_target.skyBoxMaterial = (Material)skyboxObject;
			EditorGUI.indentLevel -= 1;
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			GUILayout.Label("Probes", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Probe",buttonStyle))
			{
				
				string probeName = "Probe_" + probes.Count.ToString();
				//string probeName = "!Rename Me!";

				GameObject newProbes = new GameObject(probeName);
				
				if (Selection.activeTransform != null)
				{
					newProbes.transform.parent = Selection.activeTransform;
					newProbes.transform.position = newProbes.transform.parent.position;
					newProbes.AddComponent<AntonovSuitProbe>();
					newProbes.tag = "AntonovSuitProbe";
					probes.Add(newProbes);
				}
			}
			GUILayout.EndHorizontal();
			
			EditorGUILayout.BeginVertical();
			EditorGUI.indentLevel += 1;
			c_showProbes = EditorGUILayout.Foldout(c_showProbes, "Probe Objects");
			if(	c_showProbes == true)
			{
				for (int i = 0; i < m_target.probes.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					m_target.probes[i] = (GameObject)EditorGUILayout.ObjectField( m_target.probes[i], typeof(GameObject), true);

					if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(50)) )
					{
						DestroyImmediate( m_target.probes[i], false );
						m_target.probes.RemoveAt(i);
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUI.indentLevel -= 1;
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			GUILayout.Label("Cubemap Settings", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical();
			EditorGUI.indentLevel += 1;
			GUILayout.Label("Diffuse Cubemap");
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();	
			diffuseCubeObject = EditorGUILayout.ObjectField(m_target.diffuseCube, typeof(Cubemap), false);
			m_target.diffuseCube = (Cubemap)diffuseCubeObject;
			m_target.diffuseExposure = EditorGUILayout.FloatField("Diffuse Exposure",m_target.diffuseExposure);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			GUILayout.Label("Specular Cubemap");
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			specularCubeObject = EditorGUILayout.ObjectField(m_target.specularCube, typeof(Cubemap), false);
			m_target.specularCube = (Cubemap)specularCubeObject;
			m_target.specularExposure = EditorGUILayout.FloatField("Specular Exposure",m_target.specularExposure);
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel -= 1;
			EditorGUILayout.EndVertical();
		}
	}
}
#endif