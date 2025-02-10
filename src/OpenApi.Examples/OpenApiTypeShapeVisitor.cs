using Microsoft.OpenApi.Any;
using PolyType.Abstractions;

namespace OpenApi.Examples;

public class OpenApiTypeShapeVisitor : TypeShapeVisitor
{
    public override object? VisitObject<T>(IObjectTypeShape<T> objectShape, object? state = null)
    {
        var propertyGetters = objectShape.Properties
            .Where(p => p.HasGetter)
            .Select(p => (p.Name, (Func<T, IOpenApiAny>)p.Accept(this)!))
            .ToArray();

        if (objectShape.Type == typeof(string))
        {
            return new Func<string?, IOpenApiAny>(value => value is null ? new OpenApiNull() : new OpenApiString(value));
        }

        if (objectShape.Type == typeof(DateTimeOffset))
        {
            return new Func<DateTimeOffset, IOpenApiAny>(dto => new OpenApiDateTime(dto));
        }

        if (objectShape.Type == typeof(DateTime))
        {
            return new Func<DateTime, IOpenApiAny>(dt => new OpenApiDate(dt));
        }

        if (objectShape.Type == typeof(bool))
        {
            return new Func<bool, IOpenApiAny>(b => new OpenApiBoolean(b));
        }

        if (objectShape.Type == typeof(int))
        {
            return new Func<int, IOpenApiAny>(i => new OpenApiInteger(i));
        }

        if (objectShape.Type == typeof(long))
        {
            return new Func<long, IOpenApiAny>(l => new OpenApiLong(l));
        }

        if (objectShape.Type == typeof(float))
        {
            return new Func<float, IOpenApiAny>(f => new OpenApiFloat(f));
        }

        if (objectShape.Type == typeof(double))
        {
            return new Func<double, IOpenApiAny>(d => new OpenApiDouble(d));
        }

        if (objectShape.Type == typeof(decimal))
        {
            return new Func<decimal, IOpenApiAny>(d => new OpenApiDouble((double)d));
        }

        if (objectShape.Type == typeof(byte))
        {
            return new Func<byte, IOpenApiAny>(b => new OpenApiByte(b));
        }

        return new Func<T, IOpenApiAny>(
            value =>
            {
                if (value is null)
                {
                    return new OpenApiNull();
                }

                var obj = new OpenApiObject();
                foreach (var propertyGetter in propertyGetters)
                {
                    obj[propertyGetter.Name] = propertyGetter.Item2(value);
                }

                return obj;
            });
    }

    public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
    {
        var getter = propertyShape.GetGetter();
        var func = (Func<TPropertyType, IOpenApiAny>)propertyShape.PropertyType.Accept(this)!;
        return new Func<TDeclaringType, IOpenApiAny>(obj => func(getter(ref obj)));
    }

    public override object? VisitEnumerable<TEnumerable, TElement>(IEnumerableTypeShape<TEnumerable, TElement> enumerableShape, object? state = null)
    {
        var func = (Func<TElement, IOpenApiAny>)enumerableShape.ElementType.Accept(this)!;
        var enumerate = enumerableShape.GetGetEnumerable();
        if (typeof(TElement) == typeof(byte))
        {
            return new Func<TEnumerable, IOpenApiAny>(
                bytes =>
                {
                    if (bytes is null)
                    {
                        return new OpenApiNull();
                    }

                    var result = new OpenApiBinary(enumerate(bytes).Cast<byte>().ToArray());
                    return result;
                });
        }
        return new Func<TEnumerable, IOpenApiAny>(
            enumerable =>
            {
                if (enumerable is null)
                {
                    return new OpenApiNull();
                }

                var result = new OpenApiArray();
                foreach (var element in enumerate(enumerable))
                {
                    result.Add(func(element));
                }

                return result;
            });
    }

    public override object? VisitDictionary<TDictionary, TKey, TValue>(IDictionaryTypeShape<TDictionary, TKey, TValue> dictionaryShape, object? state = null)
    {
        return new Func<TDictionary, IOpenApiAny>(d => new OpenApiNull());
    }

    public override object? VisitEnum<TEnum, TUnderlying>(IEnumTypeShape<TEnum, TUnderlying> enumShape, object? state = null)
    {
        if (typeof(TUnderlying) == typeof(int))
        {
            return new Func<TEnum, IOpenApiAny>(e => new OpenApiInteger((int)(object)e));
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(TUnderlying), typeof(TUnderlying).Name);
        }
    }

    public override object? VisitNullable<T>(INullableTypeShape<T> nullableShape, object? state = null)
    {
        var func = (Func<T, IOpenApiAny>)nullableShape.ElementType.Accept(this)!;
        return new Func<T?, IOpenApiAny>(nullable => nullable is null ? new OpenApiNull() : func(nullable.Value));
    }
}