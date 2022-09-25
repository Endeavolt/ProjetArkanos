using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;


public class PlayerSelectionManager : MonoBehaviour
{
    public bool launchGame = false;
    public int playerNumber = 2;
    public GameObject UiPlayer;
    public GameObject uiPanel;

    private List<InputDevice> devices = new List<InputDevice>();
    private List<InputDevice> devicesAttribut = new List<InputDevice>();

    private PlayerInputManager playerInputManager;
    private PlayerInput[] playerInputs = new PlayerInput[4];
    private ProfilPlayerUI[] playersProfilsUI = new ProfilPlayerUI[4];
    private GameSceneManager sceneManager;

    InputDevice device = null;
    // Start is called before the first frame update
    void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        GetAllDevice();
        GeneratePlayerUI();
        sceneManager = FindObjectOfType<GameSceneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerProfil();
        if (ValidateGameLaunch()) LaunchGame();
       
    }

    private void GetAllDevice()
    {
        devices.Clear();
        for (int i = 0; i < InputSystem.devices.Count; i++)
        {
            if (InputSystem.devices[i] is Gamepad || InputSystem.devices[i] is Keyboard)
                devices.Add(InputSystem.devices[i]);
        }
    }


    private  void UpdatePlayerProfil()
    {
        for (int i = 0; i < devices.Count; i++)
        {
            int test = devices.IndexOf(device);
            if (test!=-1 && !devicesAttribut.Contains(device))
            {
                AddPlayer(test);
                for (int j = devicesAttribut.Count; j < devices.Count + devicesAttribut.Count; j++)
                {
                    if (playerInputs[j].currentControlScheme == "Gamepad" && !IsAvailableGamepad<Gamepad>())
                        playerInputs[j].SwitchCurrentControlScheme( "Keyboard&Mouse", GetDevice<Keyboard>());
                    if (playerInputs[j].currentControlScheme == "Keyboard&Mouse" && !IsAvailableGamepad<Keyboard>())
                        playerInputs[j].SwitchCurrentControlScheme("Gamepad", GetDevice<Gamepad>());
                }
                return;
            }

            
        }

        for (int i = 0; i < devicesAttribut.Count; i++)
        {
            SetPlayerReady(i);
        }
    }
    private bool IsAvailableGamepad<T>()
    {
        for (int k = 0; k < devices.Count; k++)
        {
            if (devices[k] is T) return true;
        }
        return false;
    }

    private InputDevice GetDevice<T>()
    {
        for (int k = 0; k < devices.Count; k++)
        {
            if (devices[k] is T) return devices[k];
        }
        return null;
    }


    private void GetDevicePress(InputAction.CallbackContext ctx)
    {

        device = ctx.control.device;
    }

    private void  RemovePlayer(int index)
    {
        if ( playersProfilsUI[index].profilState == ProfilState.Wait)
        {
            devices.Add(playersProfilsUI[index].playerDevice);
            devicesAttribut.Remove(playersProfilsUI[index].playerDevice);
            playerInputs[index].actions["Return"].started -= ctx => RemovePlayer(index);
            playersProfilsUI[index].ChangeProfilState(ProfilState.None);
            playersProfilsUI[index].ChangePlayerImageColor(Color.red);
            playersProfilsUI[index].ChangePlayerInstruction("Press a touch to add player");
        }
    }

    private int GetPlayerUiIIndex()
    {
        for (int i = 0; i < playersProfilsUI.Length; i++)
        {
            if (playersProfilsUI[i].profilState == ProfilState.None) return i;
        }
        return -1;
    }


    private void AddPlayer(int deviceIndex)
    {
        int playerIndex = GetPlayerUiIIndex();
        playerNumber++;
        devicesAttribut.Add(devices[deviceIndex]);
        playersProfilsUI[playerIndex].playerDevice = devices[deviceIndex];
        playersProfilsUI[playerIndex].ChangeProfilState(ProfilState.Wait);
        playerInputs[playerIndex].actions["Validate"].started -= ctx => GetDevicePress(ctx);
        devices.RemoveAt(deviceIndex);
        playerInputs[playerIndex].actions["Return"].started += ctx => RemovePlayer(playerIndex);

        if (devicesAttribut[devicesAttribut.Count - 1] is Gamepad)
        {
            playerInputs[playerIndex].SwitchCurrentControlScheme("Gamepad", devicesAttribut[devicesAttribut.Count - 1]);
          
            playersProfilsUI[playerIndex].ChangePlayerInstruction("Press A");
        }
        if (devicesAttribut[devicesAttribut.Count-1] is Keyboard)
        {
            playersProfilsUI[playerIndex].ChangePlayerInstruction("Press B");
            playerInputs[playerIndex].SwitchCurrentControlScheme("Keyboard&Mouse", devicesAttribut[devicesAttribut.Count - 1]);
        }
        playersProfilsUI[playerIndex].ChangePlayerImageColor(Color.yellow);
    }

    private void SetPlayerReady(int index)
    {
        if (playerInputs[index].actions["Validate"].triggered && playersProfilsUI[index].profilState == ProfilState.Wait)
        {
            playersProfilsUI[index].ChangeProfilState(ProfilState.Ready);
            playersProfilsUI[index].ChangePlayerImageColor(Color.green);
            playersProfilsUI[index].ChangePlayerInstruction("Press A  or B to launch game");
            return;
        }
    }

    private void GeneratePlayerUI()
    {
        playerNumber = 0;
        playerInputManager.playerPrefab = UiPlayer;
        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i] is Gamepad)
                playerInputs[i] = playerInputManager.JoinPlayer(i, -1, null, devices[i]);
            if (devices[i] is Keyboard)
                playerInputs[i] = playerInputManager.JoinPlayer(i, -1, null, devices[i]);

            playerInputs[i].transform.SetParent(uiPanel.transform);
            playersProfilsUI[i] = playerInputs[i].GetComponent<ProfilPlayerUI>();
            SetPlayerUI(playersProfilsUI[i], i);
           
        }


        for (int i = 0; i < devices.Count; i++)
        {
            playerInputs[i].actions["Validate"].started += ctx => GetDevicePress(ctx);
            playerInputs[i].actions["Validate"].canceled += ctx => device = null;
        }
    }
    private void SetPlayerUI(ProfilPlayerUI playersProfilsUI,int index)
    {
        playersProfilsUI.ChangePlayerName("Player " + (1 + index).ToString());
        playersProfilsUI.ChangeProfilState(ProfilState.None);
        playersProfilsUI.ChangePlayerInstruction("Press a touch to add player");
    }

    bool ValidateGameLaunch()
    {
        bool allReady = IsAllPlayerReady();
        for (int i = 0; i < devicesAttribut.Count; i++)
        {
            if (allReady && playerInputs[i].actions["Validate"].triggered) return true;
            
        }

        return false;
    }

    private bool IsAllPlayerReady()
    {
        bool allReady = true;
        for (int i = 0; i < playerNumber; i++)
        {
            if (playersProfilsUI[i].profilState == ProfilState.Wait)
            {
                allReady = false;
            }
        }

        return allReady;
    }
    void LaunchGame()
    {
        sceneManager.LoadGame(playerNumber);
    }
}
