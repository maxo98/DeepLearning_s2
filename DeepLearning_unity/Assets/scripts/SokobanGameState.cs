using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SokobanState : State
{
    private readonly List<Vector2Int> _cratesPosition;

    public SokobanState(Vector3 agent,IReadOnlyCollection<GameObject> crates) : base(agent)
    {
        _cratesPosition = new List<Vector2Int>(crates.Count);
        foreach (var position in crates.Select(crate => crate.transform.position))
        {
            _cratesPosition.Add(GameStateUtil.PositionToGrid(position));
        }
    }
    
    public SokobanState(SokobanState state) : base(state)
    {
        _cratesPosition = new List<Vector2Int>(state._cratesPosition.Count);
        foreach (var cratePosition in state._cratesPosition)
        {
            _cratesPosition.Add(new Vector2Int(cratePosition.x , cratePosition.y));
        }
    }

    public new bool Equals(State state)
    {
        var sokobanState = (SokobanState)state;
        if (AgentPosition.x != state.AgentPosition.x || AgentPosition.y != state.AgentPosition.y) return false;
        if (_cratesPosition.Count != sokobanState._cratesPosition.Count) return false;
        for (var i = 0; i < _cratesPosition.Count; i++)
        {
            if (!_cratesPosition[i].Equals(sokobanState._cratesPosition[i])) return false;
        }
        return true;
    }
    
    public int GetCratesCount()
    {
        return _cratesPosition.Count;
    }

    public Vector2Int GetCratePosition(int index)
    {
        return _cratesPosition[index];
    }

    public void SetCratePosition(int index, Vector2Int position)
    {
        _cratesPosition[index] = position;
    }

    public void SetCratePosition(int index, int x, int y)
    {
        _cratesPosition[index] = new Vector2Int(x, y);
    }
}


public class SokobanGameState : MonoBehaviour, IGameState
{
    public int gridWidth;
    public int gridHeight;
    [SerializeField] private GameObject agent;
    [SerializeField] private List<BlockData> tiles;
    [SerializeField] private List<GameObject> crates;
    private Tuple<BlockData, float>[,] _grid;
    private SokobanState _state;

    private void Start()
    {
        _grid = new Tuple<BlockData, float>[gridWidth, gridHeight];
        _state = new SokobanState(agent.transform.position, crates);
        InitGrid();
    }

    public void InitGrid()
    {
        foreach (var block in tiles)
        {
            _grid[block.PosX, block.PosY] = new Tuple<BlockData, float>(block, block.state == BlockStates.End ? 1f : 0f);
        }
    }

    public float GetReward(State state, State otherState)
    {
        if (CompareStates(state, otherState))
             return 0f;
        var reward = GetRewardForCrates((SokobanState)state);
        var otherReward = GetRewardForCrates((SokobanState)otherState);
        if (reward < otherReward)
            reward = -1f;
        else if (reward.Equals(otherReward))
            reward = 0f;
        if (reward.Equals(((SokobanState)state).GetCratesCount()))
            reward *= 2;
        return reward;
    }

    private float GetRewardForCrates(SokobanState sokobanState)
    {
        var reward = 0f;
        for(var i = 0; i < sokobanState.GetCratesCount(); i++)
        {
            var cratePosition = sokobanState.GetCratePosition(i);
            if (GetBlockState(cratePosition.x, cratePosition.y) == BlockStates.End)
                reward += 1f;
        }
        return reward;
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

    private bool GetAtPosition(SokobanState state, Vector2Int cratePosition)
    {
        for (var i = 0; i < state.GetCratesCount(); i++)
        {
            if (state.GetCratePosition(i) == cratePosition)
                return true;
        }
        return false;
    }

    public State MoveAgent(AgentMovements move)
    {
        _state = (SokobanState) CheckMove(move,  _state);
        agent.transform.position = GameStateUtil.GridToAgentPosition(_state.AgentPosition, agent.transform.position.y);
        for (var i = 0; i < _state.GetCratesCount(); i++)
        {
            crates[i].transform.position =
                GameStateUtil.GridToAgentPosition(_state.GetCratePosition(i), crates[i].transform.position.y);
        }
        if(CheckGameOver(_state))
            Debug.Log("Game Over !");
        return _state;
    }

    public State CheckMove(AgentMovements move, State state)
    {
        var newState = CopyState(state);
        var newPosition = state.AgentPosition;
        switch (move)
        {
            case AgentMovements.Left:
                if(newPosition.x > 0)
                    newPosition.x--;
                break;
            case AgentMovements.Right:
                if(newPosition.x < gridWidth - 1)
                    newPosition.x++;
                break;
            case AgentMovements.Up:
                if (newPosition.y < gridHeight - 1)
                    newPosition.y++;
                break;
            case AgentMovements.Down:
                if (newPosition.y > 0)
                    newPosition.y--;
                break;
            case AgentMovements.Length:
                Debug.LogError("length : ne devrait pas passer ici");
                break;
            default:
                Debug.LogError("default : ne devrait pas passer ici");
                break;
        }
        if (_grid[newPosition.x, newPosition.y].Item1.state != BlockStates.Obstacle && CanMoveCrate(move, (SokobanState)newState, newPosition))
        {
            newState.SetAgentPosition(newPosition);
        }
        return newState;
    }

    private bool CanMoveCrate(AgentMovements move, SokobanState sokobanState, Vector2Int playerPosition)
    {
        for(var i = 0; i < sokobanState.GetCratesCount(); i++)
        {
            var cratePosition = sokobanState.GetCratePosition(i);
            if (cratePosition != playerPosition) continue;
            switch (move)
            {
                case AgentMovements.Left:
                {
                    if(cratePosition.x > 0)
                        cratePosition.x--;
                    break;
                }
                case AgentMovements.Right:
                {
                    if (cratePosition.x < gridWidth - 1)
                        cratePosition.x++;
                    break;
                }
                case AgentMovements.Up:
                {
                    if (cratePosition.y < gridHeight - 1)
                        cratePosition.y++;
                    break;
                }
                case AgentMovements.Down:
                {
                    if (cratePosition.y > 0)
                        cratePosition.y--;
                    break;
                }
                case AgentMovements.Length:
                    Debug.LogError("length : ne devrait pas passer ici");
                    break;
                default:
                    Debug.LogError("default : ne devrait pas passer ici");
                    break;
            }

            if (_grid[cratePosition.x, cratePosition.y].Item1.state == BlockStates.Obstacle || GetAtPosition(sokobanState, cratePosition)) return false;
            sokobanState.SetCratePosition(i, cratePosition);
            return true;
        }

        return true;
    }

    public bool CheckGameOver(State state)
    {
        var sokobanState = (SokobanState) state;
        for(var i = 0; i < sokobanState.GetCratesCount(); i++)
        {
            var cratePosition = sokobanState.GetCratePosition(i);
            if (GetBlockState(cratePosition.x, cratePosition.y) != BlockStates.End)
                return false;
        }
        return true;
    }
    
    public void SetRandomGameState()
    {
       
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

    public State CopyState(State state)
    {
        return new SokobanState((SokobanState)state);
    }

    public bool CompareStates(State state1, State state2)
    {
        var sokobanState1 = (SokobanState)state1;
        return sokobanState1.Equals((SokobanState)state2);
    }

    private bool CompareCrates(State state1, State state2)
    {
        var sokobanState1 = (SokobanState)state1;
        var sokobanState2 = (SokobanState)state2;
        for (var i = 0; i < sokobanState1.GetCratesCount(); i++)
        {
            if (!sokobanState1.GetCratePosition(i).Equals(sokobanState2.GetCratePosition(i)))
            {
                return false;
            }
        }
        return true;
    }
    
}
