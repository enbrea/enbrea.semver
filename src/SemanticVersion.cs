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
/// Semantic version type, following closely https://semver.org
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
    /// Determines whether two versions are not equal.
    /// </summary>
    /// <param name="left">The first version to compare</param>
    /// <param name="right">The second version to compare</param>
    /// <returns>True, if left not equal to right; otherwise, false.</returns>
    public static bool operator !=(SemanticVersion left, SemanticVersion right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Compare two versions using less than
    /// </summary>
    /// <param name="left">The first version to compare</param>
    /// <param name="right">The second version to compare</param>
    /// <returns>true, if left is the less than right; otherwise, false.</returns>
    public static bool operator <(SemanticVersion left, SemanticVersion right)
    {
        if (left is null) return right != null;
        if (right is null) return false;
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Compare two versions using less or equal than
    /// </summary>
    /// <param name="left">The first version to compare</param>
    /// <param name="right">The second version to compare</param>
    /// <returns>True, if left is the less or equal than right; otherwise, false.</returns>
    public static bool operator <=(SemanticVersion left, SemanticVersion right) => left == right || left < right;

    /// <summary>
    /// Determines whether two versions are equal.
    /// </summary>
    /// <param name="left">The first version to compare</param>
    /// <param name="right">The second version to compare</param>
    /// <returns>True, if left equal to right; otherwise, false.</returns>
    public static bool operator ==(SemanticVersion left, SemanticVersion right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Compare two versions using greater than
    /// </summary>
    /// <param name="left">The first version to compare</param>
    /// <param name="right">The second version to compare</param>
    /// <returns>True, if left is the greater than right; otherwise, false.</returns>
    public static bool operator >(SemanticVersion left, SemanticVersion right)
    {
        if (right is null) return left != null;
        if (left is null) return false;
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Compare two versions using greater or equal than
    /// </summary>
    /// <param name="left">The first version to compare</param>
    /// <param name="right">The second version to compare</param>
    /// <returns>True, if left is the greater or equal than right; otherwise, false.</returns>
    public static bool operator >=(SemanticVersion left, SemanticVersion right) => left == right || left > right;

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
    /// <param name="provider">An object that supplies culture-specific formatting information</param>
    /// <returns>A <see cref="SemanticVersion"/> instance</returns>
    public static SemanticVersion Parse(string value, IFormatProvider provider)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Parse(value.AsSpan(), provider);
    }

    /// <summary>
    /// Parses a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted version</param>
    /// <returns></returns>
    public static SemanticVersion Parse(ReadOnlySpan<char> value)
    {
        return Parse(value, provider: null);
    }

    /// <summary>
    /// Parses a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted version</param>
    /// <param name="provider">An optional format provider. Semantic versions are culture-independent, so this parameter is 
    /// ignored.</param>
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
    /// <param name="value">String formatted version</param>
    /// <param name="result">When this method returns, contains the <see cref="SemanticVersion"/> value equivalent to the version 
    /// contained in <paramref name="value"/>, if the conversion succeeded, or null if the conversion failed.</param>
    /// <returns>True if the parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string value, [NotNullWhen(true)] out SemanticVersion result)
    {
        return TryParse(value, provider: null, out result);
    }

    /// <summary>
    /// Tries to parse a version string into a <see cref="SemanticVersion"/> instance.
    /// </summary>
    /// <param name="value">String formatted version</param>
    /// <param name="provider">An optional format provider. Semantic versions are culture-independent, so this parameter is 
    /// ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="SemanticVersion"/> value equivalent to the version 
    /// contained in <paramref name="value"/>, if the conversion succeeded, or null if the conversion failed.</param>
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
    /// <param name="value">String formatted version</param>
    /// <param name="result">When this method returns, contains the <see cref="SemanticVersion"/> value equivalent to the version 
    /// contained in <paramref name="value"/>, if the conversion succeeded, or null if the conversion failed.</param>
    /// <returns>True if the parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out SemanticVersion result)
    {
        return TryParse(value, provider: null, out result);
    }

    /// <summary>
    /// Tries to parse a version string into a <see cref="SemanticVersion"/> instance.  
    /// </summary>
    /// <param name="value">String formatted version</param>
    /// <param name="provider">An optional format provider. Semantic versions are culture-independent, so this parameter is 
    /// ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="SemanticVersion"/> value equivalent to the version 
    /// contained in <paramref name="value"/>, if the conversion succeeded, or null if the conversion failed.</param>
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
    /// Compares the current <see cref="SemanticVersion"/> with another one and returns an integer that indicates whether the 
    /// current instance precedes, follows, or occurs in the same position in the sort order as the other.
    /// </summary>
    /// <param name="other">The <see cref="SemanticVersion"/> to compare with the current <see cref="SemanticVersion"/>.</param>
    /// <returns>A value that indicates the relative order of the objects being compared.</returns>
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
    /// Determines whether the specified <see cref="SemanticVersion"/> is equal to the current <see cref="SemanticVersion"/>.
    /// </summary>
    /// <remarks>
    /// Build metadata does not affect equality because equality follows Semantic Versioning precedence rules.
    /// </remarks>
    /// <param name="other">The <see cref="SemanticVersion"/> to compare with the current <see cref="SemanticVersion"/>.</param>
    /// <returns>True if the specified <see cref="SemanticVersion"/> is equal to the current <see cref="SemanticVersion"/>; 
    /// otherwise, false.</returns>
    public bool Equals(SemanticVersion other)
    {
        return other is not null && CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="SemanticVersion"/>.  
    /// </summary>
    /// <remarks>
    /// Build metadata does not affect equality because equality follows Semantic Versioning precedence rules.
    /// </remarks>
    /// <param name="obj">The object to compare with the current <see cref="SemanticVersion"/>.</param>
    /// <returns>True if the specified object is equal to the current <see cref="SemanticVersion"/>; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        return Equals(obj as SemanticVersion);
    }

    /// <summary>
    /// Returns a hash code for the current <see cref="SemanticVersion"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="SemanticVersion"/>. </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, PreRelease);
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="SemanticVersion"/>. 
    /// </summary>
    /// <returns>A string that represents the current <see cref="SemanticVersion"/>.</returns>
    public override string ToString()
    {
        var version = PreRelease is null
            ? $"{Major}.{Minor}.{Patch}"
            : $"{Major}.{Minor}.{Patch}-{PreRelease}";

        return BuildMetadata is null ? version : $"{version}+{BuildMetadata}";
    }

    /// <summary>
    /// Compares two pre-release version identifiers according to the rules defined in https://semver.org.
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
    /// Compares two pre-release version identifiers according to the rules defined in https://semver.org.
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
    /// Determines whether the specified span of characters represents a numeric value (consists only of ASCII digits).
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
    /// Determines whether the specified span of characters is a valid build metadata according to the rules defined 
    /// in https://semver.org.
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
    /// Determines whether the specified span of characters is a valid identifier according to the rules defined 
    /// in https://semver.org.
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
    /// Determines whether the specified span of characters is a valid pre-release version according to the rules defined 
    /// in https://semver.org.
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
    /// Tries to parse a numeric component of the version (major, minor, or patch) from the specified span of characters.
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