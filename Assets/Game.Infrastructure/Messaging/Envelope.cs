using Game.Core;
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif

namespace Game.Infrastructure.Messaging
{
    internal sealed record Envelope(
        object Payload,
        Topic Topic,
        Dispatch Dispatch,
        int Priority,
        string CorrelationId,
        System.DateTime Timestamp);
}