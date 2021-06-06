using System;
using System.Collections.Generic;
using UnityEngine;

public class NotesManager : MonoBehaviour
{
    public List<Note> notes;

    // Map/song data
    public string type;
    public string difficulty;
    public int rank;
    public float speed;
    public string title;
    public string subtitle;
    public string artist;
    public string mapper;
    public List<float> bpm;

    public Material leftMat;
    public Material rightMat;

    public AudioSource player;

    //Set from editor
    public GameObject blockPrefab;
    public GameObject sparklePrefab;
    public GameObject leftTracker;
    public GameObject rightTracker;
    public float rowWidth;
    public float columnHeight;
    public int bufferLength;

    public float time;
    public Vector3 velocity;
    public bool playing;
    Vector3 leftFirst;
    Vector3 rightFirst;

    void createBlocks()
    {
        foreach (Note note in notes)
        {
            if (note.block != null)
            {
                Destroy(note.block);
                note.block = null;
            }
        }

        foreach(Note note in notes)
        {
            generateNoteBlock(note);
        }
    }

    void pause()
    {
        this.playing = false;
        this.player.Pause();
    }

    void play()
    {
        this.playing = true;
        this.player.Play();
    }

    void seek(float time)
    {
        this.time = time;
        this.player.time = time;
    }

    int xyToDirection(float x, float y)
    {
        double angle = Math.Atan(y / x);
        if (x < 0)
        {
            angle += Math.PI;
        }
        angle /= Math.PI / 4;
        Debug.Log("Logging values of stuff: x, y, angle, index, direction");
        Debug.Log(x);
        Debug.Log(y);
        Debug.Log(angle);

        int index = Convert.ToInt32(angle);
        index = (index + 2) % 8;

        int direction = 8;

        switch (index)
        {
            case 0:
                direction = 1;
                break;
            case 1:
                direction = 6;
                break;
            case 2:
                direction = 2;
                break;
            case 3:
                direction = 4;
                break;
            case 4:
                direction = 0;
                break;
            case 5:
                direction = 5;
                break;
            case 6:
                direction = 3;
                break;
            case 7:
                direction = 7;
                break;
            default:
                Debug.LogWarning("Unable to recognize angle");
                break;
        }
        Debug.Log(index);
        Debug.Log(direction);
        return direction;
    }

    float directionToAngle(int direction)
    {
        switch (direction)
        {
            case 0:
                return 180;
            case 1:
                return 0;
            case 2:
                return 90;
            case 3:
                return 270;
            case 4:
                return 135;
            case 5:
                return 225;
            case 6:
                return 45;
            case 7:
                return 315;
            case 8:
                return 0;
            default:
                Debug.LogWarning("Invalid direction");
                return -30;
        }
    }

    void generateNoteBlock(Note note)
    {
        GameObject block = (GameObject)Instantiate(this.blockPrefab, new Vector3((note.x - 1.5F) * this.rowWidth, (note.y + 1F) * this.columnHeight, (note.time - this.time) * this.speed), Quaternion.identity);
        if (note.direction == 8)
        {
            GameObject toDeactivate = block.transform.Find("ArrowBlock")?.gameObject;
            if (toDeactivate)
            {
                toDeactivate.SetActive(false);
            }
            else
            {
                Debug.Log("Unable to find arrow block to disable");
            }
            GameObject colorBlock = block.transform.Find("CircleBlock")?.Find("Body")?.gameObject;
            colorBlock.GetComponent<MeshRenderer>().material = note.type == 0 ? leftMat : rightMat;
        }
        else
        {
            GameObject toDeactivate = block.transform.Find("CircleBlock")?.gameObject;
            if (toDeactivate)
            {
                toDeactivate.SetActive(false);
            }
            else
            {
                Debug.Log("Unable to find circle to disable");
            }
            GameObject colorBlock = block.transform.Find("ArrowBlock")?.Find("Body")?.gameObject;
            colorBlock.GetComponent<MeshRenderer>().material = note.type == 0 ? leftMat : rightMat;
            block.transform.Rotate(new Vector3(0, 0, directionToAngle(note.direction)));
        }
        note.block = block;
    }

    // Start is called before the first frame update
    void Start() {
        this.notes = new List<Note>();
        this.player = GetComponent<AudioSource>();
        this.velocity = new Vector3(0, 0, -this.speed);
        //For testing
        this.notes.Add(new Note(2, 0, 0, 0, 0));
        this.notes.Add(new Note(2, 1, 0, 1, 1));
        this.notes.Add(new Note(2, 2, 0, 0, 2));
        this.notes.Add(new Note(2, 3, 0, 1, 3));
        this.notes.Add(new Note(2, 0, 1, 0, 4));
        this.notes.Add(new Note(2, 1, 1, 1, 5));
        this.notes.Add(new Note(2, 2, 1, 0, 6));
        this.notes.Add(new Note(2, 3, 1, 1, 7));
        this.notes.Add(new Note(2, 0, 2, 0, 8));
        this.notes.Add(new Note(2, 1, 2, 1, 8));
        this.notes.Add(new Note(2, 2, 2, 0, 8));
        this.notes.Add(new Note(2, 3, 2, 1, 8));
        this.time = 0;
        this.play();
        this.createBlocks();
    }

    // Update is called once per frame
    void Update() {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
        {
            Debug.LogWarning("Button pressed");
            if (this.playing)
            {
                this.pause();
            }
            else
            {
                this.play();
            }
        }
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft))
        {
            this.seek(0F);
        }
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            rightFirst = new Vector3(rightTracker.transform.position.x, rightTracker.transform.position.y, this.time);
        }
        if (OVRInput.Get(OVRInput.Button.One))
        {
            Instantiate(sparklePrefab, new Vector3(rightTracker.transform.position.x, rightTracker.transform.position.y, rightTracker.transform.position.z), Quaternion.identity);
            Debug.Log("Spawning sparkle");
        }
        if (OVRInput.GetUp(OVRInput.Button.One))
        {
            Vector2 avg = new Vector2((rightFirst.x + rightTracker.transform.position.x) / 2, (rightFirst.y + rightTracker.transform.position.y) / 2);
            int currentX = Convert.ToInt32((avg.x + (1.5F * this.rowWidth)) / this.rowWidth);
            if (currentX > 3) currentX = 3;
            if (currentX < 0) currentX = 0;
            int currentY = Convert.ToInt32((avg.y - (1 * this.columnHeight)) / this.columnHeight);
            if (currentY > 2) currentY = 2;
            if (currentY < 0) currentY = 0;
            float deltaY = rightTracker.transform.position.y - rightFirst.y;
            float deltaX = rightTracker.transform.position.x - rightFirst.x;

            Note newNote = new Note(rightFirst.z, currentX, currentY, 1, xyToDirection(deltaX, deltaY));
            this.generateNoteBlock(newNote);
            this.notes.Add(newNote);
        }
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            leftFirst = new Vector3(leftTracker.transform.position.x, leftTracker.transform.position.y, this.time);
        }
        if (OVRInput.Get(OVRInput.Button.Three))
        {
            Instantiate(sparklePrefab, new Vector3(leftTracker.transform.position.x, leftTracker.transform.position.y, leftTracker.transform.position.z), Quaternion.identity);
            Debug.Log("Spawning sparkle");
        }
        if (OVRInput.GetUp(OVRInput.Button.Three))
        {
            Vector2 avg = new Vector2((leftFirst.x + leftTracker.transform.position.x) / 2, (leftFirst.y + leftTracker.transform.position.y) / 2);
            int currentX = Convert.ToInt32((avg.x + (1.5F * this.rowWidth)) / this.rowWidth);
            if (currentX > 3) currentX = 3;
            if (currentX < 0) currentX = 0;
            int currentY = Convert.ToInt32((avg.y - (1F * this.columnHeight)) / this.columnHeight);
            if (currentY > 2) currentY = 2;
            if (currentY < 0) currentY = 0;
            float deltaY = leftTracker.transform.position.y - leftFirst.y;
            float deltaX = leftTracker.transform.position.x - leftFirst.x;

            Note newNote = new Note(leftFirst.z, currentX, currentY, 0, xyToDirection(deltaX, deltaY));
            this.generateNoteBlock(newNote);
            this.notes.Add(newNote);
        }

        //For testing. Right stick deletes everything.
        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
        {
            foreach (Note note in notes)
            {
                Destroy(note.block);
            }
            this.notes = new List<Note>();
        }
    }

    void FixedUpdate()
    {
        if (this.playing)
        {
            this.time += Time.deltaTime;
        }
        foreach (Note note in notes)
        {
            note.block.transform.position = new Vector3((note.x - 1.5F) * this.rowWidth, (note.y + 1F) * this.columnHeight, (note.time - this.time) * this.speed);
        }
    }
}
