using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; //�߰�
using UnityEngine.UI; //�߰�

public class WizardController : MonoBehaviourPunCallbacks, IPunObservable
{
    // * Network��ü
    //- using Photon.Pun�� �ϰ�
    //- MonoBehaviourPunCallbacks�� ����ϸ� Network��ü�� ��
    //- �� ����� ���õ� ��ɾ ����� �� �ְ� �ȴ�.

    // * PhotonView
    //- ���濡�� ��Ʈ��ũ��ü�� ����ȭ �����ֱ� ���� �ʿ��� ������Ʈ
    //- ��Ʈ��ũ��ü�� �ൿ�� ��� PhotonView�� ���ؼ� ���� �� ����ȭ ��
    //- A��ü�� �̵������� �ٸ� ����(�ٸ� ��ǻ��)�� �ִ� A��ü�� Ŭ�е���
    //- PhotonView�� ���ؼ� �̵������ �ް� ��

    // * PhotonView - Syncronization (����ȭ �ɼ�)
    //- ��� ����ȭ �� ������ ���ϴ� �ɼ�
    //- off : RPC�� ����ϴ� ���(����ȭ ����)
    //- Reliable Delta Compressed : ���� �����͸� ���� ������ ������ ����
    //- Unreliable : ��� �����͸� ����(�ս� ���ɼ� ����)
    //- Unreliable OnChange : ������ ������ ��� ����
    //- *Off�̿ܿ��� observed Component�� Component�� 1���� ��� �Ǿ� �־�� ��

    // * PhotonView - Observed Components
    //- ����ȭ �� ������Ʈ�� �ִ� ��
    //- ��, ������Ʈ�� ����� ���� �����ؼ� �ٸ� Ŭ�е鿡�� ������.

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
    bool isGround; //���� ��Ҵ��� ����
    Vector3 curPos; //���� ��ġ
    int currentHp = 10; //���� ü��
    int maxHp = 10; //�ִ� ü��

    void Awake()
    {
        rig2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        phoView = GetComponent<PhotonView>();
    }

    void Start()
    {
        //phoView.IsMine : ���� ���������� true, �ƴϸ� false
        //�� true��� ����, false��� ����Ʈ
        //����Ʈ : ȭ�鿡�� ǥ�õǴ� ����� Clone(����ǰ)
        //���� ������Ʈ : �ֵ����� ������ ����
        //����Ʈ ������Ʈ : �ֵ����� ��Ʈ��ũ �ʸ� Ÿ�ο��� ����

        nickNameText.text = phoView.IsMine ? PhotonNetwork.NickName : phoView.Owner.NickName;
        nickNameText.color = phoView.IsMine ? Color.blue : Color.red;
        //������� �г��� �Ķ���, ����� ������
    }
    
    void Update()
    {
        //������ �����ϱ� ���� �������� üũ
        if(phoView.IsMine)
        {
            Move();
            Jump();
            Shot();
        }
        //��������Ʈ�̰� �������� �Ÿ��� 10�̻� ���� ���ٸ�
        else if((transform.position - curPos).sqrMagnitude >= 100)
        {
            transform.position = curPos;
            //������ġ�� �������� ��ġ�� ������
        }
        else //���̰� 10 ���϶��
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            //������������ �ڿ������� �̵�������
        }
    }

    void Move()
    {
        float axis = Input.GetAxisRaw("Horizontal");
        rig2D.velocity = new Vector2(4 * axis, rig2D.velocity.y);
        //rig2D.velocity : �ӵ��� ������
        if(axis != 0) //�¿� �Է°��� �ִٸ�
        {
            anim.SetBool("Run", true); //�̵� �ִϸ��̼�
            phoView.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);

            // * PhotonView.RPC
            //- ���� �뿡 ���ӿ� �ִ� ���� Ŭ���̾�Ʈ�� �޼ҵ带 ȣ���Ŵ
            //- ��, �ٸ� ��ǻ�Ϳ� �ִ� �� Ŭ�е鿡�� �Լ��� ȣ�������
            //- RPCȣ��(���� ȣ��)�� �Ϸ��� ȣ���ϴ� �Լ��� [PunRPC]��ŷ �ؾ� ��

            // * RpcTarget
            //1. All : �ڽ��� �Լ��� �ٷ� �����ϰ� �ٸ� ��ο��� ����(�ڽ��� ����)
            //2. AllBuffered : All�� ȣ���ϴ� �ñ⿡ ����ϰ� �����
            //- �ش� ����Ʈ�� ����� ���ۿ� ���ܵθ� �濡 �ʰ� ���� ������Ե� ����.
            //- �Ѿ� ���� ������� �ϴ� �͵��� AllBuffered�� ����Ѵ�.
            //3. AllViaServer : ��ο��� ������ ���ļ� ���� ȣ��
            //4. MasterClient : ���忡�Ը� ����
            //5. Others : ���� ������ ��ο��� ȣ��
            //6. OthersBuffered : ���� ������ ��ο��� ���ۿ� �Բ� ����
        }
        else //�¿��̵����� �ƴ϶��
        {
            anim.SetBool("Run", false);
        }

    }
    void Jump()
    {
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -1.3f), 0.07f, LayerMask.GetMask("Ground"));
        //Ground���̾�� �浹�� ����
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
        Gizmos.color = Color.black; //����� ����
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

    //�ֱ������� ������ ���� ȣ��Ǵ� �Լ�
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // * OnPhotonSerializeView
        //- ������ ���Ͽ� �ֱ������� ȣ��Ǵ� �Լ�
        //- PhotonView�� ����ȭ �����͸� �а� �� �� ����
        //- �� �Լ��� ����ϱ� ���ؼ� PhotonStream�� �ʼ��̴�.
        //- PhotonView�� ���� �����ǰ� �ִ� ��ũ��Ʈ������ ȣ���
        //- PhotonView ������Ʈ�� ����Ͽ� ����ȭ�� ������ ��� ������Ʈ��
        //  IPunObserable �������̽��� ����ϰ� OnPhotonSerialzeView �Լ���
        //  ���� �ؾ� �Ѵ�.

        // * PhotonStream
        //- ���� Ŭ���̾�Ʈ���� �ٸ� Ŭ���̾�Ʈ�� ���� ���� ���ų�
        //- �ٸ� Ŭ���̾�Ʈ�� ������ ���� ������ �����
        //- ��Ʈ�������� ������ �����̳�

        // * PhotonMessageInfo
        //- Ư�� �޼���, RPC �Ǵ� ������Ʈ�� ���� ������ ���� �����̳�

        //�� ���ӿ�����Ʈ�� ����(����)�̸� �����尡 �ȴ�.
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            //��ġ�� ����Ʈ�鿡�� ����
            stream.SendNext(healthImage.fillAmount);
            //ü�¹ٸ� ����Ʈ�鿡�� ����
        }
        //�� ���ӿ�����Ʈ�� ����Ʈ(������Ϳ� �ִ� �� ������)�̸� �б��尡 �ȴ�.
        if(stream.IsReading)
        {
            curPos = (Vector3)stream.ReceiveNext();
            //��ġ�� �޾ƿͼ� curPos�� �־���
            healthImage.fillAmount = (float)stream.ReceiveNext();
            //ü���̹������� �����κ��� �޾ƿ�
            
            // * ReceiveNext()
            //- ��Ʈ������ ���� ���� ������
            //- RecevieNext�� ���� Ÿ���� object Ÿ���̴�.
            //- ����Ÿ������ ����ȯ�� ���־�� ��
            //- SendNext�� ���� �������� ������� �����Ѵ�.
            //- ��, ���� ������� ���� �޾ƾ� �Ѵ�.(���� ������ ��ġ�ؾ� ��)
        }
    }

    [PunRPC]
    void FlipXRPC(float axis) => spriteRenderer.flipX = (axis == -1);

    [PunRPC]
    void JumpRPC()
    {
        rig2D.velocity = Vector2.zero; //�ӵ��� 0���� ����
        rig2D.AddForce(Vector2.up * 300f); //���������� ���� ����
    }
    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
