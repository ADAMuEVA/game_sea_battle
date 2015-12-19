using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
/// <summary>
/// Класс GameOptions.
/// Который создается при загрузке меню и не удаляется при переходе на другую сцену.
/// Хранит информацию, которую необходимо передать из одной сцены в другую
/// </summary>
public class GameOptions : MonoBehaviour
{

	 public static GameOptions Instance;

	/// <summary>
	/// Awake this instance.
	/// данная функция позволяет сохранить объект при переключении между сценами
	/// </summary>
	 void Awake()
	 {
		 if (!Instance)
		 {
			 Instance = this;
			 DontDestroyOnLoad(gameObject);
		 }
		 else
			 Destroy(gameObject);
	 }

	public enum GameMode
	{
		PvP,
		PvE,
	}
	/// <summary>
	/// The mode.
	/// режим игры - против компьютера (PvE) или против игрока по сети (PvP)
	/// является ли текущий игрок серверо
	/// переменная для доступа к параметром сети
	/// доступ к объекту который осуществляет обмен данными по сети
	/// </summary>
	public GameMode Mode = GameMode.PvE;		// режим игры - против компьютера (PvE) или против игрока по сети (PvP)
	public bool Server = false;					// является ли текущий игрок сервером
	public NetworkManager Network = null;		// переменная для доступа к параметром сети
	public NetworkPlayer Player = null;			// доступ к объекту который осуществляет обмен данными по сети
}
