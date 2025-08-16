using Game.Domain.Enums;

namespace Game.Application.Interfaces
{
    public interface IEnemyEncounterProvider
    {
        MonsterType[] GetRandomEnemyTeam();
        MonsterType[] GetEnemyTeamForBiome(Biome biome);
        MonsterType[] GetEnemyTeamForBiomeAndDifficulty(Biome biome, int maxDifficulty);
    }
}