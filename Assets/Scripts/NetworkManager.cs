using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks {
    public GameObject startPanel;
    public GameObject respawnPanel;
    public InputField nickNameInput;
    public Toggle soundToggle;

    public Text stateTxt;
    public Button joinBtn;

    private void Awake() {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        startPanel.SetActive(true);
        respawnPanel.SetActive(false);
    }

    void Start() {
        soundToggle.isOn = true;

        joinBtn.interactable = false;
        //마스터 서버까지 접속 실행
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    public void Connect() {
        //버튼을 비 활성화
        joinBtn.interactable = false;

        //서버와 온라인 상태일때만 룸에 접속을 시도
        if (PhotonNetwork.IsConnected) {
            print("연결됨");

            PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;

            stateTxt.text = "룸에 매칭중...";
            PhotonNetwork.JoinRandomRoom();

        } else {
            stateTxt.text = "접속실패.. 재접속 시도중";
            //마스터 서버로 재접속 시도
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    //캐릭터 소환
    public void Spawn() {
        PhotonNetwork.Instantiate("Player", new Vector3(UnityEngine.Random.Range(-7,21),-2,0), Quaternion.identity);
        respawnPanel.SetActive(false);
    }

    //다른 방 들어가기
    public void OtherRoom() {
        print("다른 방 들어가기");

        respawnPanel.SetActive(false);
        PhotonNetwork.Disconnect();
        startPanel.SetActive(true);
    }

    //게임 종료
    public void ExitGame() {
        Application.Quit();
    }

    public void OnClickSound() {
        AudioListener.volume = soundToggle.isOn ? 1 : 0;
    }
    

    #region 오버라이드 함수

    //마스터 서버 접속 성공시 자동 실행
    public override void OnConnectedToMaster() {
        print("connected to master");

        stateTxt.text = "접속성공";
        joinBtn.interactable = true;

    }

    //마스터 서버 접속 실패시 자동 실행
    public override void OnDisconnected(DisconnectCause cause) {
        print("disconnect");
        joinBtn.interactable = false;
        startPanel.SetActive(true);
        stateTxt.text = "접속실패.. 재접속 시도중";
        //마스터 서버로 재접속 시도
        PhotonNetwork.ConnectUsingSettings();
    }

    //룸에 참가 완료된 경우 자동 실행
    public override void OnJoinedRoom() {
        print("방에들어옴");

        stateTxt.text = "방이 매칭됨 : 게임이 시작 됩니다.";

        startPanel.SetActive(false);
        GameObject.Find("PotionPoint").GetComponent<MovePosition>().isStart = true;
        StartCoroutine("DestoryBullet");
        Spawn();

    }

    IEnumerator DestoryBullet() {
        yield return new WaitForSeconds(0.1f);
        foreach(GameObject bullet in GameObject.FindGameObjectsWithTag("Bullet")) {
            bullet.GetComponent<PhotonView>().RPC("DestoryRPC", RpcTarget.AllBuffered);
        }
    }


    //(빈 방이 없어)랜덤 룸 참가에 실패한 경우 자동 실행
    public override void OnJoinRandomFailed(short returnCode, string message) {
        stateTxt.text = "빈 방이 없습니다.";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 }, null);
    }
    #endregion



}
