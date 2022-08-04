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

    public int totalScore;
    public Text Txt_Score;
    public void UpdateTotalScore()
    {
        this.Txt_Score.text = totalScore.ToString();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
