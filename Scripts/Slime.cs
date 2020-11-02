using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyDATA;

public class Slime : MonoBehaviour
{

    public SlimeDATA slimeDATA;
    public float hp;
    public float currency;
    public SlimeSize slimeSize;
    public SlimeAction slimeAction;
    public List<Slime> neighbors;
    public float checkEnemyDistance;
    public Slime currentSlime;
    public float moveValue;
    public bool isCurrentSlime;

    void Start()
    {
        slimeSize = SlimeSize.small;
        slimeAction = SlimeAction.find;
        hp = 1;
        currency = 1;
        checkEnemyDistance = 1;
        neighbors = new List<Slime>();
        moveValue = Random.Range(0f, 1f);
        
        StartCoroutine("make_FindCurrentSlime");
    }

    public void FindMyNeighbors()
    {
        if (neighbors.Count > 0)
            neighbors.Clear();

        RaycastHit2D[] hit;

        hit = Physics2D.CircleCastAll(transform.position, 2, Vector2.up, 0, LayerMask.GetMask("Liquid"));

        int number = 0;
        float minDist = 10;
        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider == null)
                continue;

            if (hit[i].collider.gameObject == gameObject)
                continue;

            if (Mathf.Abs(Mathf.Pow(transform.position.y, 2) - Mathf.Pow(hit[i].collider.transform.position.y, 2)) > 2)
                continue;

            neighbors.Add(hit[i].collider.GetComponent<Slime>());

            float dist = Vector2.Distance(transform.position, hit[i].transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                number = neighbors.Count - 1;
            }
        }

        if (neighbors.Count < 1)
        {
            currentSlime = this;
            return;
        }

        currentSlime = neighbors[number];
    }

    IEnumerator make_FindCurrentSlime()
    {
        float time = 0;
        while (time < 1)
        {
            time += 2f * Time.deltaTime;
            yield return null;
        }

        FindMyNeighbors();
    }

    public void IdentifySlimeAction()
    {
        if (currentSlime == this)
        {
            slimeAction = SlimeAction.stay;
            return;
        }

        if (currentSlime.neighbors.Count > neighbors.Count)
        {
            slimeAction = SlimeAction.move;
            return;
        }

        if (currentSlime.neighbors.Count < neighbors.Count)
        {
            slimeAction = SlimeAction.stay;
            return;
        }

        if (currentSlime.neighbors.Count == neighbors.Count)
        {
            if (moveValue > currentSlime.moveValue)
                slimeAction = SlimeAction.stay;
            else
                slimeAction = SlimeAction.move;

            return;
        }

        if (currentSlime.slimeAction == SlimeAction.stay)
        {
            if (currentSlime.neighbors.Count > neighbors.Count)
            {
                slimeAction = SlimeAction.move;
                return;
            }

            if (currentSlime.neighbors.Count < neighbors.Count)
            {
                slimeAction = SlimeAction.stay;
                return;
            }

            if (currentSlime.neighbors.Count == neighbors.Count)
            {
                if (moveValue > currentSlime.moveValue)
                    slimeAction = SlimeAction.stay;
                else
                    slimeAction = SlimeAction.move;

                return;
            }
        }

    }

    void Update()
    {
        if (currentSlime != null && slimeAction == SlimeAction.find)
        {
            IdentifySlimeAction();
        }

        Move();
    }

    public Coroutine MoveCoroutine;
    public void Move()
    {
        if (slimeAction == SlimeAction.move && MoveCoroutine == null)
            MoveCoroutine = StartCoroutine(
                make_Move(
                    transform.position,
                    currentSlime.transform.position)
            );
    }
    IEnumerator make_Move(Vector2 startPos, Vector2 endPos)
    {
        float time = 0;
        while (time < 1)
        {
            GetComponent<Rigidbody2D>().MovePosition(
                new Vector2(startPos.x, transform.position.y) * (1 - time) +
                new Vector2(endPos.x, transform.position.y) * time
            );

            time += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        MoveCoroutine = null;
    }

    public enum SlimeSize
    {
        small,
        medium,
        large
    }

    public enum SlimeAction
    {
        stay,
        move,
        find
    }


}
