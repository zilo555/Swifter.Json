﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.RW
{
    internal sealed class GenericCollectionInterfaceMapper : IValueInterfaceMaper
    {
        public IValueInterface<T>? TryMap<T>()
        {
            if (typeof(T).IsArray)
            {
                return null;
            }

            var interfaceMap = typeof(T)
                .GetInterfaces()
                .Where(x => x.IsGenericType)
                .GroupBy(x => x.GetGenericTypeDefinition(), x => x.GetGenericArguments())
                .ToDictionary(x => x.Key, x => x.First());

            if (typeof(T).IsInterface && typeof(T).IsGenericType)
            {
                interfaceMap.Add(typeof(T).GetGenericTypeDefinition(), typeof(T).GetGenericArguments());
            }

            Type[]? arguments;

            var valueInterfaceType
                = interfaceMap.TryGetValue(typeof(IDictionary<,>), out arguments) ? typeof(DictionaryInterface<,,>).MakeGenericType(typeof(T), arguments[0], arguments[1])
                : interfaceMap.TryGetValue(typeof(IList<>), out arguments) ? typeof(ListInterface<,>).MakeGenericType(typeof(T), arguments[0])
                : interfaceMap.TryGetValue(typeof(ICollection<>), out arguments) ? typeof(CollectionInterface<,>).MakeGenericType(typeof(T), arguments[0])
                : interfaceMap.TryGetValue(typeof(IEnumerable<>), out arguments) ? typeof(EnumerableInterface<,>).MakeGenericType(typeof(T), arguments[0])
                : interfaceMap.TryGetValue(typeof(IEnumerator<>), out arguments) ? typeof(EnumeratorInterface<,>).MakeGenericType(typeof(T), arguments[0])
                : null;

            if (valueInterfaceType is not null)
            {
                return (IValueInterface<T>)Activator.CreateInstance(valueInterfaceType)!;
            }

            return null;
        }
    }
}