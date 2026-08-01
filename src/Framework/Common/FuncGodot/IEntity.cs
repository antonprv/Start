namespace Framework.Common.FuncGodot
{
	public interface IEntity
	{
		/// <summary>
		///  FuncGodot writes here all properties (if there are any)
		/// </summary>
		public Godot.Collections.Dictionary FuncGodotProperties { get; set; }

		/// <summary>
		/// Called through node.call() we need to maintain prescise naming
		/// </summary>
		/// <param name="entityProperties"></param>
		public void _FuncGodotApplyProperties( Godot.Collections.Dictionary entityProperties );

		/// <summary>
		/// Called by func_godot after map builds
		/// </summary>
		public void _FuncGodotBuildComplete();
	}
}
