using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RealtimeUI : MonoBehaviour
{
    [SerializeField]
    TMP_Text hpText;

    [SerializeField]
    TMP_Text scoreText;

    [SerializeField]
    RealtimeHandler rtHandler;

    [SerializeField]
    GameObject instructions;

    [SerializeField]
    GameObject quitGame;

    [SerializeField]
    GameObject win; 
    
    [SerializeField]
    GameObject loss;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (rtHandler.paused && instructions.activeSelf)
            {
                instructions.SetActive(false);
                rtHandler.ResumeGame();
            }
            if (rtHandler.paused && (quitGame.activeSelf || loss.activeSelf || win.activeSelf))
            {
                rtHandler.ResumeGame();
                SceneManager.LoadScene("MainMenu");
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!rtHandler.paused && !quitGame.activeSelf)
            {
                rtHandler.PauseGame();
                quitGame.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (rtHandler.paused && quitGame.activeSelf)
            {
                rtHandler.ResumeGame();
                quitGame.SetActive(false);
            }
            if (rtHandler.paused && (win.activeSelf || loss.activeSelf))
            {
                rtHandler.ResumeGame();
                SceneManager.LoadScene("RealTimeScene");
            }
        }
        if (!rtHandler.paused)
        {
            updateHPText();
            updateScoreText();
        }
        if (rtHandler.score >= rtHandler.requiredCollect || rtHandler.player.hp <= 0)
        {
            finishGame();
        }
    }

    void finishGame()
    {
        // player wins
        if (rtHandler.score >= rtHandler.requiredCollect)
        {
            win.SetActive(true);
            rtHandler.PauseGame();
        }
        // player loses
        else
        {
            loss.SetActive(true);
            rtHandler.PauseGame();
        }
    }

    void updateHPText()
    {
        hpText.text = "HP: " + rtHandler.player.hp;
    }

    void updateScoreText()
    {
        scoreText.text = "Collected: " + rtHandler.score + "/" + rtHandler.requiredCollect;
    }
}
