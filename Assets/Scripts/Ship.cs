using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Класс Ship.
/// Свойства корабля и позиция на игровом поле в зависимости от этих свойств
/// </summary>
public class Ship : MonoBehaviour {
	/// <summary>
	/// возможные типы кораблей
	/// </summary>
	public enum Type
	{
		Destroyer = 0,
		Cruiser = 1,
		Battleship = 2,
		AircraftCarrier = 3,
	}
	/// <summary>
	/// The ship lengths.
	/// количество клеток каждого типа корабля
	/// </summary>
	static int[] ShipLengths = new int[] { 1, 2, 3, 4 };
	/// <summary>
	/// The ship max counts.
	/// максимальное количество кораблей каждого типа
	/// </summary>
	static int[] ShipMaxCounts = new int[] { 4, 3, 2, 1 };
	/// <summary>
	/// The ship rotation.
	/// повороты каждого типа корабля, чтоб он правильно распологался на поле
	/// </summary>
	static Vector3[] ShipRotation = new Vector3[] { 
		new Vector3(270, 180, 0),
		new Vector3(0, 180, 0),
		new Vector3(180, 0, 0),
		new Vector3(270, 0, 0)
	};
	/// <summary>
	/// The ship position hor.
	/// смещение каждого типа корабля в горизонтальном положении
	/// </summary>
	static Vector3[] ShipPositionHor = new Vector3[] { 
		new Vector3(0.5f, 0, 0),
		new Vector3(0, 0, 0),
		new Vector3(-0.3f, 0, 0),
		new Vector3(0, 0, -0.2f)
	};
	/// <summary>
	/// The ship position ver.
	/// смещение каждого типа корабля в вертикальном положении
	/// </summary>
	static Vector3[] ShipPositionVer = new Vector3[] { 
		new Vector3(0, 0, -0.5f),
		new Vector3(0, 0, 0),
		new Vector3(0, 0, 0.3f),
		new Vector3(-0.3f, 0, 0)
	};
	/// <summary>
	/// The ship scale.
	/// масштаб каждого корабля
	/// </summary>
	static Vector3[] ShipScale = new Vector3[] { 
		new Vector3(1, 1, 1),
		new Vector3(1, 1, 1),
		new Vector3(1, 1, 1),
		new Vector3(1, 1, 1)
	};
	/// <summary>
	/// The vertical rotations.
	/// вектор для изменения положения корабля из горизонтального в вертикальное
	/// </summary>
	Vector3 VerticalRotations = new Vector3(0, 90, 0);
	/// <summary>
	/// The types.
	/// основные свойства корабля
	/// </summary>
	Type mType;
	bool mHorizontal;
	int mX;
	int mY;
	Vector3 mStartPt;
	Vector2 mFieldSize;
	bool mSinked = false;
	float mDepth = 0.0f;
	/// <summary>
	/// The sinking time coef.
	/// коефициент скорости с которой корабль тонет
	/// </summary>
	const float SinkingTimeCoef = 0.2f;
	/// <summary>
	/// The bottom.
	/// максимальная глубина в которой может распологаться корабль
	/// </summary>
	const float Bottom = -2.0f;
	/// <summary>
	/// Init the specified startpt, field, type, horizontal, x and y.
	/// указать параметры нового корабля
	/// </summary>
	/// <param name="startpt">Startpt.</param>
	/// <param name="field">Field.</param>
	/// <param name="type">Type.</param>
	/// <param name="horizontal">If set to <c>true</c> horizontal.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void Init(Vector3 startpt, Vector2 field, Type type, bool horizontal, int x, int y)
	{
		mType = type;
		mHorizontal = horizontal;
		mX = x;
		mY = y;
		mStartPt = startpt;
		mFieldSize = field;
		mDepth = 0.0f;
	}
	/// <summary>
	/// Move the specified horizontal, x and y.
	/// переместить корабль
	/// </summary>
	/// <param name="horizontal">If set to <c>true</c> horizontal.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void Move (bool horizontal, int x, int y)
	{
		if (mHorizontal == horizontal &&
			mX == x && mY == y)
			return;

		mHorizontal = horizontal;
		mX = x;
		mY = y;

		RefreshPosition();
	}
	/// <summary>
	/// Refreshs the position.
	/// обновить положение корабля
	/// </summary>
	void RefreshPosition()
	{
		// если корабль не на поле, скрыть
		if(mX == -1 ||
			mY == -1)
		{
			gameObject.SetActive(false);
			return;
		}

		gameObject.SetActive(true);

		// если корабль затоплен, то погружать его пока не достигнет дна
		if(mSinked)
		{
			mDepth -= Time.deltaTime * SinkingTimeCoef;

			if (mDepth < Bottom)
				mDepth = Bottom;
		}
		
		// обновить положение корабля
		Vector3 offset;

		if (mHorizontal)
		{
			offset = new Vector3(
				ShipLengths[(int)mType] * 0.5f * mFieldSize.x,
				mDepth,
				mFieldSize.y / 2);
			offset += ShipPositionHor[(int)mType];
		}
		else
		{
			offset = new Vector3(
				mFieldSize.x / 2,
				mDepth,
				ShipLengths[(int)mType] * 0.5f * mFieldSize.y);
			offset += ShipPositionVer[(int)mType];
		}

		transform.position = new Vector3(
			mStartPt.x + mFieldSize.x * mX + offset.x,
			offset.y,
			mStartPt.z + mFieldSize.y * mY + offset.z);

		transform.localScale = ShipScale[(int)mType];

		if(mHorizontal)
		{
			transform.rotation = Quaternion.Euler(ShipRotation[(int)mType]);
		}
		else
		{
			transform.rotation = Quaternion.Euler(ShipRotation[(int)mType] + VerticalRotations);
		}
	}
	/// <summary>
	/// Start this instance.
	/// Use this for initialization
	/// </summary>
	void Start()
	{

	}
	/// <summary>
	/// Update this instance.
	/// Update is called once per frame
	/// </summary>
	void Update()
	{
		RefreshPosition();
	}
	/// <summary>
	/// Gets the length of the ship.
	/// получить размер корабля
	/// </summary>
	/// <returns>The ship length.</returns>
	public int GetShipLength()
	{
		return ShipLengths[(int)mType];
	}
	/// <summary>
	/// Gets the type of the ship.
	/// получить тип корабля
	/// </summary>
	/// <returns>The ship type.</returns>
	public Type GetShipType()
	{
		return mType;
	}
	/// <summary>
	/// Gets the ships max count.
	/// получить возможное количество короблей типа type
	/// </summary>
	/// <returns>The ships max count.</returns>
	/// <param name="type">Type.</param>
	public static int GetShipsMaxCount(Type type)
	{
		return ShipMaxCounts[(int)type];
	}
	/// <summary>
	/// Gets the cells.
	/// получить клетки в которых распологается корабль
	/// </summary>
	/// <returns>The cells.</returns>
	public List<Vector2> GetCells()
	{
		List<Vector2> cells = new List<Vector2>();

		if (mHorizontal)
		{
			for (int i = 0; i < ShipLengths[(int)mType]; ++i)
			{
				cells.Add(new Vector2(mX + i, mY));
			}
		}
		else
		{
			for (int i = 0; i < ShipLengths[(int)mType]; ++i)
			{
				cells.Add(new Vector2(mX, mY + i));
			}
		}

		return cells;
	}
	/// <summary>
	/// Ises the horizontal.
	/// находится ли корабль горизонтально
	/// </summary>
	/// <returns><c>true</c>, if horizontal was ised, <c>false</c> otherwise.</returns>
	public bool isHorizontal()
	{
		return mHorizontal;
	}
	/// <summary>
	/// Gets the x,y.
	/// получить координаты корабля
	/// </summary>
	/// <returns>The x.</returns>
	public int GetX()
	{
		return mX;
	}

	public int GetY()
	{
		return mY;
	}
	/// <summary>
	/// Determines whether this instance has cell the specified cell_x cell_y.
	/// принадлежит ли клетка кораблю
	/// </summary>
	/// <returns><c>true</c> if this instance has cell the specified cell_x cell_y; otherwise, <c>false</c>.</returns>
	/// <param name="cell_x">Cell_x.</param>
	/// <param name="cell_y">Cell_y.</param>
	public bool HasCell(int cell_x, int cell_y)
	{
		if (mHorizontal)
		{
			for (int i = 0; i < ShipLengths[(int)mType]; ++i)
			{
				if(cell_x == mX + i &&
					cell_y == mY)
				{
					return true;
				}
			}
		}
		else
		{
			for (int i = 0; i < ShipLengths[(int)mType]; ++i)
			{
				if (cell_x == mX &&
					cell_y == mY + i)
				{
					return true;
				}
			}
		}

		return false;
	}
	/// <summary>
	/// Gets the length of the ship.
	/// получить размер коробля типа type
	/// </summary>
	/// <returns>The ship length.</returns>
	/// <param name="type">Type.</param>
	public static int GetShipLength(Type type)
	{
		return ShipLengths[(int)type];
	}
	/// <summary>
	/// Ises the sinked.
	/// является ли корабль затопленным
	/// </summary>
	/// <returns><c>true</c>, if sinked was ised, <c>false</c> otherwise.</returns>
	public bool isSinked()
	{
		return mSinked;
	}
	/// <summary>
	/// Sink this instance.
	/// затопить корабль
	/// </summary>
	public void Sink()
	{
		mSinked = true;

		gameObject.SetActive(true);
	}
	/// <summary>
	/// Gets the depth.
	/// получить глубину на которой находится корабль
	/// </summary>
	/// <returns>The depth.</returns>
	public float GetDepth()
	{
		return mDepth;
	}
}
