namespace Game.Core
{
    public static class ServiceLocator
    {
        private static IServiceContainer _container;
        public static void Set(IServiceContainer container) => _container = container;

        public static T Get<T>() => _container.Resolve<T>();
    }
}