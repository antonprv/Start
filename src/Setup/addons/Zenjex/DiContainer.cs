// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zenjex
{
	/// <summary>
	/// Core dependency injection container for Godot.
	/// 
	/// Manages service registration and resolution with support for:
	/// - Singleton and Transient lifetimes
	/// - Keyed bindings
	/// - Interface/Implementation separation
	/// - Factory methods
	/// - Lazy initialization
	/// 
	/// Usage:
	///   var container = new DiContainer();
	///   container.Register&lt;IService&gt;().To&lt;ServiceImpl&gt;().AsSingleton();
	///   var service = container.Resolve&lt;IService&gt;();
	/// </summary>
	public partial class DiContainer : Node
	{
		/// <summary>Global singleton instance of the container.</summary>
		public static DiContainer Instance { get; private set; }

		private static readonly Dictionary<Type, ServiceDescriptor> _services = new();
		private static readonly Dictionary<(string, Type), ServiceDescriptor> _keyedServices = new();
		private static readonly Dictionary<Type, object> _singletons = new();
		private static readonly Dictionary<(string, Type), object> _keyedSingletons = new();

		public override void _EnterTree()
		{
			if ( Instance != null )
			{
				GD.PrintErr( "DiContainer instance already exists!" );
				QueueFree();
				return;
			}

			Instance = this;
		}

		public override void _ExitTree()
		{
			_services.Clear();
			_keyedServices.Clear();
			_singletons.Clear();
			_keyedSingletons.Clear();
		}

		#region Internal Registration (called by BindingBuilder)

		/// <summary>Register an instance directly as a singleton.</summary>
		public void RegisterInstance<T>( T instance ) where T : class
		{
			var type = typeof( T );
			var descriptor = new ServiceDescriptor
			{
				ServiceType = type,
				ImplementationType = type,
				Lifetime = ServiceLifetime.Singleton,
				Instance = instance
			};
			_services[ type ] = descriptor;
			_singletons[ type ] = instance;
		}

		/// <summary>
		/// Register an instance under an explicit service type (e.g. interface).
		/// Used internally when the service type differs from the instance's concrete type.
		/// </summary>
		public void RegisterInstanceAs( Type serviceType, object instance )
		{
			var descriptor = new ServiceDescriptor
			{
				ServiceType = serviceType,
				ImplementationType = instance.GetType(),
				Lifetime = ServiceLifetime.Singleton,
				Instance = instance
			};
			_services[ serviceType ] = descriptor;
			_singletons[ serviceType ] = instance;
		}

		/// <summary>Keyed variant of RegisterInstanceAs.</summary>
		public void RegisterInstanceAs( Type serviceType, string key, object instance )
		{
			var dictKey = (key, serviceType);
			var descriptor = new ServiceDescriptor
			{
				ServiceType = serviceType,
				ImplementationType = instance.GetType(),
				Lifetime = ServiceLifetime.Singleton,
				Instance = instance,
				Key = key
			};
			_keyedServices[ dictKey ] = descriptor;
			_keyedSingletons[ dictKey ] = instance;
		}

		/// <summary>Register a keyed instance directly as a singleton.</summary>
		public void RegisterInstance<T>( string key, T instance ) where T : class
		{
			var type = typeof( T );
			var dictKey = (key, type);
			var descriptor = new ServiceDescriptor
			{
				ServiceType = type,
				ImplementationType = type,
				Lifetime = ServiceLifetime.Singleton,
				Instance = instance,
				Key = key
			};
			_keyedServices[ dictKey ] = descriptor;
			_keyedSingletons[ dictKey ] = instance;
		}

		#endregion

		#region Internal Registration (called by BindingBuilder)

		internal void RegisterService<TService, TImpl>(
			ServiceLifetime lifetime,
			TImpl instance = null,
			Func<TImpl> factory = null,
			string key = "" )
			where TService : class
			where TImpl : class, TService
		{
			var serviceType = typeof( TService );
			var implType = typeof( TImpl );

			var descriptor = new ServiceDescriptor
			{
				ServiceType = serviceType,
				ImplementationType = implType,
				Lifetime = lifetime,
				Instance = instance,
				Factory = factory as Delegate,
				Key = key
			};

			if ( string.IsNullOrEmpty( key ) )
			{
				_services[ serviceType ] = descriptor;

				if ( lifetime == ServiceLifetime.Singleton && instance != null )
					_singletons[ serviceType ] = instance;
			}
			else
			{
				var dictKey = (key, serviceType);
				_keyedServices[ dictKey ] = descriptor;

				if ( lifetime == ServiceLifetime.Singleton && instance != null )
					_keyedSingletons[ dictKey ] = instance;
			}
		}

		#endregion

		#region Resolution API

		/// <summary>Resolve a service of type T. Returns the singleton if registered as such.</summary>
		public T Resolve<T>( string key = "" ) where T : class
		{
			var type = typeof( T );

			if ( !string.IsNullOrEmpty( key ) )
			{
				return ResolveKeyed<T>( key );
			}

			// Check singleton cache first
			if ( _singletons.TryGetValue( type, out var cached ) )
			{
				return (T)cached;
			}

			// Check descriptor
			if ( _services.TryGetValue( type, out var descriptor ) )
			{
				return (T)ResolveDescriptor( descriptor, type );
			}

			throw new Exception( $"Service of type '{type.Name}' is not registered." );
		}

		/// <summary>Resolve a keyed service of type T.</summary>
		public T ResolveKeyed<T>( string key ) where T : class
		{
			var type = typeof( T );
			var dictKey = (key, type);

			// Check keyed singleton cache first
			if ( _keyedSingletons.TryGetValue( dictKey, out var cached ) )
			{
				return (T)cached;
			}

			// Check descriptor
			if ( _keyedServices.TryGetValue( dictKey, out var descriptor ) )
			{
				return (T)ResolveDescriptor( descriptor, type );
			}

			throw new Exception( $"Service of type '{type.Name}' with key '{key}' is not registered." );
		}

		/// <summary>Try to resolve a service. Returns false if not found.</summary>
		public bool TryResolve<T>( out T service, string key = "" ) where T : class
		{
			try
			{
				service = Resolve<T>( key );
				return true;
			}
			catch
			{
				service = null;
				return false;
			}
		}

		/// <summary>Resolve all instances of type T (useful for composite patterns).</summary>
		public List<T> ResolveAll<T>( string key = "" ) where T : class
		{
			var type = typeof( T );
			var result = new List<T>();

			if ( !string.IsNullOrEmpty( key ) )
			{
				var dictKey = (key, type);
				if ( _keyedServices.TryGetValue( dictKey, out var descriptor ) )
				{
					result.Add( (T)ResolveDescriptor( descriptor, type ) );
				}
			}
			else
			{
				if ( _services.TryGetValue( type, out var descriptor ) )
				{
					result.Add( (T)ResolveDescriptor( descriptor, type ) );
				}
			}

			return result;
		}

		#endregion

		#region Injection

		/// <summary>
		/// Inject all [Inject]-marked fields, properties, and methods on a target object.
		/// Called automatically for nodes that inherit from InjectableBehaviour.
		/// </summary>
		public void Inject( object target )
		{
			if ( target == null )
				return;

			Type targetType = target.GetType();
			List<FieldInfo> fields = new List<FieldInfo>();
			List<PropertyInfo> properties = new List<PropertyInfo>();
			List<MethodInfo> methods = new List<MethodInfo>();

			for ( Type current = targetType; current != null && current != typeof( object ); current = current.BaseType )
			{
				const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

				fields.AddRange( current.GetFields( flags ).Where( f => f.GetCustomAttribute<InjectAttribute>() != null ) );
				properties.AddRange( current.GetProperties( flags ).Where( p => p.GetCustomAttribute<InjectAttribute>() != null && p.CanWrite ) );
				methods.AddRange( current.GetMethods( flags ).Where( m => m.GetCustomAttribute<InjectAttribute>() != null ) );
			}

			// Inject fields
			foreach ( FieldInfo field in fields )
			{
				InjectAttribute attr = field.GetCustomAttribute<InjectAttribute>();
				object value;

				try
				{
					value = Resolve( field.FieldType, attr.Key );
				}
				catch ( Exception ex )
				{
					throw new InvalidOperationException(
						$"[Zenjex] Failed to resolve field '{field.Name}' of type '{field.FieldType.Name}' " +
						$"on '{targetType.Name}': {ex}", ex );
				}

				if ( value == null )
				{
					throw new InvalidOperationException(
						$"[Zenjex] Resolved a null instance for field '{field.Name}' of type " +
						$"'{field.FieldType.Name}' on '{targetType.Name}'. A factory/instance binding returned " +
						"null instead of throwing - check whatever FromFactory()/FromInstance() call registered this service." );
				}

				field.SetValue( target, value );
			}

			// Inject properties
			foreach ( PropertyInfo property in properties )
			{
				InjectAttribute attr = property.GetCustomAttribute<InjectAttribute>();
				object value;

				try
				{
					value = Resolve( property.PropertyType, attr.Key );
				}
				catch ( Exception ex )
				{
					throw new InvalidOperationException(
						$"[Zenjex] Failed to resolve property '{property.Name}' of type '{property.PropertyType.Name}' " +
						$"on '{targetType.Name}': {ex}", ex );
				}

				if ( value == null )
				{
					throw new InvalidOperationException(
						$"[Zenjex] Resolved a null instance for property '{property.Name}' of type " +
						$"'{property.PropertyType.Name}' on '{targetType.Name}'. A factory/instance binding returned " +
						"null instead of throwing - check whatever FromFactory()/FromInstance() call registered this service." );
				}

				property.SetValue( target, value );
			}

			// Inject methods
			foreach ( MethodInfo method in methods )
			{
				System.Reflection.ParameterInfo[] parameters = method.GetParameters();
				object[] args = new object[ parameters.Length ];

				for ( int i = 0; i < parameters.Length; i++ )
				{
					try
					{
						args[ i ] = Resolve( parameters[ i ].ParameterType );
					}
					catch ( Exception ex )
					{
						throw new InvalidOperationException(
							$"[Zenjex] Failed to resolve parameter '{parameters[ i ].Name}' of type " +
							$"'{parameters[ i ].ParameterType.Name}' while injecting method '{method.Name}' on " +
							$"'{targetType.Name}': {ex}", ex );
					}

					if ( args[ i ] == null )
					{
						throw new InvalidOperationException(
							$"[Zenjex] Resolved a null instance for parameter '{parameters[ i ].Name}' of type " +
							$"'{parameters[ i ].ParameterType.Name}' while injecting method '{method.Name}' on " +
							$"'{targetType.Name}'. A factory/instance binding returned null instead of throwing - " +
							"check whatever FromFactory()/FromInstance() call registered this service." );
					}
				}

				try
				{
					method.Invoke( target, args );
				}
				catch ( Exception ex )
				{
					throw new InvalidOperationException(
						$"[Zenjex] Injected method '{method.Name}' on '{targetType.Name}' threw: {ex.InnerException ?? ex}", ex );
				}
			}
		}

		#endregion

		#region Helpers

		private object Resolve( Type serviceType, string key = "" )
		{
			if ( string.IsNullOrEmpty( key ) )
			{
				if ( _singletons.TryGetValue( serviceType, out var cached ) )
					return cached;

				if ( _services.TryGetValue( serviceType, out var descriptor ) )
					return ResolveDescriptor( descriptor, serviceType );
			}
			else
			{
				var dictKey = (key, serviceType);
				if ( _keyedSingletons.TryGetValue( dictKey, out var cached ) )
					return cached;

				if ( _keyedServices.TryGetValue( dictKey, out var descriptor ) )
					return ResolveDescriptor( descriptor, serviceType );
			}

			throw new Exception( $"Service of type '{serviceType.Name}' is not registered." );
		}

		private object ResolveDescriptor( ServiceDescriptor descriptor, Type serviceType )
		{
			// If it's an instance (singleton or pre-created), return it
			if ( descriptor.Instance != null )
				return descriptor.Instance;

			// If it's a factory, invoke it
			if ( descriptor.Factory != null )
			{
				var instance = descriptor.Factory.DynamicInvoke();

				// Cache if singleton
				if ( descriptor.Lifetime == ServiceLifetime.Singleton )
				{
					if ( string.IsNullOrEmpty( descriptor.Key ) )
						_singletons[ serviceType ] = instance;
					else
						_keyedSingletons[ (descriptor.Key, serviceType) ] = instance;
				}

				return instance;
			}

			// Otherwise create new instance (Transient)
			try
			{
				object instance;

				if ( typeof( Node ).IsAssignableFrom( descriptor.ImplementationType ) )
				{
					var node = (Node)Activator.CreateInstance( descriptor.ImplementationType );
					AddChild( node );
					instance = node;
				}
				else
				{
					instance = Activator.CreateInstance( descriptor.ImplementationType );
				}

				if ( descriptor.Lifetime == ServiceLifetime.Singleton )
				{
					if ( string.IsNullOrEmpty( descriptor.Key ) )
						_singletons[ serviceType ] = instance;
					else
						_keyedSingletons[ (descriptor.Key, serviceType) ] = instance;
				}

				return instance;
			}
			catch ( Exception ex )
			{
				throw new Exception( $"Failed to create instance of '{descriptor.ImplementationType.Name}': {ex.Message}", ex );
			}
		}

		#endregion
	}

	/// <summary>Service lifetime enumeration.</summary>
	public enum ServiceLifetime
	{
		/// <summary>New instance created each time.</summary>
		Transient,

		/// <summary>Single instance shared across container.</summary>
		Singleton
	}

	/// <summary>Internal service descriptor.</summary>
	internal class ServiceDescriptor
	{
		public Type ServiceType { get; set; }
		public Type ImplementationType { get; set; }
		public ServiceLifetime Lifetime { get; set; }
		public object Instance { get; set; }
		public Delegate Factory { get; set; }
		public string Key { get; set; }
	}
}