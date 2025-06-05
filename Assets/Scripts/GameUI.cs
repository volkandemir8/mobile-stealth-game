using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public GameObject gameLoseUI;
    public GameObject gameWinUI;
    public GameObject gameLoseButton;
    public GameObject gameWinButton;
    public GameObject joystick;

    public Animator leftDoor;
    public Animator rightDoor;
    public GameObject UnlockMsg;
    public GameObject Counter;
    bool first = true;

    void Start()
    {
        Guard.onGuardSpottedPlayer += ShowGameLoseUI;
    }

    void Update()
    {
        if (CollectibleCount.count == Collectibles.total & first == true)
        {
            leftDoor.SetTrigger("DoorLeft");
            rightDoor.SetTrigger("DoorRight");
            StartCoroutine(UnlockMessage());
        }
    }

    IEnumerator UnlockMessage()
    {
        first = false;
        Counter.SetActive(false);
        UnlockMsg.SetActive(true);
        yield return new WaitForSecondsRealtime(1f); 
        UnlockMsg.SetActive(false);
    }

    public void GameOverButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Collectibles.total = 0;
    }
    public void GameWinButton()
    {
        if (SceneManager.GetActiveScene().buildIndex < (SceneManager.sceneCountInBuildSettings-1))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            Collectibles.total = 0;
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ShowGameWinUI()
    {
        OnGameOver(gameWinUI);
    }
    void ShowGameLoseUI()
    {
        OnGameOver(gameLoseUI);
    }


    void OnGameOver(GameObject gameOverUI)
    {
        gameOverUI.SetActive(true);
        joystick.SetActive(false);
        Counter.SetActive(false);
        Guard.onGuardSpottedPlayer -= ShowGameLoseUI;
    }

}
