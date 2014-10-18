using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AntonovSuitPlayer : MonoBehaviour 
{

	private static GameObject[] probeObjects;
	public GameObject[] m_probeObjects;
	private static GameObject farest = null;
	private static GameObject closest = null;

	private Material[] m_materials;

	public float distance;
	public Vector3 diff;
	public Color lerping;

	public Cubemap previousCube;
	public Cubemap currentCube;
	public Cubemap nextCube;

	GameObject FindClosestCubemap(GameObject[] probe)
	{
		this.distance = Mathf.Infinity;
		foreach (GameObject point in probe)
		{
			this.diff = point.transform.position - transform.position;
			if (this.diff.sqrMagnitude < distance)
			{
				closest = point;
				this.distance = diff.sqrMagnitude;
			}
		}
		return closest;
	}
	
	// Use this for initialization
	void Start () 
	{

		//probeObjects = GetComponent<AntonovSuitManager>().probeObjects;

	}

	
	// Update is called once per frame
	void Update () 
	{


		m_probeObjects = GetComponent<AntonovSuitManager>().probeObjects;
		probeObjects = m_probeObjects;


		//previousCube = this.FindCubemap(probeObjects).GetComponent<AntonovSuitProbe>().specularCube;

		currentCube = this.FindClosestCubemap(probeObjects).GetComponent<AntonovSuitProbe>().specularCube;


		//nextCube = currentCube;

		float weight = Mathf.Sqrt(2.0f / (distance + 2.0f));

		Shader.SetGlobalFloat("_weight1", weight);
		Shader.SetGlobalFloat("_weight2", 1-weight);

		Shader.SetGlobalTexture("_DiffCubeIBL", this.FindClosestCubemap(probeObjects).GetComponent<AntonovSuitProbe>().diffuseCube);

		Shader.SetGlobalTexture("_SpecCubeIBL", currentCube);

		//Shader.SetGlobalTexture("_SpecCubeIBL_02", previousCube);

	/*
		Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
		
		foreach (Renderer mr in renderers) 
		{
			
			this.m_materials = mr.renderer.materials;
			
			foreach( Material mat in this.m_materials ) 
			{		
				mat.SetTexture("_DiffCubeIBL", this.FindClosestCubemap().GetComponent<AntonovSuitProbe>().diffuseCube);
				mat.SetTexture("_SpecCubeIBL", this.FindClosestCubemap().GetComponent<AntonovSuitProbe>().specularCube);
			}
		}
		*/
	}
}
