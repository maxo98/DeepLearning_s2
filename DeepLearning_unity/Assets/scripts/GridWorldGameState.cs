using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public enum BlockStates
{
    None,
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
    Right,
    Length
}

public class GridWorldGameState : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;
    [SerializeField] private GameObject agent;
    [SerializeField] private List<BlockData> tiles;
    private Tuple<BlockData, float>[,] _grid;

    private void Start()
    {
        _grid = new Tuple<BlockData, float>[gridWidth, gridHeight];
        InitGameState();
    }

    private void InitGameState()
    {
        foreach (var block in tiles)
        {
            _grid[block.PosX, block.PosY] = new Tuple<BlockData, float>(block, block.state == BlockStates.End ? 1f : 0f);
        }
    }

    public float GetReward(int x, int y)
    {
        return _grid[x, y].Item2;
    }
    
    public void SetReward(int x, int y, float reward)
    {
        _grid[x, y] = new Tuple<BlockData, float>(_grid[x, y].Item1, reward);
    }

    public BlockStates GetBlockState(int x, int y)
    {
        return _grid[x, y].Item1.state;
    }

    public Vector2Int GetAgentPosition()
    {
        return PositionToGrid(agent.transform.position);
    }

    public Vector2Int MoveAgent(AgentMovements move)
    {
        var position = PositionToGrid(agent.transform.position);
        agent.transform.position = GridToAgentPosition(CheckMove(move, position));
        if(CheckGameOver(position.x, position.y))
            Debug.Log("Game Over !");
        return position;
    }

    public Vector2Int CheckMove(AgentMovements move, Vector2Int position)
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
            case AgentMovements.Up when newPosition.y < gridHeight - 1:
                newPosition.y++;
                break;
            case AgentMovements.Down when newPosition.y > 0:
                newPosition.y--;
                break;
            default:
                return position;
        }
        var playerPosition = newPosition;
        return _grid[playerPosition.x, playerPosition.y].Item1.state != BlockStates.Obstacle ? newPosition : position;
    }

    public bool CheckGameOver(int x, int y)
    {
        return _grid[x, y].Item1.state == BlockStates.End;
    }
    
    public static BlockStates TileStateFromTag(GameObject tileGo)
    {
        if (tileGo.CompareTag("Start"))
        {
            return BlockStates.Start;
        }
        if (tileGo.CompareTag("Obstacle"))
        {
            return BlockStates.Obstacle;
        }
        if (tileGo.CompareTag("End"))
        {
            return BlockStates.End;
        }
        return tileGo.CompareTag("Empty") ? BlockStates.Empty : BlockStates.None;
    }

    private Vector2Int PositionToGrid(Vector3 position)
    {
        var x = (int)Math.Round(position.x);
        var y = (int)Math.Round(position.z);
        return new Vector2Int(x, y);
    }

    private Vector3 GridToAgentPosition(Vector2Int position)
    {
        return new Vector3(position.x, agent.transform.position.y, position.y);
    }

    public void SetRandomGameState()
    {
        var x = 0;
        var y = 0;
        do
        {
            x = Random.Range(0, gridWidth);
            y = Random.Range(0, gridHeight);
        } while (_grid[x, y].Item1.state != BlockStates.Empty && _grid[x, y].Item1.state != BlockStates.Start);
        agent.transform.position = new Vector3(x, 0.75f, y);
    }

}
