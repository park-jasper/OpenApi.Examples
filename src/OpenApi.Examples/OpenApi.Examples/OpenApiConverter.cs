using Microsoft.OpenApi.Any;
using PolyType.Abstractions;
using PolyType;
using System.Collections.Concurrent;

namespace OpenApi.Examples;

public static class OpenApiConverter
{
    private static readonly ConcurrentDictionary<Type, object> FuncDictionary = new();

    public static IOpenApiAny Parse<T>(T value)
        where T : IShapeable<T>
    {
        return Parse(value, T.GetShape());
    }

    public static IOpenApiAny Parse<T>(T value, ITypeShape<T> shape)
    {
        var visitor = new OpenApiTypeShapeVisitor();
        var func = (Func<T, IOpenApiAny>)FuncDictionary.GetOrAdd(
            key: typeof(T),
            valueFactory: _ => shape.Accept(visitor)!);
        return func(value);
    }
}