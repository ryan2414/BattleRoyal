using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class Bullet : MonoBehaviourPunCallbacks {
    //앞으로 나아가면서 크기가 커지고
    //플레이어랑 부딪히면
    //데미지를 주고
    //꺼진다
    int dir;
    public float speed;
    public PhotonView pv;
    public Player owner;

    private void Start() {
        Destroy(gameObject, 1f);
    }

    float curTime;

    private void Update() {
        curTime += Time.deltaTime;

        if (curTime >= 0.1f) {
            //스케일이 점점 커진다
            gameObject.transform.localScale += new Vector3(0.1f, 0.1f, 0);
            curTime = 0;
        }

        //일정 방향으로 움직인다
        transform.Translate(Vector2.right * speed * Time.deltaTime * dir);

    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!pv.IsMine && other.tag == "Player" && other.GetComponent<PhotonView>().Owner.UserId != pv.Owner.UserId) {
            //플레이어 죽음
            if (other.GetComponent<Player>().healthImage.fillAmount <= 0) {
                owner.GetComponent<PhotonView>().RPC("KillCount", RpcTarget.AllBuffered);
                other.GetComponent<Player>().Die();
            }
            //플레이어 데미지
            other.GetComponent<Player>().Hit();
            //플레이어 애니메이션 멈추기
            owner.GetComponent<Player>().DoStop();
            //총알을 파괴하고
            pv.RPC("DestoryRPC", RpcTarget.AllBuffered);
        } else if(other.tag == "Ground") {
            //플레이어 애니메이션 멈추기
            owner.GetComponent<Player>().DoStop();
            //총알을 파괴하고
            pv.RPC("DestoryRPC", RpcTarget.AllBuffered);
        }

    }

    [PunRPC]
    void Setowner() {
        //총알의 주인 정보 가져오기
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            if (player.GetComponent<PhotonView>().Owner.ActorNumber == pv.Owner.ActorNumber) {
                owner = player.GetComponent<Player>();
            }
        }
    }

    [PunRPC]
    void DestoryRPC() {
        
        Destroy(gameObject);
    }

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;
}
