using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MaieBlazorLib
{
    public class DragDropApi<T> // Done because using <T> in every method was bad for performance
    {
        public static (IList<T> List, int Index)? Dragged;
        public static (IList<T> List, int Index)? Over;

        static void Clear()
        {
            Dragged = null;
            Over = null;
        }

        public static void StartDrag(IList<T> t, int e)
        {
            Dragged = (t, e);
        }

        /// <summary>
        /// Stops the drag and Swaps the items if both Dragged and Over are not null and have valid indices.
        /// </summary>
        /// <returns>Returns true if the swap was sucessful</returns>
        public static bool StopDragSwap()
        {
            if (Over != null && Dragged != null)
            {
                if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                {
                    Debug.WriteLine($"Switcheroo!");
                    var temp = Over.Value.List[Over.Value.Index];
                    Over.Value.List[Over.Value.Index] = Dragged.Value.List[Dragged.Value.Index];
                    Dragged.Value.List[Dragged.Value.Index] = temp;
                    Clear();
                    return true;
                }
            }
            Clear();
            return false;
        }

        /// <summary>
        /// Use 'true' to only allow a swap if both items are in the same list. Stops the drag and Swaps the items if both Dragged and Over are not null and have valid indices.
        /// </summary>
        /// <returns>Returns true if the swap was sucessful</returns>
        public static bool StopDragSwap(bool OnlyinSameList)
        {
            if (OnlyinSameList)
            {
                if (Over != null && Dragged != null && Over.Value.List == Dragged.Value.List)
                {
                    if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                    {
                        Debug.WriteLine($"Switcheroo!");
                        var temp = Over.Value.List[Over.Value.Index];
                        Over.Value.List[Over.Value.Index] = Dragged.Value.List[Dragged.Value.Index];
                        Dragged.Value.List[Dragged.Value.Index] = temp;
                        Clear();
                        return true;
                    }
                }
            }
            else
                return StopDragSwap();
            return false;
        }

        /// <summary>
        /// Stops drag and Inserts the dragged element in the List of the hovered element, at its position
        /// </summary>
        /// <returns>Returns true if the insertion was sucessful</returns>
        public static bool StopDragInsert()
        {
            if (Over != null && Dragged != null)
            {
                if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                {
                    Debug.WriteLine($"Insertooo!");
                    T temp = GetDragged();
                    Dragged.Value.List.RemoveAt(Dragged.Value.Index);
                    Over.Value.List.Insert(Over.Value.Index, temp);
                    Clear();
                    return true;
                }
            }
            Clear();
            return false;
        }

        /// <summary>
        /// Use 'true' to only allow insertion only if both items are in the same list. Stops drag and Inserts the dragged element in the List of the hovered element, at its position
        /// </summary>
        /// <returns>Returns true if insertion was sucessful</returns>
        public static bool StopDragInsert(bool OnlyinSameList)
        {
            if (OnlyinSameList)
            {
                if (Over != null && Dragged != null && Over.Value.List == Dragged.Value.List)
                {
                    if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                    {
                        Debug.WriteLine($"Insertooo!");
                        T temp = GetDragged();
                        Dragged.Value.List.RemoveAt(Dragged.Value.Index);
                        Over.Value.List.Insert(Over.Value.Index, temp);
                        Clear();
                        return true;
                    }
                }
            }
            else
                return StopDragInsert();
            return false;
        }

        public static void OverHere(IList<T> t, int e)
        {
            Debug.WriteLine($"Over i = {e}");
            Over = (t, e);
        }

        public static void OuttaHere()
        {
            Debug.WriteLine($"Mouse out!");
            Over = null;
        }

        public static T GetDragged()
        {
            if (Dragged != null)
            {
                return Dragged.Value.List[Dragged.Value.Index];
            }
            return default(T);
        }

        public static T GetOver()
        {
            if (Over != null)
            {
                return Over.Value.List[Over.Value.Index];
            }
            return default(T);
        }

        public static IList<T> GetDraggedList()
        {
            if (Dragged != null)
            {
                return Dragged.Value.List;
            }
            return null;
        }

        public static IList<T> GetOverList()
        {
            if (Over != null)
            {
                return Over.Value.List;
            }
            return null;
        }

    }
}
