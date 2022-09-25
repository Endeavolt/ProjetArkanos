using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum ProfilState
{
    None,
    Wait,
    Ready,
}

public class ProfilPlayerUI : MonoBehaviour
{
    public Text playerName;
    public Text instructionText;
    public Image playerImage;
    public ProfilState profilState = ProfilState.None;
    public InputDevice playerDevice;
    

    public void ChangePlayerName(string name)
    {
        playerName.text = name;
    }

    public void ChangePlayerInstruction(string name)
    {
        instructionText.text = name;
    }

    public void ChangePlayerImageColor(Color color)
    {
        playerImage.color = color;
    }

    public void ChangeProfilState(ProfilState profil)
    {
        profilState = profil;
    }
}
