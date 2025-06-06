using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MaieBlazorLib.DragDrop
{
    public class DragDropApi<T> // Done because using <T> in every method was bad for performance
    {
        static (IList<T> List, int Index)? Dragged;
        static (IList<T> List, int Index)? Over;

        static void Clear()
        {
            Dragged = null;
            Over = null;
        }

        public static void StartDrag(IList<T> t, int e)
        {
            Dragged = (t, e);
        }

        public static void StopDrag()
        {
            if (Over != null && Dragged != null)
            {
                if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                {
                    Debug.WriteLine($"Switcheroo!");
                    var temp = Over.Value.List[Over.Value.Index];
                    Over.Value.List[Over.Value.Index] = Dragged.Value.List[Dragged.Value.Index];
                    Dragged.Value.List[Dragged.Value.Index] = temp;
                }
            }
            Clear();
        }

        public static void OverHere(IList<T> t, int e)
        {
            Over = (t, e);
        }

        public static void OuttaHere()
        {
            //Debug.WriteLine($"Mouse out!");
            Over = null;
        }
    }
}
