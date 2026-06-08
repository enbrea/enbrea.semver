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

using Xunit;

namespace Enbrea.SemVer.XUnit
{
    /// <summary>
    /// Unit tests for <see cref="SemanticVersion"/>.
    /// </summary>
    public class SemanticVersionTest
    {
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
        }

        [Fact]
        public void Compare_PreRelease_Identifiers_Uses_SemVer_Precedence_Rules()
        {
            var alpha1 = SemanticVersion.From("1.0.0-alpha.1");
            var alphaBeta = SemanticVersion.From("1.0.0-alpha.beta");
            var beta = SemanticVersion.From("1.0.0-beta");
            var beta2 = SemanticVersion.From("1.0.0-beta.2");
            var beta11 = SemanticVersion.From("1.0.0-beta.11");
            var rc1 = SemanticVersion.From("1.0.0-rc.1");
            var stable = SemanticVersion.From("1.0.0");

            Assert.True(alpha1 < alphaBeta);
            Assert.True(alphaBeta < beta);
            Assert.True(beta < beta2);
            Assert.True(beta2 < beta11);
            Assert.True(beta11 < rc1);
            Assert.True(rc1 < stable);
        }

        [Fact]
        public void Compare_PreRelease_Length_Affects_Precedence_When_Prefix_Is_Equal()
        {
            var shorter = SemanticVersion.From("1.0.0-alpha");
            var longer = SemanticVersion.From("1.0.0-alpha.1");

            Assert.True(shorter < longer);
        }

        [Fact]
        public void From_Parses_BuildMetadata()
        {
            var version = SemanticVersion.From("1.2.3+build.123");

            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(3, version.Patch);
            Assert.Null(version.PreRelease);
            Assert.Equal("build.123", version.BuildMetadata);
        }

        [Fact]
        public void From_Parses_Full_Version()
        {
            var version = SemanticVersion.From("1.2.3");

            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(3, version.Patch);
            Assert.Null(version.PreRelease);
        }

        [Fact]
        public void From_Parses_Multiple_PreRelease_Identifiers()
        {
            var version = SemanticVersion.From("3.4.5-alpha-extra");

            Assert.Equal("alpha-extra", version.PreRelease);
        }

        [Fact]
        public void From_Parses_Partial_Version_And_Defaults_Missing_Parts()
        {
            var version = SemanticVersion.From("2.5");

            Assert.Equal(2, version.Major);
            Assert.Equal(5, version.Minor);
            Assert.Equal(0, version.Patch);
            Assert.Null(version.PreRelease);
        }

        [Fact]
        public void From_Parses_PreRelease()
        {
            var version = SemanticVersion.From("1.2.3-beta");

            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(3, version.Patch);
            Assert.Equal("beta", version.PreRelease);
            Assert.Null(version.BuildMetadata);
        }

        [Fact]
        public void From_Parses_PreRelease_And_BuildMetadata()
        {
            var version = SemanticVersion.From("1.2.3-beta+build.123");

            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(3, version.Patch);
            Assert.Equal("beta", version.PreRelease);
            Assert.Equal("build.123", version.BuildMetadata);
        }

        [Fact]
        public void ToString_Formats_With_And_Without_PreRelease()
        {
            var stable = new SemanticVersion(1, 0, 0);
            var preRelease = new SemanticVersion(1, 0, 0, "rc1");
            var stableWithBuildMetadata = new SemanticVersion(1, 0, 0, buildMetadata: "build.123");
            var preReleaseWithBuildMetadata = new SemanticVersion(1, 0, 0, "rc1", "build.123");

            Assert.Equal("1.0.0", stable.ToString());
            Assert.Equal("1.0.0-rc1", preRelease.ToString());
            Assert.Equal("1.0.0+build.123", stableWithBuildMetadata.ToString());
            Assert.Equal("1.0.0-rc1+build.123", preReleaseWithBuildMetadata.ToString());
        }
    }
}
