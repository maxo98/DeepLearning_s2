using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using Random = System.Random;

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
        AgentPosition = new Vector2Int(agent.x, agent.y);
    }

    public State(State state)
    {
        AgentPosition = state.AgentPosition;
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

    public bool Equals(State state)
    {
        return AgentPosition.x == state.AgentPosition.x && AgentPosition.y == state.AgentPosition.y;
    }
}
    
public interface IGameState
{
    public void InitGrid();
    
    float GetReward(State state, State otherState);

    Vector2Int GetAgentPosition();
    
    State MoveAgent(AgentMovements move);
    
    State CheckMove(AgentMovements move, State state);
    
    bool CheckGameOver(State state);

    void SetRandomGameState();

    public List<State> GetAllStates();

    State GetState();

    State CopyState(State state);

    bool CompareStates(State state1, State state2);
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

    public static void ShuffleArray<T>(this Random rng, T[] array)
    {
        var n = array.Length;
        while (n > 1) 
        {
            var k = rng.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }
}