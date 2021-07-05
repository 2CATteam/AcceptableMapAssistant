using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum JSONTypes
{
    Boolean,
    Integer,
    Double,
    String,
    Array,
    Object,
    Null
}

public class JSONData
{
    public bool _bool;
    public int _int;
    public double _double;
    public string _string;
    public List<JSONData> _array;
    public Dictionary<string, JSONData> _object;

    public JSONTypes type;

    public JSONData(bool val)
    {
        this._bool = val;
        this.type = JSONTypes.Boolean;
    }

    public JSONData(int val)
    {
        this._int = val;
        this.type = JSONTypes.Integer;
    }

    public JSONData(double val)
    {
        this._double = val;
        this.type = JSONTypes.Double;
    }

    public JSONData(string val)
    {
        this._string = val;
        this.type = JSONTypes.String;
    }

    public JSONData(List<JSONData> val)
    {
        this._array = val;
        this.type = JSONTypes.Array;
    }

    public JSONData(Dictionary<string, JSONData> val)
    {
        this._object = val;
        this.type = JSONTypes.Object;
    }

    public JSONData()
    {
        this.type = JSONTypes.Null;
    }

    public int Count()
    {
        if (type == JSONTypes.Object)
        {
            return this._object.Count;
        }
        else if (type == JSONTypes.Array)
        {
            return this._array.Count;
        }
        else
        {
            return -1;
        }
    }

    public JSONData get(string key)
    {
        if (type != JSONTypes.Object)
        {
            return null;
        }
        return _object[key];
    }
    public JSONData get(int index)
    {
        if (type != JSONTypes.Array)
        {
            return null;
        }
        return _array[index];
    }

    public static JSONData parseJSON(string data)
    {
        Debug.Log(data);
        JSONData toReturn = new JSONData();
        int index = 0;
        if (data[index] == '{')
        {
            parseObject(ref toReturn, ref data, ref index);
        }
        else if (data[index] == '[')
        {
            parseList(ref toReturn, ref data, ref index);
        }
        return toReturn;
    }

    private static void parseList(ref JSONData parent, ref string data, ref int index)
    {
        parent.type = JSONTypes.Array;
        parent._array = new List<JSONData>();
        //Until high-level reading reaches the end of the object
        while (data[index++] != ']')
        {
            string starts = "\"-+1234567890.{[ntf";
            if (starts.Contains(data[index].ToString()))
            {
                JSONData child = new JSONData();
                switch (data[index])
                {
                    case '"':
                        child.type = JSONTypes.String;
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
                        child._string = value.ToString();
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
                            child.type = JSONTypes.Boolean;
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
                            child.type = JSONTypes.Boolean;
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
                            child.type = JSONTypes.Double;
                            child._double = double.Parse(str);
                        }
                        else
                        {
                            child.type = JSONTypes.Integer;
                            child._int = int.Parse(str);
                        }
                        break;
                }
                parent._array.Add(child);
            }
        }
    }

    private static void parseObject(ref JSONData parent, ref string data, ref int index)
    {
        parent.type = JSONTypes.Object;
        parent._object = new Dictionary<string, JSONData>();
        //Until high-level reading reaches the end of the object
        while (data[index++] != '}')
        {
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
                    }
                    else if (data[index] == '\\')
                    {
                        escaped = true;
                    }
                    else
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
                while (!starts.Contains(data[index].ToString()))
                {
                    if (data[index] != ' ')
                    {
                        Debug.LogWarning(data[index]);
                    }
                    index++;
                }
                JSONData child = new JSONData();
                switch (data[index])
                {
                    case '"':
                        child.type = JSONTypes.String;
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
                        child._string = value.ToString();
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
                            child.type = JSONTypes.Boolean;
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
                            child.type = JSONTypes.Boolean;
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
                            child.type = JSONTypes.Double;
                            child._double = double.Parse(str);
                        }
                        else
                        {
                            child.type = JSONTypes.Integer;
                            child._int = int.Parse(str);
                        }
                        break;
                }
                parent._object.Add(key.ToString(), child);
            }
        }
        return;
    }

    public string toString(int depth = 0)
    {
        StringBuilder toReturn = new StringBuilder();

        if (this.type == JSONTypes.Object)
        {
            toReturn.Append("{\n");
            depth++;
            foreach (KeyValuePair<string, JSONData> kvp in this._object)
            {
                toReturn.Append(this.genPadding(depth));
                //Forgive me for what I am about to do to this syntax
                string key = kvp.Key;
                key = key.Replace("\b", "\\b").Replace("\f", "\\f").Replace("\n", "\\n").Replace("\r", "\\r");
                key = key.Replace("\t", "\\t").Replace("\"", "\\\"").Replace("\\", "\\\\");
                toReturn.Append("\"" + key + "\": " + kvp.Value.toString(depth) + ",\n");
            }
            if (this._object.Count > 0)
            {
                toReturn.Remove(toReturn.Length - 2, 1);
            }
            depth--;
            toReturn.Append(this.genPadding(depth) + "}");
        }
        else if (this.type == JSONTypes.Array)
        {
            toReturn.Append("[\n");
            depth++;
            foreach (JSONData element in this._array)
            {
                toReturn.Append(this.genPadding(depth));
                toReturn.Append(element.toString(depth) + ",\n");
            }
            if (this._array.Count > 0)
            {
                toReturn.Remove(toReturn.Length - 2, 1);
                toReturn.Append(this.genPadding(depth - 1) + "]");
            }
            else
            {
                toReturn.Remove(toReturn.Length - 1, 1);
                toReturn.Append("]");
            }
            depth--;

        }
        else if (this.type == JSONTypes.Boolean)
        {
            if (this._bool)
            {
                toReturn.Append("true");
            }
            else
            {
                toReturn.Append("false");
            }
        }
        else if (this.type == JSONTypes.Double)
        {
            toReturn.Append(this._double.ToString());
        }
        else if (this.type == JSONTypes.Integer)
        {
            toReturn.Append(this._int.ToString());
        }
        else if (this.type == JSONTypes.String)
        {
            string key = this._string;
            key = key.Replace("\b", "\\b").Replace("\f", "\\f").Replace("\n", "\\n").Replace("\r", "\\r");
            key = key.Replace("\t", "\\t").Replace("\"", "\\\"").Replace("\\", "\\\\");
            toReturn.Append("\"" + key + "\"");
        }
        else if (this.type == JSONTypes.Null)
        {
            toReturn.Append("null");
        }

        return toReturn.ToString();
    }

    private string genPadding(int depth)
    {
        string toReturn = "";
        for (int i = 0; i < depth; i++)
        {
            toReturn += "\t";
        }
        return toReturn;
    }
}

