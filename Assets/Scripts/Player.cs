using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class Player : MonoBehaviourPunCallbacks, IPunObservable {
    public PhotonView pv;
    public Text nickNameTxt;
    public Text killCountTxt;
    public SpriteRenderer sr;
    public Animator anim;
    public Rigidbody2D rig;
    public GameObject bulletFactory;
    public Image healthImage;
    public int killCount;


    Vector3 curPos;

    bool isGround;
    bool isFire;

    private void Awake() {
        //NickName 설정
        nickNameTxt.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        nickNameTxt.color = pv.IsMine ? Color.green : Color.red;
        killCountTxt.color = pv.IsMine ? Color.green : Color.red;
        if (pv.IsMine) {
            var cm = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            cm.Follow = transform;
            cm.LookAt = transform;
        }

    }

    void Update() {

        killCountTxt.text = $"Kill : {killCount}";
        if (pv.IsMine && !isFire) {

            //만약 공격 중이 아니라면 움직일 수 있다
            Move();
            Jump();
            //바닥에 닿아 있을 때에만 공격이 가능하다
            if (isGround)
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

    void Attack() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            //공격 중일 때에는 움직이지 못하게 하고 싶다.
            rig.velocity = Vector2.zero;

            isFire = true;

            anim.SetTrigger("doAttack");

            GameObject bullet = PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(sr.flipX ? -0.4f : 0.4f, -0.01f, 0), Quaternion.identity);
            bullet.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, sr.flipX ? -1 : 1);

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

    //남의 총알에 맞고 죽었다. 
    //죽었을 때
    //누구에게 죽었는지 확인하고 싶다.
    //총알 주인의 킬 카운트를 올려 주고 싶다. 
    public void Hit() {
        healthImage.fillAmount -= 0.1f;
      
    }


    public void Die() {

        if (healthImage.fillAmount <= 0) {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            pv.RPC("DestoryRPC", RpcTarget.AllBuffered);

        }
    }
    [PunRPC]
    void KillCount() {
        killCount++;
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
            //stream.SendNext(killCount);
            stream.SendNext(healthImage.fillAmount);
        } else {
            curPos = (Vector3)stream.ReceiveNext();
            //killCount = (int)stream.ReceiveNext();
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
