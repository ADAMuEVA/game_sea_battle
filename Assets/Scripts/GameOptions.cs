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
	
}