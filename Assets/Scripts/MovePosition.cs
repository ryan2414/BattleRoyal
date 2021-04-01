using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

//일정 시간마다 스폰 포인트를 움직이고 싶다.
//- 일정시간
//흐익 포션이 2개가 떨어진다.

public class MovePosition : MonoBehaviourPunCallbacks {
    public float createTime;
    public float minTime = 45;
    public float maxTime = 60;
    public bool isStart;
    public PhotonView pv;

    float curTime;

    // Start is called before the first frame update
    void Start() {
        createTime = Random.Range(minTime, maxTime);
    }

    // Update is called once per frame
    void Update() {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        curTime += Time.deltaTime;
        if (isStart) {
            if (curTime >= createTime) {
                transform.position = new Vector2(Random.Range(-7, 21), 10);
                PhotonNetwork.Instantiate("Potion", transform.position, Quaternion.identity);

                curTime = 0;
                createTime = Random.Range(minTime, maxTime);
            }

        }
    }
}
