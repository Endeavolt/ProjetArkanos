using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameScore
{
    [SerializeField]
    private  int[] m_playerScore;
    public void AddScore(int scoreToAdd, int playerID)
    {
        m_playerScore[playerID] += scoreToAdd;
    }

    public void ResetScore()
    {
        for (int i = 0; i < m_playerScore.Length; i++)
        {
            m_playerScore[i] = 0;
        }
    }

    public int[] GetScoreArray()
    {
        return m_playerScore;
    }

    public int GetPlayer(int playerID)
    {
        return m_playerScore[playerID];
    }

    public GameScore(int number)
    {
        m_playerScore = new int[number];
    }
}