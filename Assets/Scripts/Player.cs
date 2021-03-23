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

    bool isGround;

    private void Awake() {
        nickNameTxt.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        nickNameTxt.color = pv.IsMine ? Color.green : Color.red;
    }

    void Update() {
        if (pv.IsMine) {
            Move();
            Jump();
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
        } else anim.SetBool("isWalk", false);
    }

    void Jump() {
        //점프
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.43f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));

        anim.SetBool("isJump", !isGround);

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGround) {
            pv.RPC("JumpRPC", RpcTarget.All);
        }
    }
    void Attack() {
        //공격
        if (Input.GetKeyDown(KeyCode.Space)) {
            anim.SetTrigger("isAttack");
            GameObject bullet = Instantiate(bulletFactory);
            bullet.transform.position = transform.position;
        }
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

    }
}
