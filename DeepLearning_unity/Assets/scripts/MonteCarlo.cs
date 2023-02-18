using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public struct MonteCarloStruct
{
    public State State { get; }
    public readonly float[] ReturnsForState;
    public readonly int[] NForState;
    public readonly float[] VsForState;

    public MonteCarloStruct(State state)
    {
        State = state;
        const int nbMoves = (int)AgentMovements.Length;
        ReturnsForState = new float[nbMoves];
        NForState = new int[nbMoves];
        VsForState = new float[nbMoves];
    }
}


public class MonteCarlo : MonoBehaviour
{
    [SerializeField] private GameObject gameManager;
    private IGameState _gameState;

    [SerializeField] private bool isExploringStart;

    [SerializeField] private bool isEveryVisit;

    [SerializeField] private int maxEpochs = 1000;
    [SerializeField] private  int maxMovements = 100;
    [SerializeField] private  int maxIteration = 50;

    private const float EpsilonStart = 0.7f;
    private const float EpsilonEnd = 0.1f;

    private List<State> _states;
    private List<Tuple<State, AgentMovements>> _generation;
    private List<Tuple<State, AgentMovements>> _policy;
    private List<MonteCarloStruct> _genResults;

    private bool _policyIsStable;
    private bool _monteCarloDone;

    private void Start()
    {
        _gameState = gameManager.GetComponent<IGameState>();
        InitStates();
        InitRandomPolicy();
        if(isExploringStart)
            _gameState.SetRandomGameState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _monteCarloDone = false;
            for (var i = 0; i < maxIteration; i++)
            {
                Debug.Log("iteration : " + i);
                EveryVisitMcPrediction();
                GenerateNewPolicy();
                if (_policyIsStable)
                    break;
            }
            Debug.Log("Iterations done");
            _monteCarloDone = true;
        }

        if (Input.GetKeyDown(KeyCode.E) && _monteCarloDone)
        {
            StartCoroutine(MoveAgentFromPolicy());
        }
    }

    private void InitStates()
    {
        _states = new List<State> { _gameState.GetState() };
        _generation = new List<Tuple<State, AgentMovements>>();
        _genResults = new List<MonteCarloStruct> { new (_states[0]) };
    }

    private void InitRandomPolicy()
    {
        _policy = new List<Tuple<State, AgentMovements>> { new (_states[0], GetRandomMove()) };
    }
    
    private void AddState(State state)
    {
        if (_states.Any(result => result.Equals(state)))
        {
            return;
        }
        _states.Add(state);
    }

    private void CheckStateInResultList(State state)
    {
        if (_genResults.Any(result => result.State.Equals(state)))
        {
            return;
        }
        _genResults.Add(new MonteCarloStruct(state));
    }

    private MonteCarloStruct GetResultsFromState(State state)
    {
        foreach (var result in _genResults.Where(result => result.State.Equals(state)))
        {
            return result;
        }
        return new MonteCarloStruct(state);
    }

    private void UpdateVsForStateAndMove(State state, AgentMovements move)
    {
        var result = GetResultsFromState(state);
        var returns = result.ReturnsForState[(int) move];
        var n = result.NForState[(int) move];
        if (n == 0) n = 1;
        result.VsForState[(int) move] = returns / n;
    }

    private void ClearResults()
    {
        foreach (var result in _genResults)
        {
            for (var i = 0; i < (int)AgentMovements.Length; i++)
            {
                result.ReturnsForState[i] = 0f;
                result.NForState[i] = 0;
                result.VsForState[i] = 0;
            }
        }
    }

    private AgentMovements GetMovementFromPolicy(State state)
    {
        foreach (var (policyState, move) in _policy)
        {
            if (!policyState.Equals(state)) continue;
            return move;
        }

        var newPolicy = new Tuple<State, AgentMovements>(state, GetRandomMove());
        _policy.Add(newPolicy);
        return newPolicy.Item2;
    }

    private void GenerateNewPolicy()
    {
        _policyIsStable = true;
        foreach (var state in _states)
        {
            var maxValue = 0f;
            var curMove = AgentMovements.Up;
            var result = GetResultsFromState(state);
            for (var i = 0; i < (int) AgentMovements.Length; i++)
            {
                var curVal = result.VsForState[i];
                if (curVal < maxValue) continue;
                maxValue = curVal;
                curMove = (AgentMovements) i;
            }
            for(var i = 0; i < _policy.Count; i++)
            {
                var (policyState, move) = _policy[i];
                if (!policyState.Equals(state)) continue;
                if (move == curMove) break;
                _policyIsStable = false;
                _policy[i] = new Tuple<State, AgentMovements>(policyState, curMove);
                break;
            }
        }
        ClearResults();
    }

    private void EveryVisitMcPrediction()
    {
        var epochs = 1;
        
        for (; epochs < maxEpochs; epochs++)
        {
            _generation.Clear();
            var r = Mathf.Max((maxEpochs - epochs) / maxEpochs, 0);
            var epsilon = r * (EpsilonStart - EpsilonEnd) + EpsilonEnd;
            var g = 0f;
            var currentGameState = _gameState.GetState();
            for (var i = 0; i < maxMovements; i++)
            {
                AgentMovements currentMove;
                if (Random.Range(0f, 1f) > epsilon)
                    currentMove = GetRandomMove();
                else
                {
                    currentMove = GetMovementFromPolicy(currentGameState);
                }
                _generation.Add(new Tuple<State, AgentMovements>(currentGameState, currentMove));
                CheckStateInResultList(currentGameState);
                AddState(currentGameState);
                currentGameState = _gameState.CheckMove(currentMove, currentGameState);
                
                if (!_gameState.CheckGameOver(currentGameState)) continue;
                _generation.Add(new Tuple<State, AgentMovements>(currentGameState, currentMove));
                CheckStateInResultList(currentGameState);
                AddState(currentGameState);
                break;
            }
            for (var t = _generation.Count - 2; t >= 0; t--)
            {
                var (state, move) = _generation[t];
                var result = GetResultsFromState(state);
                var indexMove = (int)move;
                var returns = result.ReturnsForState[indexMove];
                var n = result.NForState[indexMove];
                g += _gameState.GetReward(_generation[t+1].Item1);
                result.ReturnsForState[indexMove] = g + returns;
                result.NForState[indexMove] = n+1;
            }
        }

        foreach (var state in _states)
        {
            for (var i = 0; i < (int) AgentMovements.Length; i++)
            {
                UpdateVsForStateAndMove(state, (AgentMovements) i);
            }
        }
    }

    private AgentMovements GetRandomMove()
    {
        return (AgentMovements)Random.Range(0, (int)AgentMovements.Length);
    }

    private IEnumerator MoveAgentFromPolicy()
    {
        var state = _gameState.GetState();
        var win = false;
        for (var i = 0; i < maxMovements; i++)
        {
            yield return new WaitForSeconds(0.5f);
            var move = GetMovementFromPolicy(state);
            state = _gameState.MoveAgent(move);
            if (!_gameState.CheckGameOver(state)) continue;
            win = true;
            break;
        }
        if(!win)
            Debug.Log("Looser !");
        yield return null;
    }

}
