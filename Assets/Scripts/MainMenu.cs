using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour {

	public GameObject InputFieldIp;
	public GameObject InputFieldPort;
	public GameObject CanvasMenu;
	public GameObject NetworkManager;

	enum MenuState
	{
		Initial = 0,
		Main = 1,
		Multiplayer = 2,
	}

	// текущее положение камеры
	MenuState mCurrentMenuState;

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
	// положения меню сетевой игры при различных положениях камеры
	Vector3[] mMultiplayerMenuPositions = new Vector3[]{
		new Vector3(1.26f, -0.17f, 0),
		new Vector3(1.26f, 0.27f, 0),
		new Vector3(0.5f, 0.50f, 0),
	};

	// скорость вращения камеры
	public float mSmooth = 2.0f;
	GameObject panelMainMenu;
	GameObject panelMultiplayerMenu;

	void Start()
	{
		// устанавливается начальные положения меню и камеры
		mCurrentMenuState = MenuState.Main;
		panelMainMenu = GameObject.Find("panelMainMenu");
		panelMainMenu.transform.position = TranslateCoords(mMainMenuPositions[(int)MenuState.Initial]);
		panelMultiplayerMenu = GameObject.Find("panelMultiplayerMenu");
		panelMultiplayerMenu.transform.position = TranslateCoords(mMultiplayerMenuPositions[(int)MenuState.Initial]);

		if (GameOptions.Instance == null)
		{
			GameObject.Find("GameOptions").AddComponent<GameOptions>();
		}

		GameOptions.Instance.Network = NetworkManager.GetComponent<NetworkManager>();
	}
	 
	// обновление положений меню и камеры
	void Update()
	{
		Quaternion target = Quaternion.Euler(
			mStatesRotations[(int)mCurrentMenuState]);

		if(Camera.main.transform.rotation != target)
		{
			Camera.main.transform.rotation =
				Quaternion.Slerp(
					Camera.main.transform.rotation, 
					target, 
					Time.deltaTime * mSmooth);
		}

		if (panelMainMenu.transform.position !=
			TranslateCoords(mMainMenuPositions[(int)mCurrentMenuState]))
		{
			panelMainMenu.transform.position =
				Vector3.Lerp(
					panelMainMenu.transform.position,
					TranslateCoords(mMainMenuPositions[(int)mCurrentMenuState]),
					Time.deltaTime * mSmooth);
		}

		if (panelMultiplayerMenu.transform.position !=
			TranslateCoords(mMultiplayerMenuPositions[(int)mCurrentMenuState]))
		{
			panelMultiplayerMenu.transform.position =
				Vector3.Lerp(
					panelMultiplayerMenu.transform.position,
					TranslateCoords(mMultiplayerMenuPositions[(int)mCurrentMenuState]),
					Time.deltaTime * mSmooth);
		}
	}

	// преобразование координат относительных в экранные координаты
	public Vector3 TranslateCoords(Vector3 vec)
	{
		return new Vector3(vec.x * Screen.width, vec.y * Screen.height, vec.z);
	}

	// начать игру
	public void LoadGame()
	{
		GameOptions.Instance.Mode = GameOptions.GameMode.PvE;
		Application.LoadLevel(1);
	}

	// перейти к меню сетевой игры
	public void LoadMultiplayerMenu()
	{
		mCurrentMenuState = MenuState.Multiplayer;
		EnableMultiplayerMenu();
	}

	// загрузить основное меню
	public void LoadMainMenu()
	{
		mCurrentMenuState = MenuState.Main;
	}

	// выйти из игры
	public void Exit()
	{
		Application.Quit();
	}

	// сделать активными кнопки в меню сетевой игры
	void EnableMultiplayerMenu()
	{
		GameObject.Find("btnHost").GetComponent<Button>().interactable = true;
		GameObject.Find("btnConnect").GetComponent<Button>().interactable = true;
		GameObject.Find("btnBack").GetComponent<Button>().interactable = true;
		GameObject.Find("InputFieldIp").GetComponent<InputField>().interactable = true;
		GameObject.Find("InputFieldPort").GetComponent<InputField>().interactable = true;
	}

	// сделать не активными кнопки в меню сетевой игры
	void DisableMultiplayerMenu()
	{
		GameObject.Find("btnHost").GetComponent<Button>().interactable = false;
		GameObject.Find("btnConnect").GetComponent<Button>().interactable = false;
		GameObject.Find("btnBack").GetComponent<Button>().interactable = false;
		GameObject.Find("InputFieldIp").GetComponent<InputField>().interactable = false;
		GameObject.Find("InputFieldPort").GetComponent<InputField>().interactable = false;
	}

	// совершить подключение к серверу
	public void Connect()
	{
		string ip;
		int port;
		ip = InputFieldIp.GetComponent<InputField>().text;
		if(!int.TryParse(InputFieldPort.GetComponent<InputField>().text, out port))
			return;


		NetworkManager manager = GameOptions.Instance.Network;
		manager.networkPort = port;
		manager.networkAddress = ip;
		manager.StartClient();

		GameOptions.Instance.Mode = GameOptions.GameMode.PvP;
		GameOptions.Instance.Server = false;
		DisableMultiplayerMenu();
	}

	// создать сервер
	public void Host()
	{
		int port;
		if (!int.TryParse(InputFieldPort.GetComponent<InputField>().text, out port))
			return;

		NetworkManager manager = GameOptions.Instance.Network;
		manager.networkPort = port;
		manager.StartServer();

		GameOptions.Instance.Mode = GameOptions.GameMode.PvP;
		GameOptions.Instance.Server = true;
		DisableMultiplayerMenu();
	}

}
