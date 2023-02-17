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

    private AgentMovements[,] _policy;
    private Dictionary<AgentMovements, Tuple<float, int>>[,] _returnsAndNForState;
    private Dictionary<AgentMovements, float>[,] _vForState;

    private List<Tuple<AgentMovements, int, int>> _generation;

    private bool _policyIsStable;
    private bool _monteCarloDone;

    private void Start()
    {
        _gameState = gameManager.GetComponent<IGameState>();
        
        SetRandomPolicy();
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
                EveryVisitMcPrediction();
                GenerateNewPolicy();
                if (_policyIsStable)
                    break;
            }
            _monteCarloDone = true;
            for (var x = 0; x < _gameState.GetGridWidth(); x++)
            {
                for (var y = 0; y < _gameState.GetGridHeight(); y++)
                {
                    Debug.Log("x : " + x + " y : " + y + " direction : " + _policy[x, y]);
                }

            }
        }

        if (Input.GetKeyDown(KeyCode.E) && _monteCarloDone)
        {
            StartCoroutine(MoveAgentFromPolicy());
        }
    }

    private void InitReturnAndN()
    {
        _returnsAndNForState = new Dictionary<AgentMovements, Tuple<float, int>>[_gameState.GetGridWidth(), _gameState.GetGridHeight()];
        for (var x = 0; x < _gameState.GetGridWidth(); x++)
        {
            for (var y = 0; y < _gameState.GetGridHeight(); y++)
            {
                var dict = new Dictionary<AgentMovements, Tuple<float, int>>();
                for (var i = 0; i < (int) AgentMovements.Length; i++)
                {
                    dict[(AgentMovements)i] = new Tuple<float, int>(0f, 0);
                }
                _returnsAndNForState[x, y] = dict;
            }
        }
    }

    private void InitV()
    {
        _vForState = new Dictionary<AgentMovements, float>[_gameState.GetGridWidth(), _gameState.GetGridHeight()];
        for (var x = 0; x < _gameState.GetGridWidth(); x++)
        {
            for (var y = 0; y < _gameState.GetGridHeight(); y++)
            {
                var dict = new Dictionary<AgentMovements, float>();
                for (var i = 0; i < (int) AgentMovements.Length; i++)
                {
                    dict[(AgentMovements)i] = 0f;
                }
                _vForState[x, y] = dict;
            }
        }
    }

    private void SetRandomPolicy()
    {
        _policy = new AgentMovements[_gameState.GetGridWidth(), _gameState.GetGridHeight()];
        for (var x = 0; x < _gameState.GetGridWidth(); x++)
        {
            for (var y = 0; y < _gameState.GetGridHeight(); y++)
            {
                _policy[x, y] = GetRandomMove();
            }
        }
    }

    private void GenerateNewPolicy()
    {
        _policyIsStable = true;
        for (var x = 0; x < _gameState.GetGridWidth(); x++)
        {
            for (var y = 0; y < _gameState.GetGridHeight(); y++)
            {
                var maxValue = 0f;
                var curMove = AgentMovements.Up;
                for (var i = 0; i < (int)AgentMovements.Length; i++)
                {
                    var curVal = _vForState[x, y][(AgentMovements)i];
                    if (!(curVal > maxValue)) continue;
                    maxValue = curVal;
                    curMove = (AgentMovements)i;
                }
                if (_policy[x, y] == curMove) continue;
                _policyIsStable = false;
                _policy[x, y] = curMove;
            }
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
            var currentGameState = _gameState.GetAgentPosition();

            var generation = new List<Tuple<AgentMovements, int, int>>();
            
            for (var i = 0; i < maxMovements; i++)
            {
                AgentMovements currentMove;
                if (Random.Range(0f, 1f) > epsilon)
                    currentMove = GetRandomMove();
                else 
                    currentMove = _policy[currentGameState.x, currentGameState.y];
                generation.Add(new Tuple<AgentMovements, int, int>(currentMove, currentGameState.x, currentGameState.y));
                currentGameState = _gameState.CheckMove(currentMove, currentGameState);
                if (!_gameState.CheckGameOver(currentGameState.x, currentGameState.y)) continue;
                generation.Add(new Tuple<AgentMovements, int, int>(currentMove, currentGameState.x, currentGameState.y));
                break;
            }
            for (var t = generation.Count - 2; t >= 0; t--)
            {
                var action = generation[t];
                var actionValues = _returnsAndNForState[action.Item2, action.Item3][action.Item1];
                G += _gameState.GetReward(generation[t+1].Item2, generation[t+1].Item3);
                var returnAndN = new Tuple<float, int>(actionValues.Item1 + G, actionValues.Item2 + 1);
                _returnsAndNForState[generation[t].Item2, generation[t].Item3][generation[t].Item1] = returnAndN;
            }
        }

        for (var x = 0; x < _gameState.GetGridWidth(); x++)
        {
            for (var y = 0; y < _gameState.GetGridHeight(); y++)
            {
                for (var i = 0; i < (int) AgentMovements.Length; i++)
                {
                    var values = _returnsAndNForState[x, y][(AgentMovements)i];
                    _vForState[x, y][(AgentMovements) i] = values.Item1 / values.Item2;
                }
            }
        }
        
        
    }

    private AgentMovements GetRandomMove()
    {
        return (AgentMovements)Random.Range(0, (int)AgentMovements.Length);
    }

    private IEnumerator MoveAgentFromPolicy()
    {
        for (var i = 0; i < maxMovements; i++)
        {
            yield return new WaitForSeconds(0.5f);
            var position = _gameState.GetAgentPosition();
            var move = _policy[position.x, position.y];
            position = _gameState.MoveAgent(move);
            if (_gameState.CheckGameOver(position.x, position.y))
                break;
        }

        yield return null;
    }

}
