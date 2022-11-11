using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class BaseGameManger : MonoBehaviourPunCallbacks
{
    PhotonView photonView;
    public int role;
    playerData data1;
    bool twoPlayers = false;
    public int roleOfRetiredPlayer = -1;
    [SerializeField] GameObject cam1;
    [SerializeField] GameObject cam2;
    [SerializeField] Camera mainCamera;
    public bool inRoom;
    player player1;



    public void DisableCamera()
    {
        if (cam2.activeInHierarchy == true)
        {
            cam1.SetActive(true);
            cam2.SetActive(false);
        }
    }

    public void CheckCamera()
    {
        if (inRoom == false) { return; }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && role==1)
        {
            cam1.SetActive(false);
            cam2.SetActive(true);
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 3 && role==2)
        {
            cam1.SetActive(false);
            cam2.SetActive(true);
        }
    }
    private void Start()
    {
         player1 = FindObjectOfType<player>();

        photonView = GetComponent<PhotonView>();

        AICharacterControl[] cats = FindObjectsOfType<AICharacterControl>();

        playerData[] datas=FindObjectsOfType<playerData>();

        if (PhotonNetwork.CurrentRoom.PlayerCount==2)
        {
            twoPlayers = true;
        }

        foreach (playerData data in datas)
        {
            if (data.photonView.IsMine)
            {
                role = data.GetNumber();
                data1 = data;
                if (twoPlayers)
                {
                    FootActions(role,0);
                    TrapActions(role,1);
                    CatActions(role,1);
                }
                else
                {
                    FootActions(role,0);
                    TrapActions(role,1);
                    CatActions(role,2);
                }
                break;
            }

        }
    }

    private void CatActions(int role, int number )
    {

        if (role != number) {

            mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Cats"));
           
        }
        else
        {
            mainCamera.cullingMask |= 1 << LayerMask.NameToLayer("Cats");
      
        }
    }


    private void TrapActions(int role, int number)
    {
        Traps1[] traps = FindObjectsOfType<Traps1>();

        if (role != number)
        {
            foreach (Traps1 trap in traps)
            {
                ChangeSettings(trap, true);
            }
        }

        else
        {

            foreach (Traps1 trap in traps)
            {
                ChangeSettings(trap, false);

            }

        }
    }

    private static void ChangeSettings(Traps1 trap, bool state)
    {
        GameObject trap1 = trap.gameObject;
        MeshRenderer renderer = trap1.GetComponent<MeshRenderer>();

        if (renderer != null && renderer.enabled == state)
        {
            trap1.GetComponent<MeshRenderer>().enabled = !state;
        }
        else
        {
            foreach (Transform child in trap.transform)
            {
                child.gameObject.SetActive(!state);
            }
        }
    }

    private void FootActions(int role, int number)
    {
        enemyAI[] cats = FindObjectsOfType<enemyAI>();
        if (role != number)
        {
            player1.GetComponent<Collider>().enabled = false;
            player1.GetComponent<Rigidbody>().isKinematic = true;
            player1.GetComponent<ThirdPersonUserControl>().canControl = false;


        }
        else
        {
            player1.GetComponent<Collider>().enabled = true;
            player1.GetComponent<Rigidbody>().isKinematic = false;
            player1.GetComponent<ThirdPersonUserControl>().canControl = true;
            player1.GetOwnership();

            foreach (enemyAI cat in cats)
            {
                cat.GetOwnership();
              
            }
        }
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        List<int> roles = new List<int> { 0, 1, 2 };
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel(0);
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {

            playerData[] datas = FindObjectsOfType<playerData>();
         

            if (PhotonNetwork.IsMasterClient)
            {
                List<Player> players = PhotonNetwork.CurrentRoom.Players.Select(f => f.Value).ToList();
                foreach (playerData obj in datas)
                {

                    Player player = players.FirstOrDefault(f => f.ActorNumber == obj.photonView.CreatorActorNr);
                    if (player == null)
                    {
                        roleOfRetiredPlayer = obj.GetNumber();
                        ChangeRoleOnDat();
                        ChangesRoles();
                        photonView.RPC("UpdateRoleOfRetiredPlayer", RpcTarget.Others, roleOfRetiredPlayer);
                        PhotonNetwork.Destroy(obj.GetComponent<PhotonView>());
                        break;
                    }



                }
            }
    

        }

    }

    private void ChangeRoleOnDat()
    {
        if (roleOfRetiredPlayer == 0 && role == 1)
        {
            role = 0;
        }
        else if (roleOfRetiredPlayer == 0 && role == 2)
        {
            role = 1;
        }
        else if (roleOfRetiredPlayer == 1 && role == 2)
        {
            role = 1;
        }


        data1.SetRoleInGame(role);
    }

    [PunRPC]
    public void UpdateRoleOfRetiredPlayer(int role)
    {
        roleOfRetiredPlayer = role;
        ChangeRoleOnDat();
        ChangesRoles();
    }



    private void ChangesRoles()
    {
     
        FootActions(role, 0);
        TrapActions(role, 1);
        CatActions(role, 1);
        DisableCamera();
        CheckCamera();
        AICharacterControl[] cats = FindObjectsOfType<AICharacterControl>();
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (AICharacterControl cat in cats)
            {
                cat.GetComponent<Animator>().applyRootMotion = true;
                if (inRoom)
                {
                    cat.SetTarget(player1.transform);
                }
            }
        }
    }
}
