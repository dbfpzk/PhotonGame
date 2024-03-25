using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField nicknameInput; //닉네임 입력창
    public GameObject connectionPanel; //연결 창
    public GameObject respawnPanel; //재시작 창

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        //해상도를 (960, 540, 창모드) 로 변경
        PhotonNetwork.SendRate = 60;
        //초당 몇번이나 패킷을 전송해야하는지
        //이 값을 변경하면 SerializationRate의 값도 변경해야 함

        PhotonNetwork.SerializationRate = 30;
        //PhotonView들이 OnPhotonSerialize를 초당 몇회 호출하는지
    }

    // * ButtonEvent
    //서버 접속 시도
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings(); //마스터 서버 접속 시도
    }

    //서버 접속시도시 호출되는 함수
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
        //마스터 서버에 방이 있다면 참가
        //방이 없다면 방을 만듬
    }

    //방에 참여가 완료된 경우 호출
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        //플레이어의 닉네임을 접속시 입력한 닉네임으로 서버에 등록

        connectionPanel.SetActive(false); //접속창 비활성화
        StartCoroutine(CoDestroyBullet());
        OnPlayerSpawn();
    }

    IEnumerator CoDestroyBullet()
    {
        yield return new WaitForSeconds(0.2f); //0.2초 대기
        //하이어라키에 Bullet태그를 가진 오브젝트를 모두 찾음
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            go.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
            //RPC함수를 호출(모든 네트워크 객체에 함수 호출)

        }
    }

    public void OnPlayerSpawn()
    {
        PhotonNetwork.Instantiate("Wizard", new Vector3(0, 5, 0), Quaternion.identity);
        //Resources폴더에 Player라는 이름을 가진 프리팹을 찾아서 생성
        respawnPanel.SetActive(false); //재시작 팝업 비활성화
    }

    private void Update()
    {
        //서버와 연결중이고 ESC키를 눌렀다면
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect(); //서버와 연결해제
            Application.Quit(0); //게임 종료
        }
    }

    //서버와 연결이 해제된 경우 호출
    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionPanel.SetActive(true); //연결창 활성화
        respawnPanel.SetActive(false); //리스폰창 비활성화
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
