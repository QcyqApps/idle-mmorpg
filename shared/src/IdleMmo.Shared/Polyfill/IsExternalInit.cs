// Polyfill required so C# 9+ `record` and `init` setters compile on netstandard2.1.
// Internal so it does not leak into Unity's reference assemblies.
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit
{
}
