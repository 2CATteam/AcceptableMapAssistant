using UnityEngine;

public class Note
{
    public float time;
    public int x;
    public int y;
    public int type;
    public int direction;
    public GameObject block;

    public Note(float time = 0, int x = 0, int y = 0, int type = 1, int direction = 8)
    {
        this.time = time;
        this.x = x;
        this.y = y;
        this.type = type;
        this.direction = direction;
    }
}
