using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class GridWorldGameState : MonoBehaviour, IGameState
{
    public int gridWidth;
    public int gridHeight;
    [SerializeField] private GameObject agent;
    [SerializeField] private List<BlockData> tiles;
    private Tuple<BlockData, float>[,] _grid;
    private State _state;

    private void Start()
    {
        _grid = new Tuple<BlockData, float>[gridWidth, gridHeight];
        _state = new State(agent.transform.position);
        InitGrid();
    }

    public void InitGrid()
    {
        foreach (var block in tiles)
        {
            _grid[block.PosX, block.PosY] = new Tuple<BlockData, float>(block, block.state == BlockStates.End ? 1f : 0f);
        }
    }

    public float GetReward(State state)
    {
        return _grid[state.AgentPosition.x, state.AgentPosition.y].Item2;
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
        return GameStateUtil.PositionToGrid(agent.transform.position);
    }

    public State MoveAgent(AgentMovements move)
    {
        _state = CheckMove(move,  _state);
        agent.transform.position = GameStateUtil.GridToAgentPosition(_state.AgentPosition, agent.transform.position.y);
        if(CheckGameOver(_state))
            Debug.Log("Game Over !");
        return _state;
    }

    public State CheckMove(AgentMovements move, State state)
    {
        var newPosition = state.AgentPosition;
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
            case AgentMovements.Length:
            default:
                return state;
        }
        var playerPosition = state.AgentPosition;
        if (_grid[newPosition.x, newPosition.y].Item1.state != BlockStates.Obstacle)
        {
            playerPosition = newPosition;
        }
        return new State(playerPosition);
    }

    public bool CheckGameOver(State state)
    {
        return _grid[state.AgentPosition.x, state.AgentPosition.y].Item1.state == BlockStates.End;
    }

    public void SetRandomGameState()
    {
        int x;
        int y;
        do
        {
            x = Random.Range(0, gridWidth);
            y = Random.Range(0, gridHeight);
        } while (_grid[x, y].Item1.state != BlockStates.Empty && _grid[x, y].Item1.state != BlockStates.Start);
        agent.transform.position = new Vector3(x, 0.75f, y);
    }

    public List<State> GetAllStates()
    {
        var list = new List<State> { _state };
        return list;
    }

    public State GetState()
    {
        return _state;
    }
}
