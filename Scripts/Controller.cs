using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Grapth;
using Random = UnityEngine.Random;


namespace AI_Controller
{
    public class Controller : MonoBehaviour
    {
        public GenerateGraph tileClass;
        public MyTile[,] visitedTile;
        public List<MyTile> crossroads;
        public List<CreatePath> createPath;
        public List<Path> path;
        public List<Path> rpath;
        public List<Path> path2;
        Rigidbody2D rb;
        public AnimationCurve jumpCurve;
        public AnimationCurve jumpDownCurve;
        public AnimationCurve jumpStairsCurve;
        public Transform ai;
        public Transform player;
        Vector2Int startTileCoord;
        Vector2Int currentTileCoord;
        Vector2Int endTileCoord;

        public GrapthTools graphTools;

        [Header("PatrolPoint")]
        public List<Vector2Int> patrolPoint;
        int i = 0;
        public AI_Action ai_Action;

        public enum AI_Action
        {
            Patrol,
            Chase,
            Attack

        }

        void Start()
        {
            //tileClass = FindObjectOfType<GenerateGraph>().GetComponent<GenerateGraph>();
            player = FindObjectOfType<PlayerController>().transform;
            tileClass = GenerateGraph.generateGraph;
           // Debug.Log(tileClass);
            graphTools = GrapthTools.GetInstance();
            visitedTile = new MyTile[tileClass.bounds.size.x, tileClass.bounds.size.y];
            createPath = new List<CreatePath>();
            rpath = new List<Path>();
            path = new List<Path>();
            crossroads = new List<MyTile>();
            path2 = rpath;
            rb = GetComponent<Rigidbody2D>();
            patrolPoint = new List<Vector2Int>();

            foreach (var a in GameObject.FindGameObjectsWithTag("PatrolPoint"))
            {
                patrolPoint.Add(graphTools.RoundToTileCoord(a.transform.position.x , a.transform.position.y));
            }

            //Check Pattern Singleton in code
            int tx = graphTools.RoundToTileCoord(transform.position.x, 0).x;
            int ty = graphTools.RoundToTileCoord(0, transform.position.y).y;

            startTileCoord = new Vector2Int(tx, ty);
            endTileCoord = startTileCoord;
            if (patrolPoint.Count > 0)
                endTileCoord = patrolPoint[patrolPointNumber];
           // Debug.Log(endTileCoord);
            // UpdatePath();

        }

        void OnDrawGizmos()
        {
            if (tileClass == null)
            {
                return;
            }

            foreach (var a in tileClass.myTile)
            {
                if (a == null)
                    continue;

                if (a.type == MyTile.TileType.Angle)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(a.pos, .25f);
                }

                if (a.type == MyTile.TileType.Hor)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(a.pos, .15f);
                }

                for (int i = 0; i < a.neighbors.Count; i++)
                {
                    Debug.DrawLine(a.pos, a.neighbors[i].neighbor.pos, Color.white);
                }
            }

            for (int i = 0; i < rpath.Count - 1; i++)
            {
                Debug.DrawLine(
                    new Vector3(
                        rpath[i].tilePos.x + tileClass.tileOffset + tileClass.offset.x,
                        rpath[i].tilePos.y + tileClass.tileOffset + tileClass.offset.y),
                    new Vector3(
                        rpath[i + 1].tilePos.x + tileClass.tileOffset + tileClass.offset.x,
                        rpath[i + 1].tilePos.y + tileClass.tileOffset + tileClass.offset.y),
                    Color.red
                );
            }
        }

        void FixedUpdate()
        {
            int tx = graphTools.RoundToTileCoord(transform.position.x, 0).x;
            int ty = graphTools.RoundToTileCoord(0, transform.position.y).y;

            if (tileClass.myTile[tx, ty] != null)
            {
                startTileCoord = new Vector2Int(tx, ty);
            }


            if (ai_Action == AI_Action.Attack)
            {
                ChangeActionToChaseAfterAttack();
            }

            if (ai_Action == AI_Action.Patrol)
            {

                if (patrolPoint.Count < 2)
                {                    
                    GetRandomPointToPatrol(Random.Range(-5, 5));
                    
                }
                  //  return;

                MovePatrol();
                ChangeActionToChase();
            }

            if (ai_Action == AI_Action.Chase)
            {

                int endPosX = graphTools.RoundToTileCoord(player.transform.position.x, 0).x;
                int endPosY = graphTools.RoundToTileCoord(0, player.transform.position.y).y;

                if (tileClass.myTile[endPosX, endPosY] != null)
                {
                    endTileCoord = new Vector2Int(endPosX, endPosY);
                    //Debug.Log("endTileCoord"  + endTileCoord);
                }

                MakeMove();
                ChangeActionToPatrol();
            }

            CheckEndPosMethod();
        }

        void Update()
        {


        }
        void ChangeActionToChaseAfterAttack()
        {
            if (path2.Count < 1)
                return;

            int endPosX = graphTools.RoundToTileCoord(player.transform.position.x, 0).x;
            int endPosY = graphTools.RoundToTileCoord(0, player.transform.position.y).y;
            int tx = graphTools.RoundToTileCoord(transform.position.x, 0).x;
            int ty = graphTools.RoundToTileCoord(0, transform.position.y).y;

            if (tileClass.myTile[endPosX, endPosY] == null)
                return;

            if (tileClass.myTile[tx, ty] == null)
                return;

            if (Vector2.Distance(tileClass.myTile[endPosX, endPosY].tileCoord, tileClass.myTile[tx, ty].tileCoord) > 2)
            {
                //Debug.Log("Change");
                ai_Action = AI_Action.Chase;
                UpdatePath();
            }
        }

        void ChangeActionToChase()
        {
            if (path2.Count < 1)
                return;

            int endPosX = graphTools.RoundToTileCoord(player.transform.position.x, 0).x;
            int endPosY = graphTools.RoundToTileCoord(0, player.transform.position.y).y;
            int tx = graphTools.RoundToTileCoord(transform.position.x, 0).x;
            int ty = graphTools.RoundToTileCoord(0, transform.position.y).y;

            if (tileClass.myTile[endPosX, endPosY] == null)
                return;

            if (tileClass.myTile[tx, ty] == null)
                return;

            if (Vector2.Distance(tileClass.myTile[endPosX, endPosY].tileCoord, tileClass.myTile[tx, ty].tileCoord) < 10)
            {
                //Debug.Log("Change");
                ai_Action = AI_Action.Chase;
                UpdatePath();
            }
        }

        void ChangeActionToPatrol()
        {
            if (path2.Count < 1)
                return;

            int endPosX = graphTools.RoundToTileCoord(player.transform.position.x, 0).x;
            int endPosY = graphTools.RoundToTileCoord(0, player.transform.position.y).y;
            int tx = graphTools.RoundToTileCoord(transform.position.x, 0).x;
            int ty = graphTools.RoundToTileCoord(0, transform.position.y).y;

               // Debug.Log(1);
            if(tileClass.myTile[endPosX, endPosY] == null)
                return;

               // Debug.Log(2);
            if (tileClass.myTile[tx, ty] == null)
                return;

               // Debug.Log(3);
            if (Vector2.Distance(tileClass.myTile[endPosX, endPosY].tileCoord, tileClass.myTile[tx, ty].tileCoord) > 10)
            {
               // Debug.Log("Change2");
                ai_Action = AI_Action.Patrol;
                endTileCoord = patrolPoint[patrolPointNumber];
                UpdatePath();
            }
        }

        bool canUpdatePath = true;
        float time = 0;
        int patrolPointNumber = 0;
        Vector2Int n = Vector2Int.zero;
       /* Vector2Int RoundToTileCoord(float x, float y)
        {
            var tileCoord = new Vector2Int(
                Mathf.RoundToInt(x - tileClass.offset.x - tileClass.tileOffset),
                Mathf.RoundToInt(y - tileClass.offset.y - tileClass.tileOffset)
            );
            return tileCoord;
        }*/

        public void GetRandomPointToPatrol(int dir)
        {
            int tx = graphTools.RoundToTileCoord(transform.position.x, 0).x;
            int ty = graphTools.RoundToTileCoord(0, transform.position.y).y;

            if (tileClass.myTile[tx + dir, ty] == null 
                || tileClass.myTile[tx + dir, ty].type == MyTile.TileType.Angle)
            return;

            patrolPoint.Add(new Vector2Int(tx + dir, ty));


        }

        void MovePatrol()
        {          
            if (startTileCoord == endTileCoord)
            {
                if (patrolPoint.Count > 0)
                {


                    patrolPointNumber++;


                    if (patrolPointNumber >= patrolPoint.Count)
                        patrolPointNumber = 0;

                    //Debug.Log("suc");
                    endTileCoord = patrolPoint[patrolPointNumber];
                }
            }

            if(path2.Count < 1)
                return;

            if (startTileCoord == path2[i].tilePos && i < path2.Count - 1)
            {
                i++;
                n = path2[i].tilePos - tileClass.myTile[startTileCoord.x, startTileCoord.y].tileCoord;
            }

            if (n.y == 0 && Mathf.Abs(n.x) == 1)
                Move(path2[i].tilePos);

            if (n.y < 0 && n.x == 0)
                MoveDown(path2[i].tilePos);

            if (n.y < 0 && Mathf.Abs(n.x) > 0)
                Jump(path2[i], n);

            if (n.y == 0 && Mathf.Abs(n.x) > 1)
                JumpBridge(path2[i], n);

            if (n.y > 0)
                Jump(path2[i], n);
        }

        void MakeMove()
        {
           // Debug.Log(n);
            if (path2.Count < 1)
            {
                i = 0;
                //Debug.Log("asdasdsa");
                return;
            }

            if (i > path2.Count - 1)
            {
                i = 0;
               // Debug.Log("00");
                return;
            }

            if (tileClass.myTile[startTileCoord.x, startTileCoord.y] == null)
            {
               // Debug.Log("StartNull");
                return;
            }

            if (tileClass.myTile[endTileCoord.x, endTileCoord.y] == null)
            {
               // Debug.Log("endNull");
                endTileCoord = path2[path2.Count - 1].tilePos;
                //return;
            }

            if (path2[i].tilePos == endTileCoord)
            {
               // Debug.Log("End");
                n = Vector2Int.zero;
                ai_Action = AI_Action.Attack;
               // i = 0;
               // return;
            }

            if (i < path2.Count - 1 && startTileCoord == path2[i].tilePos)
            {
                i++;
                n = path2[i].tilePos - tileClass.myTile[startTileCoord.x, startTileCoord.y].tileCoord;
               // Debug.Log("n = " + n);
            }

            if (n.y == 0 && Mathf.Abs(n.x) == 1)
            {
               // Debug.Log("NNNNN");
                Move(path2[i].tilePos);
            }

            if (n.y < 0 && n.x == 0)
                MoveDown(path2[i].tilePos);

            if (n.y < 0 && Mathf.Abs(n.x) > 0)
                Jump(path2[i], n);

            if (n.y == 0 && Mathf.Abs(n.x) > 1)
                JumpBridge(path2[i], n);

            if (n.y > 0)
                Jump(path2[i], n);


        }

        void CheckEndPosMethod()
        {
            int tx = graphTools.RoundToTileCoord(transform.position.x, 0).x;
            int ty = graphTools.RoundToTileCoord(0, transform.position.y).y;

            if (tileClass.myTile[tx, ty] == null)
                return;

         //   if (tileClass.myTile[endTileCoord.x, endTileCoord.y] == null)
         //   return;

            if (tileClass.myTile[startTileCoord.x, startTileCoord.y].type == MyTile.TileType.Angle)
                return;

            if (ai_Action == AI_Action.Chase)
            {
                int tx1 = graphTools.RoundToTileCoord(player.position.x, 0).x;
                int ty1 = graphTools.RoundToTileCoord(0, player.position.y).y;

                //Debug.Log(currentTileCoord);

                if (Vector2.Distance(currentTileCoord, endTileCoord) > 1)
                {
                    if (tileClass.myTile[tx1, ty1] != null)
                        currentTileCoord = tileClass.myTile[tx1, ty1].tileCoord;

                    if (startTileCoord != endTileCoord || path2.Count == 0)
                    {
                        if (coroutine != null)
                        {
                            StopCoroutine(coroutine);
                            coroutine = null;
                            //Debug.Log("upd");

                        }
                       
                       // Debug.Log("recalc");
                        UpdatePath();

                    }
                }
            }

            if (ai_Action == AI_Action.Patrol)
            {
                if (time == 0)
                {
                    if (startTileCoord != endTileCoord || path2.Count == 0)
                    {
                        if (coroutine != null)
                        {
                           // Debug.Log("upd");
                            StopCoroutine(coroutine);
                            coroutine = null;

                        }

                        UpdatePath();

                    }
                }

                if (time <= 1)
                    time += Time.deltaTime;
                else
                    time = 0;

            }
        }

        void UpdatePath()
        {
            i = 0;
            visitedTile = new MyTile[tileClass.bounds.size.x, tileClass.bounds.size.y];
            createPath.Clear();
            path.Clear();
            rpath.Clear();
            crossroads.Clear();

            DFS(
                tileClass.myTile[startTileCoord.x, startTileCoord.y],
                tileClass.myTile[endTileCoord.x, endTileCoord.y],
                tileClass.myTile[startTileCoord.x, startTileCoord.y]
            );
        }

        void DFS(MyTile tile, MyTile endTile, MyTile cameFrom)
        {

            if (visitedTile[tile.tileCoord.x, tile.tileCoord.y] == null)
            {
                visitedTile[tile.tileCoord.x, tile.tileCoord.y] = tile;
                tile.cameFrom = cameFrom;
                createPath.Add(new CreatePath(cameFrom.tileCoord, tile.tileCoord, tile.type, tile.offset));

                if (tile == cameFrom)
                    crossroads.Add(tile);
                else if (tile.neighbors.Count > 1)
                {
                    int v = 0;
                    for (int i = 0; i < tile.neighbors.Count; i++)
                    {
                        if (visitedTile[tile.neighbors[i].neighbor.tileCoord.x,
                                        tile.neighbors[i].neighbor.tileCoord.y] != null)
                            continue;
                        v++;
                    }

                    if (v > 1)
                        crossroads.Add(tile);
                }
            }

            if (tile.tileCoord == endTile.tileCoord)
            {
                CalculatePath(endTile.tileCoord);
                return;
            }

            if (tile.neighbors.Count == 0)
            {
                if (crossroads.Count == 0)
                    return;

                var a = crossroads[crossroads.Count - 1];
                crossroads.RemoveAt(crossroads.Count - 1);

                DFS(a, endTile, a.cameFrom);
                return;
            }

            int num = -1;
            float minDist = Mathf.Infinity;

            /*if (tile.neighbors[0] != null)
            {
                float d0 = Vector2.Distance(tile.neighbors[0].neighbor.tileCoord, endTile.tileCoord);
                if (tile.neighbors[1] != null)
                {
                    float d1 = Vector2.Distance(tile.neighbors[1].neighbor.tileCoord, endTile.tileCoord);
                    num = (d1 > d0) ? 0 : 1;
                }
                else num = 0;
            }*/
            for (int i = 0; i < tile.neighbors.Count; i++)
            {
                if (visitedTile[tile.neighbors[i].neighbor.tileCoord.x, tile.neighbors[i].neighbor.tileCoord.y] != null)
                    continue;

                float currentDist = Vector2.Distance(tile.neighbors[i].neighbor.tileCoord, endTile.tileCoord);
                if (currentDist < minDist)
                {
                    minDist = currentDist;
                    num = i;
                }
            }

            if (num == -1)
            {
                if (crossroads.Count == 0)
                    return;

                var a = crossroads[crossroads.Count - 1];
                crossroads.RemoveAt(crossroads.Count - 1);

                DFS(a, endTile, a.cameFrom);
                return;
            }

            DFS(tile.neighbors[num].neighbor, endTile, tile);
        }

        void CalculatePath(Vector2Int lastTilePos)
        {
            for (int i = 0; i < createPath.Count; i++)
            {
                if (lastTilePos == createPath[i].tilePos)
                {
                    path.Add(new Path(createPath[i].tilePos, createPath[i].tileType, createPath[i].offset));
                    if (createPath[i].tilePos == createPath[i].from)
                    {
                        for (int j = 0; j < path.Count; j++)
                        {
                            rpath.Add(path[path.Count - 1 - j]);
                        }

                        return;
                    }

                    CalculatePath(createPath[i].from);
                }
            }
        }

        void JumpBridge(Path endTilePos, Vector2Int n)
        {
            if (coroutine != null)
                return;

            coroutine = StartCoroutine(makeJumpBrigde(endTilePos, n));
        }

        void Jump(Path endTilePos, Vector2Int n)
        {
            if (coroutine != null)
                return;

            coroutine = StartCoroutine(makeJump(endTilePos, n));
        }

        void Move(Vector2 endTilePos)
        {
            if (coroutine != null)
                return;

            coroutine = StartCoroutine(makeMove(endTilePos));

        }

        void MoveDown(Vector2 endTilePos)
        {
            if (coroutine != null)
                return;

            coroutine = StartCoroutine(makeDown(endTilePos));

        }

        Coroutine coroutine;

        IEnumerator makeMove(Vector2 moveToTile)
        {
            float time = 0;
            Vector2 myPos = ai.transform.position;
            Vector2 endPos = moveToTile + new Vector2(
                                 tileClass.offset.x + tileClass.tileOffset,
                                 tileClass.offset.y + tileClass.tileOffset);
            //Debug.Log("Move" + n);
            while (time <= 1)
            {
                rb.MovePosition(
                    new Vector2(
                        myPos.x,
                        transform.position.y) * (1 - time)
                    + new Vector2(
                        endPos.x,
                        transform.position.y) * (time)
                );


                time += 2 * Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            coroutine = null;
        }

        IEnumerator makeDown(Vector2 moveToTile)
        {
            float time = 0;
            Vector2 myPos = ai.transform.position;
            Vector2 endPos = moveToTile + new Vector2(
                                 tileClass.offset.x + tileClass.tileOffset,
                                 tileClass.offset.y + tileClass.tileOffset);
            // Debug.Log("Down" + n);
            while (time <= 1)
            {
                rb.MovePosition(myPos * (1 - time) + endPos * (time));
                time += 2 * Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            coroutine = null;
        }

        IEnumerator makeJumpBrigde(Path jumpToTile, Vector2Int n)
        {
            float time = 0;
            float curve;
            float coefY = 1;
            AnimationCurve aCurve = jumpCurve;
            Vector2Int n1 = n;
            Vector2 myPos = ai.transform.position;
            Vector2 endPos = jumpToTile.tilePos + new Vector2(
                                 tileClass.offset.x + tileClass.tileOffset,
                                 tileClass.offset.y + tileClass.tileOffset);
           //  Debug.Log("JumpBrigde" + n1);
            while (time < 1)
            {
                coefY = (time < 0.5f) ? time : 1 - time;
                curve = aCurve.Evaluate(time);

                rb.MovePosition(
                    new Vector2(
                        myPos.x * (1 - time),
                        (myPos.y + coefY) * (1 - curve)
                    )
                    + new Vector2(
                        endPos.x * time,
                        (endPos.y + coefY) * curve)
                );

                time += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            coroutine = null;
        }

        IEnumerator makeJump(Path jumpToTile, Vector2Int n)
        {
            float time = 0;
            float curve;
            AnimationCurve aCurve = jumpCurve;
            Vector2Int n1 = n;
            Vector2 myPos = ai.transform.position;
            Vector2 endPos = jumpToTile.tilePos + new Vector2(
                                 tileClass.offset.x + tileClass.tileOffset,
                                 tileClass.offset.y + tileClass.tileOffset);

            // Debug.Log("Jump" + n1);
            if (Mathf.Abs(n1.x) == 1 && Mathf.Abs(n1.y) == 1)
                aCurve = jumpStairsCurve;

            if (n1.y < 0)
                aCurve = jumpDownCurve;

            while (time < 1)
            {
                curve = aCurve.Evaluate(time);

                rb.MovePosition(
                    new Vector2(
                        myPos.x * (1 - time),
                        myPos.y * (1 - curve)
                    )
                    + new Vector2(
                        endPos.x * time,
                        endPos.y * curve)
                );

                time += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            coroutine = null;
        }

        
    }

}

