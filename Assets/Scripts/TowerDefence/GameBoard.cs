using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TowerDefence
{
    public class GameBoard : MonoBehaviour
    {
        
        [SerializeField] private Transform ground;          // 地板
        [SerializeField] private GameTile tilePrefab;       // 地图中的网格所使用的预制体
        private Vector2Int m_Size;
        private GameTile[] m_Tiles;                         // 记录了所有网格
        private Queue<GameTile> m_SearchFrontier = new();   // 记录“正在排队等待去搜索邻居”的网格
        public void Init(Vector2Int size)
        {
            this.m_Size = size;
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

            FindPath();
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
        
        #region 寻路算法实现

        private void FindPath()
        {
            foreach (var tile in m_Tiles)
            {
                tile.ClearPath();
            }
            m_Tiles[m_Tiles.Length / 2].BecomeDestination();
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
                tile.ShowPath();
            }
        }

        #endregion
    }
}