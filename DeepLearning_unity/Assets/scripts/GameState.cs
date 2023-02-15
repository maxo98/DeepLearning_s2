using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public enum TileStates
{
    Start,
    Obstacle,
    End,
    Empty
}

public enum AgentMovements
{
    Up,
    Down,
    Left,
    Right
}

public class GameState : MonoBehaviour
{
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private GameObject agent;
    [SerializeField] private List<GameObject> tiles;
    private float[,] _grid;

    void InitGameState()
    {
        var reward = 0;
        foreach (var go in tiles)
        {
            if (go.CompareTag("End"))
                reward = 1;
            var position = go.transform.position;
            _grid[(int) Math.Round(position.x), (int) Math.Round(position.z)] = reward;
        }
        for (var i = 0; i < gridWidth; i++)
        {
            for (var j = 0; j < gridHeight; j++)
            {
                if (tiles[i + j].CompareTag("End"))
                    _grid[i, j] = 1;
                else
                    _grid[i, j] = 0;
            }
        }
    }

    public float GetReward(AgentMovements move, int x, int y)
    {
        var newPosition = agent.transform.position;
        switch (move)
        {
            case AgentMovements.Left when agent.transform.position.x > 0:
                newPosition.x--;
                break;
            case AgentMovements.Right when agent.transform.position.x < gridWidth - 1:
                newPosition.x++;
                break;
            case AgentMovements.Up when agent.transform.position.z < gridHeight - 1:
                newPosition.z++;
                break;
            case AgentMovements.Down when agent.transform.position.z > 0:
                newPosition.z--;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(move), move, null);
        }

        if (!tiles[(int)Math.Round(newPosition.x + newPosition.z)].CompareTag("Obstacle"))
            return _grid[(int)Math.Round(newPosition.x), (int) Math.Round(newPosition.z)];
        var position = agent.transform.position;
        return _grid[(int)Math.Round(position.x), (int)Math.Round(position.z)];
    }

    public void MoveAgent(AgentMovements move)
    {
        var newPosition = agent.transform.position;
        switch (move)
        {
            case AgentMovements.Left when agent.transform.position.x > 0:
                newPosition.x--;
                break;
            case AgentMovements.Right when agent.transform.position.x < gridWidth - 1:
                newPosition.x++;
                break;
            case AgentMovements.Up when agent.transform.position.z < gridHeight - 1:
                newPosition.z++;
                break;
            case AgentMovements.Down when agent.transform.position.z > 0:
                newPosition.z--;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(move), move, null);
        }
        if(!tiles[(int) Math.Round(newPosition.x + newPosition.z)].CompareTag("Obstacle"))
            agent.transform.position = newPosition;
    }

    bool CheckGameOver()
    {
        var position = agent.transform.position;
        return tiles[(int) Math.Round(position.x + position.z)].CompareTag("End");
    }

}
