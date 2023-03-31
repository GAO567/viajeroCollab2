using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell 

{
    public Vector3 pos;
    public int x;
    public int y;
    public int z;

    public int height;
    public int depth;
    public int width;
    public string id;

    public bool filled = false;

    public GridCell(int x, int y,int z, Vector3 pos)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.pos = pos;
    }

    public GridCell(int x, int y, int z, int width, int height, int depth, Vector3 pos)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.width = width;
        this.height = height;
        this.depth = depth;
        this.pos = pos;

        id = x + "" + y + "" + z + "l";
    }


}
