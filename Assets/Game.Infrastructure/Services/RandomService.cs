using System.Collections.Generic;
using UnityEngine;
using Game.Core.Randomness;

namespace Game.Infrastructure.Randomness
{
    public class UnityRandomService : IRandomService
    {
        public float Range(float min, float max)
        {
            return Random.Range(min, max);
        }

        public int Range(int min, int max)
        {
            return Random.Range(min, max);
        }

        public bool Chance(float probability)
        {
            return Random.value < probability;
        }

        public T PickOne<T>(IReadOnlyList<T> list)
        {
            if (list == null || list.Count == 0)
                throw new System.ArgumentException("List must not be empty");

            return list[Random.Range(0, list.Count)];
        }
    }
}