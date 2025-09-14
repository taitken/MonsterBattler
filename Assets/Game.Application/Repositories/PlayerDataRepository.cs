using System.Collections.Generic;
using Game.Application.IFactories;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Entities.Abilities;
using Game.Domain.Entities.Player;

namespace Game.Application.Repositories
{
    /// <summary>
    /// Repository for managing persistent player data during a run.
    /// Pure data storage - no business logic.
    /// </summary>
    public class PlayerDataRepository : IPlayerDataRepository
    {
        private readonly IMonsterEntityFactory _monsterFactory;
        private readonly ILoggerService _logger;
        private List<MonsterEntity> _playerTeam;
        private PlayerResources _playerResources;
        private BackpackEntity _backpack;
        private int _battlesWon;
        private bool _teamInitialized;

        public PlayerDataRepository(IMonsterEntityFactory monsterFactory, ILoggerService logger)
        {
            _monsterFactory = monsterFactory;
            _logger = logger;
            
            InitializeNewRun();
        }

        public void InitializeNewRun()
        {
            _playerTeam = new List<MonsterEntity>();
            _playerResources = new PlayerResources();
            _backpack = new BackpackEntity();
            _battlesWon = 0;
            _teamInitialized = false;
            
            InitializeDefaultTeam();
            
            _logger?.Log("Initialized new player run");
        }

        // Team Management (copied from existing PlayerTeamRepository)
        public List<MonsterEntity> GetPlayerTeam()
        {
            if (_playerTeam.Count == 0 && !_teamInitialized)
            {
                _logger?.Log("No player team found, initializing default team");
                InitializeDefaultTeam();
            }

            // Return deep copies of the monsters to avoid reference issues
            var teamCopies = new List<MonsterEntity>();
            foreach (var monster in _playerTeam)
            {
                teamCopies.Add(new MonsterEntity(monster));
            }

            _logger?.Log($"Retrieved player team with {teamCopies.Count} monsters");
            return teamCopies;
        }

        public void UpdatePlayerTeam(List<MonsterEntity> monsters)
        {
            if (monsters == null)
            {
                _logger?.LogError("Cannot update player team with null monster list");
                return;
            }

            // Clear and copy the monsters to avoid reference issues
            _playerTeam.Clear();
            _playerTeam.AddRange(monsters);
            _teamInitialized = true;

            _logger?.Log($"Updated player team with {_playerTeam.Count} monsters");
        }

        public bool HasPlayerTeam()
        {
            return _playerTeam.Count > 0;
        }

        public void InitializeDefaultTeam()
        {
            _playerTeam.Clear();
            
            // Create default starting team (copied from existing logic)
            var ashwick = _monsterFactory.Create(MonsterType.Ashwick, BattleTeam.Player);
            var dropletus = _monsterFactory.Create(MonsterType.Dropletus, BattleTeam.Player);
            var shardilo = _monsterFactory.Create(MonsterType.Shardilo, BattleTeam.Player);
            
            _playerTeam.Add(ashwick);
            _playerTeam.Add(dropletus);
            _playerTeam.Add(shardilo);

            _teamInitialized = true;
            _logger?.Log("Initialized default player team: Ashwick, Dropletus, Shardilo");
        }

        // Resource Management
        public PlayerResources GetPlayerResources()
        {
            return _playerResources;
        }

        public void UpdatePlayerResources(PlayerResources resources)
        {
            if (resources == null)
            {
                _logger?.LogError("Cannot update player resources with null resources");
                return;
            }

            _playerResources = resources;
            _logger?.Log("Updated player resources");
        }

        // Backpack Management
        public BackpackEntity GetBackpack()
        {
            return _backpack;
        }

        public void UpdateBackpack(BackpackEntity backpack)
        {
            if (backpack == null)
            {
                _logger?.LogError("Cannot update backpack with null backpack");
                return;
            }

            _backpack = backpack;
            _logger?.Log($"Updated backpack with {_backpack.Count} cards");
        }

        // Run Statistics
        public int GetBattlesWon()
        {
            return _battlesWon;
        }

        public void SetBattlesWon(int battlesWon)
        {
            _battlesWon = battlesWon;
            _logger?.Log($"Set battles won to {battlesWon}");
        }
    }
}