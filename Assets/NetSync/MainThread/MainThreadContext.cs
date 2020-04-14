using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadContext : MonoBehaviour
{
	static MainThreadContext _instance;
	static volatile bool _queued = false;
	static List<Action> _backlog = new List<Action>(8);
	static List<Action> _actions = new List<Action>(8);

	public static void RunOnMainThread(Action action)
	{
		lock (_backlog)
		{
			_backlog.Add(action);
			_queued = true;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		if (_instance == null)
		{
			_instance = new GameObject("MainThreadContext").AddComponent<MainThreadContext>();
			DontDestroyOnLoad(_instance.gameObject);
		}
	}

	private void Update()
	{
		if (_queued)
		{
			lock (_backlog)
			{
				var tmp = _actions;
				_actions = _backlog;
				_backlog = tmp;
				_queued = false;
			}

			foreach (var action in _actions)
				action();

			_actions.Clear();
		}
	}

}
