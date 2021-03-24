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

    private void Start() {
        Destroy(gameObject, 1f);

    }

    private void Update() {
        //스케일이 점점 커진다
        gameObject.transform.localScale += new Vector3(0.003f, 0.003f, 0);

        //일정 방향으로 움직인다
        transform.Translate(Vector2.right * speed * Time.deltaTime * dir);

    }

    private void OnTriggerEnter2D(Collider2D other) {
        //플레이어의 정보를 가져온다.
        if (other.tag == "Player"||other.tag == "Ground") {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().AnimStop();//플레이어의 애니메이션을 멈추고 싶다.
               
            if (!pv.IsMine && other.tag == "Player" && other.GetComponent<PhotonView>().IsMine) {
                other.GetComponent<Player>().Hit();
            }
            //총알을 파괴하고
            pv.RPC("DestoryRPC",RpcTarget.AllBuffered); 
        }

    }

    [PunRPC]
    void DestoryRPC() => Destroy(gameObject);
    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;
}
