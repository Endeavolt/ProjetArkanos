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
    public int playerActiveNumber = 2;
    private int playerNumber = 0;
    public GameObject UiPlayer;
    public GameObject uiPanel;

    private List<InputDevice> devices = new List<InputDevice>();
    private List<InputDevice> devicesAttribut = new List<InputDevice>();

    private PlayerInputManager playerInputManager;
    private PlayerInput[] playerInputs = new PlayerInput[4];
    private ProfilPlayerUI[] playersProfilsUI = new ProfilPlayerUI[4];
    private GameSceneManager sceneManager;
    private bool isInputLaunchGame = false;
    private bool isInputFrameFree = false;

    InputDevice device = null;
    // Start is called before the first frame update
    void Awake()
    {

        playerInputManager = GetComponent<PlayerInputManager>();
        GetAllDevice();
        GeneratePlayerUI();
        sceneManager = FindObjectOfType<GameSceneManager>();


    }

    private void Start()
    {
        InputSystem.onDeviceChange += (device, change) => AddGameDevice(device, change);
        InputSystem.onDeviceChange += (device, change) => RemoveGameDevice(device, change);
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= (device, change) => AddGameDevice(device, change);
        InputSystem.onDeviceChange -= (device, change) => RemoveGameDevice(device, change);
    }

    // Update is called once per frame
    void Update()
    {


        isInputFrameFree = true;
        UpdatePlayerProfil();
        if (ValidateGameLaunch() && isInputLaunchGame && playerActiveNumber > 1) LaunchGame();

        isInputLaunchGame = false;
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


    private void UpdatePlayerProfil()
    {
        for (int i = 0; i < devices.Count; i++)
        {
            int test = devices.IndexOf(device);
            if (test != -1 && !devicesAttribut.Contains(device))
            {
                AddPlayer(test);
                UpdateFreeDeviceControlScheme();
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

    private void RemovePlayer(int index)
    {
        if (playersProfilsUI[index].profilState == ProfilState.Wait && isInputFrameFree)
        {
            isInputFrameFree = false;
            playerActiveNumber--;
            devices.Add(playersProfilsUI[index].playerDevice);
            devicesAttribut.Remove(playersProfilsUI[index].playerDevice);
            playerInputs[index].actions["Return"].started -= ctx => RemovePlayer(index);
            playersProfilsUI[index].ChangerUiState(ProfilState.None, Color.red, "Press a touch to add player");
            playersProfilsUI[index].SeActiveUI(false);
        }
    }

    private void UnreadyPlayer(int index)
    {
        if (playersProfilsUI[index].profilState == ProfilState.Ready && isInputFrameFree)
        {
            isInputFrameFree = false;
            playersProfilsUI[index].ChangerUiState(ProfilState.Wait, Color.yellow);
            playerInputs[index].actions["Return"].started -= ctx => UnreadyPlayer(index);
            if (devicesAttribut[devicesAttribut.Count - 1] is Gamepad)
            {
                playersProfilsUI[index].ChangePlayerInstruction("Press A");
            }
            if (devicesAttribut[devicesAttribut.Count - 1] is Keyboard)
            {
                playersProfilsUI[index].ChangePlayerInstruction("Press B");
            }
            playerInputs[index].actions["Return"].started += ctx => RemovePlayer(index);
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
        playerActiveNumber++;

        playersProfilsUI[playerIndex].SeActiveUI(true);
        playersProfilsUI[playerIndex].playerDevice = devices[deviceIndex];
        playersProfilsUI[playerIndex].ChangerUiState(ProfilState.Wait, Color.yellow);

        playerInputs[playerIndex].actions["Validate"].started -= ctx => GetDevicePress(ctx);
        playerInputs[playerIndex].actions["Validate"].canceled -= ctx => device = null;
        playerInputs[playerIndex].actions["Return"].started += ctx => RemovePlayer(playerIndex);


        devicesAttribut.Add(devices[deviceIndex]);
        devices.RemoveAt(deviceIndex);

        if (devicesAttribut[devicesAttribut.Count - 1] is Gamepad)
        {
            playerInputs[playerIndex].SwitchCurrentControlScheme("Gamepad", devicesAttribut[devicesAttribut.Count - 1]);
            playersProfilsUI[playerIndex].ChangePlayerInstruction("Press A");
        }
        if (devicesAttribut[devicesAttribut.Count - 1] is Keyboard)
        {
            playersProfilsUI[playerIndex].ChangePlayerInstruction("Press B");
            playerInputs[playerIndex].SwitchCurrentControlScheme("Keyboard&Mouse", devicesAttribut[devicesAttribut.Count - 1]);
        }
    }

    private void SetPlayerReady(int index)
    {
        if (playerInputs[index].actions["Validate"].triggered && playersProfilsUI[index].profilState == ProfilState.Wait)
        {
            playersProfilsUI[index].ChangerUiState(ProfilState.Ready, Color.green, "Press A  or B to launch game");
            playerInputs[index].actions["Validate"].started += ctx => isInputLaunchGame = true;
            playerInputs[index].actions["Return"].started += ctx => UnreadyPlayer(index);
            return;
        }
    }

    private void GeneratePlayerUI()
    {

        playerActiveNumber = 0;
        playerInputManager.playerPrefab = UiPlayer;
        for (int i = 0; i < devices.Count && i < 4; i++)
        {
            if (devices[i] is Gamepad)
                playerInputs[i] = playerInputManager.JoinPlayer(i, -1, null, devices[i]);
            if (devices[i] is Keyboard)
                playerInputs[i] = playerInputManager.JoinPlayer(i, -1, null, devices[i]);
            playerNumber++;
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
    private void SetPlayerUI(ProfilPlayerUI playersProfilsUI, int index)
    {
        playersProfilsUI.ChangerUiState(ProfilState.None, Color.red, "Press a touch to add player", "Player " + (1 + index).ToString());
        playersProfilsUI.SeActiveUI(false);
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
        for (int i = 0; i < playerActiveNumber; i++)
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
        sceneManager.LoadGame(playerActiveNumber);
    }

    #region Device Gestion

    private void AddGameDevice(InputDevice device, InputDeviceChange state)
    {
        if (state == InputDeviceChange.Added)
        {
            if (device is Gamepad || device is Keyboard)
            {
                InputSystem.AddDevice(device);
                devices.Add(device);
               
            }

            if (playerNumber < 4)
            {
                if (device is Gamepad)
                    playerInputs[playerNumber] = playerInputManager.JoinPlayer(playerNumber, -1, null, device);
                if (device is Keyboard)
                    playerInputs[playerNumber] = playerInputManager.JoinPlayer(playerNumber, -1, null, device);
                playerInputs[playerNumber].transform.SetParent(uiPanel.transform);
                playersProfilsUI[playerNumber] = playerInputs[playerNumber].GetComponent<ProfilPlayerUI>();
                SetPlayerUI(playersProfilsUI[playerNumber], playerNumber);
                playerInputs[playerNumber].actions["Validate"].started += ctx => GetDevicePress(ctx);
                playerInputs[playerNumber].actions["Validate"].canceled += ctx => device = null;
                playerNumber++;
            }


        }
    }
    private void RemoveGameDevice(InputDevice device, InputDeviceChange state)
    {
        if (state == InputDeviceChange.Removed)
        {
            if (this.devices.Contains(device))
            {
                this.devices.Remove(device);
                InputSystem.RemoveDevice(device);
            }
            int index = devicesAttribut.IndexOf(device);
            if (devicesAttribut.Contains(device))
            {
                devicesAttribut.Remove(device);
                playerActiveNumber--;
                InputSystem.RemoveDevice(device);
            }

            if (Application.isPlaying)
            {
                if (this.devices.Count < 4)
                {
                    playerNumber--;
                    Destroy(playerInputs[index].gameObject);
                    playerInputs[index] = null;
                    playersProfilsUI[index] = null;
                }
            }

        }
    }
    private void UpdateFreeDeviceControlScheme()
    {
        for (int j = devicesAttribut.Count; j < devices.Count + devicesAttribut.Count; j++)
        {
            if (playerInputs[j].currentControlScheme == "Gamepad" && !IsAvailableGamepad<Gamepad>())
                playerInputs[j].SwitchCurrentControlScheme("Keyboard&Mouse", GetDevice<Keyboard>());
            if (playerInputs[j].currentControlScheme == "Keyboard&Mouse" && !IsAvailableGamepad<Keyboard>())
                playerInputs[j].SwitchCurrentControlScheme("Gamepad", GetDevice<Gamepad>());
        }
    }
    #endregion
}
