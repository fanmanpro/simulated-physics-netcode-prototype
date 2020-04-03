using UnityEngine;

public static class SceneHelper
{
	public static bool GetComponentSingle<T>(this UnityEngine.SceneManagement.Scene s, out T component)
	{
		foreach (GameObject rootGameObject in s.GetRootGameObjects())
		{
			component = rootGameObject.GetComponentInChildren<T>();
			if (component != null)
			{
				return true;
			}
		}
		component = default(T);
		return false;
	}
}