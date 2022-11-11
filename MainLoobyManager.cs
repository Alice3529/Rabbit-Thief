using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;

public class MainLoobyManager : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{

    public static MainLoobyManager lobbyManager;
    GameObject loadingCanvas;
    GameObject lobbyCanvas;
    GameObject roomCanvas;
     GameObject chat;
    public static Action<int[], string[]> updateRoomCanvas;

    public static List<string> invitedPlayers = new List<string>();



    public static Action clearRoomCanvas;
    public static Action updateActionsArea;
    public Dictionary<int, string> dict = new Dictionary<int, string>();
    PhotonView photonView;
    Dictionary<string, playerData> playerdict = new Dictionary<string, playerData>();
    playerData data;
    bool shouldJoinRandom = false;
    public bool setAuthorized = false;
    public static Action<string> destroyAllInvintations;



    public void SetFields(GameObject loadingCanvas1, GameObject lobbyCanvas1, GameObject roomCanvas1, GameObject chat1)
    {
        loadingCanvas = loadingCanvas1;
        lobbyCanvas = lobbyCanvas1;
        roomCanvas = roomCanvas1;
        chat = chat1;
    }

    public void LeaveGame()
    {
        photonView.RPC("PlayerLeaveRoom", RpcTarget.MasterClient);
    }

    

    [PunRPC]

    public void PlayerLeaveRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }
    private void Awake()
    {
        Time.timeScale = 1f;
         
        PhotonNetwork.AddCallbackTarget(this);
        InviteUI.OnAcceptNetwork += InintationAccept;

        Roles.changeRole += ChangeRoles;
        photonView = GetComponent<PhotonView>();

        for (int i = 0; i < 3; i++)
        {
            dict.Add(i, "");
        }

    }

    private void OnDestroy()
    {
        InviteUI.OnAcceptNetwork -= InintationAccept;
        Roles.changeRole -= ChangeRoles;
        PhotonNetwork.RemoveCallbackTarget(this);

    }


    public void SetUpPlayers()
    {
        playerData[] datas = FindObjectsOfType<playerData>();

        data = datas.First(f => f.GetComponent<PhotonView>().Owner.NickName==PhotonNetwork.LocalPlayer.NickName);

        FindObjectOfType<MainLoobyUI>().chat.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            int playerNumber = PhotonNetwork.CurrentRoom.Players.Count;
            Dictionary<int, Player> players = PhotonNetwork.CurrentRoom.Players;

           

          dict.Clear();

            foreach (KeyValuePair<int, Player> player in players)
            {
                playerData data1 = datas.First(f => f.GetComponent<PhotonView>().Owner.NickName == player.Value.NickName);
                dict.Add(data1.GetNumber(), data1.GetComponent<PhotonView>().Owner.NickName);
            }


            for (int i = 0; i < 3; i++)
            {
                if (!dict.ContainsKey(i))
                {
                    dict.Add(i, "");
                }

            }

            UpdateRoomDict();
            updateActionsArea?.Invoke();

        }


    }

    private void ChangeRoles(int oldNumber, int newNumber)
    {

        if (PhotonNetwork.IsMasterClient)
        {
            int[] keys = dict.Select(f => f.Key).ToArray();
            string[] values = dict.Select(f => f.Value).ToArray();

            int p = 0;
            int m = 0;

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == oldNumber)

                {
                    p = i;
                }

                if (keys[i] == newNumber)
                {
                    m = i;
                }
            }

            keys[p] = newNumber;
            keys[m] = oldNumber;

            Dictionary<int, string> newDict = new Dictionary<int, string>();


            for (int i = 0; i < keys.Length; i++)
            {
                
                newDict.Add(keys[i], values[i]);
                if (values[i] == PhotonNetwork.MasterClient.NickName)
                {
                    playerData[] datas = FindObjectsOfType<playerData>();


                    if (data == null)
                    {
                        data = datas.First(f => f.GetComponent<PhotonView>().Owner.NickName == PhotonNetwork.LocalPlayer.NickName);
                    }

                    data.SetRole(keys[i]);

                }
            }

            dict = newDict;
            UpdateRoomDict();


        }

    }


    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        photonView = GetComponent<PhotonView>();
    }

    private void InintationAccept(string roomName)
    {
        PlayerPrefs.SetString("ROOM", roomName);
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.InLobby)
            {
                JoinPlayerRoom();
            }
        }
    }

    private void JoinPlayerRoom()
    {
        string roomName = PlayerPrefs.GetString("ROOM");
        PlayerPrefs.SetString("ROOM", "");
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        GameObject newNotificationLabel = Instantiate(FindObjectOfType<MainLoobyUI>().invintation, lobbyCanvas.transform);
        newNotificationLabel.GetComponent<RectTransform>().localPosition = new Vector2(lobbyCanvas.GetComponent<RectTransform>().sizeDelta.x / 2 - newNotificationLabel.GetComponent<RectTransform>().sizeDelta.x / 2, 0);
        newNotificationLabel.GetComponentInChildren<Label>().SetLabel($"Unable to accept invitation. The number of players in this room is maximum.");
        JoinLobby();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 3; i++)
            {

                if (dict[i]=="")
                {
                    dict[i] = newPlayer.NickName;
                    break;
                }
            }
            UpdateRoomDict();
            updateActionsArea?.Invoke();
        }


    }

    private void UpdateRoomDict()
    {
        int[] keys = dict.Select(f => f.Key).ToArray();
        string[] values = dict.Select(f => f.Value).ToArray();
        photonView.RPC("UpdateDictionaries", RpcTarget.Others, keys, values);
        UpdateInformation();
    }

    [PunRPC]

    public void UpdateDictionaries(int[] keys, string[] values)
    {
        Dictionary<int, string> newDict = new Dictionary<int, string>();
        List<playerData> datas = FindObjectsOfType<playerData>().ToList();


        for (int i = 0; i < keys.Length; i++)
        {
            newDict.Add(keys[i], values[i]);
            if (PhotonNetwork.LocalPlayer.NickName == values[i])
            {
                data.SetRole(keys[i]);
            }
        
        }
        dict = newDict;

    }


    public void UpdateInformation()
    {
        int[] keys1 = new int[3] { -1, -1, -1 };
        string[] values = new string[3] { "", "", ""};
        int m = 0;

        foreach (KeyValuePair<int, string> d in dict)
        {
            if (!string.IsNullOrEmpty(d.Value))
            {
                keys1[m] = d.Key;
                values[m] = d.Value;
            }
            else
            {
                keys1[m] = -1;
                values[m] = "";
            }
            m++;
        }


        updateRoomCanvas.Invoke(keys1, values);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

       
        if (PhotonNetwork.IsMasterClient)
        {
            playerData[] datas = FindObjectsOfType<playerData>();

            List<Player> players = PhotonNetwork.CurrentRoom.Players.Select(f => f.Value).ToList();
            foreach (playerData obj in datas)
            {

                Player player = players.FirstOrDefault(f => f.ActorNumber == obj.photonView.CreatorActorNr);
                if (player == null)
                {
                    PhotonNetwork.Destroy(obj.GetComponent<PhotonView>());
                    break;
                }



            }


            foreach (KeyValuePair<int, string> pair in dict)
            {
                if (string.Equals(pair.Value, otherPlayer.NickName))
                {
                    dict[pair.Key] = "";
                    break;
                }
            }


            ConvertInRoomTwoPlayers();

            int[] keys = dict.Select(f => f.Key).ToArray();
            string[] values = dict.Select(f => f.Value).ToArray();

            UpdateRoomDict();
            updateActionsArea?.Invoke();


        }


    }


    private void ConvertInRoomTwoPlayers()
    {
        Dictionary<int, string> newDict = new Dictionary<int, string>();

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            int i = 0;
            foreach (KeyValuePair<int, string> pair in dict)
            {
                if (dict[pair.Key] != "")
                {
                    if (dict[pair.Key] == PhotonNetwork.LocalPlayer.NickName)
                    {
                        data.SetRole(i);
                    }
                    newDict.Add(i, pair.Value);
                    i++;
                }

            }
            newDict.Add(i, "");

            dict = newDict;
        }
    }

 

    public void ConnectToMaster()
    {      
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        JoinLobby();

    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        loadingCanvas.SetActive(false);
        lobbyCanvas.SetActive(true);
        FindObjectOfType<MainLoobyUI>().chat.SetActive(true);

        FindObjectOfType<PlayfabFriendSystem>().FriendList();

        string roomName = PlayerPrefs.GetString("ROOM");

        if (!string.IsNullOrEmpty(roomName))
        {
            JoinPlayerRoom();
        }
        else
        {
            CreateRoom();

        }
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 3;
        roomOptions.PublishUserId = true;
        roomOptions.CleanupCacheOnLeave = false;
        string roomName = System.Guid.NewGuid().ToString();
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }



    public override void OnJoinedRoom()
    {
        GameObject player = PhotonNetwork.Instantiate("PlayerData", Vector3.zero, Quaternion.identity);
        data = player.GetComponent<playerData>();
        data.SetName(PhotonNetwork.LocalPlayer.NickName);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            dict[0] = PhotonNetwork.LocalPlayer.NickName;
        }
    }

    public override void OnLeftRoom()
    {
        foreach (string player in invitedPlayers)
        {
            destroyAllInvintations?.Invoke(player);
        }
        invitedPlayers.Clear();
        clearRoomCanvas.Invoke();
        ClearDict();
    }

    public void ClearDict()
    {
        dict.Clear();
        for (int i = 0; i < 3; i++)
        {
            dict.Add(i, "");
        }
        dict[0] = PhotonNetwork.LocalPlayer.NickName;

    }


    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (targetView != base.photonView)
        {
            return;
        }
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
       
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (targetView != base.photonView)
        {
            return;
        }
        base.photonView.TransferOwnership(requestingPlayer);

    }
}



