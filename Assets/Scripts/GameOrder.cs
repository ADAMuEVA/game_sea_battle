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
	

	GameCamera mGameCamera;
	FieldOperations mPlayerField;
	FieldOperations mEnemyField;

	
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
	void Update () 
	{

	}
	public GameState State {
		get {
			return mGameState;
		}
	}
}