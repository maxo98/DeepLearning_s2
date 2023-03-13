using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class TemporalDifferences : MonoBehaviour
{
    [SerializeField] private GameObject gameManager;
    private IGameState _gameState;

    [SerializeField] private bool isExploringStart;
    [SerializeField] private bool isQLearning;

    [SerializeField] private int maxEpochs = 1000;
    [SerializeField] private int maxMovements = 100;
    [SerializeField] private float gamma = 0.5f;
    [SerializeField] private float alpha = 0.5f;

    private const float EpsilonStart = 0.5f;
    private const float EpsilonEnd = 0.1f;

    private Dictionary<State, float[]> _policy;

    private bool _policyIsStable;
    private bool _sarsaDone;

    private void Start()
    {
        _gameState = gameManager.GetComponent<IGameState>();
        
        if(isExploringStart)
            _gameState.SetRandomGameState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            InitRandomPolicy();
            Simulate();
            watch.Stop();
            var elapsed = watch.Elapsed;
            Debug.Log("Iterations done in " + elapsed.TotalSeconds);
            _sarsaDone = true;
        }

        if (Input.GetKeyDown(KeyCode.E) && _sarsaDone)
        {
            StartCoroutine(MoveAgentFromPolicy());
        }
    }

    private void InitRandomPolicy()
    {
        var policy0 = new float[(int)AgentMovements.Length];
        _policy = new Dictionary<State, float[]> { { _gameState.CopyState(_gameState.GetState()), policy0 } };
    }

    private float GetPolicyValue(State state, AgentMovements move)
    {
        foreach (var (key,_) in _policy)
        {
            if (!_gameState.CompareStates(key, state)) continue;
            return _policy[key][(int)move];
        }
        _policy.Add(_gameState.CopyState(state), new float[(int) AgentMovements.Length]);
        return 0f;
    }

    private void SetPolicyValue(State state, AgentMovements move, float value)
    {
        var stateFound = false;
        foreach (var (key,_) in _policy)
        {
            if (!_gameState.CompareStates(key, state)) continue;
            _policy[key][(int)move] = value;
            stateFound = true;
        }
        if (stateFound) return;
        _policy.Add(_gameState.CopyState(state), new float[(int)AgentMovements.Length]);
        _policy[state][(int)move] = value;
    }

    private float GetHighestValueInPolicyForState(State state)
    {
        foreach (var (key, values) in _policy)
        {
            if (!_gameState.CompareStates(key, state)) continue;
            var maxValue = 0f;
            foreach (var value in values)
            {
                if (value > maxValue)
                    maxValue = value;
            }
            return maxValue;
        }
        _policy.Add(_gameState.CopyState(state), new float[(int)AgentMovements.Length]);
        return 0f;
    }
    
    private AgentMovements GetMovementFromPolicy(State state)
    {
        foreach (var (policyState, movements) in _policy)
        {
            if (!_gameState.CompareStates(policyState, state)) continue;
            var maxValue = 0f;
            var curMove = AgentMovements.Up;
            for (var i = 0; i < (int) AgentMovements.Length; i++)
            {
                var curVal = movements[i];
                if (curVal < maxValue) continue;
                maxValue = curVal;
                curMove = (AgentMovements) i;
            }

            return curMove;
        }

        var newPolicy = new float[(int) AgentMovements.Length];
        _policy.Add(_gameState.CopyState(state), newPolicy);
        return GetRandomMove();
    }

    private void Simulate()
    {
        var epochs = 1;
        
        for (; epochs < maxEpochs; epochs++)
        {
            var r = Mathf.Max((maxEpochs - epochs) / maxEpochs, 0);
            var epsilon = r * (EpsilonStart - EpsilonEnd) + EpsilonEnd;
            var currentGameState = _gameState.GetState();
            AgentMovements currentMove;
            if (Random.Range(0f, 1f) > epsilon)
                currentMove = GetRandomMove();
            else
                currentMove = GetMovementFromPolicy(currentGameState);
            for (var i = 0; i < maxMovements; i++)
            {
                var nextState = _gameState.CheckMove(currentMove, currentGameState);
                AgentMovements nextMove;
                if (Random.Range(0f, 1f) > epsilon)
                    nextMove = GetRandomMove();
                else
                    nextMove = GetMovementFromPolicy(currentGameState);
                var currentValue = GetPolicyValue(currentGameState,currentMove);
                var nextValue = GetPolicyValue(nextState,nextMove);
                if (isQLearning)
                    nextValue = GetHighestValueInPolicyForState(nextState);
                currentValue += alpha * (_gameState.GetReward(nextState, currentGameState) +
                    gamma * nextValue - currentValue);
                SetPolicyValue(currentGameState, currentMove, currentValue);
                currentGameState = nextState;
                currentMove = nextMove;
                if (!_gameState.CheckGameOver(currentGameState)) continue;
                break;
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
