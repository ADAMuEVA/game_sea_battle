using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GamePvE : MonoBehaviour {

	public GameObject GameOrder;

	GameOrder mGameOrder;
	bool mGameBegun = false;

	// Use this for initialization
	void Start () {

		// добавление обработчиков событий для объекта GameOrder
		mGameOrder = GameOrder.GetComponent<GameOrder>();
		mGameOrder.OnBeginGame += mGameOrder_OnBeginGame;
		mGameOrder.OnYourTurnBegin += mGameOrder_OnYourTurnBegin;
		mGameOrder.OnYourTurnEnd += mGameOrder_OnYourTurnEnd;
		mGameOrder.OnEnemyTurnBegin += mGameOrder_OnEnemyTurnBegin;
		mGameOrder.OnEnemyTurnEnd += mGameOrder_OnEnemyTurnEnd;
	}

	// если сейчас ход игрока, тогда игрок выделяет клетку для атаки
	void mGameOrder_OnYourTurnBegin()
	{
		FieldOperations field = mGameOrder.EnemyField.GetComponent<FieldOperations>();
		field.SelectAim((x, y) =>
		{
			bool res = false;
			mGameOrder.AddTask(1.0f, () =>
			{
				res = field.AttackCell(x, y);
			});
			mGameOrder.AddTask(1.0f, () =>
			{
				mGameOrder.EndTurn(res);
			});
		});
	}

	void mGameOrder_OnYourTurnEnd(bool succ)
	{
		
	}

	// если ход компьютера, просчет клетки компьютером для удара 
	void mGameOrder_OnEnemyTurnBegin()
	{
		FieldOperations field = mGameOrder.PlayerField.GetComponent<FieldOperations>();
		bool res = false;
		int x, y;
		EvaluateEnemyAim(out x, out y);

		mGameOrder.AddTask(0.5f, () =>
		{
			res = field.AttackCell(x, y);
		});
		mGameOrder.AddTask(res ? 1.5f : 0.5f, () =>
		{
			mGameOrder.EndTurn(res);
		});
	}

	// просчет клетки компьютером для удара
	private void EvaluateEnemyAim(out int x, out int y)
	{
		FieldOperations field = mGameOrder.PlayerField.GetComponent<FieldOperations>();

		// определение клеток, где находятся потопленные корабли
		// они пропускаются при расчете
		bool[,] sinked_fields = new bool[FieldOperations.FieldsHorCount, FieldOperations.FieldsVerCount];
		
		for(int i = 0; i < FieldOperations.FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldOperations.FieldsVerCount; ++j)
			{
				sinked_fields[i, j] = false;
			}
		}

		foreach(var ship in field.GetShips())
		{
			Ship subship = ship.GetComponent<Ship>();

			if(subship.isSinked())
			{
				if(subship.isHorizontal())
				{
					for(int i = subship.GetX(); i < subship.GetX() + subship.GetShipLength(); ++i)
					{
						sinked_fields[i, subship.GetY()] = true;
					}
				}
				else
				{
					for(int i = subship.GetY(); i < subship.GetY() + subship.GetShipLength(); ++i)
					{
						sinked_fields[subship.GetX(), i] = true;
					}
				}
			}
		}

		// поиск клеток с попаданием
		for(int i = 0; i < FieldOperations.FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldOperations.FieldsVerCount; ++j)
			{
				if (sinked_fields[i, j])
					continue;

				if(field.isCellAttacked(i, j) && field.isCellBelongsShip(i, j))
				{
					int num = Random.Range(0, 2);

					// проверка, есть ли смежные клетки с попаданиями по вертикали
					// или по горизонтали
					if (num == 0)
					{
						if (CheckVertical(i, j, out x, out y))
							return;

						if (CheckHorizontal(i, j, out x, out y))
							return;
					}
					else
					{
						if (CheckHorizontal(i, j, out x, out y))
							return;

						if (CheckVertical(i, j, out x, out y))
							return;
					}

					// если нет, то атаковать случайно - влево, вправо, вверх, вниз
					// от клетки с попаданием
					if (AttackRandomSide(i, j, out x, out y))
						return;
				}
			}
		}

		int count = 0;

		// подсчет клеток, которые не были атакованы
		for (int i = 0; i < FieldOperations.FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldOperations.FieldsVerCount; ++j)
			{
				if (field.isCellAttacked(i, j))
					continue;

				count++;
			}
		}

		// случайно выбирается клетка для атаки
		int random_attack = Random.Range(0, count);

		for (int i = 0; i < FieldOperations.FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldOperations.FieldsVerCount; ++j)
			{
				if (field.isCellAttacked(i, j))
					continue;

				if (random_attack == 0)
				{
					x = i;
					y = j;
					return;
				}

				random_attack--;
			}
		}

		x = 0;
		y = 0;
	}

	struct AimPos
	{
		public int X;
		public int Y;
	}

	// атака случайно вверх, вниз, влево, вправо
	// от клетки i, j
	private bool AttackRandomSide(int i, int j, out int x, out int y)
	{
		FieldOperations field = mGameOrder.PlayerField.GetComponent<FieldOperations>();

		// просчет возможных вариантов для атаки
		bool possible_left = (i > 0) && !field.isCellAttacked(i - 1, j);
		bool possible_top = (j > 0) && !field.isCellAttacked(i, j - 1);
		bool possible_right = (i < FieldOperations.FieldsHorCount-1) && !field.isCellAttacked(i + 1, j);
		bool possible_bottom = (j < FieldOperations.FieldsVerCount - 1) && !field.isCellAttacked(i, j + 1);

		List<AimPos> aims = new List<AimPos>();

		if (possible_left)
			aims.Add(new AimPos { X = i - 1, Y = j });

		if (possible_top)
			aims.Add(new AimPos { X = i, Y = j - 1 });

		if (possible_right)
			aims.Add(new AimPos { X = i + 1, Y = j });

		if (possible_bottom)
			aims.Add(new AimPos { X = i, Y = j + 1 });

		if(aims.Count == 0)
		{
			x = 0;
			y = 0;
			return false;
		}

		// из возможных целей, выбирается одна случайно
		int num = Random.Range(0, aims.Count);

		x = aims[num].X;
		y = aims[num].Y;
		return true;
	}

	// определение смежных клеток по горизонтали с попаданиями от клетки i, j
	private bool CheckHorizontal(int i, int j, out int x, out int y)
	{
		FieldOperations field = mGameOrder.PlayerField.GetComponent<FieldOperations>();

		List<AimPos> aims = new List<AimPos>();
		bool _long = false;

		// просчитываются возможные клетки для атаки - слева или справа
		// и проверяется, есть ли смежные клетки с попаданиями
		for(int k = i+1; k < FieldOperations.FieldsHorCount; ++k)
		{
			if(!field.isCellAttacked(k, j))
			{
				aims.Add(new AimPos { X = k, Y = j });
				break;
			}
			else if(!field.isCellBelongsShip(k, j))
			{
				break;
			}
			else
			{
				_long = true;
			}
		}

		for (int k = i - 1; k >= 0; --k)
		{
			if (!field.isCellAttacked(k, j))
			{
				aims.Add(new AimPos { X = k, Y = j });
				break;
			}
			else if (!field.isCellBelongsShip(k, j))
			{
				break;
			}
			else
			{
				_long = true;
			}
		}

		if (!_long || aims.Count == 0)
		{
			x = 0;
			y = 0;
			return false;
		}

		// выбирается случайно возможная цель
		int num = Random.Range(0, aims.Count);

		x = aims[num].X;
		y = aims[num].Y;

		return true;
	}

	// определение смежных клеток по вертикали с попаданиями от клетки i, j
	private bool CheckVertical(int i, int j, out int x, out int y)
	{
		FieldOperations field = mGameOrder.PlayerField.GetComponent<FieldOperations>();

		List<AimPos> aims = new List<AimPos>();
		bool _long = false;

		// просчитываются возможные клетки для атаки - сверху или снизу
		// и проверяется, есть ли смежные клетки с попаданиями
		for (int k = j + 1; k < FieldOperations.FieldsVerCount; ++k)
		{
			if (!field.isCellAttacked(i, k))
			{
				aims.Add(new AimPos { X = i, Y = k });
				break;
			}
			else if (!field.isCellBelongsShip(i, k))
			{
				break;
			}
			else
			{
				_long = true;
			}
		}

		for (int k = j - 1; k >= 0; --k)
		{
			if (!field.isCellAttacked(i, k))
			{
				aims.Add(new AimPos { X = i, Y = k });
				break;
			}
			else if (!field.isCellBelongsShip(i, k))
			{
				break;
			}
			else
			{
				_long = true;
			}
		}

		if (!_long || aims.Count == 0)
		{
			x = 0;
			y = 0;
			return false;
		}

		// выбирается случайно возможная цель
		int num = Random.Range(0, aims.Count);

		x = aims[num].X;
		y = aims[num].Y;

		return true;
	}

	void mGameOrder_OnEnemyTurnEnd(bool succ)
	{
		
	}
	
	// Update is called once per frame
	void Update () {

		if (mGameBegun)
		{
			GameCamera game_camera = mGameOrder.GameCamera.GetComponent<GameCamera>();
			game_camera.CanvasBeginGame.SetActive(false);
		}
	}	

	struct ShipPos
	{
		public int X;
		public int Y;
		public bool Horizontal;
	}

	// определение списка возможных позиций для корабля типа type на поле
	private List<ShipPos> GetPossiblePoitions(int type)
	{
		List<ShipPos> pos = new List<ShipPos>();
		FieldOperations field = mGameOrder.EnemyField.GetComponent<FieldOperations>();
		for (int i = 0; i < FieldOperations.FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldOperations.FieldsVerCount; ++j)
			{
				int len = Ship.GetShipLength((Ship.Type)type);

				for(int k = 0; k < 2; ++k)
				{
					bool hor = ((k == 0) ? true : false);

					if (field.isPossibleShipPos(len, hor, i, j))
					{
						pos.Add(new ShipPos { X = i, Y = j, Horizontal = hor });
					}
				}
			}
		}

		return pos;
	}


	// когда игрок расставил свои корабли
	// компьютер тоже расставляет свои корабли
	void mGameOrder_OnBeginGame()
	{
		mGameBegun = true;

		FieldOperations field = mGameOrder.EnemyField.GetComponent<FieldOperations>();
		// список кораблей которые нужно расставить противнику
		List<int> ship_types = new List<int>
		{
			3, 2, 2, 1, 1, 1, 0, 0, 0, 0
		};

		// для каждого корабля из списка
		// проверяются возможные клетки для постановки корабля
		// и выбирается случайно из вожмных вариантов и ставится
		foreach(var type in ship_types)
		{
			List<ShipPos> pos = GetPossiblePoitions(type);

			if(pos.Count > 0)
			{
				int num = Random.Range(0, pos.Count);

				field.PlaceShip(type, pos[num].Horizontal, pos[num].X, pos[num].Y, false);
			}
		}

		// случайным образом выбирается чей ход
		if (Random.Range(0, 2) == 0)
		{
			mGameOrder.SetPlayerTurn();
		}
		else
		{
			mGameOrder.SetEnemyTurn();
		}

		mGameOrder.PlayerField.GetComponent<FieldOperations>().RefreshRedPlanes();
	}
}
