using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

namespace Grapth
{
    
    public class GenerateGraph : MonoBehaviour
    {
    #region VARIABLES

        public float tileOffset;
        public Tilemap tilemap;
        public BoundsInt bounds;
        public Vector3Int offset;
        public MyTile[,] myTile;
        public TileBase[] tile;

        #endregion

        public static GenerateGraph generateGraph;

        void Awake()
        {
            generateGraph = (generateGraph == null)
                            ? this
                            : generateGraph;

            

            tileOffset = .5f;
            bounds = tilemap.cellBounds;
            offset = bounds.position;
            tile = tilemap.GetTilesBlock(bounds);
            myTile = new MyTile[bounds.size.x, bounds.size.y];

            GenerateGrapthArray();
            AddPathAndPoint();
            SortTileNeighborsByWeight();
        }

        public void GenerateGrapthArray()
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    if (tile[x + y * bounds.size.x] == null)
                        continue;

                    if (Del(x, y, 2, 0) && Del(x, y, 0, 2) && Del(x, y, -2, 0) && Del(x, y, -2, -2))
                        if (tile[x + (y + 1) * bounds.size.x] == null)
                        {
                            myTile[x, y + 1] = new MyTile()
                            {
                                pos = new Vector2(x + offset.x + tileOffset, (y + 1) + offset.y + tileOffset),
                                type = MyTile.TileType.Hor,
                                tileCoord = new Vector2Int(x, y + 1)
                            };
                        }

                    if (Del(x, y, 2, 2) && Del(x, y, 2, 0) && Del(x, y, 0, 2) && Del(x, y, -2, 0) && Del(x, y, -2, -2))
                        if (tile[x + (y + 1) * bounds.size.x] == null
                            && tile[(x + 1) + (y + 1) * bounds.size.x] == null
                            && tile[(x + 1) + y * bounds.size.x] == null)
                        {

                            myTile[x + 1, y + 1] = new MyTile()
                            {
                                pos = new Vector2((x + 1) + offset.x + tileOffset, (y + 1) + offset.y + tileOffset),
                                type = MyTile.TileType.Angle,
                                tileCoord = new Vector2Int(x + 1, y + 1)
                            };
                        }

                    if (Del(x, y, 0, 2) && Del(x, y, -2, 0) && Del(x, y, -2, -2))
                        if (tile[x + (y + 1) * bounds.size.x] == null
                            && tile[(x - 1) + (y + 1) * bounds.size.x] == null
                            && tile[(x - 1) + y * bounds.size.x] == null)
                        {
                            myTile[x - 1, y + 1] = new MyTile()
                            {
                                pos = new Vector2((x - 1) + offset.x + tileOffset, (y + 1) + offset.y + tileOffset),
                                type = MyTile.TileType.Angle,
                                tileCoord = new Vector2Int(x - 1, y + 1)
                            };
                        }
                }
            }
        }

        public void AddPathAndPoint()
        {
            for (int i = 0; i < myTile.GetLength(0); i++)
            {
                for (int j = 0; j < myTile.GetLength(1); j++)
                {
                    if (myTile[i, j] == null)
                        continue;

                    HorTileNeigbors(i, j);
                    VertTileNeighbors(i, j);
                    JumpTileNeighbors(i, j);
                }
            }
        }

        public void SortTileNeighborsByWeight()
        {
            for (int i = 0; i < myTile.GetLength(0); i++)
            {
                for (int j = 0; j < myTile.GetLength(1); j++)
                {
                    if (myTile[i, j] == null)
                        continue;

                    SortNeighbors(myTile[i, j]);
                }
            }
        }

        public void SortNeighbors(MyTile myTile)
        {
            for (int i = 0; i < myTile.neighbors.Count - 2; i++)
            {
                for (int j = 0; j < myTile.neighbors.Count - 2; j++)
                {
                    if (myTile.neighbors[j].weight < myTile.neighbors[j + 1].weight)
                        continue;

                    Neighbors neighbor = myTile.neighbors[j + 1];
                    myTile.neighbors[j + 1] = myTile.neighbors[j];
                    myTile.neighbors[j] = neighbor;

                }
            }
        }

        public bool Del(int x, int y, int _x, int _y)
        {
            if (x + _x > bounds.size.x)
            {
                return false;
            }

            if (y + _y > bounds.size.y)
            {
                return false;
            }

            if (y + _y < 0)
            {
                return false;
            }

            if (x + _x < 0)
            {
                return false;
            }

            return true;

        }

        public void HorTileNeigbors(int x, int y)
        {
            if (myTile[x, y].type == MyTile.TileType.Angle)
                return;

            if (Del(x, y, 2, 0))
            {
                if (myTile[x + 1, y] != null)
                    myTile[x, y].neighbors.Add(new Neighbors(myTile[x + 1, y], 1));

                if (myTile[x - 1, y] != null)
                    myTile[x, y].neighbors.Add(new Neighbors(myTile[x - 1, y], 1));
            }

        }

        public void VertTileNeighbors(int x, int y)
        {
            if (myTile[x, y].type != MyTile.TileType.Angle)
                return;

            for (int i = y; i >= 0; i--)
            {
                if (myTile[x, i] == null)
                    continue;

                if (myTile[x, i].type == MyTile.TileType.Hor)
                {
                    int deltaY = (y - i);
                    myTile[x, y].neighbors.Add(new Neighbors(myTile[x, i], deltaY));
                    break;
                }
            }
        }

        public void JumpTileNeighbors(int x, int y)
        {

            if (MyTileType(x, y, MyTile.TileType.Angle))
                return;

            JumpTile(x, y, 2, 0, 1, 0);
            JumpTile(x, y, 3, 0, 1, 0);

            JumpTile(x, y, -2, 0, -1, 0);
            JumpTile(x, y, -3, 0, -1, 0);

            JumpTile(x, y, 0, 1, 1, 0);
            JumpTile(x, y, 0, 1, -1, 0);

            JumpTile(x, y, 1, 1, 1, 0);
            JumpTile(x, y, 1, 2, 1, 0);
            JumpTile(x, y, 1, 3, 1, 0);
            JumpTile(x, y, 1, 4, 1, 0);
            JumpTile(x, y, 1, 5, 1, 0);

            JumpTile(x, y, -1, 1, -1, 0);
            JumpTile(x, y, -1, 2, -1, 0);
            JumpTile(x, y, -1, 3, -1, 0);
            JumpTile(x, y, -1, 4, -1, 0);
            JumpTile(x, y, -1, 5, -1, 0);

            JumpTile(x, y, 2, 1, 1, 0);
            JumpTile(x, y, -2, 1, -1, 0);

            JumpDown(x, y, 2, -1, 1, 0);
            JumpDown(x, y, 2, -2, 1, 0);
            JumpDown(x, y, 2, -3, 1, 0);
             JumpDown(x, y, 2, -4, 1, 0);

            JumpDown(x, y, -2, -1, -1, 0);
            JumpDown(x, y, -2, -2, -1, 0);
            JumpDown(x, y, -2, -3, -1, 0);
             JumpDown(x, y, -2, -4, -1, 0);

            JumpDown(x, y, 1, -1, 1, 0);
            JumpDown(x, y, 1, -2, 1, 0);
             JumpDown(x, y, 1, -3, 1, 0);

            JumpDown(x, y, -1, -1, -1, 0);
            JumpDown(x, y, -1, -2, -1, 0);
             JumpDown(x, y, -1, -3, -1, 0);

            JumpDown(x, y, 0, -1, 1, 0);
            JumpDown(x, y, 0, -1, -1, 0);

        }

        public void JumpTile(int x, int y, int _x, int _y, int deltaX, int deltaY)
        {
            if (Del(x, y, 5, 5) && Del(x, y, -5, 5))
            {
                if (MyTileType(x + _x, y + _y, MyTile.TileType.Angle)
                    && MyTileType(x + _x + deltaX, y + _y + deltaY, MyTile.TileType.Hor))
                {
                    myTile[x, y].neighbors.Add(
                        new Neighbors(
                            myTile[x + _x + deltaX, y + _y + deltaY],
                            Convert.ToInt32(
                                Math.Floor(
                                    Math.Sqrt(
                                        (_x + deltaX) * (_x + deltaX) + (_y + deltaY) * (_y + deltaY))
                                )
                            )
                        )
                    );
                }
            }
        }

        public void JumpDown(int x, int y, int _x, int _y, int deltaX, int deltaY)
        {
            if (Del(x, y, -5, -5) && Del(x, y, 5, -5))
            {
                if (_x != 0 && MyTileType(x + _x / Mathf.Abs(_x), y, MyTile.TileType.Angle))
                {
                    if (MyTileType(x + _x, y + _y, MyTile.TileType.Angle)
                        && MyTileType(x + _x + deltaX, y + _y + deltaY, MyTile.TileType.Hor))
                    {
                        myTile[x, y].neighbors.Add(
                            new Neighbors(
                                myTile[x + _x + deltaX, y + _y + deltaY],
                                Convert.ToInt32(
                                    Math.Floor(
                                        Math.Sqrt(
                                            (_x + deltaX) * (_x + deltaX) + (_y + deltaY) * (_y + deltaY))
                                    )
                                )
                            )
                        );
                    }
                }
            }
        }

        public bool MyTileType(int dx, int dy, MyTile.TileType type)
        {
            if (myTile[dx, dy] != null && myTile[dx, dy].type == type)
                return true;

            return false;
        }

    }

    public class GrapthTools
    {
        private GenerateGraph generatePath;

        private static GrapthTools _instance;

        public static GrapthTools GetInstance()
        {
            if(_instance == null)
                _instance = new GrapthTools();

            return _instance;
        }
        public GrapthTools()
        {
            generatePath = GenerateGraph.generateGraph;
           

        }

        public Vector2Int RoundToTileCoord(float x, float y)
        {
            var tileCoord = new Vector2Int(
                Mathf.RoundToInt(x - generatePath.offset.x - generatePath.tileOffset),
                Mathf.RoundToInt(y - generatePath.offset.y - generatePath.tileOffset)
            );
            return tileCoord;
        }

        public Vector2 TileCoordToDecardCoord(int x, int y)
        {
            var decardCoord = new Vector2(
                x + generatePath.offset.x + generatePath.tileOffset,
                y + generatePath.offset.y + generatePath.tileOffset
            );
            return decardCoord;
        }
    }

    [Serializable]
    public class MyTile
    {
        public Vector2 pos;
        public Vector2Int tileCoord;
        public TileType type;
        public List<Neighbors> neighbors;
        public MyTile cameFrom;
        public Vector2Int offset;

        public MyTile()
        {
            neighbors = new List<Neighbors>();
        }

        public enum TileType : int
        {
            Hor,
            Angle
        }
    }

    [Serializable]
    public class Neighbors
    {
        public MyTile neighbor;
        public int weight;

        public Neighbors(MyTile neighbor, int weight)
        {
            this.neighbor = neighbor;
            this.weight = weight;
        }
    }

    [Serializable]
    public class CreatePath
    {
        public Vector2Int tilePos;
        public MyTile.TileType tileType;
        public Vector2Int from;
        public Vector2Int offset;

        public CreatePath(Vector2Int from, Vector2Int tilePos, MyTile.TileType tileType, Vector2Int offset)
        {
            this.from = from;
            this.tilePos = tilePos;
            this.tileType = tileType;
            this.offset = offset;
        }
    }

    [Serializable]
    public class Path
    {
        public Vector2Int tilePos;
        public MyTile.TileType tileType;
        public Vector2Int offset;

        public Path(Vector2Int tilePos, MyTile.TileType tileType, Vector2Int offset)
        {
            this.tilePos = tilePos;
            this.tileType = tileType;
            this.offset = offset;
        }
    }
}