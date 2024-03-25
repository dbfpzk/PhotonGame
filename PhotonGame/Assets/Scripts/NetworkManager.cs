using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField nicknameInput; //�г��� �Է�â
    public GameObject connectionPanel; //���� â
    public GameObject respawnPanel; //����� â

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        //�ػ󵵸� (960, 540, â���) �� ����
        PhotonNetwork.SendRate = 60;
        //�ʴ� ����̳� ��Ŷ�� �����ؾ��ϴ���
        //�� ���� �����ϸ� SerializationRate�� ���� �����ؾ� ��

        PhotonNetwork.SerializationRate = 30;
        //PhotonView���� OnPhotonSerialize�� �ʴ� ��ȸ ȣ���ϴ���
    }

    // * ButtonEvent
    //���� ���� �õ�
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings(); //������ ���� ���� �õ�
    }

    //���� ���ӽõ��� ȣ��Ǵ� �Լ�
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
        //������ ������ ���� �ִٸ� ����
        //���� ���ٸ� ���� ����
    }

    //�濡 ������ �Ϸ�� ��� ȣ��
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        //�÷��̾��� �г����� ���ӽ� �Է��� �г������� ������ ���

        connectionPanel.SetActive(false); //����â ��Ȱ��ȭ
        StartCoroutine(CoDestroyBullet());
        OnPlayerSpawn();
    }

    IEnumerator CoDestroyBullet()
    {
        yield return new WaitForSeconds(0.2f); //0.2�� ���
        //���̾��Ű�� Bullet�±׸� ���� ������Ʈ�� ��� ã��
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            go.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
            //RPC�Լ��� ȣ��(��� ��Ʈ��ũ ��ü�� �Լ� ȣ��)

        }
    }

    public void OnPlayerSpawn()
    {
        PhotonNetwork.Instantiate("Wizard", new Vector3(0, 5, 0), Quaternion.identity);
        //Resources������ Player��� �̸��� ���� �������� ã�Ƽ� ����
        respawnPanel.SetActive(false); //����� �˾� ��Ȱ��ȭ
    }

    private void Update()
    {
        //������ �������̰� ESCŰ�� �����ٸ�
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect(); //������ ��������
            Application.Quit(0); //���� ����
        }
    }

    //������ ������ ������ ��� ȣ��
    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionPanel.SetActive(true); //����â Ȱ��ȭ
        respawnPanel.SetActive(false); //������â ��Ȱ��ȭ
    }

    [Header("Button")]
    public Button connectionButton;
    public Button respawnButton;

    private void Start()
    {
        connectionButton.onClick.AddListener(() => { Connect(); });
        respawnButton.onClick.AddListener(() => { OnPlayerSpawn(); });
    }
}
