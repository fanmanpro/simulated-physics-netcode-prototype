﻿using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetSync))]
public class Seat : MonoBehaviour
{
	public bool Assigned;

	public Type LocalPlayerComponent { get { return typeof(Player); } }

	private string guid;
	public string GUID { get => guid; private set => guid = value; }
	
	public List<NetSync> ownerOf;

	void Awake()
	{
		NetSync netSync = GetComponent<NetSync>();
		if (netSync == null)
		{
			Debug.LogWarning("Rigidbody2D must contain NetSync component to sync across network.");
			enabled = false;
			return;
		}
		GUID = netSync.GUID;
	}
}
