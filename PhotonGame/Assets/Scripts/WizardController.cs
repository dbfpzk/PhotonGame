using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; //추가
using UnityEngine.UI; //추가

public class WizardController : MonoBehaviourPunCallbacks, IPunObservable
{
    // * Network객체
    //- using Photon.Pun을 하고
    //- MonoBehaviourPunCallbacks를 상속하면 Network객체가 됨
    //- 즉 포톤과 관련된 명령어를 사용할 수 있게 된다.

    // * PhotonView
    //- 포톤에서 네트워크객체를 동기화 시켜주기 위해 필요한 컴포넌트
    //- 네트워크객체의 행동은 모두 PhotonView를 통해서 관찰 후 동기화 됨
    //- A객체가 이동했을때 다른 세계(다른 컴퓨터)에 있는 A객체의 클론들은
    //- PhotonView를 통해서 이동명령을 받게 됨

    // * PhotonView - Syncronization (동기화 옵션)
    //- 어떻게 동기화 할 것인지 정하는 옵션
    //- off : RPC만 사용하는 경우(동기화 안함)
    //- Reliable Delta Compressed : 받은 데이터를 비교해 같으면 보내지 않음
    //- Unreliable : 계속 데이터를 보냄(손실 가능성 있음)
    //- Unreliable OnChange : 변경이 있을때 계속 보냄
    //- *Off이외에는 observed Component에 Component가 1개라도 등록 되어 있어야 함

    // * PhotonView - Observed Components
    //- 동기화 할 컴포넌트를 넣는 곳
    //- 즉, 컴포넌트의 변경된 값을 관찰해서 다른 클론들에게 보낸다.

    [Header("Component")]
    public Rigidbody2D rig2D;
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public PhotonView phoView;

    [Header("UI")]
    public Text nickNameText;
    public Image healthImage;

    [Header("FirePos")]
    public Transform firePos;

    //State
    bool isGround; //땅에 닿았는지 여부
    Vector3 curPos; //현재 위치
    int currentHp = 10; //현재 체력
    int maxHp = 10; //최대 체력

    void Awake()
    {
        rig2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        phoView = GetComponent<PhotonView>();
    }

    void Start()
    {
        //phoView.IsMine : 내가 생성했으면 true, 아니면 false
        //즉 true라면 로컬, false라면 리모트
        //리모트 : 화면에서 표시되는 상대의 Clone(복제품)
        //로컬 오브젝트 : 주도권이 나에게 있음
        //리모트 오브젝트 : 주도권이 네트워크 너머 타인에게 있음

        nickNameText.text = phoView.IsMine ? PhotonNetwork.NickName : phoView.Owner.NickName;
        nickNameText.color = phoView.IsMine ? Color.blue : Color.red;
        //내꺼라면 닉네임 파란색, 상대라면 빨간색
    }
    
    void Update()
    {
        //내꺼만 조종하기 위해 내꺼인지 체크
        if(phoView.IsMine)
        {
            Move();
            Jump();
            Shot();
        }
        //상대오브젝트이고 서버와의 거리가 10이상 차이 난다면
        else if((transform.position - curPos).sqrMagnitude >= 100)
        {
            transform.position = curPos;
            //현재위치와 서버와의 위치로 맞춰줌
        }
        else //차이가 10 이하라면
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            //선형보간으로 자연스럽게 이동시켜줌
        }
    }

    void Move()
    {
        float axis = Input.GetAxisRaw("Horizontal");
        rig2D.velocity = new Vector2(4 * axis, rig2D.velocity.y);
        //rig2D.velocity : 속도를 변경함
        if(axis != 0) //좌우 입력값이 있다면
        {
            anim.SetBool("Run", true); //이동 애니메이션
            phoView.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);

            // * PhotonView.RPC
            //- 같은 룸에 접속에 있는 원격 클라이언트에 메소드를 호출시킴
            //- 즉, 다른 컴퓨터에 있는 내 클론들에게 함수를 호출시켜줌
            //- RPC호출(원격 호출)을 하려면 호출하는 함수에 [PunRPC]마킹 해야 함

            // * RpcTarget
            //1. All : 자신은 함수를 바로 실행하고 다른 모두에게 전달(자신이 빠름)
            //2. AllBuffered : All은 호출하는 시기에 통신하고 사라짐
            //- 해당 리모트를 사용해 버퍼에 남겨두면 방에 늦게 들어온 사람에게도 전달.
            //- 총알 같이 사라져야 하는 것들은 AllBuffered를 사용한다.
            //3. AllViaServer : 모두에게 서버를 거쳐서 동시 호출
            //4. MasterClient : 방장에게만 전달
            //5. Others : 나를 제외한 모두에게 호출
            //6. OthersBuffered : 나를 제외한 모두에게 버퍼와 함께 전달
        }
        else //좌우이동중이 아니라면
        {
            anim.SetBool("Run", false);
        }

    }
    void Jump()
    {
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -1.3f), 0.07f, LayerMask.GetMask("Ground"));
        //Ground레이어와 충돌을 감지
        anim.SetBool("Jump", !isGround);
        if(Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            phoView.RPC("JumpRPC", RpcTarget.All);
        }
    }
    void Shot()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GameObject magic = PhotonNetwork.Instantiate("Bullet", firePos.position, Quaternion.identity);
            magic.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, spriteRenderer.flipX ? -1 : 1);
            anim.SetTrigger("Attack");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black; //기즈모 색상
        Gizmos.DrawSphere((Vector2)transform.position + new Vector2(0, -1.3f), 0.07f);
    }

    public void Hit()
    {
        currentHp--;
        healthImage.fillAmount = (float)currentHp / maxHp;
        if(currentHp <= 0)
        {
            GameObject.Find("RespawnPanel").SetActive(true);
            phoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    //주기적으로 서버에 의해 호출되는 함수
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // * OnPhotonSerializeView
        //- 서버에 의하여 주기적으로 호출되는 함수
        //- PhotonView의 동기화 데이터를 읽고 쓸 수 있음
        //- 이 함수를 사용하기 위해선 PhotonStream이 필수이다.
        //- PhotonView에 의해 관찰되고 있는 스크립트에서만 호출됨
        //- PhotonView 컴포넌트를 사용하여 동기화를 구현할 모든 컴포넌트는
        //  IPunObserable 인터페이스를 상속하고 OnPhotonSerialzeView 함수를
        //  구현 해야 한다.

        // * PhotonStream
        //- 현재 클라이언트에서 다른 클라이언트로 보낼 값을 쓰거나
        //- 다른 클라이언트가 보내온 값을 읽을때 사용할
        //- 스트림형태의 데이터 컨테이너

        // * PhotonMessageInfo
        //- 특정 메세지, RPC 또는 업데이트에 대한 정보를 위한 컨테이너

        //이 게임오브젝트가 로컬(내꺼)이면 쓰기모드가 된다.
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            //위치를 리모트들에게 전달
            stream.SendNext(healthImage.fillAmount);
            //체력바를 리모트들에게 전달
        }
        //이 게임오브젝트가 리모트(상대컴터에 있는 내 복제본)이면 읽기모드가 된다.
        if(stream.IsReading)
        {
            curPos = (Vector3)stream.ReceiveNext();
            //위치를 받아와서 curPos에 넣어줌
            healthImage.fillAmount = (float)stream.ReceiveNext();
            //체력이미지값을 서버로부터 받아옴
            
            // * ReceiveNext()
            //- 스트림으로 들어온 값을 가져옴
            //- RecevieNext로 들어온 타입은 object 타입이다.
            //- 원본타입으로 형변환을 해주어야 함
            //- SendNext를 통해 보낸값이 순서대로 도착한다.
            //- 즉, 보낸 순서대로 값을 받아야 한다.(나열 순서가 일치해야 함)
        }
    }

    [PunRPC]
    void FlipXRPC(float axis) => spriteRenderer.flipX = (axis == -1);

    [PunRPC]
    void JumpRPC()
    {
        rig2D.velocity = Vector2.zero; //속도를 0으로 만듬
        rig2D.AddForce(Vector2.up * 300f); //윗방향으로 힘을 가함
    }
    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
