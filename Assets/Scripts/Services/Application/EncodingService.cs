using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum EncodingType
{
    UTF8,
    ASCII
}

public interface IEncodingService
{
    byte[] ToBytes(string data, EncodingType type = EncodingType.UTF8);
    string FromBytes(byte[] data, EncodingType type = EncodingType.UTF8);
}

public class EncodingService : IEncodingService
{
    public string FromBytes(byte[] bytes, EncodingType type = EncodingType.UTF8)
    {
        return type switch
        {
            EncodingType.ASCII => Encoding.ASCII.GetString(bytes),
            _ => Encoding.UTF8.GetString(bytes),
        };
    }

    public byte[] ToBytes(string data, EncodingType type = EncodingType.UTF8)
    {
        return type switch
        {
            EncodingType.ASCII => Encoding.ASCII.GetBytes(data),
            _ => Encoding.UTF8.GetBytes(data)
        };        
    }
}
