using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
public class Player_AssetData : MonoBehaviour
{
    public Sprite[] playerPortrait;
    public Gradient[] playerHeadColorsGradient;
    public Gradient[] playerHitColorsGradient;

    public void AssignPlayerParameter(int playerNumber, GameObject playerObject)
    {
        playerObject.GetComponentInChildren<VisualEffect>().SetGradient("Head_Gradient", playerHeadColorsGradient[playerNumber]);
    }
}
