using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    enum PlayerID
    {
        PlayerOne,
        PlayerTwo,
        PlayerThree,
        PlayerFour,
    }

    public int playerToSpwan = 2;

    private PlayerInputManager m_playerInputManager;
    private int m_playerNumber =  4;
    private int m_currentPlayerNumber;
    List<InputDevice> devices = new List<InputDevice>();

    public void Awake()
    {
        m_playerInputManager = GetComponent<PlayerInputManager>();
    }
    public void Start()
    {
        GetAllDevice();
        SpawnPlayers();
    }

    #region  Players
    private void SpawnPlayers()
    {
        for (int i = 0; i < playerToSpwan; i++)
        {
            SpawnPlayer(m_currentPlayerNumber);
            m_currentPlayerNumber++;
        }
    }
    private void SpawnPlayer(int index)
    {
        int deviceIndex = GetGampad() == -1 ? GetKeyboard() : GetGampad();
        m_playerInputManager.JoinPlayer(index,-1,null, GetDevice(deviceIndex));
        
    }
    public int GetPlayerNumber()
    {
        return m_playerNumber;
    }
    #endregion

    #region Input Devices
    private void GetAllDevice()
    {
        for (int i = 0; i < InputSystem.devices.Count; i++)
        {
            devices.Add(InputSystem.devices[i]);
        }
    }
    private int GetGampad()
    {
        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i] is Gamepad) return i;
        }
        return -1;
    }

    private int GetKeyboard()
    {
        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i] is Keyboard) return i;
        }
        return -1;
    }

    private InputDevice GetDevice(int index)
    {
           InputDevice device =   devices[index];
        devices.Remove(device);
        return device;
    }

    #endregion


}
