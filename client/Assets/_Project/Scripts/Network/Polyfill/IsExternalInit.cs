// Polyfill so C# `record` / `init` setters compile under Unity Mono backend
// where the BCL still ships netstandard2.1 reference assemblies.
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit
{
}
