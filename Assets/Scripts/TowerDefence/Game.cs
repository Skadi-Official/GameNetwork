using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);
        [SerializeField] private GameBoard board;
        [SerializeField] private GameTileContentFactory tileContentFactory;
        private void Awake()
        {
            board.Init(boardSize);
        }

        private void OnValidate()
        {
            if(boardSize.x < 2) boardSize.x = 2;
            if(boardSize.y < 2) boardSize.y = 2;
        }
    }
}
