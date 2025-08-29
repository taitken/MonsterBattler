using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.Application.Handlers;
using Game.Application.Messaging;
using Game.Core;
using UnityEngine;

namespace Game.Presentation.Adapters
{
    /// <summary>
    /// Routes commands to their respective handlers.
    /// </summary>
    /// <remarks>
    /// This component subscribes to commands and dispatches them to the appropriate handlers.
    /// It uses reflection to discover command handlers at runtime.
    /// </remarks>
    public sealed class CommandRouter : MonoBehaviour, IDisposable
    {
        private IEventBus _bus;
        private readonly Dictionary<Type, List<Type>> _handlerTypesByCommand = new();
        private readonly List<IDisposable> _subscriptions = new();

        void Awake()
        {
            _bus = ServiceLocator.Get<IEventBus>();
            BuildHandlerMap();
            SubscribePerCommandType();
            DontDestroyOnLoad(gameObject);
        }

        private void BuildHandlerMap()
        {
            _handlerTypesByCommand.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
            foreach (var asm in assemblies)
            {
                foreach (var type in SafeGetTypes(asm))
                {
                    if (type == null || type.IsAbstract || type.IsInterface) continue;

                    foreach (var ifc in type.GetInterfaces())
                    {
                        if (!ifc.IsGenericType) continue;
                        if (ifc.GetGenericTypeDefinition() != typeof(ICommandHandler<>)) continue;

                        var commandType = ifc.GetGenericArguments()[0];
                        if (!_handlerTypesByCommand.TryGetValue(commandType, out var list))
                            _handlerTypesByCommand[commandType] = list = new List<Type>(2);

                        list.Add(type);
                    }
                }
            }
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }

        private void SubscribePerCommandType()
        {
            foreach (var cmdType in _handlerTypesByCommand.Keys)
            {
                var subscribeMethod = typeof(IEventBus).GetMethods()
                    .First(m => m.Name == "Subscribe" && m.IsGenericMethod && m.GetGenericArguments().Length == 1);
                var genericSubscribe = subscribeMethod.MakeGenericMethod(cmdType);
                var actionType = typeof(Action<>).MakeGenericType(cmdType);
                var action = Delegate.CreateDelegate(
                    actionType,
                    this,
                    GetType().GetMethod(nameof(Dispatch), BindingFlags.NonPublic | BindingFlags.Instance)!
                        .MakeGenericMethod(cmdType)
                );

                var disposable = (IDisposable)genericSubscribe.Invoke(_bus, new object[] { action, null });
                Debug.Log($"Subscribed to command type: {disposable.GetType().Name}");
                _subscriptions.Add(disposable);
            }
        }

        // Called per command type; resolves all handler instances using ServiceLocator and invokes Handle
        private void Dispatch<T>(T command) where T : ICommand
        {
            Debug.Log("Dispatching command: " + typeof(T).Name);
            if (!_handlerTypesByCommand.TryGetValue(typeof(T), out var handlerTypes)) return;

            for (int i = 0; i < handlerTypes.Count; i++)
            {
                try
                {
                    var handler = CreateHandlerInstance(handlerTypes[i]);
                    ((ICommandHandler<T>)handler).Handle(command);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private object CreateHandlerInstance(Type handlerType)
        {
            var constructors = handlerType.GetConstructors();
            var constructor = constructors[0]; // Take the first (and likely only) constructor
            
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                args[i] = typeof(ServiceLocator)
                    .GetMethod("Get")
                    .MakeGenericMethod(paramType)
                    .Invoke(null, null);
            }
            
            return constructor.Invoke(args);
        }

        public void Dispose()
        {
            Debug.Log("Disposing CommandRouter and unsubscribing from all commands.");
            for (int i = 0; i < _subscriptions.Count; i++)
                _subscriptions[i]?.Dispose();
            _subscriptions.Clear();
        }

        void OnDestroy() => Dispose();
    }
}