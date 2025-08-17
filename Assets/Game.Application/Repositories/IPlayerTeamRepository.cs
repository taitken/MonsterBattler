using System.Collections.Generic;
using Game.Domain.Entities;

namespace Game.Application.Repositories
{
    public interface IPlayerTeamRepository
    {
        /// <summary>
        /// Gets the current player monster team. Creates default team if none exists.
        /// </summary>
        List<MonsterEntity> GetPlayerTeam();
        
        /// <summary>
        /// Updates the player team with the given monsters.
        /// </summary>
        void UpdatePlayerTeam(List<MonsterEntity> monsters);
        
        /// <summary>
        /// Restores the player team to full health.
        /// </summary>
        void HealPlayerTeam();
        
        /// <summary>
        /// Checks if the player has any monsters in their team.
        /// </summary>
        bool HasPlayerTeam();
        
        /// <summary>
        /// Creates a default starting team for new players.
        /// </summary>
        void InitializeDefaultTeam();
    }
}