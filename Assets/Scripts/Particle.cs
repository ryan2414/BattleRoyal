using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    ParticleSystem particle;
    private void Awake() {
        particle = gameObject.GetComponent<ParticleSystem>();
    }
    private void Start() {
        particle.Stop();
        particle.Play();
        Destroy(gameObject, 2f);
    }
}
