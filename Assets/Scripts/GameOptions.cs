using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameOptions : MonoBehaviour
{

	 public static GameOptions Instance;

	// данная функция позволяет сохранить объект при 
	// переключении между сценами
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

	public GameMode Mode = GameMode.PvE;		// режим игры - против компьютера (PvE) или против игрока по сети (PvP)
	public bool Server = false;					// является ли текущий игрок сервером
	public NetworkManager Network = null;		// переменная для доступа к параметром сети
	//public NetworkPlayer Player = null;			// доступ к объекту который осуществляет обмен данными по сети

}
