using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOperations : MonoBehaviour {

	public const int FieldsHorCount = 10;
	public const int FieldsVerCount = 10;

	GameObject[] mPlaneObjects = new GameObject[FieldsHorCount * FieldsVerCount];
	GameObject[] mRedPlaneObjects = new GameObject[FieldsHorCount * FieldsVerCount];
	GameObject[] mRedDotObjects = new GameObject[FieldsHorCount * FieldsVerCount];

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

	GameOrder mGame;
	AimSelectedHandler mAimSelectHandler;

	class FireObject
	{
		public GameObject Object;
		public int X;
		public int Y;

		public FireObject(GameObject obj, int x, int y)
		{
			Object = obj;
			X = x;
			Y = y;
		}
	}


	int mSelectionLength = 0;
	bool mSelectionHorizontal = true;
	List<GameObject> mLastSelected = new List<GameObject>();
	List<GameObject> mShips = new List<GameObject>();
	List<FireObject> mFireList = new List<FireObject>();
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

		mGame = GameObj.GetComponent<GameOrder>();

		//создание различных вспомогательных объектов для поля - клетки, красные рамки, точки
		CreateCellObjects(mFieldStart, mFieldEnd);

		RefreshRedPlanes();
		RefreshBusyCells();

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

		ValidateFireList();

		//если стоит режим выбора ячеек
		if (mSelectionLength > 0)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			int cell_x = -1;
			int cell_y = -1;

			// при нажатии средней кнопки мыши - поворот корабля
			if (Input.GetMouseButtonDown(2))
			{
				mSelectionHorizontal = !mSelectionHorizontal;
			}

			// просчет попадания в клетку
			if (Physics.Raycast(ray, out hit))
			{
				int num = 0;
				foreach (var obj in mPlaneObjects)
				{
					if (obj == hit.collider.gameObject)
					{
						cell_x = num % FieldsHorCount;
						cell_y = num / FieldsVerCount;

						if (mSelectionLength % 2 == 0)
						{
							if (mSelectionHorizontal && hit.textureCoord.x < 0.5)
							{
								cell_x--;
							}
							else if (!mSelectionHorizontal && hit.textureCoord.y < 0.5)
							{
								cell_y--;
							}
						}

						if(mSelectionLength > 2)
						{
							if (mSelectionHorizontal)
								cell_x--;
							else
								cell_y--;
						}
					}

					num++;
				}

				if (mGame.State == GameOrder.GameState.Placing)
				{
					//если расстановка кораблей - проверка, можно ли поставить корабль
					Ship ship = mPlaceShip.GetComponent<Ship>();
					if (VerifyCell(cell_x, cell_y))
					{
						//перемещение корабля
						ship.Move(mSelectionHorizontal, cell_x, cell_y);

						//при нажатии первой кнопки мыши - корабль помещается на поле
						if (Input.GetMouseButtonDown(0))
						{
							mSelectionLength = 0;
							mPlaceShip = null;
						}
					}
					else
					{
						//скрывается корабль
						ship.Move(mSelectionHorizontal, -1, -1);
					}
				}
				else if (mGame.State == GameOrder.GameState.PlayerTurn ||
					mGame.State == GameOrder.GameState.EnemyTurn)
				{
					//если идет бой, проверка, можно ли выбрать данную клетку для удара
					if(cell_x >= 0 && cell_x < FieldsHorCount &&
						cell_y >= 0 && cell_y < FieldsVerCount)
					{
						if (!mAttackedCells[cell_x, cell_y])
						{
							//клетка помечается как выбранная
							GameObject plane = mPlaneObjects[cell_x + cell_y * FieldsHorCount];
							plane.GetComponent<MeshRenderer>().enabled = true;
							mLastSelected.Add(plane);

							//при нажатии первой кнопки мыши - производится удар
							if (Input.GetMouseButtonDown(0))
							{
								if (mAimSelectHandler != null)
								{
									mAimSelectHandler(cell_x, cell_y);
								}
								mSelectionLength = 0;
								mPlaceShip = null;
							}
						}
					}
				}
			}
			else
			{
				//если мышь не находится над клеткой - тогда кораблья скрывается
				if (mGame.State == GameOrder.GameState.Placing)
				{
					Ship ship = mPlaceShip.GetComponent<Ship>();
					ship.Move(mSelectionHorizontal, -1, -1);
				}
			}

			//если выбран корабль для помещения на поле, то нажатие второй кнопки мыши
			//убирает корабль
			if (mGame.State == GameOrder.GameState.Placing)
			{
				if (Input.GetMouseButtonDown(1))
				{
					mSelectionLength = 0;
					mShips.Remove(mPlaceShip);
					Destroy(mPlaceShip);

					BattleshipsPlacing placing = uiPlacing.GetComponent<BattleshipsPlacing>();
					placing.RefreshButtonsTexts();
				}
			}

			RefreshRedPlanes();
			RefreshBusyCells();
		}
		else if (Input.GetMouseButtonDown(1))
		{
			// если не выбран кораблья чтоб поставить на поле и нажата вторая кнопка мыши
			// проверяется выбран ли какой то корабль на поле
			// если корабль выбран, то он удаляется
			if (mGame.State == GameOrder.GameState.Placing)
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit))
				{
					int num = 0;
					foreach (var obj in mPlaneObjects)
					{
						if (obj == hit.collider.gameObject)
						{
							int cell_x = num % FieldsHorCount;
							int cell_y = num / FieldsVerCount;

							foreach (var ship in mShips)
							{
								Ship subship = ship.GetComponent<Ship>();
								if (subship.HasCell(cell_x, cell_y))
								{
									mShips.Remove(ship);
									Destroy(ship);
									break;
								}
							}

							BattleshipsPlacing placing = uiPlacing.GetComponent<BattleshipsPlacing>();

							if (uiPlacing.activeSelf)
							{
								placing.RefreshButtonsTexts();
								RefreshRedPlanes();
								RefreshBusyCells();
							}

							break;
						}

						num++;
					}
				}
			}
		}

	}

	// возможно ли поставить выбранный корабль на клетке x, y
	bool VerifyCell(int x, int y)
	{
		return isPossibleShipPos(mSelectionLength, mSelectionHorizontal, x, y);
	}

	// создание различных вспомогательных объектов на поле - рамок, точек, клеток
	void CreateCellObjects(Vector3 start, Vector3 end)
	{
		for (int i = 0; i < FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldsVerCount; ++j)
			{
				mPlaneObjects[j * FieldsHorCount + i] = CreatePlaneObj(
					i, j, start, end);
				mRedPlaneObjects[j * FieldsHorCount + i] = CreateRedPlaneObj(
					i, j, start, end);
				mRedDotObjects[j * FieldsHorCount + i] = CreateRedDotObj(
					i, j, start, end);
			}
		}
	}

	GameObject CreatePlaneObj(int x, int y, Vector3 start, Vector3 end)
	{
		GameObject obj = (GameObject)Instantiate(
			PlanePrefab,
			new Vector3(x * mFieldWidth + start.x + mFieldWidth / 2, 0.3f, y * mFieldHeight + start.z + mFieldHeight / 2), 
			Quaternion.Euler(-90, 0, 180));
		obj.transform.localScale = new Vector3(5.1f, 5.1f, 1);
		obj.GetComponent<MeshRenderer>().enabled = false;

		return obj;
	}

	GameObject CreateRedPlaneObj(int x, int y, Vector3 start, Vector3 end)
	{
		GameObject obj = (GameObject)Instantiate(
			RedPlanePrefab,
			new Vector3(x * mFieldWidth + start.x + mFieldWidth / 2, 0.1f, y * mFieldHeight + start.z + mFieldHeight / 2),
			Quaternion.Euler(-90, 0, 180));
		obj.transform.localScale = new Vector3(5.1f, 5.1f, 1);
		obj.GetComponent<MeshRenderer>().enabled = false;

		return obj;
	}
	GameObject CreateRedDotObj(int x, int y, Vector3 start, Vector3 end)
	{
		GameObject obj = (GameObject)Instantiate(
			RedDotPrefab,
			new Vector3(x * mFieldWidth + start.x + mFieldWidth / 2, 0.1f, y * mFieldHeight + start.z + mFieldHeight / 2),
			Quaternion.Euler(-90, 0, 180));
		obj.transform.localScale = new Vector3(5.1f, 5.1f, 1);
		obj.GetComponent<MeshRenderer>().enabled = false;

		return obj;
	}

	//выбрать режим установки корабля на поле
	public void PlaceShip(int itype)
	{
		BattleshipsPlacing placing = uiPlacing.GetComponent<BattleshipsPlacing>();

		//если какой то корабль был выбран, убирается с поля
		if (mPlaceShip != null)
		{
			mSelectionLength = 0;
			mShips.Remove(mPlaceShip);
			Destroy(mPlaceShip);

			placing.RefreshButtonsTexts();
		}

		// заполняются параметры - какой корабль нужно поставить
		// и создается объект на поле
		Ship.Type type = (Ship.Type)itype;

		GameObject ship;
		switch(type)
		{
			case Ship.Type.Destroyer:
				ship = (GameObject)Instantiate(DestroyerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				break;

			case Ship.Type.Cruiser:
				ship = (GameObject)Instantiate(CruiserPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				break;

			case Ship.Type.Battleship:
				ship = (GameObject)Instantiate(BattleshipPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				break;

			case Ship.Type.AircraftCarrier:
				ship = (GameObject)Instantiate(AircraftCarrierPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				break;

			default:
				return;
		}

		ship.AddComponent<Ship>();
		Ship subship = ship.GetComponent<Ship>();
		subship.Init(mFieldStart, new Vector2(mFieldWidth, mFieldHeight), type, true, -1, -1);
		mSelectionLength = subship.GetShipLength();
		mSelectionHorizontal = true;
		mPlaceShip = ship;

		mShips.Add(ship);

		placing.RefreshButtonsTexts();
		RefreshRedPlanes();
		RefreshBusyCells();
	}

	//поставить кораблья на определенной клетке, в определенном положении
	public void PlaceShip(int itype, bool horizontal, int x, int y, bool visible)
	{
		BattleshipsPlacing placing = uiPlacing.GetComponent<BattleshipsPlacing>();

		Ship.Type type = (Ship.Type)itype;

		GameObject ship;
		switch (type)
		{
			case Ship.Type.Destroyer:
				ship = (GameObject)Instantiate(DestroyerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				break;

			case Ship.Type.Cruiser:
				ship = (GameObject)Instantiate(CruiserPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				break;

			case Ship.Type.Battleship:
				ship = (GameObject)Instantiate(BattleshipPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				break;

			case Ship.Type.AircraftCarrier:
				ship = (GameObject)Instantiate(AircraftCarrierPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				break;

			default:
				return;
		}

		ship.AddComponent<Ship>();
		Ship subship = ship.GetComponent<Ship>();
		subship.Init(mFieldStart, new Vector2(mFieldWidth, mFieldHeight), type, horizontal, x, y);
		ship.SetActive(visible);

		mShips.Add(ship);
		RefreshBusyCells();
	}

	// определенить, можно ли поставить корабль с размером len и положение 
	// horizontal в клетке x, y
	public bool isPossibleShipPos(int len, bool horizontal, int x, int y)
	{
		if (x < 0 || x >= FieldsHorCount ||
			y < 0 || y >= FieldsVerCount)
		{
			return false;
		}

		if (horizontal && x + len > FieldsHorCount)
		{
			return false;
		}

		if (!horizontal && y + len > FieldsVerCount)
		{
			return false;
		}

		if (horizontal)
		{
			for (int i = x; i < x + len; ++i)
			{
				if (i < 0 || i >= FieldsHorCount)
				{
					continue;
				}

				if (mBusyCells[i, y])
				{
					return false;
				}
			}
		}
		else
		{
			for (int i = y; i < y + len; ++i)
			{
				if (i < 0 || i >= FieldsVerCount)
				{
					continue;
				}

				if (mBusyCells[x, i])
				{
					return false;
				}
			}
		}

		return true;
	}

	// определение занятых клеток, так же занятыми отмечаются смежные клетки
	public void RefreshBusyCells()
	{
		for (int i = 0; i < FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldsVerCount; ++j)
			{
				mBusyCells[i, j] = false;
			}
		}

		foreach (var ship in mShips)
		{
			if (ship == mPlaceShip)
				continue;

			Ship subship = ship.GetComponent<Ship>();

			if (subship.isHorizontal())
			{
				for (int i = subship.GetX() - 1; i <= subship.GetX() + subship.GetShipLength(); ++i)
				{
					for (int j = subship.GetY() - 1; j <= subship.GetY() + 1; ++j)
					{
						if (i >= 0 && j >= 0 &&
							i < FieldsHorCount && j < FieldsVerCount)
						{
							mBusyCells[i, j] = true;
						}
					}
				}
			}
			else
			{
				for (int i = subship.GetX() - 1; i <= subship.GetX() + 1; ++i)
				{
					for (int j = subship.GetY() - 1; j <= subship.GetY() + subship.GetShipLength(); ++j)
					{
						if (i >= 0 && j >= 0 &&
							i < FieldsHorCount && j < FieldsVerCount)
						{
							mBusyCells[i, j] = true;
						}
					}
				}
			}
		}
	}

	// размещение различных вспомогательных объектов на поле
	public void RefreshRedPlanes()
	{
		foreach (var plane in mRedPlaneObjects)
		{
			plane.GetComponent<MeshRenderer>().enabled = false;
		}

		foreach (var dot in mRedDotObjects)
		{
			dot.GetComponent<MeshRenderer>().enabled = false;
		}

		if (mGame.State != GameOrder.GameState.Placing)
		{
			for (int i = 0; i < FieldsHorCount; ++i)
			{
				for (int j = 0; j < FieldsVerCount; ++j)
				{
					if(mAttackedCells[i, j])
					{
						if (isCellBelongsShip(i, j))
						{
							mRedPlaneObjects[j * FieldsHorCount + i].GetComponent<MeshRenderer>().enabled = true;
						}
						else
						{
							mRedDotObjects[j * FieldsHorCount + i].GetComponent<MeshRenderer>().enabled = true;
						}
					}
				}
			}
		}
		else
		{
			foreach (var ship in mShips)
			{
				if (ship == mPlaceShip)
					continue;

				Ship subship = ship.GetComponent<Ship>();

				if (subship.isHorizontal())
				{
					for (int i = subship.GetX() - 1; i <= subship.GetX() + subship.GetShipLength(); ++i)
					{
						for (int j = subship.GetY() - 1; j <= subship.GetY() + 1; ++j)
						{
							if (i >= 0 && j >= 0 &&
								i < FieldsHorCount && j < FieldsVerCount)
							{
								mRedPlaneObjects[j * FieldsHorCount + i].GetComponent<MeshRenderer>().enabled = true;
							}
						}
					}
				}
				else
				{
					for (int i = subship.GetX() - 1; i <= subship.GetX() + 1; ++i)
					{
						for (int j = subship.GetY() - 1; j <= subship.GetY() + subship.GetShipLength(); ++j)
						{
							if (i >= 0 && j >= 0 &&
								i < FieldsHorCount && j < FieldsVerCount)
							{
								mRedPlaneObjects[j * FieldsHorCount + i].GetComponent<MeshRenderer>().enabled = true;
							}
						}
					}
				}
			}
		}
	}

	//посчитать количество кораблей типа type на поле
	public int GetShipsCountByType(Ship.Type type)
	{
		int res = 0;

		foreach(var ship in mShips)
		{
			Ship subship = ship.GetComponent<Ship>();

			if(subship.GetShipType() == type)
			{
				++res;
			}
		}

		return res;
	}

	//определить, все ли корабли установлены на поле
	public bool isAllShipsArePlaced 
	{
		get
		{
			return (GetShipsCountByType(Ship.Type.AircraftCarrier) ==
				Ship.GetShipsMaxCount(Ship.Type.AircraftCarrier)) &&
				(GetShipsCountByType(Ship.Type.Battleship) ==
				Ship.GetShipsMaxCount(Ship.Type.Battleship)) &&
				(GetShipsCountByType(Ship.Type.Cruiser) ==
				Ship.GetShipsMaxCount(Ship.Type.Cruiser)) &&
				(GetShipsCountByType(Ship.Type.Destroyer) ==
				Ship.GetShipsMaxCount(Ship.Type.Destroyer)) &&
				(mPlaceShip == null);
		}
	}
	
	// сделать режим выбора ячейки для удара
	// когда удар будет совершен игроком
	// вызовется функция записанная в handler
	public void SelectAim(AimSelectedHandler handler)
	{
		mAimSelectHandler = handler;
		mSelectionLength = 1;
		mSelectionHorizontal = true;
	}

	//определить, находится ли корабль на клетке x, y
	public bool isCellBelongsShip(int x, int y)
	{
		foreach(var ship in mShips)
		{
			Ship subship = ship.GetComponent<Ship>();

			if(subship.isHorizontal())
			{
				if(subship.GetX() <= x && 
					subship.GetX() + subship.GetShipLength() > x &&
					subship.GetY() == y)
				{
					return true;
				}
			}
			else
			{
				if (subship.GetY() <= y &&
					subship.GetY() + subship.GetShipLength() > y &&
					subship.GetX() == x)
				{
					return true;
				}
			}
		}

		return false;
	}

	// определить на какой глубине находится корабль в клетке x, y
	public float GetDepthOfShipOnCell(int x, int y)
	{
		foreach (var ship in mShips)
		{
			Ship subship = ship.GetComponent<Ship>();

			if (subship.isHorizontal())
			{
				if (subship.GetX() <= x &&
					subship.GetX() + subship.GetShipLength() > x &&
					subship.GetY() == y)
				{
					return subship.GetDepth();
				}
			}
			else
			{
				if (subship.GetY() <= y &&
					subship.GetY() + subship.GetShipLength() > y &&
					subship.GetX() == x)
				{
					return subship.GetDepth();
				}
			}
		}

		return 0.0f;
	}
	
	// определить, есть ли в клетке x, y потопленный корабль 
	public bool isCellBelongsSinkedShip(int x, int y)
	{
		foreach(var ship in mShips)
		{
			Ship subship = ship.GetComponent<Ship>();

			if (!subship.isSinked())
				continue;

			if(subship.isHorizontal())
			{
				if(subship.GetX() <= x && 
					subship.GetX() + subship.GetShipLength() > x &&
					subship.GetY() == y)
				{
					return true;
				}
			}
			else
			{
				if (subship.GetY() <= y &&
					subship.GetY() + subship.GetShipLength() > y &&
					subship.GetX() == x)
				{
					return true;
				}
			}
		}

		return false;
	}

	// атаковать клетку x, y
	// и возвращается значение true - если было попадение
	// false - если промах
	public bool AttackCell(int x, int y)
	{
		mAttackedCells[x, y] = true;
		RefreshRedPlanes();
		CheckSinkedShips();

		//отмечается последняя выбранная клетка
		LastCell.transform.position = new Vector3(
			x * mFieldWidth + mFieldStart.x + mFieldWidth / 2,
			0.1f,
			y * mFieldHeight + mFieldStart.z + mFieldHeight / 2);
		LastCell.GetComponent<MeshRenderer>().enabled = true;

		return isCellBelongsShip(x, y);
	}

	// найти потопленные корабли на поле
	// если найдены, тогда смежные клетки отмечаются как атакованные
	// и корабль отмечается как потопленный
	private void CheckSinkedShips()
	{
		foreach (var ship in mShips)
		{
			Ship subship = ship.GetComponent<Ship>();

			if (subship.isSinked())
				continue;
				
			bool sinked = true;

			if (subship.isHorizontal())
			{
				for(int i = subship.GetX(); i < subship.GetX() + subship.GetShipLength(); ++i)
				{
					if(!mAttackedCells[i, subship.GetY()])
					{
						sinked = false;
					}
				}
			}
			else
			{
				for (int i = subship.GetY(); i < subship.GetY() + subship.GetShipLength(); ++i)
				{
					if (!mAttackedCells[subship.GetX(), i])
					{
						sinked = false;
					}
				}
			}

			if (sinked)
			{
				subship.Sink();

				if (subship.isHorizontal())
				{
					for (int i = subship.GetX() - 1; i <= subship.GetX() + subship.GetShipLength(); ++i)
					{
						for (int j = subship.GetY() - 1; j <= subship.GetY() + 1; ++j)
						{
							if(j >= 0 && j < FieldsVerCount &&
								i >= 0 && i < FieldsHorCount)
							{
								mAttackedCells[i, j] = true;
							}
						}
					}
				}
				else
				{
					for (int i = subship.GetY() - 1; i <= subship.GetY() + subship.GetShipLength(); ++i)
					{
						for (int j = subship.GetX() - 1; j <= subship.GetX() + 1; ++j)
						{
							if (j >= 0 && j < FieldsHorCount &&
								i >= 0 && i < FieldsVerCount)
							{
								mAttackedCells[j, i] = true;
							}
						}
					}
				}

				RefreshRedPlanes();
			}
		}
	}

	//определить, все ли корабли потоплены
	public bool isCleared()
	{
		bool res = true;

		foreach(var ship in mShips)
		{
			Ship subship = ship.GetComponent<Ship>();

			if(!subship.isSinked())
			{
				res = false;
				break;
			}
		}

		return res;
	}

	// вернуть массив кораблей расположенных на поле
	public IEnumerable<GameObject> GetShips()
	{
		return mShips;
	}

	// определить, была ли клетка x, y атакованна
	public bool isCellAttacked(int x, int y)
	{
		if (x < 0 || x >= FieldsHorCount ||
			y < 0 || y >= FieldsVerCount)
			return false;

		return mAttackedCells[x, y];
	}

	// проверить, есть ли на клетке x, y огонь
	bool CheckFireExist(int x, int y)
	{
		for (int i = 0; i < mFireList.Count; ++i)
		{
			if(x == mFireList[i].X && y == mFireList[i].Y)
			{
				return true;
			}
		}

		return false;
	}

	// добавить огонь в клетку x, y
	void AddFire(int x, int y)
	{
		if (!CheckFireExist(x, y))
		{
			GameObject fire_inst = (GameObject)Instantiate(FirePrefab, 
				new Vector3(
					x * mFieldWidth + mFieldStart.x + mFieldWidth / 2,
					FirePosY,
					y * mFieldHeight + mFieldStart.z + mFieldHeight / 2), 
				Quaternion.Euler(-90.0f, 0, 0));
			mFireList.Add(new FireObject(fire_inst, x, y));
		}
	}

	// убрать лишний огонь - на затанувших кораблях
	// и добавить огонь на клетках где есть попадание
	// если там еще нет огня
	void ValidateFireList()
	{
		for(int i = 0; i < mFireList.Count; ++i)
		{
			int x = mFireList[i].X;
			int y = mFireList[i].Y;
			if(!isCellBelongsShip(x, y) || !isCellAttacked(x, y))
			{
				mFireList.RemoveAt(i);
				--i;
			}
		}

		if (EnemyField)
		{
			for (int i = 0; i < FieldsHorCount; ++i)
			{
				for (int j = 0; j < FieldsVerCount; ++j)
				{
					if (isCellBelongsSinkedShip(i, j) && isCellAttacked(i, j))
					{
						AddFire(i, j);
					}
				}
			}
		}
		else
		{
			for (int i = 0; i < FieldsHorCount; ++i)
			{
				for (int j = 0; j < FieldsVerCount; ++j)
				{
					if (isCellBelongsShip(i, j) && isCellAttacked(i, j))
					{
						AddFire(i, j);
					}
				}
			}
		}

		foreach(var fire in mFireList)
		{
			float depth = GetDepthOfShipOnCell(fire.X, fire.Y);
			Vector3 vec = fire.Object.transform.position;
			vec.y = depth + FirePosY;
			if (vec.y < 0.0f)
			{
				ParticleSystem particle = fire.Object.GetComponent<ParticleSystem>();
				particle.Stop();
			}
			fire.Object.transform.position = vec;
		}
	}
}
