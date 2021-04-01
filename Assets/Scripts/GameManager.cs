using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

//방에 있는 플레이어의 점수를 가져와서
//점수를 비교하여 최고 점수를
//화면에 나타내고 싶다

public class GameManager : MonoBehaviour, IPunObservable {
    public static GameManager instance;
    private void Awake() {
        instance = this;

    }

    public Text bestUI;
    public PhotonView pv;

    int bestScore;
    string bestName;

    int[] rank;
    public static List<PlayerData> scoreList = new List<PlayerData>();

    void Start() {
      
    }

    void Update() {

        bestUI.text = $"BEST : {bestScore} / {bestName}";

        //플레이어의 정보를 리스트에 저장한다.
        scoreList = GameObject.FindGameObjectsWithTag("Player")
                        .Select(playerObj => {
                            string nick = playerObj.GetComponent<PhotonView>().Owner.NickName;
                            int killCount = playerObj.GetComponent<Player>().killCount;
                            return new PlayerData(nick, killCount);
                        }).ToList();

        //플레이어가 destory되면 리스트에서 플레이어 목록을 제거해야 되나?

        RankingSystem();


       
    }

    void RankingSystem() {
        for (int i = 0; i < scoreList.Count; i++) {
            if (bestScore < scoreList[i].killScore) {
                bestScore = scoreList[i].killScore;
                bestName = scoreList[i].name;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(bestScore);
            stream.SendNext(bestName);
        } else {
            bestScore = (int)stream.ReceiveNext();
            bestName = (string)stream.ReceiveNext();
        }
    }
}

public class PlayerData {
    public string name { get; set; }
    public int killScore { get; set; }
    public PlayerData(string name, int killScore) {
        this.name = name;
        this.killScore = killScore;
    }
}
