using System;
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
    
public interface IGameState
{
    float GetReward(int x, int y);
    
    Vector2Int GetAgentPosition();
    
    Vector2Int MoveAgent(AgentMovements move);
    
    Vector2Int CheckMove(AgentMovements move, Vector2Int position);
    
    bool CheckGameOver(int x, int y);

    void SetRandomGameState();

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
}