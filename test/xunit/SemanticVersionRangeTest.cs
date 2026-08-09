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
using Xunit;

namespace Enbrea.SemVer.XUnit;

/// <summary>
/// Unit tests for <see cref="SemanticVersionRange"/>.
/// </summary>
public sealed class SemanticVersionRangeTest
{
    [Fact]
    public void Equals_And_GetHashCode_Work_For_Equivalent_Ranges()
    {
        var left = SemanticVersionRange.Parse("[1.2.3,2.0.0)");
        var right = new SemanticVersionRange(new SemanticVersion(1, 2, 3), includeMinVersion: true, new SemanticVersion(2, 0, 0), includeMaxVersion: false);
        var different = SemanticVersionRange.Parse("[1.2.3,2.0.0]");

        Assert.True(left.Equals(right));
        Assert.True(left.Equals((object)right));
        Assert.Equal(left.GetHashCode(), right.GetHashCode());

        Assert.False(left.Equals(different));
        Assert.False(left.Equals((object)null));
    }

    [Fact]
    public void HasLowerBound_And_HasUpperBound_Reflect_Boundaries()
    {
        var onlyLower = SemanticVersionRange.Parse("1.2.3");
        var onlyUpper = SemanticVersionRange.Parse("(,2.0.0]");
        var all = SemanticVersionRange.Parse("(,)");

        Assert.True(onlyLower.HasLowerBound);
        Assert.False(onlyLower.HasUpperBound);

        Assert.False(onlyUpper.HasLowerBound);
        Assert.True(onlyUpper.HasUpperBound);

        Assert.False(all.HasLowerBound);
        Assert.False(all.HasUpperBound);
    }

    [Fact]
    public void Parse_Parses_Bounded_Range()
    {
        var range = SemanticVersionRange.Parse("[1.2.3,2.0.0)");

        Assert.Equal(new SemanticVersion(1, 2, 3), range.MinVersion);
        Assert.True(range.IsMinInclusive);
        Assert.Equal(new SemanticVersion(2, 0, 0), range.MaxVersion);
        Assert.False(range.IsMaxInclusive);
    }

    [Fact]
    public void Parse_Parses_Exact_Range()
    {
        var range = SemanticVersionRange.Parse("[1.2.3]");

        Assert.Equal(new SemanticVersion(1, 2, 3), range.MinVersion);
        Assert.Equal(new SemanticVersion(1, 2, 3), range.MaxVersion);
        Assert.True(range.IsMinInclusive);
        Assert.True(range.IsMaxInclusive);
    }

    [Fact]
    public void Parse_Parses_Single_Version_As_Min_Inclusive()
    {
        var range = SemanticVersionRange.Parse("1.2.3");

        Assert.Equal(new SemanticVersion(1, 2, 3), range.MinVersion);
        Assert.Null(range.MaxVersion);
        Assert.True(range.IsMinInclusive);
        Assert.False(range.IsMaxInclusive);
        Assert.Equal("1.2.3", range.ToString());
    }

    [Fact]
    public void Parse_Parses_Unbounded_Ends()
    {
        var onlyUpper = SemanticVersionRange.Parse("(,2.0.0]");
        var onlyLower = SemanticVersionRange.Parse("[1.2.3,)");
        var all = SemanticVersionRange.Parse("(,)");

        Assert.Null(onlyUpper.MinVersion);
        Assert.Equal(new SemanticVersion(2, 0, 0), onlyUpper.MaxVersion);

        Assert.Equal(new SemanticVersion(1, 2, 3), onlyLower.MinVersion);
        Assert.Null(onlyLower.MaxVersion);

        Assert.Null(all.MinVersion);
        Assert.Null(all.MaxVersion);
    }

    [Fact]
    public void Parse_Span_Parses_Bounded_Range()
    {
        var range = SemanticVersionRange.Parse("[1.2.3,2.0.0)".AsSpan());

        Assert.Equal(new SemanticVersion(1, 2, 3), range.MinVersion);
        Assert.True(range.IsMinInclusive);
        Assert.Equal(new SemanticVersion(2, 0, 0), range.MaxVersion);
        Assert.False(range.IsMaxInclusive);
    }

    [Fact]
    public void Parse_Span_Throws_On_Invalid_Input()
    {
        Assert.Throws<FormatException>(() => SemanticVersionRange.Parse("[1.0.0".AsSpan()));
    }

    [Fact]
    public void Satisfies_Evaluates_Bounds_Correctly()
    {
        var range = SemanticVersionRange.Parse("[1.2.3,2.0.0)");

        Assert.True(range.Satisfies(new SemanticVersion(1, 2, 3)));
        Assert.True(range.Satisfies(new SemanticVersion(1, 9, 9)));
        Assert.False(range.Satisfies(new SemanticVersion(2, 0, 0)));
        Assert.False(range.Satisfies(new SemanticVersion(1, 2, 2)));
    }

    [Fact]
    public void ToString_Formats_Canonical_Range()
    {
        var exact = SemanticVersionRange.Parse("[1.2.3]");
        var bounded = SemanticVersionRange.Parse("(1.0.0,2.0.0]");
        var all = SemanticVersionRange.Parse("(,)");

        Assert.Equal("[1.2.3]", exact.ToString());
        Assert.Equal("(1.0.0,2.0.0]", bounded.ToString());
        Assert.Equal("(,)", all.ToString());
    }

    [Fact]
    public void TryParse_Rejects_Invalid_Ranges()
    {
        Assert.False(SemanticVersionRange.TryParse("[1.0.0", out _));
        Assert.False(SemanticVersionRange.TryParse("[2.0.0,1.0.0)", out _));
        Assert.False(SemanticVersionRange.TryParse("[1.0.0,1.0.0)", out _));
        Assert.False(SemanticVersionRange.TryParse("[1.0.0,,2.0.0)", out _));
    }

    [Fact]
    public void TryParse_Span_Parses_Range()
    {
        var success = SemanticVersionRange.TryParse("(,2.0.0]".AsSpan(), out var range);

        Assert.True(success);
        Assert.NotNull(range);
        Assert.Null(range.MinVersion);
        Assert.Equal(new SemanticVersion(2, 0, 0), range.MaxVersion);
        Assert.True(range.IsMaxInclusive);
    }

    [Fact]
    public void TryParse_Span_Rejects_Invalid_Range()
    {
        var success = SemanticVersionRange.TryParse("[2.0.0,1.0.0)".AsSpan(), out var range);

        Assert.False(success);
        Assert.Null(range);
    }
}
