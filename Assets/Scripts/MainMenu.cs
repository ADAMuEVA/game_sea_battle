using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
/// <summary>
/// Класс Main menu.
/// Описание сцены меню и движение камеры.
/// </summary>
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
	/// <summary>
	/// The state of the m current menu.
	/// текущее положение камеры
	/// </summary>
	MenuState mCurrentMenuState;
	/// <summary>
	/// The m states rotations.
	/// повороты камеры при различных положениях
	/// </summary>
	Vector3[] mStatesRotations = new Vector3[]{
		new Vector3(30, 0, 0),
		new Vector3(-25, 0, 0),
		new Vector3(-25, 90, 0)
	};
	/// <summary>
	/// The m main menu positions.
	/// положения основного меню при различных положениях камеры
	/// </summary>
	Vector3[] mMainMenuPositions = new Vector3[]{
		new Vector3(0.5f, -0.29f, 0),
		new Vector3(0.5f, 0.39f, 0),
		new Vector3(-0.25f, 0.39f, 0),
	};
	/// <summary>
	/// The m multiplayer menu positions.
	/// положения меню сетевой игры при различных положениях камеры
	/// </summary>
	Vector3[] mMultiplayerMenuPositions = new Vector3[]{
		new Vector3(1.26f, -0.17f, 0),
		new Vector3(1.26f, 0.27f, 0),
		new Vector3(0.5f, 0.50f, 0),
	};
	/// <summary>
	/// The m smooth.
	/// скорость вращения камеры
	/// </summary>
	public float mSmooth = 2.0f;
	GameObject panelMainMenu;
	GameObject panelMultiplayerMenu;

	/// <summary>
	/// Start this instance.
	/// устанавливается начальные положения меню и камеры 
	/// </summary>
	void Start()
	{
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
	 /// <summary>
	 /// Update this instance.
	/// обновление положений меню и камеры
	 /// </summary>
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
	/// <summary>
	/// Translates the coords.
	/// преобразование координат относительных в экранные координаты
	/// </summary>
	/// <returns>The coords.</returns>
	/// <param name="vec">Vec.</param>
	public Vector3 TranslateCoords(Vector3 vec)
	{
		return new Vector3(vec.x * Screen.width, vec.y * Screen.height, vec.z);
	}
	/// <summary>
	/// Loads the game.
	/// начать игру
	/// </summary>
	public void LoadGame()
	{
		GameOptions.Instance.Mode = GameOptions.GameMode.PvE;
		Application.LoadLevel(1);
	}
	/// <summary>
	/// Loads the multiplayer menu.
	/// перейти к меню сетевой игры
	/// </summary>
	public void LoadMultiplayerMenu()
	{
		mCurrentMenuState = MenuState.Multiplayer;
		EnableMultiplayerMenu();
	}
	/// <summary>
	/// Loads the main menu.
	/// загрузить основное меню
	/// </summary>
	public void LoadMainMenu()
	{
		mCurrentMenuState = MenuState.Main;
	}
	/// <summary>
	/// Exit this instance.
	/// выйти из игры
	/// </summary>
	public void Exit()
	{
		Application.Quit();
	}
	/// <summary>
	/// Enables the multiplayer menu.
	/// сделать активными кнопки в меню сетевой игры
	/// </summary>
	void EnableMultiplayerMenu()
	{
		GameObject.Find("btnHost").GetComponent<Button>().interactable = true;
		GameObject.Find("btnConnect").GetComponent<Button>().interactable = true;
		GameObject.Find("btnBack").GetComponent<Button>().interactable = true;
		GameObject.Find("InputFieldIp").GetComponent<InputField>().interactable = true;
		GameObject.Find("InputFieldPort").GetComponent<InputField>().interactable = true;
	}
	/// <summary>
	/// Disables the multiplayer menu.
	/// сделать не активными кнопки в меню сетевой игры
	/// </summary>
	void DisableMultiplayerMenu()
	{
		GameObject.Find("btnHost").GetComponent<Button>().interactable = false;
		GameObject.Find("btnConnect").GetComponent<Button>().interactable = false;
		GameObject.Find("btnBack").GetComponent<Button>().interactable = false;
		GameObject.Find("InputFieldIp").GetComponent<InputField>().interactable = false;
		GameObject.Find("InputFieldPort").GetComponent<InputField>().interactable = false;
	}
	/// <summary>
	/// Connect this instance.
	/// совершить подключение к серверу
	/// </summary>
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
	/// <summary>
	/// Host this instance.
	/// создать сервер
	/// </summary>
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
