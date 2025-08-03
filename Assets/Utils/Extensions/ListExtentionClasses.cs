using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace System.Collections.Generic
{
    public static class ListExtenionClasses
    {
        public static t1 Find<t1>(this IList<t1> list, Func<t1, bool> callback)
        {
            foreach (t1 listItem in list)
            {
                if (callback(listItem))
                {
                    return listItem;
                }
            }
            return default(t1);
        }

        public static bool Any<t1>(this IList<t1> list, Func<t1, bool> callback)
        {
            foreach (t1 listItem in list)
            {
                if (callback(listItem))
                {
                    return true;
                }
            }
            return false;
        }

        public static IList<t1> Filter<t1>(this IList<t1> list, Func<t1, bool> callback)
        {
            IList<t1> returnList = new List<t1>();
            foreach (t1 listItem in list)
            {
                if (callback(listItem))
                {
                    returnList.Add(listItem);
                }
            }
            return returnList;
        }

        public static IList<t2> Map<t1, t2>(this IList<t1> list, Func<t1, t2> callback)
        {
            if (list == null) return null;
            IList<t2> returnList = new List<t2>();
            foreach (t1 listItem in list)
            {
                returnList.Add(callback(listItem));
            }
            return returnList;
        }

        public static IList<t1> Flatten<t1>(this IList<IList<t1>> list)
        {
            if (list == null) return null;
            IList<t1> returnList = new List<t1>();
            foreach (t1 listItem in list)
            {
                foreach (t1 subListItem in list)
                {
                    returnList.Add(subListItem);
                }
            }
            return returnList;
        }

        public static int Sum<t1>(this IList<t1> list, Func<t1, int> callback)
        {
            int returnVal = 0;
            foreach (t1 listItem in list)
            {
                returnVal += callback(listItem);
            }
            return returnVal;
        }

        public static long Sum<t1>(this IList<t1> list, Func<t1, long> callback)
        {
            long returnVal = 0;
            foreach (t1 listItem in list)
            {
                returnVal += callback(listItem);
            }
            return returnVal;
        }

        public static float Sum<t1>(this IList<t1> list, Func<t1, float> callback)
        {
            float returnVal = 0;
            foreach (t1 listItem in list)
            {
                returnVal += callback(listItem);
            }
            return returnVal;
        }


        public static decimal Sum<t1>(this IList<t1> list, Func<t1, decimal> callback)
        {
            decimal returnVal = 0;
            foreach (t1 listItem in list)
            {
                returnVal += callback(listItem);
            }
            return returnVal;
        }


        public static void ForEach<t1>(this IList<t1> list, Action<t1> callback)
        {
            foreach (t1 listItem in list)
            {
                callback(listItem);
            }
        }

        public static void AddRange<T>(this IList<T> collection, IList<T> enumerable)
        {
            foreach (var cur in enumerable)
            {
                collection.Add(cur);
            }
        }

        public static void ForEach<t1>(this IList<t1> list, Action<t1, int> callback)
        {
            for (int i = 0; i < list.Count; i++)
            {
                callback(list[i], i);
            }
        }

        public static void DestroyAll<t1>(this IList<t1> list) where t1 : MonoObject
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                list[i].Destroy();
                list.RemoveAt(i);
            }
        }


        public static Vector3Int ConvertToVector3Int(this Vector3 vec3)
        {
            return new Vector3Int((int)vec3.x, (int)vec3.y, (int)vec3.z);
        }

        public static void ForEach(this Vector3Int vector3, Action<int, int> callback)
        {
            for (int x = 0; x < vector3.x; x++)
            {
                for (int y = 0; y < vector3.y; y++)
                {
                    callback(x, y);
                }
            }
        }

        public static void ForEach(this Vector3Int vector3, Action<int, int, int> callback)
        {
            for (int x = 0; x < vector3.x; x++)
            {
                for (int y = 0; y < vector3.y; y++)
                {
                    for (int z = 0; z < vector3.z; z++)
                    {
                        callback(x, y, z);

                    }
                }
            }
        }

        public static void ForEach(this Tilemap tilemap, Action<Tile> callback)
        {
            tilemap.size.ForEach((x, y, z) =>
            {
                callback(tilemap.GetTile<Tile>(new Vector3Int(x, y, z)));
            });
        }

        // Compares this list to the parameter list, and will match items by ID. 
        // Returns those that are in this list but not the parameter list.
        public static IList<t> GetNewModels<t>(this IList<t> newList, IList<t> oldList) where t : BaseModel
        {
            return newList.Filter(newModel => { return oldList.Find(oldModel => { return oldModel.ID == newModel.ID; }) == null; });
        }

        // Compares this list to the parameter list, and will match items by ID. 
        // Returns those that are in the parameter list, but not this list.
        public static IList<t> GetRemovedModels<t>(this IList<t> newList, IList<t> oldList) where t : BaseModel
        {
            return oldList.Filter(oldModel => { return newList.Find(newModel => { return newModel.ID == oldModel.ID; }) == null; });
        }

        public static List<T> ConvertToList<T>(this T[,] array)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            List<T> ret = new List<T>(width * height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    ret.Add(array[i, j]);
                }
            }
            return ret;
        }

        public static bool ValidIndex<T>(this T[,] array, int xIndex, int yIndex)
        {
            if (xIndex < 0 || xIndex >= array.GetLength(0)) return false;
            if (yIndex < 0 || yIndex >= array.GetLength(1)) return false;
            return true;
        }

        public static string ConcatStrings(this IList<string> strings, string delimiter)
        {
            if (strings.Count > 0)
            {
                string newString = strings[0];
                if (strings.Count > 1)
                {
                    for (int i = 1; i < strings.Count; i++)
                    {
                        newString = newString + delimiter + strings[i];
                    }
                }
                return newString;
            }
            else
            {
                return "";
            }
        }
    }
}