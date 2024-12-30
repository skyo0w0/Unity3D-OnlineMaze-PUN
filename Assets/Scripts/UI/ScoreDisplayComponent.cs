using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreDisplayComponent : MonoBehaviour
{
    TMP_Text scoreText;
    
    private PlayerScoreComponent _scoreComponent;
    private void Awake()
    {
        scoreText = GetComponent<TMP_Text>();
    }
    
    public void DisplayScore(int score)
    {
        scoreText.text = score.ToString();
    }
    
}
