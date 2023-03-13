using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ValueIteration : MonoBehaviour
{
    [SerializeField] GameObject[] map;
    [SerializeField] GameObject PlayerPref;
    [SerializeField] int playerPos;
    [SerializeField] int mapWidth;
    [SerializeField] int mapHeight;
    [SerializeField] int victory;
    [SerializeField] int[] rewards;
    [SerializeField] float[] valueInstantT;
    [SerializeField] float[] valueT1;
    [SerializeField] Action[] action;

    [SerializeField] Vector3 offset;

    [SerializeField] float gamma;

    [SerializeField] int[] passageNumber;
    [SerializeField] int[] returns;
    [SerializeField] List<Tuple<int, Action>> T = new List<Tuple<int, Action>>();


    private int size;

    bool PolicyTrue = false;

    private Dictionary<Action, int> movements = new Dictionary<Action, int>();

    void Start()
    {

        offset = new Vector3(0, 1, 0);

        rewards = new int[map.Length];
        valueInstantT = new float[map.Length];
        valueT1 = new float[map.Length];
        action = new Action[map.Length];
        passageNumber = new int[map.Length];
        returns = new int[map.Length];

        size = map.Length;

        movements.Add(Action.Bottom, -mapWidth);
        movements.Add(Action.Top, mapWidth);
        movements.Add(Action.Right, 1);
        movements.Add(Action.Left, -1);
        movements.Add(Action.Stop, 0);

        for (int i = 0; i < size; i++)
        {
            rewards[i] = 0;

            valueT1[i] = 0;

            valueInstantT[i] = 0;

            if (map[i].CompareTag("Obstacle"))
            {
                action[i] = Action.Stop;
                continue;
            }

            int index = Random.Range(0, 4);

            Action randomMove = (Action)index;

            while (!IsActionPossible(randomMove, i))
            {
                index = Random.Range(0, 4);
                randomMove = (Action)index;
            }

            action[i] = randomMove;
        }

        action[victory] = Action.Stop;
        rewards[victory] = 1;
    }

    bool IsActionPossible(Action random, int index)
    {
        switch (random)
        {
            case Action.Top:
                if (index + mapWidth > size - 1 || map[index + mapWidth].CompareTag("Obstacle")) return false;
                break;
            case Action.Bottom:
                if (index - mapWidth < 0 || map[index - mapWidth].CompareTag("Obstacle")) return false;
                break;
            case Action.Left:
                if (index % mapWidth == 0 || map[index - 1].CompareTag("Obstacle")) return false;
                break;
            case Action.Right:
                if (index % mapWidth == mapWidth - 1 || map[index + 1].CompareTag("Obstacle")) return false;
                break;
            default:
                break;

        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            while (!PolicyTrue)
            {
                PolicyEvaluation();
                PolicyImprovement();
            }

            //StartCoroutine(MovePlayer());
        }


        if (Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine(Move());
        }
    }

    void PolicyEvaluation()
    {
        float delta = 1.0f;
        while (delta > 0.0001f)
        {
            delta = .0f;
            for (int i = 0; i < size; i++)
            {
                valueT1[i] = CheckReward(i, movements[action[i]]) + gamma * valueT1[i + movements[action[i]]];
                delta = MathF.Max(delta, Mathf.Abs(valueInstantT[i] - valueT1[i]));
                valueInstantT = valueT1;
            }
        }
    }


    void PolicyImprovement()
    {
        var temp = new Action[size];

        for (int i = 0; i < size; i++)
        {
            temp[i] = action[i];

            if (i == victory) continue;

            if (map[i].CompareTag("Obstacle"))
            {
                action[i] = Action.Stop;
                continue;
            }

            List<Action> allActionForCase = new List<Action>();

            for (int j = 0; j < 4; j++)
            {
                Action move = (Action)j;
                if (IsActionPossible(move, i)) allActionForCase.Add(move);
            }

            float[] allActionValues = new float[allActionForCase.Count];

            for (int k = 0; k < allActionForCase.Count; k++) allActionValues[k] = Improvement(i, allActionForCase[k]);

            int bestValueIndex = Array.IndexOf(allActionValues, allActionValues.Max());

            Action bestAction = allActionForCase[bestValueIndex];

            action[i] = bestAction;

        }

        for (int i = 0; i < size; i++)
        {
            if (i == victory) continue;

            if (action[i] != temp[i])
            {
                PolicyTrue = false;
                return;
            }
        }

        PolicyTrue = true;
        return;
    }

    int CheckReward(int index, int movement)
    {
        try
        {
            if (map[index].CompareTag("Obstacle")) return 0;
            var reward = rewards[index + movement];
            return reward;
        }
        catch (Exception e)
        {
            Console.WriteLine("voici les index : " + index);
            Debug.Log(e);
            throw;
        }

    }

    float Improvement(int index, Action movement)
    {
        try
        {
            var actionToDo = movements[movement];
            var reward = valueInstantT[index + actionToDo];
            return reward;
        }
        catch (Exception e)
        {
            Console.WriteLine("index : " + index);
            Debug.Log(e);
            throw;
        }

    }

    private IEnumerator Move()
    {
        while (playerPos != victory)
        {
            yield return new WaitForSeconds(1);
            var positionToGo = map[playerPos + movements[action[playerPos]]];
            PlayerPref.transform.position = positionToGo.transform.position + offset;
            playerPos = Array.IndexOf(map, positionToGo);
        }
        Debug.Log("VICTOIRE");
    }
}
