# CircularReferenceFinder
Finds circular references in an objects graph using reflection. Useful to check an object graph before start serialization or graph traversal by code without built-in checks for circular references.
The code is intended to be used mainly with POCO models, i.e. it doesn't even try to analyze private fields or indexer properties, only simple public properties. Traversal of collections implementing `IEnumerable` are also supported.

#Usage
```csharp
var cycles = CircularReferenceFinder.FindCycles(new { Test = 1 });
```