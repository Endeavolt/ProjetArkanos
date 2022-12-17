using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralUI : MonoBehaviour
{
    public Image[] playerIcon;
    public PlayerUI[] playerUI = new PlayerUI[4];
    public Text scoreText;
    public Text startText;

    public PlayerUI GetPlayerUI(int id)
    {
        //playerUIs[id].SetIcon(playerIcon[id]);
        playerUI[id].gameObject.SetActive(true);
        return playerUI[id];
    }
    public void SetTextStartTimer(float time, bool isActive)
    {
        startText.gameObject.SetActive(isActive);
        startText.text = time.ToString("F0");
    }


}
