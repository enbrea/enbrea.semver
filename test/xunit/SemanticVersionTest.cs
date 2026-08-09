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
/// Unit tests for <see cref="SemanticVersion"/>.
/// </summary>
public sealed class SemanticVersionTest
{
    [Fact]
    public void Build_Metadata_Does_Not_Affect_Precedence_But_Affects_Equality()
    {
        var first = SemanticVersion.Parse("1.0.0+build.1");
        var second = SemanticVersion.Parse("1.0.0+build.2");

        Assert.Equal(0, first.CompareTo(second));
        Assert.True(first.HasSamePrecedence(second));
        Assert.NotEqual(first, second);
        Assert.True(first != second);
        Assert.False(first == second);
    }

    [Fact]
    public void Compare_And_Operators_Work_As_Expected()
    {
        var v1 = new SemanticVersion(1, 2, 3);
        var v2 = new SemanticVersion(1, 2, 4);
        var pre = new SemanticVersion(1, 2, 3, "beta");

        Assert.True(v1 < v2);
        Assert.True(v2 > v1);
        Assert.True(pre < v1);
        Assert.True(v1 >= pre);
        Assert.True(v1 != v2);
        Assert.True(v1 == new SemanticVersion(1, 2, 3));
        Assert.True(v1 <= new SemanticVersion(1, 2, 3));
        Assert.True(v2 >= v1);
    }

    [Fact]
    public void Compare_Equal_Length_Large_Numeric_PreRelease_Identifiers_Numerically()
    {
        var smaller = SemanticVersion.Parse("1.0.0-2147483648");
        var larger = SemanticVersion.Parse("1.0.0-2147483649");

        Assert.True(smaller < larger);
    }

    [Fact]
    public void Compare_Large_Numeric_PreRelease_Identifier_As_Numeric()
    {
        var numeric = SemanticVersion.Parse("1.0.0-2147483648");
        var nonNumeric = SemanticVersion.Parse("1.0.0-alpha");

        Assert.True(numeric < nonNumeric);
    }

    [Fact]
    public void Compare_Large_Numeric_PreRelease_Identifiers_Does_Not_Overflow()
    {
        var smaller = SemanticVersion.Parse("1.0.0-99999999999999999999999999999999999999");
        var larger = SemanticVersion.Parse("1.0.0-100000000000000000000000000000000000000");

        Assert.True(smaller < larger);
    }

    [Fact]
    public void Compare_Major_Minor_And_Patch_In_Order()
    {
        Assert.True(
            SemanticVersion.Parse("1.9.9") <
            SemanticVersion.Parse("2.0.0"));

        Assert.True(
            SemanticVersion.Parse("1.1.9") <
            SemanticVersion.Parse("1.2.0"));

        Assert.True(
            SemanticVersion.Parse("1.2.3") <
            SemanticVersion.Parse("1.2.4"));
    }

    [Fact]
    public void Compare_NonNumeric_PreRelease_Identifiers_Ordinally()
    {
        var uppercase = SemanticVersion.Parse("1.0.0-ALPHA");
        var lowercase = SemanticVersion.Parse("1.0.0-alpha");

        Assert.True(uppercase < lowercase);
    }

    [Fact]
    public void Compare_Numeric_PreRelease_Identifier_Has_Lower_Precedence_Than_NonNumeric()
    {
        var numeric = SemanticVersion.Parse("1.0.0-123");
        var nonNumeric = SemanticVersion.Parse("1.0.0-alpha");

        Assert.True(numeric < nonNumeric);
    }

    [Fact]
    public void Compare_Operators_Handle_Null_Values()
    {
        SemanticVersion nullVersion = null;
        var version = new SemanticVersion(1, 0, 0);

        Assert.Null(nullVersion);
        Assert.True(nullVersion != version);
        Assert.True(nullVersion < version);
        Assert.True(version > nullVersion);
        Assert.True(nullVersion <= version);
        Assert.True(version >= nullVersion);
        Assert.False(version < nullVersion);
        Assert.False(nullVersion > version);
    }

    [Fact]
    public void Compare_PreRelease_Identifiers_Uses_Full_SemVer_Precedence_Sequence()
    {
        var versions = new[]
        {
            SemanticVersion.Parse("1.0.0-alpha"),
            SemanticVersion.Parse("1.0.0-alpha.1"),
            SemanticVersion.Parse("1.0.0-alpha.beta"),
            SemanticVersion.Parse("1.0.0-beta"),
            SemanticVersion.Parse("1.0.0-beta.2"),
            SemanticVersion.Parse("1.0.0-beta.11"),
            SemanticVersion.Parse("1.0.0-rc.1"),
            SemanticVersion.Parse("1.0.0")
        };

        for (var i = 0; i < versions.Length - 1; i++)
        {
            Assert.True(
                versions[i] < versions[i + 1],
                $"Expected {versions[i]} to precede {versions[i + 1]}.");
        }
    }

    [Fact]
    public void Compare_PreRelease_Length_Affects_Precedence_When_Prefix_Is_Equal()
    {
        var shorter = SemanticVersion.Parse("1.0.0-alpha");
        var longer = SemanticVersion.Parse("1.0.0-alpha.1");

        Assert.True(shorter < longer);
    }

    [Fact]
    public void CompareTo_Null_Returns_Positive_Value()
    {
        var version = SemanticVersion.Parse("1.2.3");

        Assert.True(version.CompareTo(null) > 0);
    }

    [Fact]
    public void Comparison_Operators_Use_Precedence_Not_Build_Metadata()
    {
        var first = SemanticVersion.Parse("1.0.0+build.1");
        var second = SemanticVersion.Parse("1.0.0+build.2");

        Assert.True(first <= second);
        Assert.True(first >= second);
        Assert.True(second <= first);
        Assert.True(second >= first);

        Assert.False(first == second);
    }

    [Fact]
    public void Constructor_Accepts_Valid_Values()
    {
        var version = new SemanticVersion(1, 2, 3, "alpha.1", "build.123");

        Assert.Equal(1, version.Major);
        Assert.Equal(2, version.Minor);
        Assert.Equal(3, version.Patch);
        Assert.Equal("alpha.1", version.PreRelease);
        Assert.Equal("build.123", version.BuildMetadata);
        Assert.True(version.IsPrerelease);
        Assert.True(version.HasBuildMetadata);
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("build.")]
    [InlineData(".build")]
    [InlineData("build..123")]
    [InlineData("build_123")]
    [InlineData("build+123")]
    [InlineData("build 123")]
    [InlineData("ä")]
    public void Constructor_Rejects_Invalid_Build_Metadata(string buildMetadata)
    {
        Assert.Throws<ArgumentException>(
            () => new SemanticVersion(1, 2, 3, buildMetadata: buildMetadata));
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("alpha.")]
    [InlineData(".alpha")]
    [InlineData("alpha..beta")]
    [InlineData("alpha_1")]
    [InlineData("alpha+1")]
    [InlineData("alpha 1")]
    [InlineData("01")]
    [InlineData("alpha.01")]
    [InlineData("ä")]
    public void Constructor_Rejects_Invalid_PreRelease(string preRelease)
    {
        Assert.Throws<ArgumentException>(
            () => new SemanticVersion(1, 2, 3, preRelease));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    public void Constructor_Rejects_Negative_Version_Components(int major, int minor, int patch)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SemanticVersion(major, minor, patch));
    }

    [Fact]
    public void Different_PreRelease_Values_Are_Not_Equal()
    {
        var first = SemanticVersion.Parse("1.0.0-alpha");
        var second = SemanticVersion.Parse("1.0.0-beta");

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void Equals_Object_Works_As_Expected()
    {
        var version = SemanticVersion.Parse("1.2.3");

        Assert.True(version.Equals((object)new SemanticVersion(1, 2, 3)));
        Assert.False(version.Equals((object)new SemanticVersion(1, 2, 4)));
        Assert.False(version.Equals((object)"1.2.3"));
        Assert.False(version.Equals(null));
    }

    [Fact]
    public void HasSamePrecedence_Returns_False_For_Different_Precedence_Or_Null()
    {
        var first = SemanticVersion.Parse("1.0.0-alpha");
        var second = SemanticVersion.Parse("1.0.0");

        Assert.False(first.HasSamePrecedence(second));
        Assert.False(first.HasSamePrecedence(null));
    }

    [Fact]
    public void IsValid_String_And_Span_Return_Expected_Values()
    {
        Assert.True(SemanticVersion.IsValid("1.2.3-alpha.1+build.123"));
        Assert.False(SemanticVersion.IsValid("1.2"));

        Assert.True(SemanticVersion.IsValid("2.0.0".AsSpan()));
        Assert.False(SemanticVersion.IsValid("01.0.0".AsSpan()));
    }
    [Theory]
    [InlineData("0.0.0")]
    [InlineData("1.2.3")]
    [InlineData("1.2.3-alpha")]
    [InlineData("1.2.3-alpha.1")]
    [InlineData("1.2.3-alpha-beta")]
    [InlineData("1.2.3+001")]
    [InlineData("1.2.3+build.001")]
    [InlineData("1.2.3-alpha+build")]
    [InlineData("1.2.3-alpha.1+build.001")]
    [InlineData("2147483647.2147483647.2147483647")]
    public void Parse_Accepts_Valid_Versions(string value)
    {
        var version = SemanticVersion.Parse(value);

        Assert.Equal(value, version.ToString());
    }
    [Fact]
    public void Parse_Ignores_Format_Provider()
    {
        var provider = new CustomFormatProvider();
        var version = SemanticVersion.Parse("1.2.3-alpha", provider);

        Assert.Equal("1.2.3-alpha", version.ToString());
    }

    [Fact]
    public void Parse_Implements_IParsable()
    {
        var version = ParseGeneric<SemanticVersion>("1.2.3-alpha.1");

        Assert.Equal("1.2.3-alpha.1", version.ToString());
    }

    [Fact]
    public void Parse_Implements_ISpanParsable()
    {
        var version = ParseSpanGeneric<SemanticVersion>("1.2.3-alpha.1".AsSpan());

        Assert.Equal("1.2.3-alpha.1", version.ToString());
    }

    [Fact]
    public void Parse_Parses_Build_Metadata()
    {
        var version = SemanticVersion.Parse("1.2.3+build.123");

        Assert.Equal(1, version.Major);
        Assert.Equal(2, version.Minor);
        Assert.Equal(3, version.Patch);
        Assert.Null(version.PreRelease);
        Assert.Equal("build.123", version.BuildMetadata);
        Assert.False(version.IsPrerelease);
        Assert.True(version.HasBuildMetadata);
    }

    [Fact]
    public void Parse_Parses_Full_Version()
    {
        var version = SemanticVersion.Parse("1.2.3");

        Assert.Equal(1, version.Major);
        Assert.Equal(2, version.Minor);
        Assert.Equal(3, version.Patch);
        Assert.Null(version.PreRelease);
        Assert.Null(version.BuildMetadata);
        Assert.False(version.IsPrerelease);
        Assert.False(version.HasBuildMetadata);
    }

    [Fact]
    public void Parse_Parses_Multiple_PreRelease_Identifiers()
    {
        var version = SemanticVersion.Parse("3.4.5-alpha.1.beta-2");

        Assert.Equal("alpha.1.beta-2", version.PreRelease);
    }

    [Fact]
    public void Parse_Parses_PreRelease()
    {
        var version = SemanticVersion.Parse("1.2.3-beta");

        Assert.Equal(1, version.Major);
        Assert.Equal(2, version.Minor);
        Assert.Equal(3, version.Patch);
        Assert.Equal("beta", version.PreRelease);
        Assert.Null(version.BuildMetadata);
        Assert.True(version.IsPrerelease);
        Assert.False(version.HasBuildMetadata);
    }

    [Fact]
    public void Parse_Parses_PreRelease_And_Build_Metadata()
    {
        var version = SemanticVersion.Parse("1.2.3-beta.1+build.123");

        Assert.Equal(1, version.Major);
        Assert.Equal(2, version.Minor);
        Assert.Equal(3, version.Patch);
        Assert.Equal("beta.1", version.PreRelease);
        Assert.Equal("build.123", version.BuildMetadata);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("1")]
    [InlineData("1.2")]
    [InlineData("1.2.")]
    [InlineData(".1.2")]
    [InlineData("1..2")]
    [InlineData("1.2.3.4")]
    [InlineData("-1.2.3")]
    [InlineData("1.-2.3")]
    [InlineData("1.2.-3")]
    [InlineData("+1.2.3")]
    [InlineData("01.2.3")]
    [InlineData("1.02.3")]
    [InlineData("1.2.03")]
    [InlineData("2147483648.0.0")]
    [InlineData("0.2147483648.0")]
    [InlineData("0.0.2147483648")]
    [InlineData("1.2.3-")]
    [InlineData("1.2.3+")]
    [InlineData("1.2.3-alpha.")]
    [InlineData("1.2.3-.alpha")]
    [InlineData("1.2.3-alpha..1")]
    [InlineData("1.2.3-alpha.01")]
    [InlineData("1.2.3-01")]
    [InlineData("1.2.3-alpha_1")]
    [InlineData("1.2.3-alpha+")]
    [InlineData("1.2.3+build.")]
    [InlineData("1.2.3+.build")]
    [InlineData("1.2.3+build..1")]
    [InlineData("1.2.3+build+other")]
    [InlineData("1.2.3+build_1")]
    [InlineData("1.2.3 alpha")]
    [InlineData("1.2.3+build metadata")]
    [InlineData("1.2.3-ä")]
    [InlineData("v1.2.3")]
    public void Parse_Rejects_Invalid_Versions(string value)
    {
        Assert.Throws<FormatException>(
            () => SemanticVersion.Parse(value));
    }

    [Fact]
    public void Parse_Span_And_Parse_Span_With_Provider_Work()
    {
        ReadOnlySpan<char> value = "1.2.3-beta.1+build.5".AsSpan();
        var provider = new CustomFormatProvider();

        var versionFromSpan = SemanticVersion.Parse(value);
        var versionFromSpanWithProvider = SemanticVersion.Parse(value, provider);

        Assert.Equal("1.2.3-beta.1+build.5", versionFromSpan.ToString());
        Assert.Equal("1.2.3-beta.1+build.5", versionFromSpanWithProvider.ToString());
    }

    [Fact]
    public void Parse_String_Rejects_Null()
    {
        Assert.Throws<ArgumentNullException>(
            () => SemanticVersion.Parse((string)null));
    }

    [Fact]
    public void ToString_Formats_All_Supported_Combinations()
    {
        var stable = new SemanticVersion(1, 0, 0);
        var preRelease = new SemanticVersion(1, 0, 0, "rc.1");

        var stableWithBuildMetadata = new SemanticVersion(1, 0, 0, buildMetadata: "build.123");
        var preReleaseWithBuildMetadata = new SemanticVersion(1, 0, 0, "rc.1", "build.123");

        Assert.Equal("1.0.0", stable.ToString());
        Assert.Equal("1.0.0-rc.1", preRelease.ToString());
        Assert.Equal("1.0.0+build.123", stableWithBuildMetadata.ToString());
        Assert.Equal("1.0.0-rc.1+build.123", preReleaseWithBuildMetadata.ToString());
    }

    [Fact]
    public void TryParse_Span_Returns_False_For_Invalid_Version()
    {
        ReadOnlySpan<char> value = "1.2".AsSpan();

        var successful = SemanticVersion.TryParse(value, out var version);

        Assert.False(successful);
        Assert.Null(version);
    }

    [Fact]
    public void TryParse_Span_Returns_True_For_Valid_Version()
    {
        ReadOnlySpan<char> value = "1.2.3-alpha.1+build.123".AsSpan();

        var successful = SemanticVersion.TryParse(value, out var version);

        Assert.True(successful);
        Assert.NotNull(version);
        Assert.Equal("1.2.3-alpha.1+build.123", version.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData("1.2")]
    [InlineData("01.2.3")]
    [InlineData("1.2.3-alpha.01")]
    [InlineData("1.2.3+build+other")]
    public void TryParse_String_Returns_False_For_Invalid_Versions(string value)
    {
        var successful = SemanticVersion.TryParse(value, out var version);

        Assert.False(successful);
        Assert.Null(version);
    }

    [Theory]
    [InlineData("1.2.3")]
    [InlineData("1.2.3-alpha")]
    [InlineData("1.2.3+build.123")]
    [InlineData("1.2.3-alpha.1+build.123")]
    public void TryParse_String_Returns_True_For_Valid_Versions(string value)
    {
        var successful = SemanticVersion.TryParse(value, out var version);

        Assert.True(successful);
        Assert.NotNull(version);
        Assert.Equal(value, version.ToString());
    }

    [Fact]
    public void TryParse_With_Provider_Overloads_Work()
    {
        var provider = new CustomFormatProvider();

        var stringSuccess = SemanticVersion.TryParse("1.2.3-alpha", provider, out var fromString);
        var spanSuccess = SemanticVersion.TryParse("1.2.3+build.7".AsSpan(), provider, out var fromSpan);

        Assert.True(stringSuccess);
        Assert.NotNull(fromString);
        Assert.Equal("1.2.3-alpha", fromString.ToString());

        Assert.True(spanSuccess);
        Assert.NotNull(fromSpan);
        Assert.Equal("1.2.3+build.7", fromSpan.ToString());
    }

    [Fact]
    public void WithBuildMetadata_Returns_Same_Instance_When_Unchanged()
    {
        var version = SemanticVersion.Parse("1.2.3+build.7");

        var updated = version.WithBuildMetadata("build.7");

        Assert.Same(version, updated);
    }

    [Fact]
    public void WithBuildMetadata_Updates_And_Preserves_Other_Components()
    {
        var version = SemanticVersion.Parse("1.2.3-alpha.1");

        var updated = version.WithBuildMetadata("build.7");

        Assert.NotSame(version, updated);
        Assert.Equal("1.2.3-alpha.1+build.7", updated.ToString());
    }

    [Fact]
    public void WithMajor_WithMinor_And_WithPatch_Update_Expected_Component()
    {
        var version = SemanticVersion.Parse("1.2.3-alpha+build.9");

        var updatedMajor = version.WithMajor(4);
        var updatedMinor = version.WithMinor(5);
        var updatedPatch = version.WithPatch(6);

        Assert.Equal("4.2.3-alpha+build.9", updatedMajor.ToString());
        Assert.Equal("1.5.3-alpha+build.9", updatedMinor.ToString());
        Assert.Equal("1.2.6-alpha+build.9", updatedPatch.ToString());
    }

    [Fact]
    public void WithMethods_Reject_Invalid_Values()
    {
        var version = SemanticVersion.Parse("1.2.3");

        Assert.Throws<ArgumentOutOfRangeException>(() => version.WithMajor(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => version.WithMinor(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => version.WithPatch(-1));
        Assert.Throws<ArgumentException>(() => version.WithPreRelease("01"));
        Assert.Throws<ArgumentException>(() => version.WithBuildMetadata("build_1"));
    }

    [Fact]
    public void WithMethods_Return_Same_Instance_When_Value_Is_Unchanged()
    {
        var version = SemanticVersion.Parse("1.2.3-alpha+build.9");
        var withoutPreRelease = version.WithoutPreRelease();
        var withoutBuildMetadata = version.WithoutBuildMetadata();

        Assert.Same(version, version.WithMajor(1));
        Assert.Same(version, version.WithMinor(2));
        Assert.Same(version, version.WithPatch(3));
        Assert.Same(version, version.WithPreRelease("alpha"));
        Assert.Same(version, version.WithBuildMetadata("build.9"));
        Assert.Same(withoutPreRelease, withoutPreRelease.WithoutPreRelease());
        Assert.Same(withoutBuildMetadata, withoutBuildMetadata.WithoutBuildMetadata());
    }

    [Fact]
    public void WithPreRelease_Updates_And_WithoutPreRelease_Removes_It()
    {
        var version = SemanticVersion.Parse("1.2.3+build.7");

        var withPreRelease = version.WithPreRelease("beta.1");
        var withoutPreRelease = withPreRelease.WithoutPreRelease();

        Assert.Equal("1.2.3-beta.1+build.7", withPreRelease.ToString());
        Assert.Equal("1.2.3+build.7", withoutPreRelease.ToString());
    }
    
    private static T ParseGeneric<T>(string value)
        where T : IParsable<T>
    {
        return T.Parse(value, provider: null);
    }

    private static T ParseSpanGeneric<T>(ReadOnlySpan<char> value)
        where T : ISpanParsable<T>
    {
        return T.Parse(value, provider: null);
    }

    private sealed class CustomFormatProvider : IFormatProvider
    {
        public object GetFormat(Type formatType)
        {
            return null;
        }
    }
}
