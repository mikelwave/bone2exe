using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
class SaveFile
{
    public byte[] gameKey = DataShare.self.GetEncryptedKey();
    World[] worlds = new World[4];
    public bool hadRun = false;
    public double totalGameTime = 0;
    public int totalDeaths = 0;
    public SaveFile(World[] worlds)
    {
        this.worlds = worlds;
    }
    public World[] GetWorlds()
    {
        return worlds;
    }
}
[System.Serializable]
class SettingsFile
{
    GameSettings gameSettings;
    public SettingsFile(GameSettings settings)
    {
        this.gameSettings = settings;
    }
    public GameSettings GetGameSettings()
    {
        return gameSettings;
    }
}
public static class SaveLoadData
{
    static string path = string.Empty;
    public static bool saveComplete = true;
    public static bool saveFailed = false;
    public static bool loadComplete = false;

    static void InitPath()
    {
        #if UNITY_EDITOR
        path = Application.persistentDataPath;
        #endif

        #if UNITY_STANDALONE && !UNITY_EDITOR
        path = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
        #endif
        ///Debug.Log("Save path: "+path);
    }
    public static void SaveSettings(string filename)
    {
        saveComplete = false;
        if(path == string.Empty) InitPath();

        BinaryFormatter bf = new BinaryFormatter();
        
        SettingsFile settingsFile = new SettingsFile(GameSettings.self);
        FileStream file;
        try
        {
            file = File.Create(path + '/' + filename + ".save");
            try
            {
                bf.Serialize(file,settingsFile);
                file.Close();
                Debug.Log("Saved in: "+(path + '/' + filename + ".save"));
                saveComplete = true;
            }
            catch (System.Exception)
            {
                saveComplete = true;
                saveFailed = true;
                file.Close();
                throw;
            }
        }
        catch (System.UnauthorizedAccessException)
        {
            saveComplete = true;
            saveFailed = true;
            Debug.Log("Access to path: "+path+" is denied.");
            throw;
        }
    }
    public static GameSettings LoadSettings(string filename)
    {
        loadComplete = false;
        if(path == string.Empty) InitPath();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        try
        {
            file = File.OpenRead(path + '/' + filename + ".save");

            try
            {
                SettingsFile settingsFile = (SettingsFile)bf.Deserialize(file);
                file.Close();
                Debug.Log("Successfully loaded settings data for requested file: "+filename);
                loadComplete = true;
                return settingsFile.GetGameSettings();
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                file.Close();
                File.Delete(path + '/' + filename + ".save");
                Debug.LogError("Settings data has been tampered with or is corrupt. Requested file: "+filename);
            } 

        }

        catch (System.IO.FileNotFoundException)
        {  
            Debug.Log("No file settings data present to load for requested file: "+filename);
        }

        catch(System.Exception)
        {
            throw;
        }
        loadComplete = true;
        return new GameSettings();
    }
    public static void Save(string filename, World[] worlds)
    {
        saveComplete = false;
        if(path == string.Empty) InitPath();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        try
        {
            file = File.Create(path + '/' + filename + ".save");
            SaveFile saveFile = new SaveFile(worlds);
            saveFile.totalDeaths = DataShare.totalDeaths;
            saveFile.totalGameTime = DataShare.totalGameTime;
            saveFile.hadRun = DataShare.hadRun;

            try
            {
                bf.Serialize(file,saveFile);
                ///Debug.Log("Encrypted key: "+saveFile.gameKey);
                file.Close();

                Debug.Log("Saved in: "+(path + '/' + filename + ".save"));
                saveComplete = true;
            }
            catch (System.Exception)
            {
                saveComplete = true;
                saveFailed = true;
                file.Close();
                throw;
            }
        }
        catch (System.UnauthorizedAccessException)
        {
            saveComplete = true;
            saveFailed = true;
            Debug.Log("Access to path: "+path+" is denied.");
        }
    }

    public static World[] Load(string filename)
    {
        loadComplete = false;
        if(path == string.Empty) InitPath();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        try
        {
            file = File.OpenRead(path + '/' + filename + ".save");

            try
            {
                SaveFile saveFile = (SaveFile)bf.Deserialize(file);
                file.Close();
                string s = string.Empty;
                foreach (byte b in saveFile.gameKey)
                {
                    s+=b.ToString("000");
                }
                ///Debug.Log("Game key: "+s);
                if(!DataShare.self.IsValidKey(s))
                {
                    Debug.Log("Save file key does not match the game's key! Requested file: "+filename);
                    loadComplete = true;
                    return null;
                }

                loadComplete = true;
                DataShare.totalDeaths = saveFile.totalDeaths;
                DataShare.totalGameTime = saveFile.totalGameTime;
                DataShare.hadRun = saveFile.hadRun;


                Debug.Log("Successfully loaded file data for requested file: "+filename);
                return saveFile.GetWorlds();
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                file.Close();
                File.Delete(path + '/' + filename + ".save");
                Debug.LogError("File data has been tampered with or is corrupt. Requested file: "+filename);
            } 

        }

        catch (System.IO.FileNotFoundException)
        {  
            Debug.Log("No file data present to load for requested file: "+filename);
        }

        catch(System.Exception)
        {
            throw;
        }
        loadComplete = true;
        return null;
    }
}
