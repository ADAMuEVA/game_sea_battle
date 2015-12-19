using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Класс Game order.
/// Отвечает за порядок действий в игре
/// </summary>
public class GameOrder : MonoBehaviour {

	public GameObject PlayerField;
	public GameObject EnemyField;
	public GameObject GameCamera;
	public GameObject Game;
	public GameObject CanvasYourTurn;
	public GameObject CanvasEnemyTurn;
	public GameObject CanvasVictory;
	public GameObject CanvasDefeat;

	FieldOperations mPlayerField;
	FieldOperations mEnemyField;
	GameCamera mGameCamera;
	bool mGameBegun = false;

	public enum GameState
	{
		Placing,
		PlayerTurn,
		EnemyTurn,
	};
	GameState mGameState = GameState.Placing;

	public delegate void TaskHandler();
	/// <summary>
	/// установлено ли InitialTime
	/// время начала задачи
	/// время до выполнения задачи
	/// функция которую нужно выполнить
	/// когда пройдет время Time
	/// </summary>
	class Task
	{
		public bool Initialized = false;	// установлено ли InitialTime
		public float InitialTime;			// время начала задачи
		public float Time;					// время до выполнения задачи

		public TaskHandler Handler;			// функция которую нужно выполнить,
											// когда пройдет время Time

		public Task(float time, TaskHandler handler)
		{
			Time = time;
			Handler = handler;
		}
	}
	/// <summary>
	/// The m tasks.
	/// список задач, которые нужно выполнить по истечении времени
	/// </summary>
	List<Task> mTasks = new List<Task>();

	/// <summary>
	/// Start this instance.
	/// Use this for initialization
	/// </summary>
	void Start()
	{
		mPlayerField = PlayerField.GetComponent<FieldOperations>();
		mEnemyField = EnemyField.GetComponent<FieldOperations>();
		mGameCamera = GameCamera.GetComponent<GameCamera>();
	}
	/// <summary>
	/// Update this instance.
	/// Update is called once per frame
	/// проверка списка задач
	/// если задачи есть
	/// то проверяется, сколько времени осталось для выполнения первой
	/// если время пришло, выполняется первая задача и удаляется из списка
	/// </summary>
	void Update () {


		if(mTasks.Count > 0)
		{
			Task task = mTasks[0];

			if(task.Initialized)
			{
				if(Time.time - task.InitialTime > task.Time)
				{
					task.Handler();
					mTasks.RemoveAt(0);
				}
			}
			else 
			{
				task.InitialTime = Time.time;
				task.Initialized = true;
			}
		}
	}
	/// <summary>
	/// Sets the player turn.
	/// начать в игре ход игрока
	/// </summary>
	public void SetPlayerTurn()
	{
		mGameState = GameState.PlayerTurn;

		mTasks.Add(new Task(0.3f, () =>
		{
			mGameCamera.ChangePosition(2);
		}));
		mTasks.Add(new Task(1.0f, () =>
		{
			CanvasYourTurn.SetActive(true);
		}));
		mTasks.Add(new Task(1.0f, () =>
		{
			CanvasYourTurn.SetActive(false);
			YourTurnBegin();
		}));
	}
	/// <summary>
	/// Sets the enemy turn.
	/// начать в игре ход противника
	/// </summary>
	public void SetEnemyTurn()
	{
		mGameState = GameState.EnemyTurn;

		mTasks.Add(new Task(0.3f, () =>
		{
			mGameCamera.ChangePosition(1);
		}));
		mTasks.Add(new Task(1.0f, () =>
		{
			CanvasEnemyTurn.SetActive(true);
		}));
		mTasks.Add(new Task(1.0f, () =>
		{
			CanvasEnemyTurn.SetActive(false);
			EnemyTurnBegin();
		}));
	}

	public GameState State
	{
		get
		{
			return mGameState;
		}
	}

	/// <summary>
	/// Events.
	/// события:
	/// начало игры
	/// </summary>
	public delegate void BeginGameHandler();
	public event BeginGameHandler OnBeginGame;
	/// <summary>
	/// Occurs when on your turn begin.
	/// начало ходу игрока
	/// </summary>
	public delegate void YourTurnBeginHandler();
	public event YourTurnBeginHandler OnYourTurnBegin;
	/// <summary>
	/// Occurs when on your turn end
	/// конец хода игрока.
	/// </summary>
	public delegate void YourTurnEndHandler(bool succ);
	public event YourTurnEndHandler OnYourTurnEnd;
	/// <summary>
	/// Occurs when on enemy turn begin.
	/// начало хода противника
	/// </summary>
	public delegate void EnemyTurnBeginHandler();
	public event EnemyTurnBeginHandler OnEnemyTurnBegin;
	/// <summary>
	/// Occurs when on enemy turn end.
	/// конец хода противника
	/// </summary>
	public delegate void EnemyTurnEndHandler(bool succ);
	public event EnemyTurnEndHandler OnEnemyTurnEnd;

	public void BeginGame()
	{
		mGameBegun = true;
		OnBeginGame();
	}
	/// <summary>
	/// Yours the turn begin.
	/// действия в начале действия игрока
	/// </summary>
	public void YourTurnBegin()
	{
		OnYourTurnBegin();
	}
	/// <summary>
	/// Yours the turn end.
	/// действия в конце хода игрока (succ = true если было попадание)
	/// </summary>
	/// <param name="succ">If set to <c>true</c> succ.</param>
	public void YourTurnEnd(bool succ)
	{
		OnYourTurnEnd(succ);

		if (succ)
		{
			if (mEnemyField.isCleared())
			{
				mTasks.Add(new Task(1.0f, () =>
				{
					CanvasVictory.SetActive(true);
				}));
			}
			else
			{
				mGameState = GameState.PlayerTurn;

				mTasks.Add(new Task(0.3f, () =>
				{
					YourTurnBegin();
				}));
			}
		}
		else
		{
			SetEnemyTurn();
		}
	}
	/// <summary>
	/// Enemies the turn begin.
	/// действия в начале действия противника
	/// </summary>
	public void EnemyTurnBegin()
	{
		OnEnemyTurnBegin();
	}
	/// <summary>
	/// Enemies the turn end.
	/// действия в конце хода противника (succ = true если было попадание)
	/// </summary>
	/// <param name="succ">If set to <c>true</c> succ.</param>
	public void EnemyTurnEnd(bool succ)
	{
		OnEnemyTurnEnd(succ);

		if (succ)
		{
			if (mPlayerField.isCleared())
			{
				mTasks.Add(new Task(1.0f, () =>
				{
					CanvasDefeat.SetActive(true);
				}));
			}
			else
			{
				mGameState = GameState.EnemyTurn;

				mTasks.Add(new Task(0.3f, () =>
				{
					EnemyTurnBegin();
				}));
			}
		}
		else
		{
			SetPlayerTurn();
		}
	}
	/// <summary>
	/// Adds the task.
	/// добавить задачу в конец списка
	/// </summary>
	/// <param name="time">Time.</param>
	/// <param name="handler">Handler.</param>
	public void AddTask(float time, TaskHandler handler)
	{
		mTasks.Add(new Task(time, handler));
	}
	/// <summary>
	/// Ends the turn.
	/// закончить ход (succ = true если попадание)
	/// </summary>
	/// <param name="succ">If set to <c>true</c> succ.</param>
	public void EndTurn(bool succ)
	{
		if(State == GameState.PlayerTurn)
		{
			YourTurnEnd(succ);
		}
		else if(State == GameState.EnemyTurn)
		{
			EnemyTurnEnd(succ);
		}
	}
	/// <summary>
	/// Gets a value indicating whether this <see cref="GameOrder"/> is game begun.
	/// началась ли игра
	/// </summary>
	/// <value><c>true</c> if is game begun; otherwise, <c>false</c>.</value>
	public bool isGameBegun
	{
		get
		{
			return mGameBegun;
		}
	}
}
