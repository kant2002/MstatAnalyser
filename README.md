Mstat filesize analysis
=======================

Console application for getting statistic information about NativeAOT filesize.

# How to produce mstat file in your NativeAOT application

Simply add
```
<PropertyGroup>
	<IlcGenerateMstatFile>true</IlcGenerateMstatFile>
</PropertyGroup>
```

Initial idea for the application was from [@Suchiman](https://github.com/Suchiman), for which I have ethernal gratitude!

# Example usage
```
# Statistics about types, methods and blobs
dotnet run --project MstatAnalyser\MstatAnalyser.csproj --file Library.mstat

# Statistics about types and methods which derived from types in assembly Microsoft.EntityFrameworkCore
dotnet run --project MstatAnalyser\MstatAnalyser.csproj --file Library.mstat --assembly Microsoft.EntityFrameworkCore

# Statistics about types and methods which derived from types in assemblies matching pattern Microsoft.AspNetCore.*
dotnet run --project MstatAnalyser\MstatAnalyser.csproj --file Library.mstat --assembly Microsoft.AspNetCore.*

# Exclude assemblies using multiple --exclude-assembly parameter
dotnet run --project MstatAnalyser\MstatAnalyser.csproj --file Library.mstat --exclude-assembly Microsoft.AspNetCore.* --exclude-assembly Microsoft.EntityFrameworkCore* --exclude-assembly Microsoft.Identity* --exclude-assembly Microsoft.Data.SqlClient --exclude-assembly Microsoft.Extensions.*

# You can specify folder instead of file. appication will take single file, if it preset.
dotnet run --project MstatAnalyser\MstatAnalyser.csproj --file ..\some-dir\
```