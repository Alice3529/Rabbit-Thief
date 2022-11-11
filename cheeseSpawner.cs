using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class cheeseSpawner : MonoBehaviour
{
    [SerializeField] GameObject cheese;
    [SerializeField] int amount;
    int curAm = 0;
    BoxCollider boxCollider;
    float rotate;
    PhotonView photonView;
    List<CheeseInfo> cheeseInfos=new List<CheeseInfo>();
    public static Action turnOffCheese;
    List<int> cheeseObjects = new List<int>();

    struct CheeseInfo
    {
        public Vector3 position;
        public float rotation1;
    }
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        boxCollider = GetComponent<BoxCollider>();
        for (int i=0; i<amount; i++)
        {
            rotate = UnityEngine.Random.Range(0f, 360f);
            float posX = UnityEngine.Random.Range(boxCollider.bounds.min.x, boxCollider.bounds.max.x);
            float posZ = UnityEngine.Random.Range(boxCollider.bounds.min.z, boxCollider.bounds.max.z);
            Vector3 pos = new Vector3(posX, transform.position.y, posZ);

            if (PhotonNetwork.IsMasterClient)
            {
                CreateCheese(pos, rotate);

                Vector3[] positions = cheeseInfos.Select(f => f.position).ToArray();
                float[] rotations = cheeseInfos.Select(f => f.rotation1).ToArray();

                photonView.RPC("UpdateCheeseList", RpcTarget.Others, cheeseObjects.ToArray());
                photonView.RPC("UpdateCheeseInfo", RpcTarget.Others, positions, rotations);


            }
        }

    }

    [PunRPC]

    public void UpdateCheeseList(int[] cheeseObj)
    {
        cheeseObjects = cheeseObj.ToList();
    }

    public void CreateCheese(Vector3 pos, float rotation)
    {
        curAm++;
        GameObject newCheese = PhotonNetwork.Instantiate("cheese", new Vector3(pos.x, transform.position.y, pos.z), Quaternion.Euler(0, rotation, 0));
        cheeseObjects.Add(newCheese.GetComponent<PhotonView>().ViewID);

        CheeseInfo cheeseInfo = new CheeseInfo()
        {
            position = new Vector3(pos.x, transform.position.y, pos.z),
            rotation1 = rotation

        };

        cheeseInfos.Add(cheeseInfo);
        if (curAm == amount)
        {
           // turnOffCheese.Invoke();
            curAm = 0;
        }

    }

    public void DestroyCheese(int id)
    {
        photonView.RPC("DCheese", RpcTarget.MasterClient, id);
       

    }

    [PunRPC]
    public void DCheese(int id)
    {
        foreach (int obj in cheeseObjects)
        {

            if (obj == id)
            {
                PhotonView cheeseObject = PhotonView.Find(id);
                PhotonNetwork.Destroy(cheeseObject);
                cheeseObjects.Remove(obj);
                break;
            }
        }
        photonView.RPC("UpdateCheeseList", RpcTarget.Others, cheeseObjects.ToArray());

    }

    public void RestoreCheese()
    {
        foreach (int obj in cheeseObjects)
        {
            PhotonView cheeseObject = PhotonView.Find(obj);
            PhotonNetwork.Destroy(cheeseObject);
        }
        cheeseObjects.Clear();

        foreach (CheeseInfo info in cheeseInfos)
        {
            GameObject newCheese = PhotonNetwork.Instantiate("cheese", info.position, Quaternion.Euler(0, info.rotation1, 0));
            cheeseObjects.Add(newCheese.GetComponent<PhotonView>().ViewID);

        }

    }

    [PunRPC] 

    public void UpdateCheeseInfo(Vector3[] positions, float[] rotations)
    {
        for (int i=0; i<positions.Length; i++)
        {
            CheeseInfo cheeseInfo = new CheeseInfo()
            {
                position = new Vector3(positions[i].x, transform.position.y, positions[i].z),
                rotation1 = rotations[i],

            };
            cheeseInfos.Add(cheeseInfo);

        }

    }


}
