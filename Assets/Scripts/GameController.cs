using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void RefreshGameInfos()
    {
        this.Txt_Score.text = totalScore.ToString();
        this.Txt_Health.text = playerHealth.ToString();
    }

    public Text Txt_Score;
    public int totalScore;

    public Text Txt_Health;
    public int playerHealth = 5;
    public int totalHealth = 5; // я╙а©иооч
    public void UpdateHealth(int value)
    {
        playerHealth += value;
        playerHealth = playerHealth < totalHealth ? playerHealth : totalHealth;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
