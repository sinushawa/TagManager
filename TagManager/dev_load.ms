-- Development loader: bring TagManager up in a running 3ds Max without installing it
-- into bin\assemblies. Loads the Debug build (max_2025_dlls\TagManager.dll) from a
-- throwaway copy so rebuilding never fights a locked file, then runs the same startup
-- sequence Max would (IPlugin.Initialize, then AssemblyMain) through TagApi.Bootstrap().
--
-- Run it once per Max session. A second run in the same session is a no-op because the
-- assembly identity is already loaded; restart Max to pick up a new build.
(
	local src = (getFilenamePath (getSourceFileName())) + @"max_2025_dlls\"
	local stamp = (dotNetClass "System.DateTime").Now.ToString "yyyyMMdd_HHmmss"
	local dst = (getDir #temp) + @"\TagManager_dev_" + stamp + @"\"
	makeDir dst all:true
	for f in #("TagManager.dll", "TagManager.pdb", "dragonz.actb.dll", "TreeViewEx.dll", "WPFToolkit.dll", "DeepCloner.dll") do
		if doesFileExist (src + f) do copyFile (src + f) (dst + f)
	local asm = (dotNetClass "System.Reflection.Assembly").LoadFrom (dst + "TagManager.dll")
	format "TagManager loaded from %\n" asm.Location
	local api = dotNetClass "TagManager.TagApi"
	format "Bootstrap: %\n" (api.Bootstrap())
)
