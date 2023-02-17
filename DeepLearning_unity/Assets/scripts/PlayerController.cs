using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerGo;
    private IGameState _gameState;

    private void Start()
    {
        _gameState = gameManagerGo.GetComponent<IGameState>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            _gameState.MoveAgent(AgentMovements.Up);
        if (Input.GetKeyDown(KeyCode.S))
            _gameState.MoveAgent(AgentMovements.Down);
        if (Input.GetKeyDown(KeyCode.Q))
            _gameState.MoveAgent(AgentMovements.Left);
        if (Input.GetKeyDown(KeyCode.D))
            _gameState.MoveAgent(AgentMovements.Right);
    }
}
