using Game.Application.Messaging;
using Game.Core;

namespace Game.Applcation.HelperClasses
{
    public static class BusDefaults
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<System.Type, Topic> TopicCache = new();

        public static Topic ResolveTopic(System.Type t) =>
            TopicCache.GetOrAdd(t, type =>
            {
                if (typeof(ICommand).IsAssignableFrom(type)) return Topic.UI;
                if (typeof(IVisualEvent).IsAssignableFrom(type)) return Topic.Visual;
                return Topic.Domain; // default
            });

        public static Dispatch ResolveDefaultDispatch(System.Type t)
        {
            // sensible defaults: UI = Immediate, Domain = NextFrame, Visual = Queued
            if (typeof(ICommand).IsAssignableFrom(t)) return Dispatch.Immediate;
            if (typeof(IVisualEvent).IsAssignableFrom(t)) return Dispatch.Queued;
            return Dispatch.NextFrame;
        }

        public static int ResolveDefaultPriority(System.Type t) => 0;
    }
}
