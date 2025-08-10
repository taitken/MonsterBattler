using Game.Core;

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif

namespace Game.Application.Messaging
{
    public sealed record PublishOptions(
        Topic? Topic = null,
        Dispatch? Dispatch = null,
        int? Priority = null,
        string CorrelationId = null);
}