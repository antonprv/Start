// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace System.Runtime.CompilerServices
{
	[AttributeUsage( AttributeTargets.All, AllowMultiple = true, Inherited = false )]
	internal sealed class CompilerFeatureRequiredAttribute : Attribute
	{
		public CompilerFeatureRequiredAttribute( string featureName ) => FeatureName = featureName;
		public string FeatureName { get; }
		public bool IsOptional { get; set; }
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false )]
	internal sealed class RequiredMemberAttribute : Attribute { }
}