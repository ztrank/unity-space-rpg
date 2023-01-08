using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface IDirectoryService
{
    bool Exists(string path);
    DirectoryInfo CreateDirectory(string path);
}

public class DirectoryService : IDirectoryService
{
    public DirectoryInfo CreateDirectory(string path)
    {
        throw new System.NotImplementedException();
    }


    public bool Exists(string path)
    {
        return Directory.Exists(path);
    }
}
