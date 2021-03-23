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
    int dir = 1;
    public float speed;
    private void Start() {
        Destroy(gameObject, 1.2f);
    }

    private void Update() {
        transform.Translate(Vector2.right * speed *Time.deltaTime * dir); 
    }

    private void OnTriggerEnter2D(Collider2D other) {
        //Destroy(gameObject);
    }
}
