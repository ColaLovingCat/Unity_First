using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MenuController : MonoBehaviour
{
    // 开始游戏
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    // 退出游戏
    public void QuitGame()
    {
        Application.Quit();
    }
    // 动画结束后启动UI
    public void UIEnable()
    {
        GameObject.Find("Canvas/MainMenu/UI").SetActive(true);
    }

    public GameObject _pauseMenu;
    public AudioMixer _audioMixer;
    //唤醒暂停菜单
    public void PauseGame()
    {
        _pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    //返回游戏
    public void ResumeGame()
    {
        _pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    //返回主菜单
    public void ReturnMain()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SetVolume(float value)
    {
        _audioMixer.SetFloat("MainVolume", value);
    }

}
