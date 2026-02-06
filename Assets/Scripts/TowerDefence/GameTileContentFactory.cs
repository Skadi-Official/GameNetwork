using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefence
{
    [CreateAssetMenu(fileName = "GameTileContentFactory", menuName = "TowerDefence/Game Tile Content Factory")]
    public class GameTileContentFactory : ScriptableObject
    {
        [SerializeField] private GameTileContent destinationPrefab = null;
        [SerializeField] private GameTileContent emptyPrefab = null;
        [SerializeField] private GameTileContent wallPrefab = null;
        private Scene m_ContentScene;       // 工厂实例化的对象需要放入的场景
        /// <summary>
        /// 回收一个GameTileContent实例
        /// </summary>
        /// <param name="gameTileContent">需要被回收的实例</param>
        public void Reclaim(GameTileContent gameTileContent)
        {
            Debug.Assert(gameTileContent.OriginFactory == this, "Wrong factory reclaimed!");
            Destroy(gameTileContent.gameObject);
        }
        
        // 内部使用，会实际生成一个示例
        private GameTileContent Get (GameTileContent prefab) {
            GameTileContent instance = Instantiate(prefab);
            instance.OriginFactory = this;
            MoveToFactoryScene(instance.gameObject);
            return instance;
        }
        /// <summary>
        /// 提供给外部调用，获取一个GameTileContent实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameTileContent Get (GameTileContentType type) {
            switch (type) {
                case GameTileContentType.Destination: return Get(destinationPrefab);
                case GameTileContentType.Empty: return Get(emptyPrefab);
                case GameTileContentType.Wall: return Get(wallPrefab);
            }
            Debug.Assert(false, "Unsupported type: " + type);
            return null;
        }
        
        private void MoveToFactoryScene(GameObject o)
        {
            if (!m_ContentScene.isLoaded)
            {
                if (Application.isEditor)
                {
                    m_ContentScene = SceneManager.GetSceneByName(name);
                    if(!m_ContentScene.isLoaded) m_ContentScene = SceneManager.CreateScene(name);
                }
                else
                {
                    m_ContentScene = SceneManager.CreateScene(name);
                }
            }
            SceneManager.MoveGameObjectToScene(o, m_ContentScene);
        }
    }

}