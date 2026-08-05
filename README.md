[![NuGet Gallery](https://img.shields.io/badge/NuGet%20Gallery-enbrea.semver-blue.svg)](https://www.nuget.org/packages/Enbrea.SemVer/)
![GitHub](https://img.shields.io/github/license/enbrea/enbrea.semver)

# Enbrea.SemVer

A lightweight .NET implementation of [Semantic Versioning 2.0.0](https://semver.org) for parsing, validating, comparing, and formatting semantic version numbers.

+ Supports `.NET 10`, `.NET 9`, and `.NET 8`
+ Strict SemVer 2.0.0 parsing and validation
+ Parse strings and character spans into strongly typed version objects
+ Compare versions using standard comparison operators
+ Full SemVer prerelease precedence support, including `alpha`, `alpha.1`, `beta.2`, and `rc.1`
+ Support for build metadata such as `+build.123`
+ Correct comparison of arbitrarily large numeric prerelease identifiers
+ Implements `IComparable<T>`, `IEquatable<T>`, `IParsable<T>`, and `ISpanParsable<T>`
+ Zero external dependencies

## Installation

```
dotnet add package Enbrea.SemVer
```

## Getting started

Documentation is available in the [GitHub wiki](https://github.com/enbrea/enbrea.semver/wiki).

## Can I help?

Yes, that would be much appreciated. The best way to help is to post a response via the Issue Tracker and/or submit a Pull Request.
