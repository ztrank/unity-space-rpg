using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FileService
{
    public static FileService Instance { get; } = new FileService();

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    internal bool FileExists(object pat)
    {
        throw new NotImplementedException();
    }

    public void WriteAllText(string path, string data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, data);
    }

    public void WriteAllBytes(string path, byte[] data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, data);
    }

    public string FromBytes(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public byte[] ToBytes(string data)
    {
        return Encoding.UTF8.GetBytes(data);
    }
}
