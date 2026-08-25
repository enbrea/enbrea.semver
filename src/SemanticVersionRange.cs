#region Enbrea.SemVer - Copyright (c) STÜBER SYSTEMS GmbH
/*    
 *    Enbrea.SemVer 
 *    
 *    Copyright (c) STÜBER SYSTEMS GmbH
 *
 *    Licensed under the MIT License. 
 * 
 */
#endregion

using System;
using System.Diagnostics.CodeAnalysis;

namespace Enbrea.SemVer;

/// <summary>
/// Represents a range of semantic versions, defined by optional lower and upper bounds, which can 
/// be inclusive or exclusive.
/// </summary>
/// <remarks>
/// <para>
/// String representations of version ranges use interval notation:
/// </para>
/// <list type="bullet">
///   <item>
///     <description><c>1.2.3</c> is equivalent to <c>[1.2.3,)</c> and specifies a minimum version 
///     with no upper bound.</description>
///   </item>
///   <item>
///     <description><c>[1.2.3]</c> specifies an exact-precedence range.</description>
///   </item>
///   <item>
///     <description><c>[1.0.0,2.0.0)</c> includes <c>1.0.0</c> and excludes <c>2.0.0</c>.</description>
///   </item>
///   <item>
///     <description><c>(,2.0.0]</c> specifies no lower bound and includes <c>2.0.0</c>.</description>
///   </item>
/// </list>
/// <para>
/// Square brackets indicate inclusive bounds, while parentheses indicate exclusive or unbounded bounds.
/// </para>
/// </remarks>
public sealed class SemanticVersionRange : IEquatable<SemanticVersionRange>, ISpanParsable<SemanticVersionRange>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticVersionRange"/> class.
    /// </summary>
    /// <param name="minVersion">Lower bound version or null for unbounded lower range.</param>
    /// <param name="includeMinVersion">True if the lower bound is inclusive.</param>
    /// <param name="maxVersion">Upper bound version or null for unbounded upper range.</param>
    /// <param name="includeMaxVersion">True if the upper bound is inclusive.</param>
    public SemanticVersionRange(
        SemanticVersion minVersion = null,
        bool includeMinVersion = true,
        SemanticVersion maxVersion = null,
        bool includeMaxVersion = false)
    {
        MinVersion = minVersion;
        IsMinInclusive = minVersion is not null && includeMinVersion;
        MaxVersion = maxVersion;
        IsMaxInclusive = maxVersion is not null && includeMaxVersion;

        if (MinVersion is not null && MaxVersion is not null)
        {
            var comparison = MinVersion.CompareTo(MaxVersion);
            if (comparison > 0)
            {
                throw new ArgumentException("Minimum version must be less than or equal to maximum version.");
            }

            if (comparison == 0 && (!IsMinInclusive || !IsMaxInclusive))
            {
                throw new ArgumentException("Equal minimum and maximum bounds must both be inclusive.");
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticVersionRange"/> class.
    /// </summary>
    /// <param name="version">The version whose precedence defines the range.</param>
    public SemanticVersionRange(SemanticVersion version)
        : this(version, includeMinVersion: true, version, includeMaxVersion: true)
    {
    }

    /// <summary>
    /// Determines whether the range has a lower bound.
    /// </summary>
    public bool HasLowerBound => MinVersion is not null;

    /// <summary>
    /// Determines whether the range has an upper bound.
    /// </summary>
    public bool HasUpperBound => MaxVersion is not null;

    /// <summary>
    /// Determines whether the upper version bound is inclusive.
    /// </summary>
    public bool IsMaxInclusive { get; }

    /// <summary>
    /// Determines whether the lower version bound is inclusive.
    /// </summary>
    public bool IsMinInclusive { get; }

    /// <summary>
    /// Upper version bound.
    /// </summary>
    public SemanticVersion MaxVersion { get; }

    /// <summary>
    /// Lower version bound.
    /// </summary>
    public SemanticVersion MinVersion { get; }

    /// <summary>
    /// Parses the specified string into a semantic version range.
    /// </summary>
    /// <param name="value">The string representation of the semantic version range to parse.</param>
    /// <returns>The parsed semantic version range.</returns>
    public static SemanticVersionRange Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        return Parse(value.AsSpan(), provider: null);
    }

    /// <summary>
    /// Parses the specified string into a semantic version range.
    /// </summary>
    /// <param name="value">The string representation of the semantic version range to parse.</param>
    /// <param name="provider">An optional format provider. Semantic versions are 
    /// culture-independent, so this parameter is ignored.</param>
    /// <returns>The parsed semantic version range.</returns>
    public static SemanticVersionRange Parse(string value, IFormatProvider provider)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Parse(value.AsSpan(), provider);
    }

    /// <summary>
    /// Parses the specified character span into a semantic version range.
    /// </summary>
    /// <param name="value">The string representation of the semantic version range to parse.</param>
    /// <returns>The parsed semantic version range.</returns>
    public static SemanticVersionRange Parse(ReadOnlySpan<char> value)
    {
        return Parse(value, provider: null);
    }

    /// <summary>
    /// Parses the specified character span into a semantic version range.
    /// </summary>
    /// <param name="value">The string representation of the semantic version range to parse.</param>
    /// <param name="provider">An optional format provider. Semantic versions are 
    /// culture-independent, so this parameter is ignored.</param>
    /// <returns>The parsed semantic version range.</returns>
    public static SemanticVersionRange Parse(ReadOnlySpan<char> value, IFormatProvider provider)
    {
        if (TryParse(value, out var result))
        {
            return result;
        }

        throw new FormatException($"'{value}' is not a valid semantic version range.");
    }

    /// <summary>
    /// Tries to parse the specified string into a semantic version range.
    /// </summary>
    /// <param name="value">The string representation of the semantic version range to parse.</param>
    /// <param name="result">When this method returns, contains the parsed semantic version range 
    /// if parsing succeeded; otherwise, <c>null</c>.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> was parsed successfully; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParse(string value, [NotNullWhen(true)] out SemanticVersionRange result)
    {
        return TryParse(value, provider: null, out result);
    }

    /// <summary>
    /// Tries to parse the specified string into a semantic version range.
    /// </summary>
    /// <param name="value">The string representation of the semantic version range to parse.</param>
    /// <param name="provider">An optional format provider. Semantic versions are 
    /// culture-independent, so this parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed semantic version range 
    /// if parsing succeeded; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if <paramref name="value"/> was parsed successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string value, IFormatProvider provider, [NotNullWhen(true)] out SemanticVersionRange result)
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        return TryParse(value.AsSpan(), out result);
    }

    /// <summary>
    /// Tries to parse the specified character span into a semantic version range.
    /// </summary>
    /// <param name="value">The string representation of the semantic version range to parse.</param>
    /// <param name="result">When this method returns, contains the parsed semantic version range 
    /// if parsing succeeded; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if <paramref name="value"/> was parsed successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out SemanticVersionRange result)
    {
        return TryParse(value, provider: null, out result);
    }

    /// <summary>
    /// Tries to parse the specified character span into a semantic version range.
    /// </summary>
    /// <param name="value">The string representation of the semantic version range to parse.</param>
    /// <param name="provider">An optional format provider. Semantic versions are 
    /// culture-independent, so this parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the parsed semantic version range 
    /// if parsing succeeded; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if <paramref name="value"/> was parsed successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider provider, [NotNullWhen(true)] out SemanticVersionRange result)
    {
        result = null;

        var text = value.Trim();
        if (text.Length == 0)
        {
            return false;
        }

        if (SemanticVersion.TryParse(text, out var singleVersion))
        {
            result = new SemanticVersionRange(singleVersion, includeMinVersion: true, maxVersion: null, includeMaxVersion: false);
            return true;
        }

        if (text.Length < 2)
        {
            return false;
        }

        var first = text[0];
        var last = text[^1];

        if ((first != '(' && first != '[') || (last != ')' && last != ']'))
        {
            return false;
        }

        var content = text[1..^1];
        var commaIndex = content.IndexOf(',');

        if (commaIndex < 0)
        {
            if (first != '[' || last != ']')
            {
                return false;
            }

            var exactText = content.Trim();
            if (!SemanticVersion.TryParse(exactText, out var exactVersion))
            {
                return false;
            }

            result = new SemanticVersionRange(exactVersion);
            return true;
        }

        if (content[(commaIndex + 1)..].IndexOf(',') >= 0)
        {
            return false;
        }

        var minText = content[..commaIndex].Trim();
        var maxText = content[(commaIndex + 1)..].Trim();

        SemanticVersion minVersion = null;
        SemanticVersion maxVersion = null;

        if (minText.Length > 0 && !SemanticVersion.TryParse(minText, out minVersion))
        {
            return false;
        }

        if (maxText.Length > 0 && !SemanticVersion.TryParse(maxText, out maxVersion))
        {
            return false;
        }

        try
        {
            result = new SemanticVersionRange(minVersion, includeMinVersion: first == '[', maxVersion, includeMaxVersion: last == ']');
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified semantic version range is equal to the current semantic 
    /// version range.
    /// </summary>
    /// <param name="other">The semantic version range to compare with the current semantic version
    /// range.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="other"/> is equal to the current semantic version range; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(SemanticVersionRange other)
    {
        if (other is null)
        {
            return false;
        }

        return IsMinInclusive == other.IsMinInclusive
            && IsMaxInclusive == other.IsMaxInclusive
            && Equals(MinVersion, other.MinVersion)
            && Equals(MaxVersion, other.MaxVersion);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current 
    /// <see cref="SemanticVersionRange"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current semantic version range.</param>
    /// <returns><c>true</c> if the specified object is equal to the current semantic version range;
    /// otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj)
    {
        return Equals(obj as SemanticVersionRange);
    }

    /// <summary>
    /// Returns the hash code for the current semantic version range.
    /// </summary>
    /// <returns>The hash code for the current semantic version range.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(MinVersion, IsMinInclusive, MaxVersion, IsMaxInclusive);
    }

    /// <summary>
    /// Determines whether the specified semantic version satisfies the current semantic version 
    /// range.
    /// </summary>
    /// <param name="version">The semantic version to test.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="version"/> satisfies the current semantic version range; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Satisfies(SemanticVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (MinVersion is not null)
        {
            var minComparison = version.CompareTo(MinVersion);
            if (minComparison < 0 || (minComparison == 0 && !IsMinInclusive))
            {
                return false;
            }
        }

        if (MaxVersion is not null)
        {
            var maxComparison = version.CompareTo(MaxVersion);
            if (maxComparison > 0 || (maxComparison == 0 && !IsMaxInclusive))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns a string representation of the current semantic version range.
    /// </summary>
    /// <returns>A string representation of the current semantic version range.</returns>
    public override string ToString()
    {
        if (MinVersion is not null && MaxVersion is not null && IsMinInclusive && IsMaxInclusive && MinVersion == MaxVersion)
        {
            return $"[{MinVersion}]";
        }

        if (MinVersion is not null && MaxVersion is null && IsMinInclusive)
        {
            return MinVersion.ToString();
        }

        var start = IsMinInclusive ? '[' : '(';
        var end = IsMaxInclusive ? ']' : ')';
        var min = MinVersion?.ToString() ?? string.Empty;
        var max = MaxVersion?.ToString() ?? string.Empty;

        return $"{start}{min},{max}{end}";
    }
}
