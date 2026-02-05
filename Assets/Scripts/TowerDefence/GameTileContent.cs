using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class GameTileContent : MonoBehaviour
    {
        [SerializeField] private GameTileContentType type;
        private GameTileContentFactory m_OriginFactory;
        public GameTileContentType Type => type;

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

        public void Recycle()
        {
            m_OriginFactory.Reclaim(this);
        }

        #endregion
    }

}