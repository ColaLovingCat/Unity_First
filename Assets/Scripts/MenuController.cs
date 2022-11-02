using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MenuController : MonoBehaviour
{
    // ��ʼ��Ϸ
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    // �˳���Ϸ
    public void QuitGame()
    {
        Application.Quit();
    }
    // ��������������UI
    public void UIEnable()
    {
        GameObject.Find("Canvas/MainMenu/UI").SetActive(true);
    }

    public GameObject _pauseMenu;
    public AudioMixer _audioMixer;
    //������ͣ�˵�
    public void PauseGame()
    {
        _pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    //������Ϸ
    public void ResumeGame()
    {
        _pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    //�������˵�
    public void ReturnMain()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SetVolume(float value)
    {
        _audioMixer.SetFloat("MainVolume", value);
    }

}
