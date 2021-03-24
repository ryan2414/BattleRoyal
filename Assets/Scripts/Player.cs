using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable {
    public PhotonView pv;
    public Text nickNameTxt;
    public SpriteRenderer sr;
    public Animator anim;
    public Rigidbody2D rig;
    public GameObject bulletFactory;
    public Image healthImage;


    Vector3 curPos;

    bool isGround;
    bool isFire;

    private void Awake() {
        //NickName 설정
        nickNameTxt.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        nickNameTxt.color = pv.IsMine ? Color.green : Color.red;
    }

    void Update() {
        if (pv.IsMine && !isFire) {
            //만약 공격 중이 아니라면 움직일 수 있다
            Move();
            Jump();
            //바닥에 닿아 있을 때에만 공격이 가능하다
            if(isGround)
                Attack();
        }
    }
    void Move() {
        //좌우 이동
        float axis = Input.GetAxisRaw("Horizontal");
        rig.velocity = new Vector2(4 * axis, rig.velocity.y);

        if (axis != 0) {
            anim.SetBool("isWalk", true);
            pv.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
        } else { anim.SetBool("isWalk", false); }


    }

    void Jump() {
        //바닥인지 확인
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.43f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));

        anim.SetBool("isJump", !isGround);
        //점프
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGround) {
            pv.RPC("JumpRPC", RpcTarget.All);
        }
    }

    float currentTime;
    void Attack() {
        currentTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space)) {
            currentTime = 0;
            //공격 중일 때에는 움직이지 못하게 하고 싶다.
            rig.velocity = Vector2.zero;

            isFire = true;

            anim.SetTrigger("doAttack");

            PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(sr.flipX ? -0.333f : 0.333f, -0.02f, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, sr.flipX ? -1 : 1);

            Invoke("Break", 1.4f);
        }

    }
    void Break() {
        isFire = false;
        
    }


    public void AnimStop() {
        anim.SetTrigger("doTouch");
        isFire = false;
    }

    public void Hit() {
        healthImage.fillAmount -= 0.1f;
        if (healthImage.fillAmount <= 0) {
            pv.RPC("DestoryRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestoryRPC() {
        Destroy(gameObject);
    }

    [PunRPC]
    void FlipXRPC(float axis) {
        sr.flipX = axis == -1;
    }

    [PunRPC]
    void JumpRPC() {
        rig.velocity = Vector2.zero;
        rig.AddForce(Vector2.up * 400);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(healthImage.fillAmount);
        } else {
            curPos = (Vector3)stream.ReceiveNext();
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
