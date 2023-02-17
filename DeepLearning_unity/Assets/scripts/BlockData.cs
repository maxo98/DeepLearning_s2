using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData : MonoBehaviour
{
    public int PosX { get; private set; }
    public int PosY { get; private set; }
    public BlockStates state;

    private void Awake()
    {
        var position = gameObject.transform.position;
        PosX = (int)Math.Round(position.x);
        PosY = (int)Math.Round(position.z);
        state = GameStateUtil.TileStateFromTag(gameObject);
    }
}
