using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject mainMenu;
    public GameObject levelsPanel;

    public GameObject sound_On;
    public GameObject sound_Off;
    public GameObject vibration_On;
    public GameObject vibration_Off;
    public GameObject information;
    public GameObject informationMenu;
    public GameObject ResetMenu;


    public void SettingsOpen()
    {
        optionsPanel.SetActive(true);
        mainMenu.SetActive(false);

        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            sound_On.SetActive(true);
            sound_Off.SetActive(false);
            AudioListener.volume = 1;
        }
        else if (PlayerPrefs.GetInt("Sound") == 2)
        {
            sound_On.SetActive(false);
            sound_Off.SetActive(true);
            AudioListener.volume = 0;
        }

        if (PlayerPrefs.GetInt("Vibration") == 1)
        {
            vibration_On.SetActive(true);
            vibration_Off.SetActive(false);
        }
        else if (PlayerPrefs.GetInt("Vibration") == 2)
        {
            vibration_On.SetActive(false);
            vibration_Off.SetActive(true);
        }
    }

    public void SettingsClose()
    {
        optionsPanel.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OpenAudio()
    {
        sound_On.SetActive(false);
        sound_Off.SetActive(true);
        AudioListener.volume = 0;
        PlayerPrefs.SetInt("Sound", 2);
    }
    public void OpenVibration()
    {
        vibration_On.SetActive(false);
        vibration_Off.SetActive(true);
        PlayerPrefs.SetInt("Vibration", 2);
    }
    public void OpenInformation()
    {
        informationMenu.SetActive(true);
    }

    public void CloseAudio()
    {
        sound_On.SetActive(true);
        sound_Off.SetActive(false);
        AudioListener.volume = 1;
        PlayerPrefs.SetInt("Sound", 1);
    }
    public void CloseVibration()
    {
        vibration_On.SetActive(true);
        vibration_Off.SetActive(false);
        PlayerPrefs.SetInt("Vibration", 1);
    }
    public void CloseInformation()
    {
        informationMenu.SetActive(false);
    }


    public void LevelsOpen()
    {
        levelsPanel.SetActive(true);
    }

    public void LevelsClose()
    {
        levelsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenResetMenu()
    {
        ResetMenu.SetActive(true);
    }
    public void ResetPrefs()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }
    public void CloseResetMenu()
    {
        ResetMenu.SetActive(false);
    }
}
