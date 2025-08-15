using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Application.DTOs;
using Game.Domain.Entities.Overworld;

namespace Game.Application.Interfaces
{
    public interface IOverworldService
    {
        OverworldEntity InitializeOverworld(OverworldPayload payload);
        void LoadOverworld(OverworldPayload payload);
    }
}