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
        /// Stops the drag and Inserts the dragged item in the list and position of the Hovered item if both Dragged and Over are not null and have valid indexes.
        /// </summary>
        /// <returns>Returns true if the swap was sucessful</returns>
        public static bool StopDrag()
        {
            if (Over != null && Dragged != null)
            {
                if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                {
                    DragDropBehaviour.Insert.Behave<T>(Dragged, Over);
                    Clear();
                    return true;
                }
            }
            Clear();
            return false;
        }

        /// <summary>
        /// Use 'true' to only allow a insert if both items are in the same list. 
        /// Stops the drag and Inserts the dragged item in the list and position of the Hovered item if both Dragged and Over are not null and have valid indexes.
        /// </summary>
        /// <returns>Returns true if the swap was sucessful</returns>
        public static bool StopDrag(bool OnlyinSameList)
        {
            if (OnlyinSameList)
            {
                if (Over != null && Dragged != null && Over.Value.List == Dragged.Value.List)
                {
                    if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                    {
                        DragDropBehaviour.Insert.Behave<T>(Dragged, Over);
                        Clear();
                        return true;
                    }
                }
            }
            else
                return StopDrag();
            return false;
        }

        /// <summary>
        /// Stops the drag and performs the behaviour defined by the DragDropBehaviour passed as parameter.
        /// ( .Insert, .Swap, .Replace, .Copy )
        /// </summary>
        /// <returns></returns>
        public static bool StopDrag(IDragDropBehaviour behaviour)
        {
            if (Over != null && Dragged != null)
            {
                if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                {
                    behaviour.Behave<T>(Dragged, Over);
                    Clear();
                    return true;
                }
            }
            Clear();
            return false;
        }

        /// <summary>
        /// Use 'true' to only allow behaviour if both items are in the same list.
        /// Stops the drag and performs the behaviour defined by the DragDropBehaviour passed as parameter.
        /// ( .Insert, .Swap, .Replace, .Copy )
        /// </summary>
        /// <returns></returns>
        public static bool StopDrag(bool OnlyinSameList, IDragDropBehaviour behaviour)
        {
            if (OnlyinSameList)
            {
                if (Over != null && Dragged != null && Over.Value.List == Dragged.Value.List)
                {
                    if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                    {
                        behaviour.Behave<T>(Dragged, Over);
                        Clear();
                        return true;
                    }
                }
            }
            else
                return StopDrag(behaviour);
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

    public interface IDragDropBehaviour
    {
        void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over);
    }

    public class DragDropBehaviour
    {
        public static IDragDropBehaviour Swap = new DragDropBehaviourSwap();
        public static IDragDropBehaviour Insert = new DragDropBehaviourInsert();
        public static IDragDropBehaviour Replace = new DragDropBehaviourReplace();
        public static IDragDropBehaviour Copy = new DragDropBehaviourCopy();
    }

    class DragDropBehaviourSwap : IDragDropBehaviour
    {
        public void Behave<T>((IList<T> List, int Index)?Dragged, (IList<T> List, int Index)?Over) // not yet tested
        {
            Debug.WriteLine($"Switcheroo!");
            var temp = Over.Value.List[Over.Value.Index];
            Over.Value.List[Over.Value.Index] = Dragged.Value.List[Dragged.Value.Index];
            Dragged.Value.List[Dragged.Value.Index] = temp;
        }
    }

    class DragDropBehaviourInsert : IDragDropBehaviour
    {
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over) // not yet tested
        {
            Debug.WriteLine($"Insertooo!");
            T temp = Dragged.Value.List[Dragged.Value.Index];
            Dragged.Value.List.RemoveAt(Dragged.Value.Index);
            Over.Value.List.Insert(Over.Value.Index, temp);
        }
    }

    class DragDropBehaviourReplace : IDragDropBehaviour
    {
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over) // not yet tested
        {
            Debug.WriteLine($"Replasooo!");
            Over.Value.List[Over.Value.Index] = Dragged.Value.List[Dragged.Value.Index];
            Dragged.Value.List.RemoveAt(Dragged.Value.Index);
        }
    }

    class DragDropBehaviourCopy : IDragDropBehaviour
    {
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over) // not yet tested
        {
            Debug.WriteLine($"Copyooo!");
            Over.Value.List[Over.Value.Index] = Dragged.Value.List[Dragged.Value.Index];
        }
    }
}