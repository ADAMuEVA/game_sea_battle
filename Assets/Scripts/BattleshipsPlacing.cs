using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// Класс Battleships placing.
/// Обработчик канваса(Canvas) для расстановки кораблей.
/// </summary>
public class BattleshipsPlacing : MonoBehaviour {

	public GameObject PlayerField;
	FieldOperations mFieldOperations;
	/// <summary>
	/// Start this instance.
	/// Use this for initialization
	/// </summary>
	// Use this for initialization
	void Start () {
		mFieldOperations = PlayerField.GetComponent<FieldOperations>();
		RefreshButtonsTexts();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	/// <summary>
	/// Refreshs the buttons texts.
	/// обновление надписей на кнопках при расстановке кораблей
	/// </summary>
	public void RefreshButtonsTexts()
	{
		if (mFieldOperations != null)
		{
			GameObject btn1Bit = GameObject.Find("btn1Bit");
			GameObject btn2Bit = GameObject.Find("btn2Bit");
			GameObject btn3Bit = GameObject.Find("btn3Bit");
			GameObject btn4Bit = GameObject.Find("btn4Bit");

			int bit1left = Ship.GetShipsMaxCount(Ship.Type.Destroyer) -
				mFieldOperations.GetShipsCountByType(Ship.Type.Destroyer);
			int bit2left = Ship.GetShipsMaxCount(Ship.Type.Cruiser) -
				mFieldOperations.GetShipsCountByType(Ship.Type.Cruiser);
			int bit3left = Ship.GetShipsMaxCount(Ship.Type.Battleship) -
				mFieldOperations.GetShipsCountByType(Ship.Type.Battleship);
			int bit4left = Ship.GetShipsMaxCount(Ship.Type.AircraftCarrier) -
				mFieldOperations.GetShipsCountByType(Ship.Type.AircraftCarrier);

			btn1Bit.GetComponentsInChildren<Text>()[0].text =
				"Эсминец (" + bit1left.ToString() +
				"/" + Ship.GetShipsMaxCount(Ship.Type.Destroyer).ToString() + ")";

			btn2Bit.GetComponentsInChildren<Text>()[0].text =
				"Крейсер (" + bit2left.ToString() +
				"/" + Ship.GetShipsMaxCount(Ship.Type.Cruiser).ToString() + ")";

			btn3Bit.GetComponentsInChildren<Text>()[0].text =
				"Линкор (" + bit3left.ToString() +
				"/" + Ship.GetShipsMaxCount(Ship.Type.Battleship).ToString() + ")";

			btn4Bit.GetComponentsInChildren<Text>()[0].text =
				"Авианосец (" + bit4left.ToString() +
				"/" + Ship.GetShipsMaxCount(Ship.Type.AircraftCarrier).ToString() + ")";

			//если кораблей данного типа не осталось, сделать кнопку неактивной
			btn1Bit.GetComponent<Button>().interactable = (bit1left != 0);
			btn2Bit.GetComponent<Button>().interactable = (bit2left != 0);
			btn3Bit.GetComponent<Button>().interactable = (bit3left != 0);
			btn4Bit.GetComponent<Button>().interactable = (bit4left != 0);
		}
	}
}
