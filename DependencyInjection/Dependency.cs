using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace DependencyInjection {
    public class Dependency {
        static readonly ConcurrentDictionary<Type, Type> Types = new ConcurrentDictionary<Type, Type> ( );
        static readonly ConcurrentDictionary<Type, object> TypeInstances = new ConcurrentDictionary<Type, object> ( );

        static Dependency container;
        public static Dependency Container => container ?? ( container = new Dependency ( ) );

        public bool Register<TContract, TImplementation> ( ) where TImplementation : TContract => Types.TryAdd ( typeof ( TContract ), typeof ( TImplementation ) );
        public bool Register<TContract, TImplementation> ( TImplementation instance ) where TImplementation : TContract => TypeInstances.TryAdd ( typeof ( TContract ), instance );
        public T Retrieve<T> ( ) => ( T ) Retrieve ( typeof ( T ) );
        public bool Release<TContract> ( ) => Types.TryRemove ( typeof ( TContract ), out Type _ ) &&
                                              TypeInstances.TryRemove ( typeof ( TContract ), out object _ );

        object Retrieve ( Type contract ) {
            if ( TypeInstances.ContainsKey ( contract ) ) {
                object obj = TypeInstances [ contract ];
                return obj;
            }

            Type implementation = Types [ contract ];
            ConstructorInfo constructor = implementation.GetConstructors ( ) [ 0 ];
            ParameterInfo [ ] constructorParameters = constructor.GetParameters ( );
            if ( constructorParameters.Length == 0 ) {
                return Activator.CreateInstance ( implementation );
            }
            object [ ] parameters = constructorParameters
                .Select ( param => Retrieve ( param.ParameterType ) )
                .ToArray ( );
            return constructor.Invoke ( parameters );
        }
    }
}
