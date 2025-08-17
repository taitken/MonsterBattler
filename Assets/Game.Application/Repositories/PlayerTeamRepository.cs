using System.Collections.Generic;
using System.Linq;
using Game.Application.IFactories;
using Game.Application.Interfaces;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Enums;

namespace Game.Application.Repositories
{
    public class PlayerTeamRepository : IPlayerTeamRepository
    {
        private readonly IMonsterEntityFactory _monsterFactory;
        private readonly ILoggerService _log;
        private List<MonsterEntity> _playerTeam;

        public PlayerTeamRepository(IMonsterEntityFactory monsterFactory, ILoggerService log)
        {
            _monsterFactory = monsterFactory;
            _log = log;
            _playerTeam = new List<MonsterEntity>();
        }

        public List<MonsterEntity> GetPlayerTeam()
        {
            if (_playerTeam.Count == 0)
            {
                _log?.Log("No player team found, initializing default team");
                InitializeDefaultTeam();
            }

            // Return copies of the monsters to avoid reference issues
            var teamCopies = new List<MonsterEntity>();
            foreach (var monster in _playerTeam)
            {
                var copy = _monsterFactory.Create(monster.Type);
                // Preserve the persistent state (current HP, etc.)
                copy.SetCurrentHP(monster.CurrentHP);
                teamCopies.Add(copy);
            }

            _log?.Log($"Retrieved player team with {teamCopies.Count} monsters");
            return teamCopies;
        }

        public void UpdatePlayerTeam(List<MonsterEntity> monsters)
        {
            if (monsters == null)
            {
                _log?.LogError("Cannot update player team with null monster list");
                return;
            }

            _playerTeam.Clear();
            
            // Store copies to avoid reference issues
            foreach (var monster in monsters)
            {
                var copy = _monsterFactory.Create(monster.Type);
                copy.SetCurrentHP(monster.CurrentHP);
                _playerTeam.Add(copy);
            }

            _log?.Log($"Updated player team with {_playerTeam.Count} monsters");
        }

        public void HealPlayerTeam()
        {
            foreach (var monster in _playerTeam)
            {
                monster.RestoreToFullHealth();
            }

            _log?.Log("Player team healed to full health");
        }

        public bool HasPlayerTeam()
        {
            return _playerTeam.Count > 0;
        }

        public void InitializeDefaultTeam()
        {
            _playerTeam.Clear();
            
            // Create default starting team
            var goald = _monsterFactory.Create(MonsterType.Ashwick);
            var kraggan = _monsterFactory.Create(MonsterType.Dropletus);
            var flimboon = _monsterFactory.Create(MonsterType.Shardilo);
            
            _playerTeam.Add(goald);
            _playerTeam.Add(kraggan);
            _playerTeam.Add(flimboon);

            _log?.Log("Initialized default player team: Goald, Kraggan, Flimboon");
        }
    }
}