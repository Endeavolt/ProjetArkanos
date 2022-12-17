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

    private float m_gameTimer;
    private float m_gameTime = 3;
    private bool m_gameIsLaunch = false;
    private PlayerInputManager m_playerInputManager;
    private int m_playerNumber = 4;
    private int m_currentPlayerNumber;
    private PlayerInput[] players = new PlayerInput[PLAYER_MAX];
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

    public void Update()
    {
        if (!m_gameIsLaunch)
        {
            if (m_gameTimer > m_gameTime)
            {
                LaunchGame();
                generalUI.SetTextStartTimer(m_gameTime - m_gameTimer, false);
                m_gameIsLaunch = true;
            }
            else
            {
                m_gameTimer += Time.deltaTime;
                generalUI.SetTextStartTimer(m_gameTime - m_gameTimer, true);

            }
        }
    }

    private void LaunchGame()
    {
        SpawnBall();
        for (int i = 0; i < m_currentPlayerNumber; i++)
        {
            Player.CharacterControlsInterface[] components = players[i].GetComponents<Player.CharacterControlsInterface>();
            for (int j = 0; j < components.Length; j++)
            {
                components[j].ChangeControl(true);
            }
        }
    }

    public void StartGame()
    {
        GetAllDevice();
        RegulatePlayerNumber();
        SpawnPlayers();
    }

    private void SpawnBall()
    {
        GameObject ballGo = GameObject.Instantiate(ball, new Vector3(0, 0, -2.28f), Quaternion.identity);
        BallBehavior behavior = ballGo.GetComponent<BallBehavior>();
        GetComponent<General.HitScanStrikeManager>().ball = behavior;
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
        players[index] = pInput;
        m_playerAsset.AssignPlayerParameter(index, pInput.gameObject); //Add player vfx Setting
        pInput.transform.position = spawnPosition[index];
        pInput.GetComponent<MeshRenderer>().material.color = playerColor[index];
        pInput.GetComponent<Player.CharacterShoot>().playerUI = generalUI.GetPlayerUI(index);
        pInput.GetComponent<Player.CharacterMouvement>().hitScanStrikeManager = GetComponent<General.HitScanStrikeManager>();
        pInput.GetComponent<Player.CharacterJump>().hitScanStrikeManager = GetComponent<General.HitScanStrikeManager>();

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
