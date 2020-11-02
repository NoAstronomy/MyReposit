using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfScene : MonoBehaviour
{
    public Vector3 direction;
    public Action action;
    private PlayerController pl;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        if (col.tag == "Player")
            CallBack(this.action, col);
    }

    public enum Action
    {
        Move,
        Jump
    }

    private void CallBack(Action action, Collider2D col)
    {
        switch (action)
        {
            case Action.Move: StartCoroutine(come_in_walk(col));
                break;

            case Action.Jump:
                StartCoroutine(come_in_jump(col));
                break;
        }
    }

    IEnumerator come_in_walk(Collider2D col)
    {
        pl = col.GetComponent<PlayerController>();
        var startPos = col.transform.position;
        var endPos = startPos + direction;
        var rb = col.GetComponent<Rigidbody2D>();
        float time = 0;
       // Debug.Log(rb.velocity);
        while (time < 1)
        {
           //pl.controllManager.movementRealization.Movement(1f * Time.deltaTime);

            pl.controllManager.container.isHorizontalMove = true;
            rb.MovePosition(startPos *(1-time) + endPos * time);
            time += 1f * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator come_in_jump(Collider2D col)
    {
        pl = col.GetComponent<PlayerController>();
        //var startPos = col.transform.position;
        //var endPos = startPos + direction;
        //var rb = col.GetComponent<Rigidbody2D>();
        float time = 0;
        // Debug.Log(rb.velocity);
        pl.controllManager.container.isJump = true;
        pl.controllManager.movementRealization.Jumping();
        while (time < 1f)
        {
            //pl.controllManager.movementRealization.Movement(1f * Time.deltaTime);
            pl.controllManager.container.horizontal = direction.x * time;
            //rb.MovePosition(startPos * (1 - time) + endPos * time);
           // pl.controllManager.container.jumpHeld = true;
            pl.controllManager.container.jumpHeld = true;
            pl.controllManager.container.isJump = false;


            time += 2f * Time.deltaTime;
            yield return null;
        }
    }


}
