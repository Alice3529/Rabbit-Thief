using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using SFX;
using Photon.Realtime;

public class player : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks, IPunObservable
{
    [SerializeField] int maxCheeseAmount;
    [SerializeField] Transform pointToReestabilish;
    [SerializeField] Quaternion rotation;
    [SerializeField] int curAmount = 0;
    PhotonView photonView;
    [SerializeField] GameObject wall;
    [SerializeField] Transform cameraPoint;
    BaseGameManger gameManager;
    Revival revival;
    CapsuleCollider capsuleCollider;



    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        gameManager = FindObjectOfType<BaseGameManger>();
        revival = GetComponent<Revival>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void SetCheeseAmount()
    {
        maxCheeseAmount = FindObjectsOfType<cheese>().Length;
    }
    public void IncreseCheese()
    {
        curAmount++;
        if (curAmount >= maxCheeseAmount)
        {
            Win();
        }

    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curAmount);
        }
        else
        {
            curAmount=(int)stream.ReceiveNext();
            if (curAmount >= maxCheeseAmount)
            {
                Win();
            }
        }
    }

    private void Win()
    {
          FindObjectOfType<MainGameUI>().ActiveWinForm();
          AICharacterControl[] cats = FindObjectsOfType<AICharacterControl>();
          foreach (AICharacterControl cat in cats)
          {
            Destroy(cat.gameObject);
          }

          cheese[] cheese = FindObjectsOfType<cheese>();
          foreach (cheese cheeseObj in cheese)
          {
            Destroy(cheeseObj);
          }


        transform.GetChild(0).gameObject.SetActive(false);

    }


    public void CatCollide()
    { 
        curAmount = 0;
        GetComponent<ThirdPersonUserControl>().freezeMovement = true;
        transform.position = pointToReestabilish.position;
        transform.rotation = rotation;
        photonView.RPC("LooseCats", RpcTarget.All);

    }

    public void Traps(bool mouseTrap)
    {
        photonView.RPC("InTrap", RpcTarget.Others, mouseTrap);
    }

    [PunRPC]
    public void InTrap(bool mouseTrap)
    {
        if (mouseTrap == true)
        {
            FindObjectOfType<SFXAudio>().MouseTrapClip();
        }
        revival.SetRevival();
    }

    [PunRPC]

    public void LooseCats()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            FindObjectOfType<cheeseSpawner>().RestoreCheese();
        }
        gameManager.DisableCamera();
        gameManager.inRoom = false;
        FindObjectOfType<SFXAudio>().PlayCatMeow();
        revival.SetRevival();
        wall.SetActive(false);
        AICharacterControl[] cats = FindObjectsOfType<AICharacterControl>();
        foreach (AICharacterControl cat in cats)
        {
            cat.SetStartState();
        }
        if (gameManager.role == 0)
        {
            capsuleCollider.radius = 0.3f;
        }

    }

    public void GetOwnership()
    {
        GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.GetComponent<WallCollider>() != null)
        {

            if (PhotonNetwork.IsConnected)
            {
                if (transform.position.z < 33f)
                {
                    cameraPoint.transform.position = new Vector3(cameraPoint.transform.position.x, cameraPoint.transform.position.y, cameraPoint.transform.position.z - 0.3f);
                    photonView.RPC("EnableWall", RpcTarget.All);
                }
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.GetComponent<WallCollider>() != null)
        {
            cameraPoint.transform.position = new Vector3(cameraPoint.transform.position.x, cameraPoint.transform.position.y,cameraPoint.transform.position.z+0.3f);
        }

    }

    [PunRPC]
     
    public void EnableWall()
    {
        wall.SetActive(true);
        AICharacterControl[] cats = FindObjectsOfType<AICharacterControl>();
        gameManager.inRoom = true;
        gameManager.CheckCamera();
        if (gameManager.role==0)
        {
            foreach (AICharacterControl cat in cats)
            {
                cat.SetTarget(this.transform);
                cat.GetComponent<Animator>().applyRootMotion = true;
            }
            capsuleCollider.radius = 0.41f;
        }
        SetCheeseAmount();
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (targetView != base.photonView)
        {
            return;
        }
        base.photonView.TransferOwnership(requestingPlayer);
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

  
}
