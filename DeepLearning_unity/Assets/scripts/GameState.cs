using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;


public enum TileStates
{
    None,
    Start,
    Obstacle,
    End,
    Empty
}

public enum AgentMovements
{
    None,
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
    [SerializeField] private List<BlockData> tiles;
    private Tuple<BlockData, float>[,] _grid;
    private Vector2Int _playerPosition;

    private void Start()
    {
        _grid = new Tuple<BlockData, float>[gridWidth, gridHeight];
        InitGameState();
    }

    void InitGameState()
    {
        foreach (var block in tiles)
        {
            _grid[block.PosX, block.PosY] = new Tuple<BlockData, float>(block, 0f);
            SetReward(block.PosX, block.PosY, block.state == TileStates.End ? 1 : 0);
        }
    }

    public float GetReward(int x, int y)
    {
        return _grid[x, y].Item2;
    }

    public Vector2Int GetAgentPosition()
    {
        return PositionToGrid(agent.transform.position);
    }

    public Vector2Int MoveAgent(AgentMovements move)
    {
        var position = agent.transform.position;
        agent.transform.position = CheckMove(move, position);
        if(CheckGameOver())
            Debug.Log("Game Over !");
        return PositionToGrid(position);
    }

    Vector3 CheckMove(AgentMovements move, Vector3 position)
    {
        var newPosition = position;
        switch (move)
        {
            case AgentMovements.Left when newPosition.x > 0:
                newPosition.x--;
                break;
            case AgentMovements.Right when newPosition.x < gridWidth - 1:
                newPosition.x++;
                break;
            case AgentMovements.Up when newPosition.z < gridHeight - 1:
                newPosition.z++;
                break;
            case AgentMovements.Down when newPosition.z > 0:
                newPosition.z--;
                break;
            case AgentMovements.None:
                break;
        }
        var playerPosition = PositionToGrid(newPosition);
        return _grid[playerPosition.x, playerPosition.y].Item1.state != TileStates.Obstacle ? newPosition : position;
    }

    bool CheckGameOver()
    {
        var position = GetAgentPosition();
        return _grid[position.x, position.y].Item1.state == TileStates.End;
    }
    
    public static TileStates TileStateFromTag(GameObject tileGo)
    {
        if (tileGo.CompareTag("Start"))
        {
            return TileStates.Start;
        }
        if (tileGo.CompareTag("Obstacle"))
        {
            return TileStates.Obstacle;
        }
        if (tileGo.CompareTag("End"))
        {
            return TileStates.End;
        }
        return tileGo.CompareTag("Empty") ? TileStates.Empty : TileStates.None;
    }

    private Vector2Int PositionToGrid(Vector3 position)
    {
        var x = (int)Math.Round(position.x);
        var y = (int)Math.Round(position.z);
        return new Vector2Int(x, y);
    }

    public void SetReward(int x, int y, float reward)
    {
        _grid[x, y] = new Tuple<BlockData, float>(_grid[x, y].Item1, reward);
    }

}
