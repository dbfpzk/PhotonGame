using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Magic : MonoBehaviourPunCallbacks
{
    public PhotonView phoView;

    int dir; //�Ѿ��� ����

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
        //�Ѿ��� ������ �ƴϰ� �÷��̾��̰� �÷��̾ �����̶��
        //�� ���� �� �Ѿ��� �ƴ϶��
        if(!phoView.IsMine &&
            collision.CompareTag("Player") &&
            collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<WizardController>().Hit();
            phoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }


    [PunRPC] //RPC�Լ��� ����� ���� ��ŷ���־�� ��
    void DestroyWaitRPC() => Destroy(gameObject, 3.0f);

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    [PunRPC]
    void DirRPC(int _dir) => dir = _dir;
}
