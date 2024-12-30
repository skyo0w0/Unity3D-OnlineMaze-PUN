using System;
using Photon.Pun;
using UnityEngine;
using Zenject;


public class PlayerScoreComponent : MonoBehaviourPun
{
    [Inject] private ScoreDisplayComponent _scoreDisplay;
    [Inject] private DiContainer _container;
    private GameManager _gameManager => FindObjectOfType<GameManager>();
    
    [SerializeField] private int _score = 95;
    

    public void AddScore(int score)
    {
        _score += score;
        _scoreDisplay.DisplayScore(_score);

        if (_score >= 100)
        {
            // Уведомляем всех игроков через Pun RPC
            photonView.RPC("OnGameWon", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void OnGameWon()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0;
    }

    public int GetScore()
    {
        return _score;
    }
}
