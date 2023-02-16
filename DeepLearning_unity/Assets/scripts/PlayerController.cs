using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GridWorldGameState gameState;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            gameState.MoveAgent(AgentMovements.Up);
        if (Input.GetKeyDown(KeyCode.S))
            gameState.MoveAgent(AgentMovements.Down);
        if (Input.GetKeyDown(KeyCode.Q))
            gameState.MoveAgent(AgentMovements.Left);
        if (Input.GetKeyDown(KeyCode.D))
            gameState.MoveAgent(AgentMovements.Right);
    }
}
