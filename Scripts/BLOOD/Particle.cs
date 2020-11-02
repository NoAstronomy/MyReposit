using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{

    public ParticleSystem p;
    public List<ParticleCollisionEvent> pce;
    void Start()
    {
        pce = new List<ParticleCollisionEvent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
    }

    void OnParticleCollision(GameObject g)
    {
        int a = p.GetCollisionEvents(g, pce);
        //Debug.Log(a);
    }
}
