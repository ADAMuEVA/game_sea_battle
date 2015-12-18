using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkPlayer : NetworkBehaviour {

	GameObject mGame = null;
	GamePvP mGamePvP = null;

	void Start ()
	{
		//DontDestroyOnLoad(gameObject);

		mGame = GameObject.Find("Game");
		mGamePvP = mGame.GetComponent<GamePvP>();
		GameOptions.Instance.Player = this;
	}
	

	void Update () {

	}


	// обмен командами по сети

	[ClientRpc]
	public void RpcYourTurn()
	{
		mGamePvP.ReceivedYourTurn();
	}

	[ClientRpc]
	public void RpcEnemyTurn()
	{
		mGamePvP.ReceivedEnemyTurn();
	}

	[Command]
	public void CmdSetEnemyShip(int type, int x, int y, bool horizontal)
	{
		mGamePvP.ReceivedSetEnemyShip(type, x, y, horizontal);
	}

	[Command]
	public void CmdAttackCell(int x, int y)
	{
		mGamePvP.ReceivedAttackCell(x, y);
	}

	[ClientRpc]
	public void RpcSetEnemyShip(int type, int x, int y, bool horizontal)
	{
		mGamePvP.ReceivedSetEnemyShip(type, x, y, horizontal);
	}

	[ClientRpc]
	public void RpcAttackCell(int x, int y)
	{
		mGamePvP.ReceivedAttackCell(x, y);
	}



	public void YourTurn()
	{
		if (isServer)
		{
			RpcYourTurn();
		}
	}

	public void EnemyTurn()
	{
		if (isServer)
		{
			RpcEnemyTurn();
		}
	}

	public void SetEnemyShip(int type, int x, int y, bool horizontal)
	{
		if (isServer)
		{
			RpcSetEnemyShip(type, x, y, horizontal);
		}
		else
		{
			CmdSetEnemyShip(type, x, y, horizontal);
		}
	}

	public void AttackCell(int x, int y)
	{
		if (isServer)
		{
			RpcAttackCell(x, y);
		}
		else
		{
			CmdAttackCell(x, y);
		}
	}
}
