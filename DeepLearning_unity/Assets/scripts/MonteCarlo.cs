using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


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

    private Dictionary<State, AgentMovements> _policy;
    private Dictionary<State, Dictionary<AgentMovements, Tuple<float, int>>> _returnsAndNForState;
    private Dictionary<State, Dictionary<AgentMovements, float>> _vForState;

    private List<Tuple<State, AgentMovements>> _generation;
    private List<State> states;

    private bool _policyIsStable;
    private bool _monteCarloDone;

    private void Start()
    {
        _gameState = gameManager.GetComponent<IGameState>();
        InitStates();
        InitRandomPolicy();
        if(isExploringStart)
            _gameState.SetRandomGameState();
        InitReturnAndN();
        InitV();
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
            
            _monteCarloDone = true;
        }

        if (Input.GetKeyDown(KeyCode.E) && _monteCarloDone)
        {
            StartCoroutine(MoveAgentFromPolicy());
        }
    }

    private void InitStates()
    {
        states = new List<State>();
    }

    private void InitReturnAndN()
    {
        _returnsAndNForState = new Dictionary<State, Dictionary<AgentMovements, Tuple<float, int>>>();
    }

    private void InitV()
    {
        _vForState = new Dictionary<State, Dictionary<AgentMovements, float>>();
    }

    private void InitRandomPolicy()
    {
        _policy = new Dictionary<State, AgentMovements> { { _gameState.GetState(), GetRandomMove() } };
    }

    private void GenerateNewPolicy()
    {
        _policyIsStable = true;
        var maxValue = 0f;
        var curMove = AgentMovements.Up;
        foreach (var state in states)
        {
            for (var i = 0; i < (int) AgentMovements.Length; i++)
            {
                if (!_vForState.ContainsKey(state)) continue;
                if (!_vForState[state].ContainsKey((AgentMovements) i)) continue;
                var curVal = _vForState[state][(AgentMovements) i];
                if (!(curVal > maxValue)) continue;
                maxValue = curVal;
                curMove = (AgentMovements) i;
            }
        }

        foreach (var state in states)
        {
            if (!_policy.ContainsKey(state)) continue;
            if (_policy[state] == curMove) continue;
            _policyIsStable = false;
            _policy[state] = curMove;
        }
    }

    private void EveryVisitMcPrediction()
    {
        var epochs = 1;
        
        for (; epochs < maxEpochs; epochs++)
        {
            var r = Mathf.Max((maxEpochs - epochs) / maxEpochs, 0);
            var epsilon = r * (EpsilonStart - EpsilonEnd) + EpsilonEnd;
            var G = 0f;
            var currentGameState = _gameState.GetState();

            var generation = new List<Tuple<State, AgentMovements>>();
            
            for (var i = 0; i < maxMovements; i++)
            {
                if(!states.Contains(currentGameState))
                    states.Add(currentGameState);
                AgentMovements currentMove;
                if (Random.Range(0f, 1f) > epsilon)
                    currentMove = GetRandomMove();
                else
                {
                    if (_policy.ContainsKey(currentGameState))
                    {
                        currentMove = _policy[currentGameState];
                    }
                    else
                    {
                        _policy.Add(currentGameState,GetRandomMove());
                        currentMove = _policy[currentGameState];
                    }
                }

                generation.Add(new Tuple<State, AgentMovements>(currentGameState, currentMove));
                currentGameState = _gameState.CheckMove(currentMove, currentGameState);
                if (!_gameState.CheckGameOver(currentGameState)) continue;
                generation.Add(new Tuple<State, AgentMovements>(currentGameState, currentMove));
                break;
            }
            for (var t = generation.Count - 2; t >= 0; t--)
            {
                var (state, move) = generation[t];
                if (!_returnsAndNForState.ContainsKey(state))
                {
                    _returnsAndNForState.Add(state, new Dictionary<AgentMovements, Tuple<float, int>>());
                }
                if (!_returnsAndNForState[state].ContainsKey(move))
                {
                    _returnsAndNForState[state].Add(move, new Tuple<float, int>(0f, 0));
                }
                var (returns, n) = _returnsAndNForState[state][move];
                G += _gameState.GetReward(generation[t+1].Item1);
                var returnAndN = new Tuple<float, int>(returns + G, n + 1);
                _returnsAndNForState[state][move] = returnAndN;
            }
        }

        foreach (var state in states)
        {
            for (var i = 0; i < (int) AgentMovements.Length; i++)
            {
                if (!_returnsAndNForState.ContainsKey(state))
                {
                    _returnsAndNForState.Add(state, new Dictionary<AgentMovements, Tuple<float, int>>());
                }
                if (!_returnsAndNForState[state].ContainsKey((AgentMovements)i))
                {
                    _returnsAndNForState[state].Add((AgentMovements)i, new Tuple<float, int>(0f, 0));
                }
                var (returns, n) = _returnsAndNForState[state][(AgentMovements)i];
                if (n != 0)
                    _vForState[state][(AgentMovements)i] = returns / n;
                else
                    _vForState[state][(AgentMovements)i] = returns;
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
        for (var i = 0; i < maxMovements; i++)
        {
            yield return new WaitForSeconds(0.5f);
            var move = _policy[state];
            state = _gameState.MoveAgent(move);
            if (_gameState.CheckGameOver(state))
                break;
        }

        yield return null;
    }

}
