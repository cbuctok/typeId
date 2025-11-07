# TypeID .Net Standard 2.1

## A C# implementation of [TypeIDs](https://github.com/jetpack-io/typeid) (Version 0.2.0)

[![CI](https://github.com/cbuctok/typeId/actions/workflows/ci.yml/badge.svg)](https://github.com/cbuctok/typeId/actions/workflows/ci.yml)
[![Performance Tests](https://github.com/cbuctok/typeId/actions/workflows/performance.yml/badge.svg)](https://github.com/cbuctok/typeId/actions/workflows/performance.yml)
[![PR Validation](https://github.com/cbuctok/typeId/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/cbuctok/typeId/actions/workflows/pr-validation.yml)
![License: Apache 2.0](https://img.shields.io/github/license/jetpack-io/typeid-go)

TypeIDs are a modern, **type-safe**, globally unique identifier based on the upcoming
UUIDv7 standard. They provide a ton of nice properties that make them a great choice
as the primary identifiers for your data in a database, APIs, and distributed systems.
Read more about TypeIDs in their [spec](https://github.com/jetpack-io/typeid).

This particular implementation provides a C# library for generating and parsing TypeIDs.

## Usage

### Import the library

```csharp
using TypeId;
```

### Create a TypeID

```csharp
var typeId = TypeId.NewTypeId("prefix");
```

### Parse a TypeID

```csharp
var typeId = TypeId.Parse("contact_2x4y6z8a0b1c2d3e4f5g6h7j8k");
```

### Get the string representation of a TypeID

```csharp
var typeId = TypeId.NewTypeId("prefix");
var typeIdString = typeId.ToString();
```
