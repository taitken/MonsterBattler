using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Domain.Entities.Battle
{
    public readonly struct RuneFace
    {
        private readonly List<RuneType> _runes;
        
        public RuneFace(List<RuneType> runes)
        {
            _runes = runes?.ToList() ?? new List<RuneType>();
        }
        
        public RuneFace(params RuneType[] runes)
        {
            _runes = runes?.ToList() ?? new List<RuneType>();
        }
        
        public int RuneCount => _runes?.Count ?? 0;
        public bool IsEmpty => RuneCount == 0;
        public IReadOnlyList<RuneType> Runes => _runes?.AsReadOnly() ?? new List<RuneType>().AsReadOnly();
        
        public List<RuneType> GetRunesForDisplay()
        {
            return IsEmpty ? new List<RuneType> { RuneType.Plain } : new List<RuneType>(_runes);
        }
    }

    public class RuneSlotMachineEntity : BaseEntity
    {
        public class Tumbler
        {
            public List<RuneFace> AvailableFaces { get; private set; }
            public RuneFace CurrentFace { get; private set; }
            public int CurrentIndex { get; private set; }

            public Tumbler(List<RuneFace> availableFaces)
            {
                AvailableFaces = availableFaces ?? new List<RuneFace>();
                CurrentIndex = 0;
                CurrentFace = AvailableFaces.Count > 0 ? AvailableFaces[0] : new RuneFace();
            }

            public void SetToIndex(int index)
            {
                if (AvailableFaces.Count == 0) return;
                
                CurrentIndex = Math.Max(0, Math.Min(index, AvailableFaces.Count - 1));
                CurrentFace = AvailableFaces[CurrentIndex];
            }

            public RuneFace GetFaceAtIndex(int index)
            {
                if (AvailableFaces.Count == 0) return new RuneFace();
                return AvailableFaces[Math.Max(0, Math.Min(index, AvailableFaces.Count - 1))];
            }
        }

        public Tumbler FirstTumbler { get; private set; }
        public Tumbler SecondTumbler { get; private set; }
        public Tumbler ThirdTumbler { get; private set; }

        public event Action<int[]> OnSpinRequested;
        public event Action<RuneFace[]> OnRunesChanged;

        public RuneSlotMachineEntity(List<MonsterEntity> playerMonsters)
        {
            if (playerMonsters == null || playerMonsters.Count == 0)
                throw new ArgumentException("Player monsters cannot be null or empty", nameof(playerMonsters));

            // Initialize tumblers with monster-specific rune faces
            var tumblerFaces = ExtractRuneFacesFromMonsters(playerMonsters);
            
            FirstTumbler = new Tumbler(tumblerFaces.Length > 0 ? tumblerFaces[0] : new List<RuneFace>());
            SecondTumbler = new Tumbler(tumblerFaces.Length > 1 ? tumblerFaces[1] : new List<RuneFace>());
            ThirdTumbler = new Tumbler(tumblerFaces.Length > 2 ? tumblerFaces[2] : new List<RuneFace>());
        }

        private List<RuneFace>[] ExtractRuneFacesFromMonsters(List<MonsterEntity> playerMonsters)
        {
            var tumblerFaces = new List<RuneFace>[3];
            
            for (int i = 0; i < Math.Min(3, playerMonsters.Count); i++)
            {
                var monster = playerMonsters[i];
                var faces = new List<RuneFace>();
                
                // Create faces from the monster's cards (up to 5 cards)
                if (monster.AbilityDeck != null && monster.AbilityDeck.AllCards != null)
                {
                    var cardFaces = monster.AbilityDeck.AllCards
                        .Take(5) // Take up to 5 cards
                        .Select(card => new RuneFace(card.Runes ?? new List<RuneType>()))
                        .ToList();
                    
                    faces.AddRange(cardFaces);
                }
                
                // Add a face for the monster's own runes
                if (monster.Runes != null && monster.Runes.Count > 0)
                {
                    faces.Add(new RuneFace(monster.Runes));
                }
                
                // Ensure we have exactly 6 faces (pad with empty faces if needed, truncate if too many)
                while (faces.Count < 6)
                {
                    faces.Add(new RuneFace());
                }
                if (faces.Count > 6)
                {
                    faces = faces.Take(6).ToList();
                }
                
                tumblerFaces[i] = faces;
            }
            
            // Fill remaining tumblers with empty faces if less than 3 monsters
            for (int i = playerMonsters.Count; i < 3; i++)
            {
                tumblerFaces[i] = Enumerable.Repeat(new RuneFace(), 6).ToList();
            }
            
            return tumblerFaces;
        }

        public void Spin(int[] targetIndices)
        {
            if (targetIndices == null || targetIndices.Length != 3)
                throw new ArgumentException("Target indices must be an array of 3 integers", nameof(targetIndices));

            // Set tumblers to target positions
            FirstTumbler.SetToIndex(targetIndices[0]);
            SecondTumbler.SetToIndex(targetIndices[1]);
            ThirdTumbler.SetToIndex(targetIndices[2]);

            // Notify about the spin request
            OnSpinRequested?.Invoke(targetIndices);

            // Notify about rune changes
            var currentFaces = new[] 
            { 
                FirstTumbler.CurrentFace, 
                SecondTumbler.CurrentFace, 
                ThirdTumbler.CurrentFace 
            };
            OnRunesChanged?.Invoke(currentFaces);
        }

        public RuneFace[] GetCurrentFaces()
        {
            return new[]
            {
                FirstTumbler.CurrentFace,
                SecondTumbler.CurrentFace,
                ThirdTumbler.CurrentFace
            };
        }

        public List<RuneFace>[] GetAllTumblerFaces()
        {
            return new[]
            {
                FirstTumbler.AvailableFaces,
                SecondTumbler.AvailableFaces,
                ThirdTumbler.AvailableFaces
            };
        }
    }
}