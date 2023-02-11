Mstat filesize analysis
=======================

Console application for getting statistic information about NativeAOT filesize.

Example usage
```
# Statistics about types, methods and blobs
dotnet run --file Library.mstat

# Statistics about types and methods which derived from types in assembly Microsoft.EntityFrameworkCore
dotnet run --file Library.mstat --assembly Microsoft.EntityFrameworkCore

# Statistics about types and methods which derived from types in assemblies matching pattern Microsoft.AspNetCore.*
dotnet run --file Library.mstat --assembly Microsoft.AspNetCore.*
```