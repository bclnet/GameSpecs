/*
 *  Unreal Engine .NET 7 integration 
 *  Copyright (c) 2021 Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.IO;
using UnrealBuildTool;

public class UnrealCLR : ModuleRules {
	public UnrealCLR(ReadOnlyTargetRules Target) : base(Target) {
		#if UE_4_24_OR_LATER
			bLegacyPublicIncludePaths = false;
			DefaultBuildSettings = BuildSettingsVersion.V2;
		#else
			PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		#endif

		PublicIncludePaths.AddRange(new string[] { });

		PrivateIncludePaths.AddRange(new string[] { });

		PublicDependencyModuleNames.AddRange(new string[] {
			"Core"
		});

		PrivateDependencyModuleNames.AddRange(new string[] {
			"AIModule",
			"AssetRegistry",
			"CoreUObject",
			"Engine",
			"HeadMountedDisplay",
			"InputCore",
			"Slate",
			"SlateCore"
		});

		DynamicallyLoadedModuleNames.AddRange(new string[] { });

		if (Target.bBuildEditor) {
			PrivateDependencyModuleNames.AddRange(new string[] {
				"UnrealEd"
			});
		} else {
			string runtimeArch = null;
			if (Target.Platform == UnrealTargetPlatform.Win64) {
				if (Target.Architecture == UnrealArch.X64)
					runtimeArch = "win-x64";
				else if (Target.Architecture == UnrealArch.Arm64)
					runtimeArch = "win-arm64";
				else
					throw new Exception($"Unknown architecture: {Target.Architecture}");
			} else if (Target.Platform == UnrealTargetPlatform.Linux) {
				if (Target.Architecture == UnrealArch.X64)
					runtimeArch = "linux-x64";
				else if (Target.Architecture == UnrealArch.Arm64)
					runtimeArch = "linux-arm64";
				else
					throw new Exception($"Unknown architecture: {Target.Architecture}");
			} else if (Target.Platform == UnrealTargetPlatform.Mac) {
				if (Target.Architecture == UnrealArch.X64)
					runtimeArch = "osx-x64";
				else if (Target.Architecture == UnrealArch.Arm64)
					runtimeArch = "osx-arm64";
				else
					throw new Exception($"Unknown architecture: {Target.Architecture}");
			} else
				throw new Exception($"Unknown platform: {Target.Platform}");

			string runtimePath = Path.Combine(ModuleDirectory, $"../../Runtime/{runtimeArch}");

			string[] files = Directory.GetFiles(runtimePath, "*.*", SearchOption.AllDirectories);

			foreach (string file in files) {
				RuntimeDependencies.Add(file);
			}

			files = Directory.GetFiles(Path.Combine(ModuleDirectory, "../../Managed"), "*.*", SearchOption.AllDirectories);

			foreach (string file in files) {
				RuntimeDependencies.Add(file);
			}

			string userAssemblies = Path.Combine(PluginDirectory , "../../Managed");

			if (Directory.Exists(userAssemblies)) {
				files = Directory.GetFiles(userAssemblies, "*.*", SearchOption.AllDirectories);

				foreach (string file in files) {
					RuntimeDependencies.Add(file);
				}
			}
		}
	}
}