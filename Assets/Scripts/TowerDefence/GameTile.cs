using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class GameTile : MonoBehaviour
    {
        [SerializeField] private Transform arrow;
        [SerializeField] private GameTile m_NorthTile, m_SouthTile, m_EastTile, m_WestTile;
        [SerializeField] private GameTile m_NextOnPath; // 走到该网格后下一格该走哪里
        [SerializeField] private int m_distance; // 离目的地的最短距离
        public bool IsAlternative { get; set; }
        #region 提供给箭头的旋转角度

        private static readonly Quaternion 
            NorthRotation = Quaternion.Euler(90f, 0f, 0f),
            EastRotation = Quaternion.Euler(90f, 90f, 0f),
            SouthRotation = Quaternion.Euler(90f, 180f, 0f),
            WestRotation = Quaternion.Euler(90f, 270f, 0f);
        #endregion
        /// <summary>
        /// 当前网格是否可达目的地
        /// </summary>
        public bool HasPath => m_distance != int.MaxValue;
        /// <summary>
        /// 绑定两个东西相邻网格之间的东西邻居关系
        /// </summary>
        /// <param name="eastTile">东方网格</param>
        /// <param name="westTile">西方网格</param>
        public static void MakeEastWestNeighbor(GameTile eastTile, GameTile westTile)
        {
            // 如果assert里面的条件不成立，程序会立即中断并抛出错误信息
            Debug.Assert(westTile.m_EastTile == null && eastTile.m_WestTile == null,
                $"Redefined neighbors! at {eastTile.gameObject.name} and {westTile.gameObject.name}");
            eastTile.m_WestTile = westTile;
            westTile.m_EastTile = eastTile;
        }

        /// <summary>
        /// 绑定两个南北相邻网格之间的南北邻居关系
        /// </summary>
        /// <param name="northTile">北方网格</param>
        /// <param name="southTile">南方网格</param>
        public static void MakeNorthSouthNeighbor(GameTile northTile, GameTile southTile)
        {
            Debug.Assert(northTile.m_SouthTile == null && southTile.m_NorthTile == null,
                $"Redefined neighbors! at {northTile.gameObject.name} and {southTile.gameObject.name}");
            northTile.m_SouthTile = southTile;
            southTile.m_NorthTile = northTile;
        }

        public void ShowPath()
        {
            if (this.m_distance == 0)
            {
                arrow.gameObject.SetActive(false);
                return;
            }
            arrow.gameObject.SetActive(true);
            arrow.localRotation = m_NextOnPath switch
            {
                var n when n == m_NorthTile => NorthRotation,
                var e when e == m_EastTile  => EastRotation,
                var s when s == m_SouthTile => SouthRotation,
                _                                   => WestRotation // _ 代表默认情况
            };
        }
        
        #region 寻路算法相关

        /// <summary>
        /// 清空当前网格的下一格以及距离数据
        /// </summary>
        public void ClearPath()
        {
            m_distance = int.MaxValue;
            m_NextOnPath = null;
        }

        /// <summary>
        /// 标记当前网格为目的地
        /// </summary>
        public void BecomeDestination()
        {
            m_distance = 0;
            m_NextOnPath = null;
        }

        /// <summary>
        /// 将当前网格的邻居的next网格设置为自己
        /// </summary>
        /// <param name="neighbor">被拓展的邻居网格</param>
        /// <returns>被拓展的邻居网格</returns>
        private GameTile GrowPathTo(GameTile neighbor)
        {
            Debug.Assert(HasPath, $"No Path at {this.gameObject.name}");
            if (neighbor == null || neighbor.HasPath)
            {
                return null;
            }
            neighbor.m_distance = m_distance + 1;
            neighbor.m_NextOnPath = this;
            return neighbor;
        }
        
        public GameTile GrowPathNorth() => GrowPathTo(m_NorthTile);
        public GameTile GrowPathSouth() => GrowPathTo(m_SouthTile);
        public GameTile GrowPathEast() => GrowPathTo(m_EastTile);
        public GameTile GrowPathWest() => GrowPathTo(m_WestTile);
        
        #endregion
        
        
    }
}
