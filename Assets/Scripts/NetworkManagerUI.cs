using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using System.Threading;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using QFSW.QC;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button crtBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private PrintTool printTool;
    [SerializeField] private TMPro.TMP_InputField textCode;
    [SerializeField] private GameObject loadCircle;
    private Vector3 tmp;
    GameObject codeText;

    private async void Start()
    {

        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
            if (NetworkManager.Singleton.LogLevel == LogLevel.Developer)
            {
                AuthenticationService.Instance.SignedIn += () =>
                {
                    print("Signed in with id: " + AuthenticationService.Instance.PlayerId);
                };
            }
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        textCode.onValueChanged.AddListener((string inp) => { joinBtn.interactable = inp != ""; });
        crtBtn.onClick.AddListener(() =>
        {
            CreateRoom();
        });
        joinBtn.onClick.AddListener(() =>
        {
            JoinRoom(textCode.text);
        });
    }
    private void Update()
    {
        if (loadCircle.GetComponent<Image>().enabled)
        {
            loadCircle.transform.Rotate(Vector3.back, 500 * Time.deltaTime);
        }
        if (NetworkManager.Singleton.IsHost)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count > 1)
            {
                SceneManager.LoadScene("BoardScene");
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            if (NetworkManager.Singleton.IsApproved)
            {
                SceneManager.LoadScene("BoardScene");
            }
        }
    }
    private async void CreateRoom()
    {
        try
        {
            loadCircle.GetComponent<Image>().enabled = true;
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            {
                Destroy(codeText);
                NetworkManager.Singleton.Shutdown();
            }
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            codeText = printTool.std_cout("Your room code:\n" + joinCode + "\nWaiting for your oppenent...", Vector3.zero, Color.white);
            loadCircle.GetComponent<Image>().enabled = false;
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            // SceneManager.LoadScene("BoardScene");
        }
        catch (RelayServiceException e)
        {
            print(e);
        }
    }
    private async void JoinRoom(string joinCode)
    {
        try
        {
            loadCircle.GetComponent<Image>().enabled = true;
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            {
                Destroy(codeText);
                NetworkManager.Singleton.Shutdown();
            }
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            // SceneManager.LoadScene("BoardScene");
        }
        catch (RelayServiceException e)
        {
            printTool.DestroySlowly(printTool.std_cout("Code is invalid or your internet bị mất kết nốI :v", Vector3.zero, Color.red), 10);
        }
    }
    // [Command]
    // void burh()
    // {
    //     print(GameObject.Find(inp));
    // }
}
