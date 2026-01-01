using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Storage;

namespace MaieBlazorLib.HybridIO
{
    class HybridIO
    {
        private static HybridIOBase _instance;

        public static HybridIOBase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WindowsIO();
                }
                return _instance;
            }
        }
    }

    internal abstract class HybridIOBase
    {
        public abstract string ReadAllText(string path);
        public abstract void WriteAllText(string path, string content);

        public abstract void WriteAllTextAsync(string path, string content);
    }

    internal class WindowsIO : HybridIOBase
    {
        public override string ReadAllText(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public override void WriteAllText(string path, string content)
        {
            System.IO.File.WriteAllText(path, content);
        }

        public override async void WriteAllTextAsync(string path, string content)
        {
            await System.IO.File.WriteAllTextAsync(path, content);
        }
    }

    internal class AndroidIO : HybridIOBase
    {
        public override string ReadAllText(string path)
        {
            path = Path.Combine(FileSystem.AppDataDirectory, "data.txt");
            var text = File.ReadAllText(path);
            return text;
        }
        public override void WriteAllText(string path, string content)
        {
            path = Path.Combine(FileSystem.AppDataDirectory, "data.txt");
            File.WriteAllText(path, content);
        }

        public override async void WriteAllTextAsync(string path, string content)
        {
            path = Path.Combine(FileSystem.AppDataDirectory, "data.txt");
            await System.IO.File.WriteAllTextAsync(path, content);
        }
    }
}