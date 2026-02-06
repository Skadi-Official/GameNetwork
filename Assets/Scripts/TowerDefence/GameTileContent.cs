using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    /// <summary>
    /// 用于定义和标识格子的具体内容
    /// </summary>
    public class GameTileContent : MonoBehaviour
    {
        [SerializeField] private GameTileContentType type;
        private GameTileContentFactory m_OriginFactory;
        /// <summary>
        /// 格子所属的类型
        /// </summary>
        public GameTileContentType Type => type;
        /// <summary>
        /// 创建格子的源工厂
        /// </summary>
        public GameTileContentFactory OriginFactory
        {
            get => m_OriginFactory;
            set
            {
                Debug.Assert(m_OriginFactory == null, "Redefined origin factory");
                m_OriginFactory = value;
            }
        }


        #region 工厂相关逻辑

        /// <summary>
        /// 回收当前对象
        /// </summary>
        public void Recycle()
        {
            m_OriginFactory.Reclaim(this);
        }

        #endregion
    }

}