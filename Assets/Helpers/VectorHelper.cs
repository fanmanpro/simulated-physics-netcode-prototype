using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorHelper
{
	public static Serializable.Vector2 ToProtobufVector(this Vector2 v)
	{
		return new Serializable.Vector2
		{
			X = v.x,
			Y = v.y,
		};
	}
	public static Vector2 ToUnityVector(this Serializable.Vector2 v)
	{
		return new Vector2
		{
			x = v.X,
			y = v.Y,
		};
	}
	public static Serializable.Vector3 ToProtobufVector(this Vector3 v)
	{
		return new Serializable.Vector3
		{
			X = v.x,
			Y = v.y,
			Z = v.z,
		};
	}
	public static Vector3 ToUnityVector(this Serializable.Vector3 v)
	{
		return new Vector3
		{
			x = v.X,
			y = v.Y,
			z = v.Z,
		};
	}
}
