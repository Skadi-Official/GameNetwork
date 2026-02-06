using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TowerDefence
{
    public class GameBoard : MonoBehaviour
    {
        
        [SerializeField] private Transform ground;          // 地板
        [SerializeField] private GameTile tilePrefab;       // 地图中的网格所使用的预制体
        private Vector2Int m_Size;                          // 地图的大小
        private GameTile[] m_Tiles;                         // 记录了所有网格
        private Queue<GameTile> m_SearchFrontier = new();   // 记录“正在排队等待去搜索邻居”的网格
        private GameTileContentFactory m_TileContentFactory;
        public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
        {
            this.m_Size = size;
            this.m_TileContentFactory = contentFactory;
            ground.localScale = new Vector3(size.x, size.y, 1f);
            var offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
            
            m_Tiles = new GameTile[size.x * size.y];
            for (int y = 0, count = 0; y < size.y; y++)
            {
                for (var x = 0; x < size.x; x++, count++)
                {
                    var tile = Instantiate(tilePrefab, this.transform);
                    tile.transform.localPosition = new Vector3(x - offset.x, 0, y - offset.y);
                    tile.gameObject.name = $"Tile({x}, {y}), {count}";
                    tile.Content = contentFactory.Get(GameTileContentType.Empty);
                    // 对于计算机内部二进制偶数的最低位总是 0，其实就是判断是不是偶数，但在超大规模数据下比%2判断更快
                    tile.IsAlternative = (x & 1) == 0;
                    if((y & 1) == 0) tile.IsAlternative = !tile.IsAlternative;
                    m_Tiles[count] = tile;
                    if (x > 0)
                    {
                        GameTile.MakeEastWestNeighbor(tile, m_Tiles[count - 1]);
                    }

                    if (y > 0)
                    {
                        GameTile.MakeNorthSouthNeighbor(tile, m_Tiles[count - size.x]);
                    }
                }
            }

            ToggleDestination(m_Tiles[m_Tiles.Length / 2]);
        }

        public GameTile GetTile(Ray ray)
        {
            if (!Physics.Raycast(ray, out var hit)) return null;
            
            var x = (int)(hit.point.x + m_Size.x * 0.5f);
            var y = (int)(hit.point.z + m_Size.y * 0.5f);
            if (x >= 0 && x <= m_Size.x && y >= 0 && y <= m_Size.y)
            {
                return m_Tiles[x + y * m_Size.x];
            }
            return null;
        }

        public void ToggleDestination(GameTile tile)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.Content = m_TileContentFactory.Get(GameTileContentType.Empty);
                // 保证至少有一个终点
                if (!FindPath())
                {
                    tile.Content = m_TileContentFactory.Get(GameTileContentType.Destination);
                    FindPath();
                }
            }
            else if(tile.Content.Type == GameTileContentType.Empty)
            {
                tile.Content = m_TileContentFactory.Get(GameTileContentType.Destination);
                FindPath();
            }
        }

        public void ToggleWall(GameTile tile)
        {
            if (tile.Content.Type == GameTileContentType.Wall)
            {
                tile.Content = m_TileContentFactory.Get(GameTileContentType.Empty);
                FindPath();
            }
            else if (tile.Content.Type == GameTileContentType.Empty)
            {
                tile.Content = m_TileContentFactory.Get(GameTileContentType.Wall);
                if (!FindPath())
                {
                    tile.Content = m_TileContentFactory.Get(GameTileContentType.Empty);
                }
            }
        }
        
        #region 寻路算法实现

        private bool FindPath()
        {
            Debug.Log("start find path");
            foreach (var tile in m_Tiles)
            {
                if (tile.Content.Type == GameTileContentType.Destination)
                {
                    tile.BecomeDestination();
                    m_SearchFrontier.Enqueue(tile);
                }
                else
                {
                    tile.ClearPath();
                }
            }

            if (m_SearchFrontier.Count == 0) return false;
            m_SearchFrontier.Enqueue(m_Tiles[m_Tiles.Length / 2]);
            while (m_SearchFrontier.Count > 0)
            {
                var tile = m_SearchFrontier.Dequeue();
                if(tile == null) continue;
                if (tile.IsAlternative)
                {
                    m_SearchFrontier.Enqueue(tile.GrowPathNorth());
                    m_SearchFrontier.Enqueue(tile.GrowPathSouth());
                    m_SearchFrontier.Enqueue(tile.GrowPathEast());
                    m_SearchFrontier.Enqueue(tile.GrowPathWest());
                }
                else
                {
                    m_SearchFrontier.Enqueue(tile.GrowPathWest());
                    m_SearchFrontier.Enqueue(tile.GrowPathEast());
                    m_SearchFrontier.Enqueue(tile.GrowPathSouth());
                    m_SearchFrontier.Enqueue(tile.GrowPathNorth());
                }
            }

            foreach (var tile in m_Tiles)
            {
                if (!tile.HasPath)
                {
                    return false;
                }
            }
            
            foreach (var tile in m_Tiles)
            {
                tile.ShowPath();
            }

            return true;
        }

        #endregion
    }
}