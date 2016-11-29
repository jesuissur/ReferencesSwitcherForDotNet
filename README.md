# ReferencesSwitcherForDotNet
[![Build status](https://ci.appveyor.com/api/projects/status/xn98qagb6lqxe6i6?svg=true)](https://ci.appveyor.com/project/jesuissur/referencesswitcherfordotnet)

## What's for?
You got a .Net solution where projects are referencing each other with "assembly reference" and you want to switch those references to "project reference".

## How?
1. Get the latest release from the _LatestRelease source code directory
1. Execute the ReferencesSwitcherForDotNet.App.Console.exe to see each available parameters

### Requirements
* .NET Framework 4.6.1
* MS Build v14

## Examples

### Structure before
* MySolution.sln
	* Project1
		* Assembly references
			* Project2
			* Project3
			* ProjectXyz
	* Project2
		* Assembly references
			* Project3
			* AbcProject
	* Project3
	* ProjectXyz
	* AbcProject

### Switch
	ReferencesSwitcherForDotNet.App.Console.exe -solutions="C:\Path\To\MySolution.sln" -ignorePatterns=Abc,Xyz -switch
* MySolution.sln
	* Project1
		* Project references
			* Project2
			* Project3
		* Assembly references
			* ProjectXyz
	* Project2
		* Project references
			* Project3
		* Assembly references
			* AbcProject
	* Project3
	* ProjectXyz
	* AbcProject

### Rollback
	ReferencesSwitcherForDotNet.App.Console.exe -s="C:\Path\To\MySolution.sln" -rollback
Get back to the structure before

### Switch and never come back
	ReferencesSwitcherForDotNet.App.Console.exe -solutions="C:\Path\To\MySolution.sln" -ignorePatterns=Abc,Xyz -switch -noWayBack
Same as switch example but it is not possible to rollback changes.  Useful when you want to convert your projects once for all.
