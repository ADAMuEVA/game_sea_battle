using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
/// <summary>
/// Класс Game pvp.
/// Скрипт для описания действия игры против
/// другого игрока в сети.
/// Совместно с скриптом NetworkPlayer
/// </summary>
public class GamePvP : MonoBehaviour
{

	public GameObject GameOrder;

	GameOrder mGameOrder;

	bool mGameBegun = false;
	public GameObject CanvasPlaceBattleships;
	public GameObject CanvasPlacing;
	public GameObject CanvasBeginGame;
	bool? mStartWithYourTurn;
	bool mFirstUpdate = true;
	/// <summary>
	/// Start this instance.
	/// указываются обработчики событий объекта GameOrder
	/// </summary>
	void Start ()
	{

		mGameOrder = GameOrder.GetComponent<GameOrder>();
		mGameOrder.OnBeginGame += mGameOrder_OnBeginGame;
		mGameOrder.OnYourTurnBegin += mGameOrder_OnYourTurnBegin;
		mGameOrder.OnYourTurnEnd += mGameOrder_OnYourTurnEnd;
		mGameOrder.OnEnemyTurnBegin += mGameOrder_OnEnemyTurnBegin;
		mGameOrder.OnEnemyTurnEnd += mGameOrder_OnEnemyTurnEnd;
	}
	
	/// <summary>
	/// Update this instance.
	/// если сервер, то случайным образом выбирается чей ход и отправляется на клиент
	/// если клиент, тогда ожидается выбор чей ход от сервера
	/// </summary>
	void Update () 
	{
		if (mGameBegun)
		{
			// если сервер, то случайным образом выбирается чей ход и отправляется на клиент
			if (GameOptions.Instance.Server)
			{
				FieldOperations field = mGameOrder.EnemyField.GetComponent<FieldOperations>();
				if (mFirstUpdate && field.isAllShipsArePlaced)
				{
					if (Random.Range(0, 2) == 0)
					{
						mGameOrder.SetPlayerTurn();
						SendEnemyTurn();
					}
					else
					{
						mGameOrder.SetEnemyTurn();
						SendYourTurn();
					}

					mFirstUpdate = false;
				}
			}

			// если клиент, тогда ожидается выбор чей ход от сервера
			if(mStartWithYourTurn.HasValue)
			{
				if(mStartWithYourTurn.Value)
				{
					mGameOrder.SetPlayerTurn();
				}
				else
				{
					mGameOrder.SetEnemyTurn();
				}

				mStartWithYourTurn = null;
			}
		}
	}
	/// <summary>
	/// Sends your turn.
	/// отправка различных команд от одного игрока другому
	/// </summary>
	public void SendYourTurn()
	{
		NetworkPlayer player = GameOptions.Instance.Player.GetComponent<NetworkPlayer>();
		player.YourTurn();
	}

	public void SendEnemyTurn()
	{
		NetworkPlayer player = GameOptions.Instance.Player.GetComponent<NetworkPlayer>();
		player.EnemyTurn();
	}

	public void SendSetEnemyShip(int type, int x, int y, bool horizontal)
	{
		NetworkPlayer player = GameOptions.Instance.Player.GetComponent<NetworkPlayer>();
		player.SetEnemyShip(type, x, y, horizontal);
	}

	public void SendAttackCell(int x, int y)
	{
		NetworkPlayer player = GameOptions.Instance.Player.GetComponent<NetworkPlayer>();
		player.AttackCell(x, y);
	}
	/// <summary>
	/// Receiveds your turn.
	/// получение различных команд от другого игрока
	/// </summary>
	public void ReceivedYourTurn()
	{
		mStartWithYourTurn = true;
	}

	public void ReceivedEnemyTurn()
	{
		mStartWithYourTurn = false;
	}

	public void ReceivedSetEnemyShip(int type, int x, int y, bool horizontal)
	{
		FieldOperations field = mGameOrder.EnemyField.GetComponent<FieldOperations>();
		field.PlaceShip(type, horizontal, x, y, false);

	}

	public void ReceivedAttackCell(int x, int y)
	{
		FieldOperations field = mGameOrder.PlayerField.GetComponent<FieldOperations>();
		bool res = false;

		res = field.AttackCell(x, y);
		mGameOrder.EndTurn(res);
	}
	/// <summary>
	/// Ms the game order_ on begin game.
	/// при начале игры все корабли отправляются оппоненту
	/// </summary>
	void mGameOrder_OnBeginGame()
	{
		mGameBegun = true;

		FieldOperations field = mGameOrder.PlayerField.GetComponent<FieldOperations>();
		foreach (var ship in field.GetShips())
		{
			Ship subship = ship.GetComponent<Ship>();
			SendSetEnemyShip(
					(int)subship.GetShipType(),
					subship.GetX(),
					subship.GetY(),
					subship.isHorizontal());
		}

		mGameOrder.PlayerField.GetComponent<FieldOperations>().RefreshRedPlanes();
	}
	
	/// <summary>
	/// Ms the game order_ on your turn begin.
	/// когда ход игрока, игрок выбирает клетку для атаки
	/// и x, y отправляется другому игроку
	/// </summary>
	void mGameOrder_OnYourTurnBegin()
	{
		FieldOperations field = mGameOrder.EnemyField.GetComponent<FieldOperations>();
		field.SelectAim((x, y) =>
		{
			bool res = false;
			mGameOrder.AddTask(1.0f, () =>
			{
				res = field.AttackCell(x, y);
				if (GameOptions.Instance.Server)
				{
					SendAttackCell(x, y);
				}
				else
				{
					SendAttackCell(x, y);
				}
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

	void mGameOrder_OnEnemyTurnBegin()
	{
	}

	void mGameOrder_OnEnemyTurnEnd(bool succ)
	{

	}

}
