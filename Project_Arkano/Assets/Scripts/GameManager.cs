using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum PlayerID
{
    PlayerOne = 1,
    PlayerTwo = 2,
    PlayerThree = 3,
    PlayerFour = 4,
    None = 5,
}
public class GameManager : MonoBehaviour
{
    const int PLAYER_MAX = 4;

    public int playerToSpwan = 2;
    public Vector3[] spawnPosition = new Vector3[PLAYER_MAX];
    public Color[] playerColor;
    public GameScore gameScore;
    public GeneralUI generalUI;
    public GameObject ball;

    [Header("Debug")]
    public bool previewSpawnPosition = false;
    public float sphereRadius = 1.0f;
    public bool IsPlayerNumberSecurity = true;

    private PlayerInputManager m_playerInputManager;
    private int m_playerNumber = 4;
    private int m_currentPlayerNumber;
    List<InputDevice> devices = new List<InputDevice>();

    public Player_AssetData m_playerAsset; //Add player vfx Setting
    [SerializeField] public bool autoLaunch = false;

    public void Awake()
    {
        m_playerAsset = GetComponent<Player_AssetData>(); //Add player vfx Setting
        m_playerInputManager = GetComponent<PlayerInputManager>();
        gameScore = new GameScore(m_playerNumber);
    }
    public void Start()
    {
        if (autoLaunch)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        GetAllDevice();
        RegulatePlayerNumber();
        SpawnPlayers();
        SpawnBall();
    }

    private void SpawnBall()
    {
        GameObject ballGo = GameObject.Instantiate(ball, new Vector3(0, 0, -2.28f), Quaternion.identity);
        BallBehavior behavior = ballGo.GetComponent<BallBehavior>();
        behavior.gameManager = this;
    }

    public void OnDrawGizmos()
    {
        ShowEditorSpawnPosition();
    }

    public void ChangeBallScore(int score)
    {
        generalUI.scoreText.text = score.ToString();
    }

    private void ShowEditorSpawnPosition()
    {
        if (previewSpawnPosition)
        {
            for (int i = 0; i < PLAYER_MAX; i++)
            {
                Gizmos.color = playerColor[i];
                Gizmos.DrawSphere(spawnPosition[i], sphereRadius);
            }
        }
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


    private void RegulatePlayerNumber()
    {
        if (!IsEnoughDevices())
        {
            Debug.LogError("<b> Too much player for devives detected. Correct by system </b>");
            playerToSpwan = devices.Count;
        }
    }
    private bool IsEnoughDevices()
    {
        if (devices.Count < playerToSpwan) return false;
        else return true;
    }



    private void SpawnPlayer(int index)
    {
        int deviceIndex = GetGampad() == -1 ? GetKeyboard() : GetGampad();
        PlayerInput pInput = m_playerInputManager.JoinPlayer(index, -1, null, GetDevice(deviceIndex));
        m_playerAsset.AssignPlayerParameter(index, pInput.gameObject); //Add player vfx Setting
        pInput.transform.position = spawnPosition[index];
        pInput.GetComponent<MeshRenderer>().material.color = playerColor[index];
        pInput.GetComponent<Player.CharacterShoot>().playerUI = generalUI.GetPlayerUI(index);

    }
    public int GetPlayerNumber()
    {
        return m_playerNumber;
    }
    #endregion

    #region Input Devices
    private void GetAllDevice()
    {
        devices.Clear();
        for (int i = 0; i < InputSystem.devices.Count; i++)
        {
            if (InputSystem.devices[i] is Gamepad || InputSystem.devices[i] is Keyboard)
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
        if (index == -1)
        {
            Debug.LogError("No Device available");
            return null;
        }
        InputDevice device = devices[index];
        devices.Remove(device);
        return device;
    }

    #endregion


}
