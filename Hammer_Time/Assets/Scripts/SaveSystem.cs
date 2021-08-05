using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;


public static class SaveSystem
{
    public static void SavePlayer(GameManager gm)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.hmr";
        FileStream stream = new FileStream(path, FileMode.Create);

        GameData data = new GameData(gm);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static GameData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.hmr";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }
}
