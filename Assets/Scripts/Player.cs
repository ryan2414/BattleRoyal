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
    public Text FinishKillCountTxt;
    public SpriteRenderer sr;
    public Animator anim;
    public Rigidbody2D rig;
    public GameObject bulletFactory;
    public Image healthImage;
    public int killCount;
    public float attackCoolTime = 1.2f;
    public string nickname;

    Vector3 curPos;

    bool isGround;
    bool isFire;
    float curTime;

    private void Awake() {
        //NickName 설정
        nickNameTxt.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        nickNameTxt.color = pv.IsMine ? Color.green : Color.red;
        killCountTxt.color = pv.IsMine ? Color.green : Color.red;

        nickname = PhotonNetwork.NickName;

        if (pv.IsMine) {
            var cm = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            cm.Follow = transform;
            cm.LookAt = transform;
        }
        isFire = false;
    }

    void Update() {
        killCountTxt.text = $"Kill : {killCount}";
        if (pv.IsMine) {
            curTime += Time.deltaTime;

            if (!isFire) {
                //만약 공격 중이 아니라면 움직일 수 있다
                Move();
                Jump();
                Attack();
            }

            //공격 쿨타임
            if (curTime >= attackCoolTime) {
                DoStop();
            }
        }
        //isMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
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
        if (Input.GetKeyDown(KeyCode.Space) && isGround) {
            //공격 중일 때에는 움직이지 못하게 하고 싶다.
            rig.velocity = Vector2.zero;
            curTime = 0;
            isFire = true;

            anim.SetBool("isAttack", true);
            GameObject bullet = PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(sr.flipX ? -0.4f : 0.4f, -0.01f, 0), Quaternion.identity);
            bullet.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, sr.flipX ? -1 : 1);
            bullet.GetComponent<PhotonView>().RPC("Setowner", RpcTarget.AllBuffered);
        }


    }

    public void Hit() {
        healthImage.fillAmount -= 0.1f;
    }


    public void Die() {
        //오브젝트 터지는 애니메이션
        PhotonNetwork.Instantiate("FX_Explosion", transform.position, Quaternion.identity);
        //리스폰 판넬 생성
        GameObject rewpawnPanel = GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject;
        rewpawnPanel.SetActive(true);
        rewpawnPanel.transform.Find("KillCountTxt").GetComponent<Text>().text = $"Kill : {killCount}";
        //오즈젝트 삭제
        pv.RPC("DestoryRPC", RpcTarget.AllBuffered);
    }

    public void DoStop() {
        pv.RPC("RPC_DoStop", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_DoStop() {
        anim.SetBool("isAttack", false);
        isFire = false;
    }


    #region PunRPC
    [PunRPC]
    void DoAttack() {
        anim.SetBool("isAttack", true);
    }

    [PunRPC]
    public void KillCount() {
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
            stream.SendNext(killCount);
            stream.SendNext(healthImage.fillAmount);
        } else {
            curPos = (Vector3)stream.ReceiveNext();
            killCount = (int)stream.ReceiveNext();
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
    #endregion
}
