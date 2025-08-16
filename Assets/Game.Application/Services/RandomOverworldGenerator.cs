using System;
using System.Collections.Generic;
using System.Linq;
using Game.Application.Interfaces;
using Game.Core.Logger;
using Game.Core.Randomness;
using Game.Domain.Entities.Overworld;
using Game.Domain.Enums;

namespace Game.Application.Services
{
    public class RandomOverworldGenerator : IOverworldGenerator
    {
        private readonly IRandomService _random;
        private readonly ILoggerService _log;
        private const int MIN_ROOMS = 15;
        private const int MAX_ROOMS = 25;
        private const int GRID_SIZE = 20; // Large enough grid to accommodate rooms
        private const int CENTER = GRID_SIZE / 2;

        public RandomOverworldGenerator(IRandomService random, ILoggerService log)
        {
            _random = random;
            _log = log;
        }

        public OverworldEntity GenerateOverworld()
        {
            var overworld = new OverworldEntity();
            var roomCount = _random.Range(MIN_ROOMS, MAX_ROOMS + 1);
            var grid = new RoomEntity[GRID_SIZE, GRID_SIZE];
            
            _log?.Log($"Generating overworld with {roomCount} rooms");

            // Create starting room at center
            var startingRoom = new RoomEntity(CENTER, CENTER, true);
            grid[CENTER, CENTER] = startingRoom;
            overworld.AddRoom(startingRoom);

            var createdRooms = 1;
            
            // Generate main branches from center
            var mainBranches = _random.Range(2, 5); // 2-4 main branches
            var usedDirections = new List<Direction>();
            
            for (int branch = 0; branch < mainBranches && createdRooms < roomCount; branch++)
            {
                var branchDirection = GetRandomUnusedDirection(usedDirections);
                if (branchDirection == null) break;
                
                usedDirections.Add(branchDirection.Value);
                
                // Create a main hallway/branch
                var branchRooms = GenerateBranch(startingRoom, branchDirection.Value, grid, roomCount - createdRooms);
                
                foreach (var room in branchRooms)
                {
                    overworld.AddRoom(room);
                    createdRooms++;
                    
                    // Randomly add side rooms/corridors off the main branch
                    if (_random.Range(0, 100) < 40) // 40% chance for side rooms
                    {
                        var sideRooms = GenerateSideRooms(room, grid, Math.Min(3, roomCount - createdRooms));
                        foreach (var sideRoom in sideRooms)
                        {
                            overworld.AddRoom(sideRoom);
                            createdRooms++;
                            if (createdRooms >= roomCount) break;
                        }
                    }
                    
                    if (createdRooms >= roomCount) break;
                }
            }

            _log?.Log($"Generated overworld with {createdRooms} rooms");
            return overworld;
        }

        private Direction? GetRandomUnusedDirection(List<Direction> usedDirections)
        {
            var allDirections = new List<Direction> { Direction.North, Direction.South, Direction.East, Direction.West };
            var availableDirections = allDirections.Where(d => !usedDirections.Contains(d)).ToList();
            
            if (availableDirections.Count == 0) return null;
            
            return availableDirections[_random.Range(0, availableDirections.Count)];
        }

        private List<RoomEntity> GenerateBranch(RoomEntity startRoom, Direction direction, RoomEntity[,] grid, int maxRooms)
        {
            var branchRooms = new List<RoomEntity>();
            var branchLength = _random.Range(3, 8); // 3-7 rooms in a branch
            branchLength = Math.Min(branchLength, maxRooms);
            
            var currentRoom = startRoom;
            
            for (int i = 0; i < branchLength; i++)
            {
                // Occasionally curve the branch
                if (i > 1 && _random.Range(0, 100) < 25) // 25% chance to curve
                {
                    var perpendicularDirections = GetPerpendicularDirections(direction);
                    if (perpendicularDirections.Count > 0)
                    {
                        direction = perpendicularDirections[_random.Range(0, perpendicularDirections.Count)];
                    }
                }
                
                var newRoom = CreateConnectedRoom(currentRoom, direction, grid);
                if (newRoom == null) break; // Hit boundary or existing room
                
                branchRooms.Add(newRoom);
                currentRoom = newRoom;
            }
            
            return branchRooms;
        }

        private List<RoomEntity> GenerateSideRooms(RoomEntity parentRoom, RoomEntity[,] grid, int maxSideRooms)
        {
            var sideRooms = new List<RoomEntity>();
            var availableDirections = GetAvailableDirections(parentRoom, grid);
            
            // Remove directions that would create too linear paths
            availableDirections = availableDirections.Where(d => !parentRoom.HasConnection(d)).ToList();
            
            var sideRoomCount = Math.Min(_random.Range(1, 3), Math.Min(maxSideRooms, availableDirections.Count));
            
            for (int i = 0; i < sideRoomCount; i++)
            {
                if (availableDirections.Count == 0) break;
                
                var direction = availableDirections[_random.Range(0, availableDirections.Count)];
                availableDirections.Remove(direction);
                
                // Create a short side corridor (1-2 rooms)
                var sideLength = _random.Range(1, 3);
                var currentRoom = parentRoom;
                
                for (int j = 0; j < sideLength; j++)
                {
                    var sideRoom = CreateConnectedRoom(currentRoom, direction, grid);
                    if (sideRoom == null) break;
                    
                    sideRooms.Add(sideRoom);
                    currentRoom = sideRoom;
                    
                    // Sometimes continue in same direction, sometimes branch off
                    if (j > 0 && _random.Range(0, 100) < 60) // 60% chance to change direction
                    {
                        var newDirections = GetAvailableDirections(currentRoom, grid);
                        newDirections.Remove(GetOppositeDirection(direction)); // Don't go backwards
                        
                        if (newDirections.Count > 0)
                        {
                            direction = newDirections[_random.Range(0, newDirections.Count)];
                        }
                    }
                }
            }
            
            return sideRooms;
        }

        private List<Direction> GetAvailableDirections(RoomEntity room, RoomEntity[,] grid)
        {
            var available = new List<Direction>();
            
            // Check each direction for available space
            if (room.Y > 0 && grid[room.X, room.Y - 1] == null)
                available.Add(Direction.North);
            
            if (room.Y < GRID_SIZE - 1 && grid[room.X, room.Y + 1] == null)
                available.Add(Direction.South);
            
            if (room.X < GRID_SIZE - 1 && grid[room.X + 1, room.Y] == null)
                available.Add(Direction.East);
            
            if (room.X > 0 && grid[room.X - 1, room.Y] == null)
                available.Add(Direction.West);

            return available;
        }

        private List<Direction> GetPerpendicularDirections(Direction direction)
        {
            return direction switch
            {
                Direction.North or Direction.South => new List<Direction> { Direction.East, Direction.West },
                Direction.East or Direction.West => new List<Direction> { Direction.North, Direction.South },
                _ => new List<Direction>()
            };
        }

        private RoomEntity CreateConnectedRoom(RoomEntity parentRoom, Direction direction, RoomEntity[,] grid)
        {
            var (newX, newY) = GetNewPosition(parentRoom.X, parentRoom.Y, direction);
            
            if (newX < 0 || newX >= GRID_SIZE || newY < 0 || newY >= GRID_SIZE)
                return null;

            if (grid[newX, newY] != null)
                return null;

            var newRoom = new RoomEntity(newX, newY);

            // Set bidirectional connections
            parentRoom.SetConnection(direction, newRoom.Id);
            newRoom.SetConnection(GetOppositeDirection(direction), parentRoom.Id);

            grid[newX, newY] = newRoom;
            return newRoom;
        }

        private (int x, int y) GetNewPosition(int currentX, int currentY, Direction direction)
        {
            return direction switch
            {
                Direction.North => (currentX, currentY - 1),
                Direction.South => (currentX, currentY + 1),
                Direction.East => (currentX + 1, currentY),
                Direction.West => (currentX - 1, currentY),
                _ => (currentX, currentY)
            };
        }

        private Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.North => Direction.South,
                Direction.South => Direction.North,
                Direction.East => Direction.West,
                Direction.West => Direction.East,
                _ => direction
            };
        }

    }
}