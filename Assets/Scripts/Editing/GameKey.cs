// AES code provided by https://stackoverflow.com/questions/165808/simple-insecure-two-way-data-obfuscation

using System.Security.Cryptography;
using System.IO;
using UnityEngine;

class SimpleAES
{
    private byte[] Key = { 1 , 2 };
    byte[] Vector = { 1 };

    private ICryptoTransform EncryptorTransform, DecryptorTransform;
    private System.Text.UTF8Encoding UTFEncoder;

    public SimpleAES(byte [] Key, byte [] Vector)
    {
        this.Key = Key;
        this.Vector = Vector;

        // This is our encryption method
        RijndaelManaged rm = new RijndaelManaged();

        // Create an encryptor and a decryptor using our encryption method, key, and vector.
        EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
        DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

        // Used to translate bytes to text and vice versa
        UTFEncoder = new System.Text.UTF8Encoding();
    }

    public string EncryptToString(string TextValue)
    {
        return ByteArrToString(Encrypt(TextValue));
    }

    // Encrypt some text and return an encrypted byte array
    public byte[] Encrypt(string TextValue)
    {
        // Translates our text value into a byte array
        byte[] bytes = UTFEncoder.GetBytes(TextValue);

        // Used to stream the data in and out of the CryptoStream
        MemoryStream memoryStream = new MemoryStream();

        /*
         * We will have to write the unencrypted bytes to the stream,
         * then read the encrypted result back from the stream.
         */
        #region Write the decrypted value to the encryption stream
        CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
        cs.Write(bytes, 0, bytes.Length);
        cs.FlushFinalBlock();
        #endregion

        #region Read encrypted value back out of the stream
        memoryStream.Position = 0;
        byte[] encrypted = new byte[memoryStream.Length];
        memoryStream.Read(encrypted, 0, encrypted.Length);
        #endregion

        // Clean up
        cs.Close();
        memoryStream.Close();

        return encrypted;
    }
    public string DecryptString(string EncryptedString)
    {
        return Decrypt(StrToByteArray(EncryptedString));
    }

    /// Decryption when working with byte arrays  
    public string Decrypt(byte[] EncryptedValue)
    {
        #region Write the encrypted value to the decryption stream
        MemoryStream encryptedStream = new MemoryStream();
        CryptoStream decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);
        decryptStream.Write(EncryptedValue, 0, EncryptedValue.Length);
        try
        {
            decryptStream.FlushFinalBlock(); 
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            encryptedStream.Close();
            return string.Empty;
            throw;
        }
        #endregion

        #region Read the decrypted value from the stream.
        encryptedStream.Position = 0;
        byte[] decryptedBytes = new byte[encryptedStream.Length];
        encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
        encryptedStream.Close();
        #endregion
        return UTFEncoder.GetString(decryptedBytes);
    }

    public byte[] StrToByteArray(string str)
    {
        if (str.Length == 0)
        {
            Debug.LogError("Invalid string value in StrToByteArray");
            return null;
        }

        byte val;
        byte[] byteArr = new byte[str.Length / 3];
        int i = 0;
        int j = 0;
        do
        {
            val = byte.Parse(str.Substring(i, 3));
            byteArr[j++] = val;
            i += 3;
        }
        while (i < str.Length);
        return byteArr;
    }
    public string ByteArrToString(byte[] byteArr)
    {
        byte val;
        string tempStr = "";
        for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
        {
            val = byteArr[i];
            if (val < (byte)10)
                tempStr += "00" + val.ToString();
            else if (val < (byte)100)
                tempStr += "0" + val.ToString();
            else
                tempStr += val.ToString();
        }
        return tempStr;
    }
}

[CreateAssetMenu(fileName = "GameKey", menuName = "ScriptableObjects/GameKey", order = 1)]
[System.Serializable]
public class GameKey : ScriptableObject
{
    [Tooltip ("The unique ID for this game, this will make only save files with this value in it acceptable")]
    [SerializeField]
    string GameID = "Hiu8H7hg7FJ6905H";

    public string GetGameID { get { return GameID;}}

    [Tooltip ("The encryption key for the game's ID, it is advised to change this for your game")]
    [SerializeField]
    byte[] Key = { 123, 217, 19, 11, 24, 26, 85, 45, 114, 184, 27, 162, 37, 112, 222, 209, 241, 24, 175, 144, 173, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 };
    
    [Tooltip ("The encryption vector for the game's ID, it is advised to change this for your game")]
    [SerializeField]
    byte[] Vector = { 146, 64, 191, 111, 23, 3, 113, 119, 231, 121, 252, 112, 79, 32, 114, 156 };

    SimpleAES simpleAES;

    public byte[] Encrypt(string stringToEncrypt)
    {
        simpleAES = new SimpleAES(Key,Vector);

        return simpleAES.Encrypt(stringToEncrypt);
    }
    public string EncryptString(string stringToEncrypt)
    {
        if(simpleAES == null) simpleAES = new SimpleAES(Key,Vector);

        return simpleAES.EncryptToString(stringToEncrypt);
    }
    public string Decrypt(string stringToDecrypt)
    {
        if(simpleAES == null) simpleAES = new SimpleAES(Key,Vector);

        return simpleAES.DecryptString(stringToDecrypt);
    }
}