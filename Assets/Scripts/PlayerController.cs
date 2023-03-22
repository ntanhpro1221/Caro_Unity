using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] AudioSource dropChessSound;
    [SerializeField] AudioSource winSound;
    [SerializeField] AudioSource loseSound;
    GameObject chooseCell;
    PrintTool printTool;
    UIController uIController;
    SpriteRenderer imgChooseCell;
    Transform camTrans;
    Camera cam;
    Vector2 pixelPerfectChoose;
    [SerializeField] GameObject x;
    [SerializeField] GameObject o;
    GameObject tmp;
    Vector2 tmpVec2;
    bool forLoadBoardScene = false;
    enum Status
    {
        nope,
        holding,
        dragged,
        multi
    }
    Status curStatus = Status.nope;
    bool IsOverUI(Vector2 coord)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = coord;
        List<RaycastResult> cnt = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, cnt);
        return cnt.Count > 0;
    }
    public override void OnNetworkSpawn()
    {
        Object.DontDestroyOnLoad(gameObject);
    }
    [ServerRpc]
    void drawServerRpc(Vector2 coord)
    {
        dropChessSound.Play();
        tmp = Instantiate(o);
        Hub.cello.Add(coord);
        tmp.transform.position = coord;
        Hub.hostTurn = true;
    }
    [ClientRpc]
    void drawClientRpc(Vector2 coord)
    {
        dropChessSound.Play();
        tmp = Instantiate(x);
        Hub.cellx.Add(coord);
        tmp.transform.position = coord;
        Hub.hostTurn = false;
    }
    [ClientRpc]
    void resultHandleClientRpc(bool HostIsWinner)
    {
        uIController.EndGameUI(IsHost == HostIsWinner);
        if (IsHost == HostIsWinner)
            Invoke("PlayWinSound", 1);
        else
            Invoke("PlayLoseSound", 1);
        Hub.win = true;
    }
    [ServerRpc]
    void resultHandleServerRpc(bool HostIsWinner)
    {
        resultHandleClientRpc(HostIsWinner);
    }
    void PlayWinSound()
    {
        winSound.Play();
    }
    void PlayLoseSound()
    {
        loseSound.Play();
    }
    void checkWinHost(Vector2 delta)
    {
        if (Hub.win)
            return;
        int cnt = 0;
        tmpVec2 = pixelPerfectChoose;
        for (tmpVec2 = pixelPerfectChoose + delta; Hub.cellx.Contains(tmpVec2); tmpVec2 += delta)
            cnt++;
        for (tmpVec2 = pixelPerfectChoose - delta; Hub.cellx.Contains(tmpVec2); tmpVec2 -= delta)
            cnt++;
        Hub.win = cnt >= 4;
    }
    void checkWinClient(Vector2 delta)
    {
        if (Hub.win)
            return;
        int cnt = 0;
        tmpVec2 = pixelPerfectChoose;
        for (tmpVec2 = pixelPerfectChoose + delta; Hub.cello.Contains(tmpVec2); tmpVec2 += delta)
            cnt++;
        for (tmpVec2 = pixelPerfectChoose - delta; Hub.cello.Contains(tmpVec2); tmpVec2 -= delta)
            cnt++;
        Hub.win = cnt >= 4;
    }
    void click()
    {
        if (Hub.cello.Contains(pixelPerfectChoose) || Hub.cellx.Contains(pixelPerfectChoose) || Hub.win)
            return;
        if (IsHost != Hub.hostTurn)
        {
            printTool.DestroySlowly(printTool.std_cout("NOT YOUR TURN!", Vector3.zero, Color.red), 5);
            return;
        }
        if (NetworkManager.Singleton.IsHost)
        {
            Hub.hostTurn = false;
            drawClientRpc(pixelPerfectChoose);
            checkWinHost(new Vector2(1, 0));
            checkWinHost(new Vector2(0, 1));
            checkWinHost(new Vector2(1, 1));
            checkWinHost(new Vector2(1, -1));
            if (Hub.win)
                resultHandleClientRpc(true);
        }
        else
        {
            dropChessSound.Play();
            Hub.hostTurn = true;
            tmp = Instantiate(o);
            Hub.cello.Add(pixelPerfectChoose);
            tmp.transform.position = pixelPerfectChoose;
            drawServerRpc(pixelPerfectChoose);
            checkWinClient(new Vector2(1, 0));
            checkWinClient(new Vector2(0, 1));
            checkWinClient(new Vector2(1, 1));
            checkWinClient(new Vector2(1, -1));
            if (Hub.win)
                resultHandleServerRpc(false);
        }
    }
    void choose()
    {
        pixelPerfectChoose = cam.ScreenToWorldPoint(Input.touches[0].position);
        chooseCell.transform.position = pixelPerfectChoose = new Vector2((int)pixelPerfectChoose.x + ((pixelPerfectChoose.x > 0) ? 0.5f : -0.5f), (int)pixelPerfectChoose.y + ((pixelPerfectChoose.y > 0) ? 0.5f : -0.5f));
        imgChooseCell.enabled = true;
    }
    void unchoose()
    {
        imgChooseCell.enabled = false;
    }
    void drag()
    {
        camTrans.Translate(cam.ScreenToWorldPoint(Vector3.zero) - cam.ScreenToWorldPoint(Input.touches[0].deltaPosition));
    }
    void zoom()
    {
        cam.orthographicSize *= Vector2.Distance(Input.touches[0].position - Input.touches[0].deltaPosition, Input.touches[1].position - Input.touches[1].deltaPosition) / Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
    }
    void change(Status newStatus)
    {
        curStatus = newStatus;
    }
    bool compPhase(int ind, TouchPhase targetPhase)
    {
        return Input.touches[ind].phase == targetPhase;
    }
    void ForLoadBoardScene()
    {
        cam = Camera.main;
        camTrans = cam.transform;
        chooseCell = GameObject.Find("chooseCell");
        imgChooseCell = chooseCell.GetComponent<SpriteRenderer>();
        imgChooseCell.enabled = false;
        uIController = GameObject.Find("Canvas").GetComponent<UIController>();
        GameObject.Find("Quit").GetComponent<Button>().onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Hub.cello.Clear();
            Hub.cellx.Clear();
            SceneManager.LoadScene("MenuScene");
        });
        GameObject.Find("Retry").GetComponent<Button>().onClick.AddListener(() =>
        {
            Hub.cello.Clear();
            Hub.cellx.Clear();
            SceneManager.LoadScene("BoardScene");
        });
        Hub.win = false;
        Hub.hostTurn = true;
        printTool = GameObject.Find("PrintTool").GetComponent<PrintTool>();
    }
    void Update()
    {
        if (SceneManager.GetActiveScene().name != "BoardScene")
            return;
        if (cam == null)
            ForLoadBoardScene();
        if (!IsOwner)
            return;
        if (Input.touches.Length > 0 && IsOverUI(Input.touches[0].position))
            return;
        switch (curStatus)
        {
            case Status.nope:
                if (Input.touches.Length > 1)
                    change(Status.multi);
                else if (Input.touches.Length > 0)
                {
                    change(Status.holding);
                    choose();
                }
                break;
            case Status.holding:
                if (Input.touches.Length > 1)
                {
                    change(Status.multi);
                    unchoose();
                }
                else if (compPhase(0, TouchPhase.Ended))
                {
                    change(Status.nope);
                    click();
                    unchoose();
                }
                else if (compPhase(0, TouchPhase.Moved))
                {
                    change(Status.dragged);
                    drag();
                    unchoose();
                }
                break;
            case Status.dragged:
                if (Input.touches.Length > 1)
                    change(Status.multi);
                else if (compPhase(0, TouchPhase.Moved))
                    drag();
                else if (compPhase(0, TouchPhase.Ended))
                    change(Status.nope);
                break;
            case Status.multi:
                if (Input.touches.Length < 1)
                    change(Status.nope);
                else if (Input.touches.Length < 2)
                    change(Status.dragged);
                else
                    zoom();
                break;
        }
    }
}
