using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [SerializeField] private Image m_iconPlayer;
    [SerializeField] private Image m_loadStrike;
    [SerializeField] private Image m_stateSuperStrike;


    public  void  SetIcon(Image image) { m_iconPlayer = image; }
    public void FillStrikeImage( float ratio){ m_loadStrike.fillAmount = ratio;}

    public void FillSuperStrikeImage(float ratio)
    {
        m_stateSuperStrike.fillAmount = ratio;
    }

}
