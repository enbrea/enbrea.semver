#region Enbrea.SemVer - Copyright (c) STÜBER SYSTEMS GmbH
/*    
 *    Enbrea.SemVer 
 *    
 *    Copyright (c) STÜBER SYSTEMS GmbH
 *
 *    Licensed under the MIT License, Version 2.0. 
 * 
 */
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Enbrea.SemVer;

/// <summary>
/// Represents a semantic version according to <see href="https://semver.org">Semantic Versioning 2.0.0</see>.
/// </summary>
public sealed class SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>, ISpanParsable<SemanticVersion>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticVersion"/> class.
    /// </summary>
    /// <param name="major">Major version part</param>
    /// <param name="minor">Minor version part</param>
    /// <param name="patch">Patch version part</param>
    /// <param name="preRelease">Additional label for pre-release</param>
    /// <param name="buildMetadata">Additional build metadata</param>
    public SemanticVersion(int major, int minor, int patch, string preRelease = null, string buildMetadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);
        ArgumentOutOfRangeException.ThrowIfNegative(patch);

        if (preRelease is not null && !IsValidPreRelease(preRelease.AsSpan()))
        {
            throw new ArgumentException("The prerelease value is invalid.", nameof(preRelease));
        }

        if (buildMetadata is not null && !IsValidBuildMetadata(buildMetadata.AsSpan()))
        {
            throw new ArgumentException("The build metadata is invalid.", nameof(buildMetadata));
        }

        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        BuildMetadata = buildMetadata;
    }

    /// <summary>
    /// Additional build metadata
    /// </summary>
    public string BuildMetadata { get; }

    /// <summary>
    /// Determines whether the version has build metadata.
    /// </summary>
    public bool HasBuildMetadata => BuildMetadata is not null;

    /// <summary>
    /// Determines whether the version is a pre-release version.
    /// </summary>
    public bool IsPrerelease => PreRelease is not null;
    
    /// <summary>
    /// Major version part
    /// </summary>
    public int Major { get; }

    /// <summary>
    /// Minor version part
    /// </summary>
    public int Minor { get; }

    /// <summary>
    /// Patch version part
    /// </summary>
    public int Patch { get; }

    /// <summary>
    /// Additional label for pre-release
    /// </summary>
    public string PreRelease { get; }

    /// <summary>
    /// Determines whether the specified string represents a valid semantic version.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> represents a valid semantic version; otherwise, 
    /// <c>false</c>.
    /// </returns>
    public static bool IsValid(string value)
    {
        return TryParse(value, provider: null, out _);
    }

    /// <summary>
    /// Determines whether the specified span of characters is a valid semantic version.
    /// </summary>
    /// <param name="value">The span of characters to validate.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> represents a valid semantic version; otherwise, 
    /// <c>false</c>.
    /// </returns>
    public static bool IsValid(ReadOnlySpan<char> value)
    {
        return TryParse(value, provider: null, out _);
    }

    /// <summary>
    /// Determines whether two semantic versions are not equal.
    /// </summary>
    /// <param name="left">The first semantic version to compare.</param>
    /// <param name="right">The second semantic version to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> and <paramref name="right"/> are not equal; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool operator !=(SemanticVersion left, SemanticVersion right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Compares two semantic versions using less than.
    /// </summary>
    /// <param name="left">The first semantic version to compare.</param>
    /// <param name="right">The second semantic version to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool operator <(SemanticVersion left, SemanticVersion right)
    {
        if (left is null) return right != null;
        if (right is null) return false;
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Compares two semantic versions using less than or equal to.
    /// </summary>
    /// <param name="left">The first semantic version to compare.</param>
    /// <param name="right">The second semantic version to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool operator <=(SemanticVersion left, SemanticVersion right) => !(left > right);

    /// <summary>
    /// Determines whether two semantic versions are equal.
    /// </summary>
    /// <param name="left">The first semantic version to compare.</param>
    /// <param name="right">The second semantic version to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is equal to <paramref name="right"/>; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(SemanticVersion left, SemanticVersion right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Compares two semantic versions using greater than.
    /// </summary>
    /// <param name="left">The first semantic version to compare.</param>
    /// <param name="right">The second semantic version to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool operator >(SemanticVersion left, SemanticVersion right)
    {
        if (right is null) return left != null;
        if (left is null) return false;
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Compares two semantic versions using greater than or equal to.
    /// </summary>
    /// <param name="left">The first semantic version to compare.</param>
    /// <param name="right">The second semantic version to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator >=(SemanticVersion left, SemanticVersion right) => !(left < right);

    /// <summary>
    /// Parses a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted version</param>
    public static SemanticVersion Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Parse(value.AsSpan(), provider: null);
    }

    /// <summary>
    /// Parses a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted version</param>
    /// <param name="provider">An optional format provider. Semantic versions are 
    /// culture-independent, so this parameter is ignored.</param>
    /// <returns>A <see cref="SemanticVersion"/> instance</returns>
    public static SemanticVersion Parse(string value, IFormatProvider provider)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Parse(value.AsSpan(), provider);
    }

    /// <summary>
    /// Parses a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted semantic version</param>
    /// <returns>A <see cref="SemanticVersion"/> instance</returns>
    public static SemanticVersion Parse(ReadOnlySpan<char> value)
    {
        return Parse(value, provider: null);
    }

    /// <summary>
    /// Parses a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted semantic version</param>
    /// <param name="provider">An optional format provider. Semantic versions are 
    /// culture-independent, so this parameter is ignored.</param>
    /// <returns>A <see cref="SemanticVersion"/> instance</returns>
    public static SemanticVersion Parse(ReadOnlySpan<char> value, IFormatProvider provider)
    {
        if (TryParse(value, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"'{value}' is not a valid semantic version.");
    }

    /// <summary>
    /// Tries to parse a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted semantic version</param>
    /// <param name="result">When this method returns, contains the <see cref="SemanticVersion"/> value equivalent to the version 
    /// contained in <paramref name="value"/>, if the conversion succeeded, or null if the 
    /// conversion failed.</param>
    /// <returns>True if the parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string value, [NotNullWhen(true)] out SemanticVersion result)
    {
        return TryParse(value, provider: null, out result);
    }

    /// <summary>
    /// Tries to parse a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted semantic version</param>
    /// <param name="provider">An optional format provider. Semantic versions are 
    /// culture-independent, so this parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="SemanticVersion"/> value equivalent to the version 
    /// contained in <paramref name="value"/>, if the conversion succeeded, or null if the 
    /// conversion failed.</param>
    /// <returns>True if the parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string value, IFormatProvider provider, [NotNullWhen(true)] out SemanticVersion result)
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        return TryParse(value.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Tries to parse a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted semantic version</param>
    /// <param name="result">When this method returns, contains the <see cref="SemanticVersion"/> value equivalent to the version 
    /// contained in <paramref name="value"/>, if the conversion succeeded, or null if the 
    /// conversion failed.</param>
    /// <returns>True if the parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out SemanticVersion result)
    {
        return TryParse(value, provider: null, out result);
    }

    /// <summary>
    /// Tries to parse a version string into a <see cref="SemanticVersion"/> instance.  
    /// </summary>
    /// <param name="value">String formatted semantic version</param>
    /// <param name="provider">An optional format provider. Semantic versions are 
    /// culture-independent, so this parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="SemanticVersion"/> value equivalent to the version 
    /// contained in <paramref name="value"/>, if the conversion succeeded, or null if the 
    /// conversion failed.</param>
    /// <returns>True if the parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider provider, [NotNullWhen(true)] out SemanticVersion result)
    {
        result = null;
        if (value.IsEmpty) return false;

        var versionAndPreRelease = value;
        ReadOnlySpan<char> buildMetadata = default;

        var plusIndex = value.IndexOf('+');
        if (plusIndex >= 0)
        {
            versionAndPreRelease = value[..plusIndex];
            buildMetadata = value[(plusIndex + 1)..];

            if (buildMetadata.IsEmpty || buildMetadata.IndexOf('+') >= 0 || !IsValidBuildMetadata(buildMetadata))
            {
                return false;
            }
        }

        var normalVersion = versionAndPreRelease;
        ReadOnlySpan<char> preRelease = default;

        var hyphenIndex = versionAndPreRelease.IndexOf('-');
        if (hyphenIndex >= 0)
        {
            normalVersion = versionAndPreRelease[..hyphenIndex];
            preRelease = versionAndPreRelease[(hyphenIndex + 1)..];

            if (preRelease.IsEmpty || !IsValidPreRelease(preRelease)) return false;
        }

        var firstDotIndex = normalVersion.IndexOf('.');
        if (firstDotIndex <= 0) return false;

        var remaining = normalVersion[(firstDotIndex + 1)..];
        var secondDotIndex = remaining.IndexOf('.');
        if (secondDotIndex <= 0) return false;

        var majorSpan = normalVersion[..firstDotIndex];
        var minorSpan = remaining[..secondDotIndex];
        var patchSpan = remaining[(secondDotIndex + 1)..];

        if (patchSpan.IsEmpty || patchSpan.IndexOf('.') >= 0) return false;

        if (!TryParseNumericComponent(majorSpan, out var major) ||
            !TryParseNumericComponent(minorSpan, out var minor) ||
            !TryParseNumericComponent(patchSpan, out var patch))
        {
            return false;
        }

        result = new SemanticVersion(
            major,
            minor,
            patch,
            preRelease.IsEmpty ? null : preRelease.ToString(),
            buildMetadata.IsEmpty ? null : buildMetadata.ToString());

        return true;
    }

    /// <summary>
    /// Compares the precedence of the current semantic version with another semantic version.
    /// Build metadata is ignored when determining precedence.
    /// </summary>
    /// <param name="other">The semantic version to compare with the current semantic version.</param>
    /// <returns>
    /// A value less than zero if this version has lower precedence than <paramref name="other"/>;
    /// zero if both versions have the same precedence; or a value greater than zero if this
    /// version has higher precedence than <paramref name="other"/>.
    /// </returns>
    public int CompareTo(SemanticVersion other)
    {
        if (other is null) return 1;

        var result = Major.CompareTo(other.Major);
        if (result != 0) return result;

        result = Minor.CompareTo(other.Minor);
        if (result != 0) return result;

        result = Patch.CompareTo(other.Patch);
        if (result != 0) return result;

        if (PreRelease is null) return other.PreRelease is null ? 0 : 1;
        if (other.PreRelease is null) return -1;
        if (string.Equals(PreRelease, other.PreRelease, StringComparison.Ordinal)) return 0;

        return ComparePreRelease(PreRelease.AsSpan(), other.PreRelease.AsSpan());
    }

    /// <summary>
    /// Determines whether all components of the specified semantic version, including build 
    /// metadata, are equal to those of the current semantic version.
    /// </summary>
    /// <param name="other">The semantic version to compare with the current semantic version.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="other"/> is equal to the current semantic version; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(SemanticVersion other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return 
            Major == other.Major
            && Minor == other.Minor
            && Patch == other.Patch
            && string.Equals(PreRelease, other.PreRelease, StringComparison.Ordinal)
            && string.Equals(BuildMetadata, other.BuildMetadata, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current semantic version.
    /// </summary>
    /// <param name="obj">The object to compare with the current semantic version.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="obj"/> is equal to the current semantic version; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
        return Equals(obj as SemanticVersion);
    }

    /// <summary>
    /// Returns the hash code for the current semantic version.
    /// </summary>
    /// <returns>The hash code for the current semantic version.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, PreRelease, BuildMetadata);
    }

    /// <summary>
    /// Determines whether the specified semantic version has the same precedence as the current
    /// semantic version.
    /// </summary>
    /// <remarks>
    /// Build metadata does not affect precedence because it is not considered when determining 
    /// version precedence. Two versions that differ only in build metadata have the same precedence.
    /// </remarks>
    /// <param name="other">The semantic version to compare with the current semantic version.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="other"/> has the same precedence as the current semantic 
    /// version; otherwise, <c>false</c>.
    /// </returns>
    public bool HasSamePrecedence(SemanticVersion other)
    {
        return other is not null && CompareTo(other) == 0;
    }

    /// <summary>
    /// Returns a string representation of the current semantic version.
    /// </summary>
    /// <returns>A string representation of the current semantic version.</returns>
    public override string ToString()
    {
        var version = PreRelease is null
            ? $"{Major}.{Minor}.{Patch}"
            : $"{Major}.{Minor}.{Patch}-{PreRelease}";

        return BuildMetadata is null ? version : $"{version}+{BuildMetadata}";
    }

    /// <summary>
    /// Creates a new semantic version with the specified build metadata.
    /// </summary>
    /// <param name="buildMetadata">The build metadata to set, or <c>null</c> to remove it.</param>
    /// <returns>A new semantic version with the specified build metadata.</returns>
    public SemanticVersion WithBuildMetadata(string buildMetadata)
    {
        if (buildMetadata is not null && !IsValidBuildMetadata(buildMetadata.AsSpan()))
        {
            throw new ArgumentException("The build metadata value is invalid.", nameof(buildMetadata));
        }

        if (BuildMetadata != buildMetadata)
        {
            return new SemanticVersion(Major, Minor, Patch, PreRelease, buildMetadata);
        }

        return this;
    }

    /// <summary>
    /// Creates a new semantic version with the specified major version.
    /// </summary>
    /// <param name="major">The major version to set.</param>
    /// <returns>A new semantic version with the specified major version.</returns>
    public SemanticVersion WithMajor(int major)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);

        if (Major != major)
        {
            return new SemanticVersion(major, Minor, Patch, PreRelease, BuildMetadata);
        }

        return this;
    }

    /// <summary>
    /// Creates a new semantic version with the specified minor version.
    /// </summary>
    /// <param name="minor">The minor version to set.</param>
    /// <returns>A new semantic version with the specified minor version.</returns>
    public SemanticVersion WithMinor(int minor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minor);

        if (Minor != minor)
        {
            return new SemanticVersion(Major, minor, Patch, PreRelease, BuildMetadata);
        }

        return this;
    }

    /// <summary>
    /// Creates a new semantic version without build metadata.
    /// </summary>
    /// <returns>A new semantic version without build metadata.</returns>
    public SemanticVersion WithoutBuildMetadata()
    {
        if (BuildMetadata is not null)
        {
            return new SemanticVersion(Major, Minor, Patch, PreRelease, null);
        }

        return this;
    }

    /// <summary>
    /// Creates a new semantic version without a pre-release version.
    /// </summary>
    /// <returns>A new semantic version without a pre-release version.</returns>
    public SemanticVersion WithoutPreRelease()
    {
        if (PreRelease is not null)
        {
            return new SemanticVersion(Major, Minor, Patch, null, BuildMetadata);
        }

        return this;
    }

    /// <summary>
    /// Creates a new semantic version with the specified patch version.
    /// </summary>
    /// <param name="patch">The patch version to set.</param>
    /// <returns>A new semantic version with the specified patch version.</returns>
    public SemanticVersion WithPatch(int patch)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(patch);

        if (Patch != patch)
        {
            return new SemanticVersion(Major, Minor, patch, PreRelease, BuildMetadata);
        }

        return this;
    }

    /// <summary>
    /// Creates a new semantic version with the specified pre-release version.
    /// </summary>
    /// <param name="preRelease">The pre-release version to set, or <c>null</c> to remove it.</param>
    /// <returns>A new semantic version with the specified pre-release version.</returns>
    public SemanticVersion WithPreRelease(string preRelease)
    {
        if (preRelease is not null && !IsValidPreRelease(preRelease.AsSpan()))
        {
            throw new ArgumentException("The prerelease value is invalid.", nameof(preRelease));
        }

        if (PreRelease != preRelease)
        {
            return new SemanticVersion(Major, Minor, Patch, preRelease, BuildMetadata);
        }

        return this;
    }

    /// <summary>
    /// Compares two pre-release values according to the Semantic Versioning precedence rules.
    /// </summary>
    private static int ComparePreRelease(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        while (true)
        {
            var leftDotIndex = left.IndexOf('.');
            var rightDotIndex = right.IndexOf('.');

            var leftIdentifier = leftDotIndex >= 0 ? left[..leftDotIndex] : left;
            var rightIdentifier = rightDotIndex >= 0 ? right[..rightDotIndex] : right;

            var result = ComparePreReleaseIdentifier(leftIdentifier, rightIdentifier);
            if (result != 0) return result;

            var leftHasMore = leftDotIndex >= 0;
            var rightHasMore = rightDotIndex >= 0;

            if (!leftHasMore || !rightHasMore)
            {
                if (leftHasMore) return 1;
                if (rightHasMore) return -1;
                return 0;
            }

            left = left[(leftDotIndex + 1)..];
            right = right[(rightDotIndex + 1)..];
        }
    }

    /// <summary>
    /// Compares two individual pre-release identifiers according to the Semantic Versioning 
    /// precedence rules.
    /// </summary>
    private static int ComparePreReleaseIdentifier(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        var leftIsNumeric = IsNumeric(left);
        var rightIsNumeric = IsNumeric(right);

        if (leftIsNumeric && rightIsNumeric)
        {
            var lengthComparison = left.Length.CompareTo(right.Length);
            return lengthComparison != 0 ? lengthComparison : left.SequenceCompareTo(right);
        }

        if (leftIsNumeric) return -1;
        if (rightIsNumeric) return 1;

        return left.SequenceCompareTo(right);
    }

    /// <summary>
    /// Determines whether the specified character is an ASCII digit (0-9).
    /// </summary>
    private static bool IsAsciiDigit(char value)
    {
        return value is >= '0' and <= '9';
    }

    /// <summary>
    /// Determines whether the specified span of characters represents a numeric value 
    /// (consists only of ASCII digits).
    /// </summary>
    private static bool IsNumeric(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return false;

        foreach (var character in value)
        {
            if (!IsAsciiDigit(character)) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified span of characters is a valid build metadata according 
    /// to the rules defined in <see href="https://semver.org">Semantic Versioning 2.0.0</see>.
    /// </summary>
    private static bool IsValidBuildMetadata(ReadOnlySpan<char> value)
    {
        while (true)
        {
            var dotIndex = value.IndexOf('.');
            var identifier = dotIndex >= 0 ? value[..dotIndex] : value;

            if (!IsValidIdentifier(identifier)) return false;
            if (dotIndex < 0) return true;

            value = value[(dotIndex + 1)..];
            if (value.IsEmpty) return false;
        }
    }

    /// <summary>
    /// Determines whether the specified character span is a valid identifier according to
    /// <see href="https://semver.org">Semantic Versioning 2.0.0</see>.
    /// </summary>
    private static bool IsValidIdentifier(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return false;

        foreach (var character in value)
        {
            if (!IsAsciiDigit(character) &&
                character is not (>= 'A' and <= 'Z') &&
                character is not (>= 'a' and <= 'z') &&
                character != '-')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified character span is a valid pre-release version according
    /// to <see href="https://semver.org">Semantic Versioning 2.0.0</see>.
    /// </summary>
    private static bool IsValidPreRelease(ReadOnlySpan<char> value)
    {
        while (true)
        {
            var dotIndex = value.IndexOf('.');
            var identifier = dotIndex >= 0 ? value[..dotIndex] : value;

            if (!IsValidIdentifier(identifier)) return false;

            if (IsNumeric(identifier) && identifier.Length > 1 && identifier[0] == '0')
            {
                return false;
            }

            if (dotIndex < 0) return true;

            value = value[(dotIndex + 1)..];
            if (value.IsEmpty) return false;
        }
    }

    /// <summary>
    /// Tries to parse a numeric component of the version (major, minor, or patch) from the 
    /// specified character span.
    /// </summary>
    private static bool TryParseNumericComponent(ReadOnlySpan<char> value, out int result)
    {
        result = default;

        if (value.IsEmpty || (value.Length > 1 && value[0] == '0') || !IsNumeric(value))
        {
            return false;
        }

        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out result);
    }
}