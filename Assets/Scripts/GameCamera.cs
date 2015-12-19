using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
/// <summary>
/// Класс Game camera.
/// Поведение камеры и вывод различных канвасов
/// </summary>
public class GameCamera : MonoBehaviour {


	public float mSmooth = 2.0f;
	public enum Position
	{
		Main,
		PlayerField,
		EnemyField,
	}
	public GameObject CanvasPlaceBattleships;
	public GameObject CanvasPlacing;
	public GameObject CanvasBeginGame;
	public GameObject CanvasWaitingForPlayer;
	public GameObject PlayerField;
	public GameObject GameObj;
	public GameObject GameOrder;
	/// <summary>
	/// The m current position.
	/// текущее положение камеры
	/// </summary>
	Position mCurrentPosition = Position.Main;
	/// <summary>
	/// The m positions.
	/// позиции и повороты камеры для различного положения
	/// </summary>
	Vector3[] mPositions = new Vector3[]{
		new Vector3(0.04f, 12.5f, -9.3f),
		new Vector3(8.0f, 12.5f, -3.7f),
		new Vector3(-8.0f, 12.5f, -3.7f),
	};
	Vector3[] mRotations = new Vector3[]{
		new Vector3(61.0f, 0.0f, 0.0f),
		new Vector3(80.4f, 0.0f, 0.0f),
		new Vector3(80.4f, 0.0f, 0.0f),
	};
	float mStartTime;
	bool mStartedCanvasLoaded = false;
	bool mCanvasPlacingLoaded = false;
	float mAllShipsPlacedTime;
	bool mAllShipsPlaced = false;
	/// <summary>
	/// Start this instance.
	/// Use this for initializati
	/// установка начального положения камеры
	/// создание объекта GameOptions если еще не был создан
	/// в зависимости от того какой режим был выбран в меню
	/// добавляется скрипт GamePvE или GamePvP в объект Game
	/// </summary>
	// Use this for initialization
	void Start()
	{
		// установка начального положения камеры
		transform.position = mPositions[(int)mCurrentPosition];
		transform.rotation = Quaternion.Euler(mRotations[(int)mCurrentPosition]);
		mStartTime = Time.time;

		// создание объекта GameOptions если еще не был создан
		if (GameOptions.Instance == null)
		{
			GameObject.Find("GameOptions").AddComponent<GameOptions>();
		}

		// в зависимости от того какой режим был выбран в меню
		// добавляется скрипт GamePvE или GamePvP в объект Game
		switch(GameOptions.Instance.Mode)
		{
			case GameOptions.GameMode.PvE:
				GamePvE gamepve = GameObj.AddComponent<GamePvE>();
				gamepve.GameOrder = GameOrder;

				break;
			case GameOptions.GameMode.PvP:
				GamePvP gamepvp = GameObj.AddComponent<GamePvP>();
				gamepvp.GameOrder = GameOrder;
				gamepvp.CanvasPlaceBattleships = CanvasPlaceBattleships;
				gamepvp.CanvasPlacing = CanvasPlacing;
				gamepvp.CanvasBeginGame = CanvasBeginGame;

				break;
		}
	}
	/// <summary>
	/// Update this instance.
	/// Update is called once per fram
	/// плавное перемещение камеры в точку где должна находится камера
	/// если игра только началась, тогда если текущий игрок - сервер
	/// идет проверка, подключился ли игрок, если еще нет, тогда 
	/// отображается надпись "Ожидание игрока"
	/// показываем окно для расстановки кораблей
	/// если все корабли расставленны, тогда появляется кнопка - "Начать игру"
	/// </summary>
	// Update is called once per frame
	void Update () {

		// плавное перемещение камеры в точку где должна находится камера
		Quaternion target = Quaternion.Euler(
			mRotations[(int)mCurrentPosition]);

		if (transform.rotation != target)
		{
			transform.rotation =
				Quaternion.Slerp(
					transform.rotation,
					target,
					Time.deltaTime * mSmooth);
		}

		if (transform.position !=
			mPositions[(int)mCurrentPosition])
		{
			transform.position =
				Vector3.Lerp(
					transform.position,
					mPositions[(int)mCurrentPosition],
					Time.deltaTime * mSmooth);
		}

		if (!isPlayersReady && !mAllShipsPlaced)
		{
			// если игра только началась, тогда если текущий игрок - сервер
			// идет проверка, подключился ли игрок, если еще нет, тогда 
			// отображается надпись "Ожидание игрока"
			CanvasWaitingForPlayer.SetActive(true);
		}
		else
		{
			CanvasWaitingForPlayer.SetActive(false);

			// показываем окно для расстановки кораблей
			if (!mStartedCanvasLoaded && Time.time - mStartTime > 1.0f)
			{
				CanvasPlaceBattleships.SetActive(true);
				mStartedCanvasLoaded = true;
			}

			if (mCanvasPlacingLoaded)
			{
				GameOrder gameOrder = GameOrder.GetComponent<GameOrder>();

				if (!gameOrder.isGameBegun)
				{
					FieldOperations field_operations = PlayerField.GetComponent<FieldOperations>();
					if (field_operations.isAllShipsArePlaced)
					{

						//если все корабли расставленны, тогда появляется кнопка - "Начать игру"
						if (mAllShipsPlaced)
						{
							if (Time.time - mAllShipsPlacedTime > 1.0f)
							{
								CanvasBeginGame.SetActive(true);
								CanvasPlacing.SetActive(false);
							}
						}
						else
						{
							mAllShipsPlacedTime = Time.time;
							mAllShipsPlaced = true;
						}
					}
					else
					{
						CanvasBeginGame.SetActive(false);
						CanvasPlacing.SetActive(true);
						BattleshipsPlacing placing = CanvasPlacing.GetComponent<BattleshipsPlacing>();
						placing.RefreshButtonsTexts();
						field_operations.RefreshRedPlanes();
						mAllShipsPlaced = false;
					}
				}
				else
				{
					CanvasBeginGame.SetActive(false);
					CanvasPlacing.SetActive(false);
					CanvasPlaceBattleships.SetActive(false);
				}
			}
		}
	}
	/// <summary>
	/// Changes the position.
	/// изменить положение камеры
	/// </summary>
	/// <param name="pos">Position.</param>
	public void ChangePosition(int pos)
	{
		mCurrentPosition = (Position)pos;
	}
	/// <summary>
	/// Hides the canvas place battleships.
	/// скрыть меню с кнопкой "Расставить корабли"
	/// </summary>
	public void HideCanvasPlaceBattleships()
	{
		CanvasPlaceBattleships.SetActive(false);
	}
	/// <summary>
	/// Loads the canvas placing.
	/// скрыть меню расстановки кораблей
	/// </summary>
	public void LoadCanvasPlacing()
	{
		CanvasPlacing.SetActive(true);
		mCanvasPlacingLoaded = true;
	}
	/// <summary>
	/// Loads the main menu.
	/// перейти в главное меню
	/// </summary>
	public void LoadMainMenu()
	{
		if (GameOptions.Instance.Mode == GameOptions.GameMode.PvP)
		{
			if (GameOptions.Instance.Server)
			{
				GameOptions.Instance.Network.StopHost();
			}
			else
			{
				GameOptions.Instance.Network.StopClient();
			}
		}
		else
		{
			Application.LoadLevel(0);
		}
	}
	/// <summary>
	/// Gets a value indicating whether this <see cref="GameCamera"/> is players ready.
	/// определить, подключен ли игрок к серверу
	/// </summary>
	/// <value><c>true</c> if is players ready; otherwise, <c>false</c>.</value>
	public bool isPlayersReady
	{
		get
		{
			if (GameOptions.Instance.Mode == GameOptions.GameMode.PvE)
				return true;

			if (GameOptions.Instance.Player == null)
				return false;

			if(GameOptions.Instance.Player.isServer)
			{
				return (NetworkServer.connections.Count >= 1);
			}

			return true;
		}
	}
}
