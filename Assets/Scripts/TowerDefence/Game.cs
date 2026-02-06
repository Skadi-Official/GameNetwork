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
        private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
        private void Awake()
        {
            board.Initialize(boardSize, tileContentFactory);
        }

        private void OnValidate()
        {
            if(boardSize.x < 2) boardSize.x = 2;
            if(boardSize.y < 2) boardSize.y = 2;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleTouch();
            }
        }

        private void HandleTouch()
        {
            var tile = board.GetTile(TouchRay);
            if(tile == null) return;
            board.ToggleDestination(tile);
        }
    }
}
