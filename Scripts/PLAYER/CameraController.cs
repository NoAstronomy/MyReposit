using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = player.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, player.position - offset, 5);
    }
}
