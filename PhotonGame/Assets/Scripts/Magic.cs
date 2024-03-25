using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Magic : MonoBehaviourPunCallbacks
{
    public PhotonView phoView;

    int dir; //총알의 방향

    void Start()
    {
        phoView = GetComponent<PhotonView>();
        phoView.RPC("DestroyWaitRPC", RpcTarget.All);
    }
    
    void Update()
    {
        transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Ground"))
        {
            phoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
        //총알이 내것이 아니고 플레이어이고 플레이어가 내것이라면
        //즉 내가 쏜 총알이 아니라면
        if(!phoView.IsMine &&
            collision.CompareTag("Player") &&
            collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<WizardController>().Hit();
            phoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }


    [PunRPC] //RPC함수로 만들기 위해 마킹해주어야 함
    void DestroyWaitRPC() => Destroy(gameObject, 3.0f);

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    [PunRPC]
    void DirRPC(int _dir) => dir = _dir;
}
