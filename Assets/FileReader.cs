using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FileReader : MonoBehaviour
{
    public NotesManager notesManager;
    public AudioImporter importer;
    public AudioSource source;

    public void readSong(string folder, string difficulty)
    {
        StreamReader streamReader = new StreamReader(Path.Combine(folder, "Info.dat"));
        JSONElement element = this.parseJSON(streamReader.ReadToEnd());
        Debug.Log(element);
    }

    public void readNotes(string name)
    {

    }

    public void readAudio(string name)
    {
        Destroy(source.clip);
        StartCoroutine(Import(name));
    }

    IEnumerator Import(string path)
    {
        importer.Import(path);

        while (!importer.isInitialized && !importer.isError)
            yield return null;

        if (importer.isError)
            Debug.LogError(importer.error);

        source.clip = importer.audioClip;
    }

    private JSONElement parseJSON(string data)
    {
        JSONElement toReturn = new JSONElement();
        int index = 0;
        if (data[index] == '{')
        {
            parseObject(ref toReturn, ref data, ref index);
        } else if (data[index] == '[')
        {
            parseList(ref toReturn, ref data, ref index);
        }
        return toReturn;
    }

    private void parseList(ref JSONElement parent, ref string data, ref int index)
    {
        parent.type = DataTypes.Object;
        parent._array = new List<JSONElement>();
        //Until high-level reading reaches the end of the object
        while (data[++index] != ']')
        {
            string starts = "\"-+1234567890.{[ntf";
            if (starts.Contains(data[++index].ToString()))
            {
                JSONElement child = new JSONElement();
                switch (data[index])
                {
                    case '"':
                        child.type = DataTypes.String;
                        bool escaped = false;
                        StringBuilder value = new StringBuilder();
                        while (data[++index] != '"' || escaped)
                        {
                            if (escaped)
                            {
                                switch (data[index])
                                {
                                    case 'b':
                                        value.Append("\b");
                                        break;
                                    case 'f':
                                        value.Append("\f");
                                        break;
                                    case 'n':
                                        value.Append("\n");
                                        break;
                                    case 'r':
                                        value.Append("\r");
                                        break;
                                    case 't':
                                        value.Append("\t");
                                        break;
                                    case '"':
                                        value.Append("\"");
                                        break;
                                    case '\\':
                                        value.Append("\\");
                                        break;
                                }
                                escaped = false;
                            }
                            else if (data[index] == '\\')
                            {
                                escaped = true;
                            }
                            else
                            {
                                value.Append(data[index]);
                            }
                        }
                        break;
                    case 'n':
                        if (data[index + 1] != 'u' || data[index + 2] != 'l' || data[index + 2] != 'l')
                        {
                            Debug.LogWarning("Invalid null reading");
                            Debug.LogWarning(index);
                            Debug.LogWarning(data);
                        }
                        index += 3;
                        break;
                    case 't':
                        if (data[index + 1] == 'r' && data[index + 2] == 'u' && data[index + 3] != 'e')
                        {
                            child.type = DataTypes.Boolean;
                            child._bool = true;
                        }
                        else
                        {
                            Debug.LogWarning("Invalid True reading");
                            Debug.LogWarning(index);
                            Debug.LogWarning(data);
                        }
                        index += 3;
                        break;
                    case 'f':
                        if (data[index + 1] == 'a' && data[index + 2] == 'l' && data[index + 3] != 's' && data[index + 3] != 'e')
                        {
                            child.type = DataTypes.Boolean;
                            child._bool = false;
                        }
                        else
                        {
                            Debug.LogWarning("Invalid False reading");
                            Debug.LogWarning(index);
                            Debug.LogWarning(data);
                        }
                        index += 4;
                        break;
                    case '{':
                        parseObject(ref child, ref data, ref index);
                        break;
                    case '[':
                        parseList(ref child, ref data, ref index);
                        break;
                    case '.':
                    case '-':
                    case '+':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '0':
                        string numberStarts = "-+.1234567890";
                        StringBuilder buffer = new StringBuilder();
                        buffer.Append(data[index]);
                        while (numberStarts.Contains(data[++index].ToString()))
                        {
                            buffer.Append(data[index]);
                        }
                        string str = buffer.ToString();
                        if (str.Contains('.'.ToString()))
                        {
                            child.type = DataTypes.Double;
                            child._double = double.Parse(str);
                        }
                        else
                        {
                            child.type = DataTypes.Integer;
                            child._int = int.Parse(str);
                        }
                        break;
                }
                parent._array.Add(child);
            }
        }
    }

    private void parseObject(ref JSONElement parent, ref string data, ref int index)
    {
        parent.type = DataTypes.Object;
        parent._object = new Dictionary<string, JSONElement>();
        //Until high-level reading reaches the end of the object
        while (data[++index] != '}') {
            //Read a key
            if (data[index] == '"')
            {
                bool escaped = false;
                StringBuilder key = new StringBuilder();
                while (data[++index] != '"' || escaped)
                {
                    if (escaped)
                    {
                        switch (data[index])
                        {
                            case 'b':
                                key.Append("\b");
                                break;
                            case 'f':
                                key.Append("\f");
                                break;
                            case 'n':
                                key.Append("\n");
                                break;
                            case 'r':
                                key.Append("\r");
                                break;
                            case 't':
                                key.Append("\t");
                                break;
                            case '"':
                                key.Append("\"");
                                break;
                            case '\\':
                                key.Append("\\");
                                break;
                        }
                        escaped = false;
                    } else if (data[index] == '\\')
                    {
                        escaped = true;
                    } else
                    {
                        key.Append(data[index]);
                    }
                }
                while (data[++index] != ':')
                {
                    Debug.LogWarning(data[index]);
                }
                ++index;
                string starts = "\"-+1234567890.{[ntf";
                while (!starts.Contains(data[++index].ToString()))
                {
                    if (data[index] != ' ')
                    {
                        Debug.LogWarning(data[index]);
                    }
                }
                JSONElement child = new JSONElement();
                switch (data[index])
                {
                    case '"':
                        child.type = DataTypes.String;
                        escaped = false;
                        StringBuilder value = new StringBuilder();
                        while (data[++index] != '"' || escaped)
                        {
                            if (escaped)
                            {
                                switch (data[index])
                                {
                                    case 'b':
                                        value.Append("\b");
                                        break;
                                    case 'f':
                                        value.Append("\f");
                                        break;
                                    case 'n':
                                        value.Append("\n");
                                        break;
                                    case 'r':
                                        value.Append("\r");
                                        break;
                                    case 't':
                                        value.Append("\t");
                                        break;
                                    case '"':
                                        value.Append("\"");
                                        break;
                                    case '\\':
                                        value.Append("\\");
                                        break;
                                }
                                escaped = false;
                            }
                            else if (data[index] == '\\')
                            {
                                escaped = true;
                            }
                            else
                            {
                                value.Append(data[index]);
                            }
                        }
                        break;
                    case 'n':
                        if (data[index + 1] != 'u' || data[index + 2] != 'l' || data[index + 2] != 'l')
                        {
                            Debug.LogWarning("Invalid null reading");
                            Debug.LogWarning(index);
                            Debug.LogWarning(data);
                        }
                        index += 3;
                        break;
                    case 't':
                        if (data[index + 1] == 'r' && data[index + 2] == 'u' && data[index + 3] != 'e')
                        {
                            child.type = DataTypes.Boolean;
                            child._bool = true;
                        } else
                        {
                            Debug.LogWarning("Invalid True reading");
                            Debug.LogWarning(index);
                            Debug.LogWarning(data);
                        }
                        index += 3;
                        break;
                    case 'f':
                        if (data[index + 1] == 'a' && data[index + 2] == 'l' && data[index + 3] != 's' && data[index + 3] != 'e')
                        {
                            child.type = DataTypes.Boolean;
                            child._bool = false;
                        }
                        else
                        {
                            Debug.LogWarning("Invalid False reading");
                            Debug.LogWarning(index);
                            Debug.LogWarning(data);
                        }
                        index += 4;
                        break;
                    case '{':
                        parseObject(ref child, ref data, ref index);
                        break;
                    case '[':
                        parseList(ref child, ref data, ref index);
                        break;
                    case '.':
                    case '-':
                    case '+':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '0':
                        string numberStarts = "-+.1234567890";
                        StringBuilder buffer = new StringBuilder();
                        buffer.Append(data[index]);
                        while (numberStarts.Contains(data[++index].ToString()))
                        {
                            buffer.Append(data[index]);
                        }
                        string str = buffer.ToString();
                        if (str.Contains('.'.ToString()))
                        {
                            child.type = DataTypes.Double;
                            child._double = double.Parse(str);
                        } else
                        {
                            child.type = DataTypes.Integer;
                            child._int = int.Parse(str);
                        }
                        break;
                }
                parent._object.Add(key.ToString(), child);
            }
        }
        return;
    }
}

public enum DataTypes
{
    Boolean,
    Integer,
    Double,
    String,
    Array,
    Object,
    Null
}

public class JSONElement
{
    public bool _bool;
    public int _int;
    public double _double;
    public string _string;
    public List<JSONElement> _array;
    public Dictionary<string, JSONElement> _object;

    public DataTypes type;

    public JSONElement(bool val)
    {
        this._bool = val;
        this.type = DataTypes.Boolean;
    }

    public JSONElement(int val)
    {
        this._int = val;
        this.type = DataTypes.Integer;
    }

    public JSONElement(double val)
    {
        this._double = val;
        this.type = DataTypes.Double;
    }

    public JSONElement(string val)
    {
        this._string = val;
        this.type = DataTypes.String;
    }

    public JSONElement(List<JSONElement> val)
    {
        this._array = val;
        this.type = DataTypes.Array;
    }

    public JSONElement(Dictionary<string, JSONElement> val)
    {
        this._object = val;
        this.type = DataTypes.Object;
    }

    public JSONElement()
    {
        this.type = DataTypes.Null;
    }

    public DataTypes getType()
    {
        return type;
    }

    public JSONElement get(string key)
    {
        if (type != DataTypes.Object)
        {
            return null;
        }
        return _object[key];
    }

    public JSONElement get(int index)
    {
        if (type != DataTypes.Array)
        {
            return null;
        }
        return _array[index];
    }
}

