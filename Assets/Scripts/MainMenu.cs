using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour {
<<<<<<< HEAD


	public GameObject CanvasMenu;


=======
	
	
	public GameObject CanvasMenu;
	
	
>>>>>>> origin/master
	enum MenuState
	{
		Initial = 0,
		Main = 1,
		Multiplayer = 2,
	}
<<<<<<< HEAD

	// текущее положение камеры
	MenuState mCurrentMenuState;

=======
	
	// текущее положение камеры
	MenuState mCurrentMenuState;
	
>>>>>>> origin/master
	// повороты камеры при различных положениях
	Vector3[] mStatesRotations = new Vector3[]{
		new Vector3(30, 0, 0),
		new Vector3(-25, 0, 0),
		new Vector3(-25, 90, 0)
	};
	// положения основного меню при различных положениях камеры
	Vector3[] mMainMenuPositions = new Vector3[]{
		new Vector3(0.5f, -0.29f, 0),
		new Vector3(0.5f, 0.39f, 0),
		new Vector3(-0.25f, 0.39f, 0),
	};
<<<<<<< HEAD


=======
	
	
>>>>>>> origin/master
	// скорость вращения камеры
	public float mSmooth = 2.0f;
	GameObject panelMainMenu;
	GameObject panelMultiplayerMenu;
<<<<<<< HEAD

=======
	
>>>>>>> origin/master
	void Start()
	{
		// устанавливается начальные положения меню и камеры
		mCurrentMenuState = MenuState.Main;
		panelMainMenu = GameObject.Find("panelMainMenu");
		panelMainMenu.transform.position = TranslateCoords(mMainMenuPositions[(int)MenuState.Initial]);
<<<<<<< HEAD


=======
		
		
>>>>>>> origin/master
		if (GameOptions.Instance == null)
		{
			GameObject.Find("GameOptions").AddComponent<GameOptions>();
		}
<<<<<<< HEAD


	}
	 
=======
		
		
	}
	
>>>>>>> origin/master
	// обновление положений меню и камеры
	void Update()
	{
		Quaternion target = Quaternion.Euler(
			mStatesRotations[(int)mCurrentMenuState]);
<<<<<<< HEAD

=======
		
>>>>>>> origin/master
		if(Camera.main.transform.rotation != target)
		{
			Camera.main.transform.rotation =
				Quaternion.Slerp(
					Camera.main.transform.rotation, 
					target, 
					Time.deltaTime * mSmooth);
		}
<<<<<<< HEAD

		if (panelMainMenu.transform.position !=
			TranslateCoords(mMainMenuPositions[(int)mCurrentMenuState]))
=======
		
		if (panelMainMenu.transform.position !=
		    TranslateCoords(mMainMenuPositions[(int)mCurrentMenuState]))
>>>>>>> origin/master
		{
			panelMainMenu.transform.position =
				Vector3.Lerp(
					panelMainMenu.transform.position,
					TranslateCoords(mMainMenuPositions[(int)mCurrentMenuState]),
					Time.deltaTime * mSmooth);
		}
<<<<<<< HEAD



	}

=======
		
		
		
	}
	
>>>>>>> origin/master
	// преобразование координат относительных в экранные координаты
	public Vector3 TranslateCoords(Vector3 vec)
	{
		return new Vector3(vec.x * Screen.width, vec.y * Screen.height, vec.z);
	}
<<<<<<< HEAD

=======
	
>>>>>>> origin/master
	// начать игру
	public void LoadGame()
	{
		Application.LoadLevel(1);
	}
	
<<<<<<< HEAD

=======
	
>>>>>>> origin/master
	// загрузить основное меню
	public void LoadMainMenu()
	{
		mCurrentMenuState = MenuState.Main;
	}
<<<<<<< HEAD

=======
	
>>>>>>> origin/master
	// выйти из игры
	public void Exit()
	{
		Application.Quit();
	}
<<<<<<< HEAD




=======
	
	
	
	
>>>>>>> origin/master
}
