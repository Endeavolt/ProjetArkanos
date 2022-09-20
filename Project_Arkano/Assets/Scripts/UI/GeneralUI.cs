using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralUI : MonoBehaviour
{
    public Image[] playerIcon;
    public PlayerUI[] playerUIs = new PlayerUI[4];
    public Text scoreText;
    
    public PlayerUI GetPlayerUI(int id)
    {
        //playerUIs[id].SetIcon(playerIcon[id]);
        playerUIs[id].gameObject.SetActive(true);
        return playerUIs[id];
    }


}
