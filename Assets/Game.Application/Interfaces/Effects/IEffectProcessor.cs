using Game.Application.DTOs.Effects;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Interfaces.Effects
{
    public interface IEffectProcessor
    {
        /// <summary>
        /// Processes damage modification effects when a monster takes damage
        /// </summary>
        int ProcessDamageTaken(MonsterEntity target, int damage, MonsterEntity source);

        /// <summary>
        /// Processes effects that trigger when a monster attempts to play a card
        /// </summary>
        bool ProcessCardPlayed(MonsterEntity player, AbilityCard card);

        /// <summary>
        /// Processes effects that trigger at the end of a monster's turn
        /// </summary>
        void ProcessTurnEnd(MonsterEntity target);

        /// <summary>
        /// Processes effects that trigger when a new status effect is applied
        /// </summary>
        void ProcessEffectApplied(MonsterEntity target, StatusEffect newEffect);

        /// <summary>
        /// Processes effects that modify outgoing damage from the caster
        /// </summary>
        int ProcessOutgoingDamage(MonsterEntity caster, MonsterEntity target, int baseDamage);
    }
}