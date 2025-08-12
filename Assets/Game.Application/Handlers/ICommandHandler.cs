using Game.Application.Messaging;

namespace Game.Application.Handlers
{
    /// <summary>
    /// Interface for handling commands in the application.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle.</typeparam>
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }
}