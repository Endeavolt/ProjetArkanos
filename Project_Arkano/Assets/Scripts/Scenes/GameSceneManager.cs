using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public int playerChoiceScene = 1;
    private int playerNum;
    private bool sceneFind;
    private Scene scene;
    public void Start()
    {
        LoadPlayerChoiceScene();
    }

    public void Update()
    {
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (!sceneFind && scene.isLoaded)
        {
            LauchGameScene();
            return;
        }
    }

    private void LoadPlayerChoiceScene()
    {
        SceneManager.LoadScene(playerChoiceScene, LoadSceneMode.Additive);
    }

    public void LoadGame(int playerNumber)
    {
        SceneManager.UnloadSceneAsync(1);
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
        scene = SceneManager.GetSceneAt(2);
        playerNum = playerNumber;
    }

    private void LauchGameScene()
    {
        GameObject[] gameOb = scene.GetRootGameObjects();
        for (int i = 0; i < gameOb.Length; i++)
        {
            if (gameOb[i].name == "Game Manager")
            {
                GameManager gm = gameOb[i].GetComponent<GameManager>();
                gm.playerToSpwan = playerNum;
                SceneManager.SetActiveScene(scene);
                gm.StartGame();
                break;
            }
        }
        sceneFind = true;
    }
}
