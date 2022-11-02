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
    public int totalHealth = 5; // 血量上限
    public void UpdateHealth(int value)
    {
        playerHealth += value;
        playerHealth = playerHealth < totalHealth ? playerHealth : totalHealth;
    }

    // 重新加载场景，参数为当前场景名字
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
