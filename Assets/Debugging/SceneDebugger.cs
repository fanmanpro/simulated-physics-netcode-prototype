using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDebugger : MonoBehaviour
{
	public Object sceneObject;
	public int buildIndex;

	private void Awake()
	{
		// for some reason this variable value doesn't persist between methods. but the name and buildIndex does.
		Scene scene = SceneManager.GetSceneByName(sceneObject.name);
		buildIndex = scene.buildIndex;
	}

	public void Unload()
	{
		Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
		if (scene.isLoaded)
		{
			SceneManager.UnloadSceneAsync(buildIndex);
		}
	}
	public void Load()
	{
		Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
		if (!scene.isLoaded)
		{
			SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
		}
	}
}
