[![NuGet](https://buildstats.info/nuget/CircularReferenceFinder)](https://www.nuget.org/packages/CircularReferenceFinder/)
[![Build Status](https://www.myget.org/BuildSource/Badge/circularreferencefinder?identifier=769edca6-0345-40ae-80b7-4375b73c213b)](https://www.myget.org/)

# CircularReferenceFinder
Finds circular references in an objects graph using reflection. Useful to check an object graph before start serialization or graph traversal by code without built-in checks for circular references.
The code is intended to be used mainly with POCO models, i.e. it doesn't even try to analyze private fields or indexer properties, only simple public properties. Traversal of collections implementing `IEnumerable` are also supported.

#Usage
```csharp
var cycles = CircularReferenceFinder.FindCycles(new { Test = 1 });
```