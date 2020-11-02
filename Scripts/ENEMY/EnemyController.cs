using System.Collections;
using System.Collections.Generic;
using r_BloodDATA;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public ControllDATA.ControllManager controllManager;
    public r_BloodDATA.BloodManager bloodManager;
    void Start()
    {
        controllManager = new ControllDATA.ControllManager(gameObject);
    }

    void Update()
    {
        
    }
}
