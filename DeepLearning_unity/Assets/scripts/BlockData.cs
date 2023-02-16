using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData : MonoBehaviour
{
    public int posX;
    public int posY;
    public TileStates state;

    private void Awake()
    {
        var position = gameObject.transform.position;
        posX = (int)Math.Round(position.x);
        posY = (int)Math.Round(position.z);
        state = GameState.TileStateFromTag(gameObject);
    }
}
