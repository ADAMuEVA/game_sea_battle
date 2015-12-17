using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	
	// список задач, которые нужно выполнить по истечении времени
	List<Task> mTasks = new List<Task>();
	
	
	// Use this for initialization
	void Start()
	{
		mPlayerField = PlayerField.GetComponent<FieldOperations>();
		mEnemyField = EnemyField.GetComponent<FieldOperations>();
		mGameCamera = GameCamera.GetComponent<GameCamera>();
	}
	
	// Update is called once per frame
	void Update () {
		
		// проверка списка задач
		// если задачи есть
		// то проверяется, сколько времени осталось для выполнения первой
		// если время пришло, выполняется первая задача и удаляется из списка
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
	
	// начать в игре ход игрока
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
	
	// начать в игре ход противника
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
	
	
	// события:
	// начало игры
	public delegate void BeginGameHandler();
	public event BeginGameHandler OnBeginGame;
	// начало ходу игрока
	public delegate void YourTurnBeginHandler();
	public event YourTurnBeginHandler OnYourTurnBegin;
	// конец хода игрока
	public delegate void YourTurnEndHandler(bool succ);
	public event YourTurnEndHandler OnYourTurnEnd;
	// начало хода противника
	public delegate void EnemyTurnBeginHandler();
	public event EnemyTurnBeginHandler OnEnemyTurnBegin;
	// конец хода противника
	public delegate void EnemyTurnEndHandler(bool succ);
	public event EnemyTurnEndHandler OnEnemyTurnEnd;
	
	public void BeginGame()
	{
		mGameBegun = true;
		OnBeginGame();
	}
	
	// действия в начале действия игрока
	public void YourTurnBegin()
	{
		OnYourTurnBegin();
	}
	
	// действия в конце хода игрока (succ = true если было попадание)
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
	
	// действия в начале действия противника
	public void EnemyTurnBegin()
	{
		OnEnemyTurnBegin();
	}
	
	// действия в конце хода противника (succ = true если было попадание)
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
	
	// добавить задачу в конец списка
	public void AddTask(float time, TaskHandler handler)
	{
		mTasks.Add(new Task(time, handler));
	}
	
	// закончить ход (succ = true если попадание)
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
	
	// началась ли игра
	public bool isGameBegun
	{
		get
		{
			return mGameBegun;
		}
	}
}
