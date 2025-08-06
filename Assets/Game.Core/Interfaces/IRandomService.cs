using System.Collections.Generic;

namespace Game.Core.Randomness
{
    public interface IRandomService
    {
        float Range(float min, float max);
        int Range(int min, int max);
        bool Chance(float probability); // e.g. 0.25f = 25% chance
        T PickOne<T>(IReadOnlyList<T> list);
    }
}