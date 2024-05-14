using System;
using System.IO;

public class stream_reader
{
    private char delimiter;
    private List<Tuple<int,string>> stream_message_list = new List<Tuple<int,string>>();

    public stream_reader(char delimiter)
    {
        this.delimiter = delimiter;
    }

    public void AddStreamConnection(int id)
    {
        stream_message_list.Add(new Tuple<int,string>(id, string.Empty)); 
    }
    public void RemoveStreamConnection(int id)
    {
        stream_message_list.RemoveAt(id_index(id));
    }

    public void AddData(int id, string data) //add data to stream connection with id
    {
        int i = id_index(id);
        stream_message_list[i] = new Tuple<int,string>(id, stream_message_list[i].Item2 + data);
    }

    public string GetData(int id)     //get data from stream connection with id, and remove it from the stream up to delimiter
    {
        int i = id_index(id);
        string data = stream_message_list[i].Item2;
        int index = data.IndexOf(delimiter);
        if (index == -1)
        {
            return string.Empty;
        }
        string message = data.Substring(0, index);
        stream_message_list[i] = new Tuple<int,string>(id, data.Substring(index + 1));
        return message;
    }
    private int id_index(int id)
    {
        for (int i = 0; i < stream_message_list.Count; i++)
        {
            if (stream_message_list[i].Item1 == id)
            {
                return i;
            }
        }
        return -1;
    }
    


}