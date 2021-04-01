using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Potion : MonoBehaviourPunCallbacks {
    private void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.tag == "Player") {
            other.gameObject.GetComponent<Player>().healthImage.fillAmount += 0.1f;
            Destroy(gameObject);
        }
    }
}
