using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MaieBlazorLib
{
    public class CopyPasteApi
    {
        static object? Clipboard;

        public static void Copy<T>(T obj)
        {
            Clipboard = obj;
        }

        public static void Paste<T>(List<T> list, int index)
        {
            if (Clipboard is T item && index < list.Count)
            {
                list[index] = item;
            }
            else
                Debug.WriteLine("Error trying to paste incompatible types!");
        }
    }

    public class CopyPasteApiInstance
    {
        object? Clipboard;

        public void Copy<T>(T obj)
        {
            Clipboard = obj;
        }

        public void Paste<T>(List<T> list, int index)
        {
            if (Clipboard is T item && index < list.Count)
            {
                list[index] = item;
            }
            else
                Debug.WriteLine("Error trying to paste incompatible types!");
        }
    }
}
