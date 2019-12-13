using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorHelper
{
	public static Gamedata.Vector2 ToProtobufVector(this Vector2 v)
	{
		return new Gamedata.Vector2
		{
			X = v.x,
			Y = v.y,
		};
	}
	public static Vector2 ToUnityVector(this Gamedata.Vector2 v)
	{
		return new Vector2
		{
			x = v.X,
			y = v.Y,
		};
	}
}
