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
        static IDragDropBehaviour DefaultBehaviour = DragDropBehaviour.Insert;

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
            return StopDrag(DefaultBehaviour);
        }

        /// <summary>
        /// Use 'true' to only allow a insert if both items are in the same list. 
        /// Stops the drag and Inserts the dragged item in the list and position of the Hovered item if both Dragged and Over are not null and have valid indexes.
        /// </summary>
        /// <returns>Returns true if the swap was sucessful</returns>
        public static bool StopDrag(bool OnlyinSameList)
        {
            return StopDrag(OnlyinSameList, DefaultBehaviour);
        }

        /// <summary>
        /// Stops the drag and performs the behaviour defined by the DragDropBehaviour passed as parameter.
        /// ( .Insert, .Swap, .Replace, .Copy )
        /// </summary>
        /// <returns></returns>
        public static bool StopDrag(IDragDropBehaviour behaviour)
        {
            try
            {
                if (Over != null && Dragged != null)
                {
                    if (Dragged.Value.Index >= 0 && Over.Value.Index >= 0)
                    {
                        try
                        {
                            behaviour.Behave<T>(Dragged, Over);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("DragDropBehavior error: " + e.Message);
                        }
                        Clear();
                        Debug.WriteLine($"DragDrop operation successful with behaviour {behaviour.GetType().Name}");
                        return true;
                    }
                    else Debug.WriteLine($"DragDrop failed: Invalid list index for Dragged item or Hovered item");
                }
                else Debug.WriteLine($"DragDrop failed: {(Over == null ? "No Hovered item":"")} {(Dragged == null ? "No Dragged item" : "")}");
                Clear();
                
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unhandled DragDrop operation error: {e.Message}");
                Clear();
                return false;
            }
        }

        /// <summary>
        /// Use 'true' to only allow behaviour if both items are in the same list.
        /// Stops the drag and performs the behaviour defined by the DragDropBehaviour passed as parameter.
        /// ( .Insert, .Swap, .Replace, .Copy )
        /// </summary>
        /// <returns></returns>
        public static bool StopDrag(bool OnlyinSameList, IDragDropBehaviour behaviour)
        {
            try
            {
                if (OnlyinSameList)
                {
                    if (Over!.Value.List == Dragged!.Value.List)
                        return StopDrag(behaviour);
                    else
                    {
                        Debug.WriteLine("DragDrop operation failed: Items are not of the same list");
                        Clear();
                        return false;
                    }
                }
                else
                    return StopDrag(behaviour);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unhandled DragDrop operation Exception: {e.Message}");
                Clear();
                return false;
            }
        }

        public static void OverHere(IList<T> t, int e)
        {
            //Debug.WriteLine($"Over i = {e}");
            Over = (t, e);
        }

        public static void OuttaHere()
        {
            //Debug.WriteLine($"Mouse out!");
            Over = null;
        }

        public static T? GetDragged()
        {
            if (Dragged != null)
            {
                return Dragged.Value.List[Dragged.Value.Index];
            }
            return default(T);
        }

        public static T? GetOver()
        {
            if (Over != null)
            {
                return Over.Value.List[Over.Value.Index];
            }
            return default(T);
        }

        public static IList<T>? GetDraggedList()
        {
            if (Dragged != null)
            {
                return Dragged.Value.List;
            }
            return null;
        }

        public static IList<T>? GetOverList()
        {
            if (Over != null)
            {
                return Over.Value.List;
            }
            return null;
        }

        public static int GetDraggedIndex()
        {
            if (Dragged != null)
            {
                return Dragged.Value.Index;
            }
            return -1;
        }

        public static int GetOverIndex()
        {
            if (Over != null)
            {
                return Over.Value.Index;
            }
            return -1;
        }

    }

    public interface IDragDropBehaviour
    {
        void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over);
    }

    public class DragDropBehaviour
    {
        public static IDragDropBehaviour Swap => DragDropBehaviourSwap.Instance;
        public static IDragDropBehaviour Insert => DragDropBehaviourInsert.Instance;
        public static IDragDropBehaviour InsertAfter => DragDropBehaviourInsertAfter.Instance;
        public static IDragDropBehaviour Push => DragDropBehaviourPush.Instance;
        public static IDragDropBehaviour Add => DragDropBehaviourAdd.Instance;
        public static IDragDropBehaviour Replace => DragDropBehaviourReplace.Instance;
        public static IDragDropBehaviour Copy => DragDropBehaviourCopy.Instance;
        public static IDragDropBehaviour Cancel => DragDropBehaviourCancel.Instance;
    }

    internal class DragDropBehaviourSwap : IDragDropBehaviour
    {
        public static readonly DragDropBehaviourSwap Instance = new();
        public void Behave<T>((IList<T> List, int Index)?Dragged, (IList<T> List, int Index)?Over) 
        {
            //Debug.WriteLine($"Switcheroo!");
            var temp = Over!.Value.List[Over.Value.Index];
            Over.Value.List[Over.Value.Index] = Dragged!.Value.List[Dragged.Value.Index];
            Dragged.Value.List[Dragged.Value.Index] = temp;
        }
    }

    internal class DragDropBehaviourInsert : IDragDropBehaviour
    {
        public static readonly DragDropBehaviourInsert Instance = new();
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over)
        {
            // Debug.WriteLine($"Insertooo!");
            T temp = Dragged!.Value.List[Dragged.Value.Index];
            Dragged.Value.List.RemoveAt(Dragged.Value.Index);
            Over!.Value.List.Insert(Over.Value.Index, temp);
        }
    }

    internal class DragDropBehaviourPush : IDragDropBehaviour
    {
        public static readonly DragDropBehaviourPush Instance = new();
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over)
        {
            //Debug.WriteLine($"Insertooo!");
            T temp = Dragged!.Value.List[Dragged.Value.Index];
            Dragged.Value.List.RemoveAt(Dragged.Value.Index);
            Over!.Value.List.Insert(0, temp);
        }
    }

    internal class DragDropBehaviourAdd : IDragDropBehaviour
    {
        public static readonly DragDropBehaviourAdd Instance = new();
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over)
        {
            //Debug.WriteLine($"Insertooo!");
            T temp = Dragged!.Value.List[Dragged.Value.Index];
            Dragged!.Value.List.RemoveAt(Dragged.Value.Index);
            Over!.Value.List.Add(temp);
        }
    }

    internal class DragDropBehaviourInsertAfter : IDragDropBehaviour
    {
        public static readonly DragDropBehaviourInsertAfter Instance = new();
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over)
        {
            //Debug.WriteLine($"Insertooo Afterooo!");
            T temp = Dragged!.Value.List[Dragged.Value.Index];
            Dragged.Value.List.RemoveAt(Dragged.Value.Index);
            if (Over!.Value.Index >= Over.Value.List.Count - 1)
                Over.Value.List.Add(temp);
            else
                Over.Value.List.Insert(Over.Value.Index + 1, temp);
        }
    }

    internal class DragDropBehaviourReplace : IDragDropBehaviour
    {
        public static readonly DragDropBehaviourReplace Instance = new();
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over)
        {
            //Debug.WriteLine($"Replasooo!");
            Over!.Value.List[Over.Value.Index] = Dragged!.Value.List[Dragged.Value.Index];
            Dragged.Value.List.RemoveAt(Dragged.Value.Index);
        }
    }

    internal class DragDropBehaviourCopy : IDragDropBehaviour
    {
        public static readonly DragDropBehaviourCopy Instance = new();
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over)
        {
            //Debug.WriteLine($"Copyooo!");
            Over!.Value.List[Over.Value.Index] = Dragged!.Value.List[Dragged.Value.Index];
        }
    }

    internal class DragDropBehaviourCancel : IDragDropBehaviour
    {
        public static readonly DragDropBehaviourCopy Instance = new();
        public void Behave<T>((IList<T> List, int Index)? Dragged, (IList<T> List, int Index)? Over)
        {
            Debug.WriteLine($"Cancelled DragDrop operation");
        }
    }
}