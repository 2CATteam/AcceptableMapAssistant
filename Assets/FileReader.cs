using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FileReader : MonoBehaviour
{
    public NotesManager notesManager;
    public AudioSource source;

    public void readSong(string folder, string type, string difficulty)
    {
        this.notesManager.pause();
        StreamReader streamReader = new StreamReader(Path.Combine(folder, "Info.dat"));
        JSONData element = JSONData.parseJSON(streamReader.ReadToEnd());
        Debug.Log(element.toString());

        Song songData = new Song();

        songData.title = element.get("_songName")?._string;
        songData.subtitle = element.get("_songSubName")?._string;
        songData.artist = element.get("_songAuthorName")?._string;
        songData.mapper = element.get("_levelAuthorName")?._string;
        songData.bpmTimes = new List<float>();
        songData.bpmValues = new List<float>();
        if (element.get("_beatsPerMinute")?.type == JSONTypes.Double)
        {
            songData.bpmValues.Add((float) element.get("_beatsPerMinute")?._double);
            songData.bpmTimes.Add(0F);
        } else if (element.get("_beatsPerMinute")?.type == JSONTypes.Integer)
        {
            songData.bpmValues.Add((float) element.get("_beatsPerMinute")?._int);
            songData.bpmTimes.Add(0F);
        }

        this.notesManager.songData = songData;

        foreach (JSONData el in element.get("_difficultyBeatmapSets")._array)
        {
            if (el?.get("_beatmapCharacteristicName")?._string == type)
            {
                foreach (JSONData el2 in el.get("_difficultyBeatmaps")._array)
                {
                    if (el2.get("_difficulty")?._string == difficulty)
                    {
                        Difficulty difficultyData = new Difficulty();
                        difficultyData.type = type;
                        difficultyData.difficulty = difficulty;
                        if (el2.get("_noteJumpMovementSpeed").type == JSONTypes.Integer)
                        {
                            difficultyData.speed = el2.get("_noteJumpMovementSpeed")._int;
                        } else if (el2.get("_noteJumpMovementSpeed").type == JSONTypes.Double)
                        {
                            difficultyData.speed = (float) el2.get("_noteJumpMovementSpeed")._double;
                        }
                        this.notesManager.difficultyData = difficultyData;
                        this.notesManager.velocity = new Vector3(0, 0, -this.notesManager.difficultyData.speed);
                        this.readNotes(Path.Combine(folder, el2.get("_beatmapFilename")._string));
                    }
                }
            }
        }

        this.readAudio(Path.Combine(folder, element.get("_songFilename")._string));
    }

    public void readNotes(string name)
    {
        StreamReader streamReader = new StreamReader(name);
        JSONData element = JSONData.parseJSON(streamReader.ReadToEnd());
        Debug.Log(element.toString());

        foreach (JSONData el in element.get("_notes")?._array)
        {
            float time = 0F;
            if (el.get("_time").type == JSONTypes.Integer)
            {
                time = (float)el.get("_time")._int;
            } else if (el.get("_time").type == JSONTypes.Double)
            {
                time = (float)el.get("_time")._double;
            }
            time /= notesManager.songData.bpmValues[0] / 60;
            int x = el.get("_lineIndex") != null ? el.get("_lineIndex")._int : 0;
            int y = el.get("_lineLayer") != null ? el.get("_lineLayer")._int : 0;
            int type = el.get("_type") != null ? el.get("_type")._int : 0;
            int direction = el.get("_cutDirection") != null ? el.get("_cutDirection")._int : 8;

            Note toAdd = new Note(time, x, y, type, direction);
            notesManager.notes.Add(toAdd);
        }

        notesManager.createBlocks();
    }

    public void readAudio(string name)
    {
        source.Stop();
        StartCoroutine(Import(name));
    }

    void Start()
    {
        //For testing purposes only
        this.readSong("D:\\SteamLibrary\\steamapps\\common\\Beat Saber\\Beat Saber_Data\\CustomWIPLevels\\OOParts", "Standard", "ExpertPlus");
    }

    IEnumerator Import(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            if (www.isHttpError || www.isNetworkError)
            {
                Debug.LogWarning(www.error);
            }
            else
            {
                source.clip = DownloadHandlerAudioClip.GetContent(www);
                this.notesManager.play();
            }
        }
    }
}