using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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
	public GameObject PlayerField;
	public GameObject GameObj;
	public GameObject GameOrder;

	//текущее положение камеры
	Position mCurrentPosition = Position.Main;

	//позиции и повороты камеры для различного положения
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

			
		
	}
	
	// Update is called once per frame
	void Update () {


			// показываем окно для расстановки кораблей
			if (!mStartedCanvasLoaded && Time.time - mStartTime > 1.0f)
			{
				CanvasPlaceBattleships.SetActive(true);
				mStartedCanvasLoaded = true;
			}

			
		}

}
