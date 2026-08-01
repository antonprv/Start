namespace Framework.Common.Extensions
{
	public static class CodeExtensions
	{
		static public bool Is<T>( this object @this ) =>
		  @this is T
		  || ( @this is IProxy proxy && proxy.Original.Is<T>() );

		static public T As<T>( this object @this )
			where T : class
		{
			var result = ( @this as T );
			if ( result == default( T )
				&& @this is IProxy proxy )
				result = proxy.Original.As<T>();

			return result;
		}
	}
}
