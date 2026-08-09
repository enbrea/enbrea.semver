[![NuGet Gallery](https://img.shields.io/badge/NuGet%20Gallery-enbrea.semver-blue.svg)](https://www.nuget.org/packages/Enbrea.SemVer/)
![GitHub](https://img.shields.io/github/license/enbrea/enbrea.semver)

# Enbrea.SemVer

A lightweight .NET implementation of [Semantic Versioning 2.0.0](https://semver.org) for parsing, validating, comparing, formatting, and working with semantic versions and version ranges.

* Supports `.NET 10`, `.NET 9`, and `.NET 8`
* Strict validation of supported SemVer 2.0.0 syntax
* Parse strings and character spans into strongly typed semantic version objects
* Full SemVer prerelease precedence support, including `alpha`, `alpha.1`, `beta.2`, and `rc.1`
* Support for build metadata such as `+build.123`, without affecting version precedence
* Correct comparison of arbitrarily large numeric prerelease identifiers without numeric overflow
* Implements `IComparable<T>`, `IEquatable<T>`, `IParsable<T>`, and `ISpanParsable<T>`
* Parse and evaluate semantic version ranges with inclusive, exclusive, and unbounded endpoints
* Test whether a version satisfies a range with `SemanticVersionRange.Satisfies`
* Zero external dependencies

## Installation

```
dotnet add package Enbrea.SemVer
```

## Getting started

Documentation is available in the [GitHub wiki](https://github.com/enbrea/enbrea.semver/wiki).

## Can I help?

Yes, that would be much appreciated. The best way to help is to post a response via the Issue Tracker and/or submit a Pull Request.
