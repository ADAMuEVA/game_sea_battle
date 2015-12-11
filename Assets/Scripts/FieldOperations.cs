using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOperations : MonoBehaviour {
	
	public const int FieldsHorCount = 10;
	public const int FieldsVerCount = 10;
	
	GameObject[] mPlaneObjects = new GameObject[FieldsHorCount * FieldsVerCount];
	
	public GameObject PlanePrefab;
	public GameObject RedPlanePrefab;
	public GameObject RedDotPrefab;
	public GameObject DestroyerPrefab;
	public GameObject CruiserPrefab;
	public GameObject BattleshipPrefab;
	public GameObject AircraftCarrierPrefab;
	public GameObject uiPlacing;
	public bool EnemyField;
	public GameObject GameObj;
	public GameObject FirePrefab;
	public GameObject LastCell;

	AimSelectedHandler mAimSelectHandler;
	
	int mSelectionLength = 0;
	bool mSelectionHorizontal = true;
	List<GameObject> mLastSelected = new List<GameObject>();
	List<GameObject> mShips = new List<GameObject>();
	Vector3 mFieldStart;
	Vector3 mFieldEnd;
	float mFieldWidth;
	float mFieldHeight;
	GameObject mPlaceShip;
	bool[,] mBusyCells = new bool[FieldsHorCount, FieldsVerCount];
	bool[,] mAttackedCells = new bool[FieldsHorCount, FieldsVerCount];
	public delegate void AimSelectedHandler(int x, int y);
	
	const float FirePosY = 0.5f;
	
	
	// Use this for initialization
	void Start () {
		
		//высчитывание размеров клеток и положения поля
		Vector3 pos = transform.position;
		Vector3 bounds = GetComponent<Renderer>().bounds.size;
		
		mFieldStart = new Vector3(
			pos.x - bounds.x / 2,
			pos.y,
			pos.z - bounds.z / 2);
		
		mFieldEnd = new Vector3(
			pos.x + bounds.x / 2,
			pos.y,
			pos.z + bounds.z / 2);
		
		mFieldWidth = (mFieldEnd.x - mFieldStart.x) / FieldsHorCount;
		mFieldHeight = (mFieldEnd.z - mFieldStart.z) / FieldsVerCount;

		
		//создание различных вспомогательных объектов для поля - клетки, красные рамки, точки
		CreateCellObjects(mFieldStart, mFieldEnd);


		for (int i = 0; i < FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldsVerCount; ++j)
			{
				mAttackedCells[i, j] = false;
			}
		}
		
		LastCell.GetComponent<MeshRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
		foreach (var obj in mLastSelected)
		{
			obj.GetComponent<MeshRenderer>().enabled = false;
		}
		

		
		//если стоит режим выбора ячеек
		if (mSelectionLength > 0)
		{

			RaycastHit hit;

		}
	}

	// создание различных вспомогательных объектов на поле 
	void CreateCellObjects(Vector3 start, Vector3 end)
	{
		for (int i = 0; i < FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldsVerCount; ++j)
			{
				mPlaneObjects[j * FieldsHorCount + i] = CreatePlaneObj(
					i, j, start, end);
			}
		}
	}

	GameObject CreatePlaneObj(int x, int y, Vector3 start, Vector3 end)
	{
		GameObject obj = (GameObject)Instantiate (
			PlanePrefab,
			new Vector3 (x * mFieldWidth + start.x + mFieldWidth / 2, 0.3f, y * mFieldHeight + start.z + mFieldHeight / 2), 
			Quaternion.Euler (-90, 0, 180));
		obj.transform.localScale = new Vector3 (5.1f, 5.1f, 1);
		obj.GetComponent<MeshRenderer> ().enabled = false;
		
		return obj;
	}

}