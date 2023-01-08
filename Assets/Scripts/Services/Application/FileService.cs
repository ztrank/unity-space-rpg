using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Zenject;

public interface IFileService
{
    bool Exists(string path);
    string ReadAllText(string path);
    byte[] ReadAllBytes(string path);
    void WriteAllText(string path, string data);
    void WriteAllBytes(string path, byte[] data);
}

public class FileService : IFileService
{
    private readonly IDirectoryService directoryService;

    [Inject]
    public FileService(IDirectoryService directoryService)
    {
        this.directoryService = directoryService;
    }

    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    public void WriteAllText(string path, string data)
    {
        this.directoryService.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, data);
    }

    public void WriteAllBytes(string path, byte[] data)
    {
        this.directoryService.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, data);
    }
}
