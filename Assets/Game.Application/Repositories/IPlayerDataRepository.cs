using System.Collections.Generic;
using Game.Domain.Entities;

namespace Game.Application.Repositories
{
    /// <summary>
    /// Repository for managing persistent player data during a run.
    /// Pure data storage - no business logic.
    /// </summary>
    public interface IPlayerDataRepository
    {
        // Team Management
        /// <summary>
        /// Gets the current player monster team. Creates default team if none exists.
        /// </summary>
        List<MonsterEntity> GetPlayerTeam();
        
        /// <summary>
        /// Updates the player team with the given monsters.
        /// </summary>
        void UpdatePlayerTeam(List<MonsterEntity> monsters);
        
        /// <summary>
        /// Checks if the player has any monsters in their team.
        /// </summary>
        bool HasPlayerTeam();
        
        /// <summary>
        /// Creates a default starting team for new players.
        /// </summary>
        void InitializeDefaultTeam();

        // Resource Management
        /// <summary>
        /// Gets the player's resources entity.
        /// </summary>
        PlayerResources GetPlayerResources();
        
        /// <summary>
        /// Updates the player's resources entity.
        /// </summary>
        void UpdatePlayerResources(PlayerResources resources);

        // Run Statistics
        /// <summary>
        /// Gets the number of battles won in this run.
        /// </summary>
        int GetBattlesWon();
        
        /// <summary>
        /// Sets the number of battles won in this run.
        /// </summary>
        void SetBattlesWon(int battlesWon);

        // Initialization
        /// <summary>
        /// Initializes all player data for a new run.
        /// </summary>
        void InitializeNewRun();
    }
}