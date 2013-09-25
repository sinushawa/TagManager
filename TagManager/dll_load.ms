clearListener()
pathdll = @"C:\Users\JLatteux\Documents\Visual Studio 2012\Projects\TagManager\TagManager\bin\Debug\TagManager.dll"
DLL_assembly = (dotNetClass "System.Reflection.Assembly").Load ((dotNetClass "System.IO.File").ReadAllBytes pathdll)
esp = DLL_assembly.createInstance("TagManager.TagManager")
esp.Manage()