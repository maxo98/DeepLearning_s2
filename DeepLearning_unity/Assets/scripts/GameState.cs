using System;
using System.Collections.Generic;
using UnityEngine;

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

public class State
{
    public Vector2Int AgentPosition { get; protected set; }

    public State(Vector3 agent)
    {
        AgentPosition = GameStateUtil.PositionToGrid(agent);
    }

    public State(Vector2Int agent)
    {
        AgentPosition = agent;
    }
    
    public void SetAgentPosition(int x, int y)
    {
        AgentPosition = new Vector2Int(x, y);
    }
    
    public void SetAgentPosition(Vector3 agent)
    {
        AgentPosition = GameStateUtil.PositionToGrid(agent);
    }
    
    public void SetAgentPosition(Vector2Int agent)
    {
        AgentPosition = agent;
    }
}
    
public interface IGameState
{
    public void InitGrid();
    
    float GetReward(State state);
    
    Vector2Int GetAgentPosition();
    
    State MoveAgent(AgentMovements move);
    
    State CheckMove(AgentMovements move, State state);
    
    bool CheckGameOver(State state);

    void SetRandomGameState();

    public List<State> GetAllStates();

    State GetState();

    int GetGridWidth();

    int GetGridHeight();
}

public static class GameStateUtil
{
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
    
    public static Vector2Int PositionToGrid(Vector3 position)
    {
        var x = (int)Math.Round(position.x);
        var y = (int)Math.Round(position.z);
        return new Vector2Int(x, y);
    }
    
    public static Vector3 GridToAgentPosition(Vector2Int position, float height)
    {
        return new Vector3(position.x, height, position.y);
    }
}