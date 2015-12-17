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
		mGameOrder.OnYourTurnEnd += mGameOrder_OnYourTurnEnd;
		mGameOrder.OnEnemyTurnEnd += mGameOrder_OnEnemyTurnEnd;
	}


	void mGameOrder_OnYourTurnEnd(bool succ)
	{
		
	}



	struct AimPos
	{
		public int X;
		public int Y;
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
