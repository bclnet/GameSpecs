namespace StereoKit.Maui.Handlers
{
	//	Allows mappings to make decisions based on whether the cross-platform properties are updating the platform UI or vice-versa
	//  TODO Consider making this public for .NET 8
	internal enum DataFlowDirection
	{
		ToPlatform,
		FromPlatform
	}
}
