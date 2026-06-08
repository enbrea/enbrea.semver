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

namespace Enbrea.SemVer
{
    /// <summary>
    /// Semantic version type, following closely https://semver.org
    /// </summary>
    public class SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>
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
        /// Parses a version string into a <see cref="SemanticVersion"/> instance.
        /// </summary>
        /// <param name="version">String formatted version</param>
        public static SemanticVersion From(string version)
        {
            var versionSpan = version.AsSpan();

            var plusIndex = versionSpan.IndexOf('+');
            var versionWithoutBuildMetadataSpan = plusIndex >= 0 ? versionSpan[..plusIndex] : versionSpan;

            string buildMetadata = null;
            if (plusIndex >= 0)
            {
                buildMetadata = versionSpan[(plusIndex + 1)..].ToString();
            }

            var firstHyphenIndex = versionWithoutBuildMetadataSpan.IndexOf('-');
            var versionNumbersSpan = firstHyphenIndex >= 0 ? versionWithoutBuildMetadataSpan[..firstHyphenIndex] : versionWithoutBuildMetadataSpan;

            string preRelease = null;
            if (firstHyphenIndex >= 0)
            {
                var preReleaseSpan = versionWithoutBuildMetadataSpan[(firstHyphenIndex + 1)..];
                preRelease = preReleaseSpan.ToString();
            }

            var major = 0;
            var minor = 0;
            var patch = 0;

            var firstDotIndex = versionNumbersSpan.IndexOf('.');
            if (firstDotIndex < 0)
            {
                major = int.Parse(versionNumbersSpan);
            }
            else
            {
                major = int.Parse(versionNumbersSpan[..firstDotIndex]);

                var remaining = versionNumbersSpan[(firstDotIndex + 1)..];
                var secondDotIndex = remaining.IndexOf('.');
                if (secondDotIndex < 0)
                {
                    minor = int.Parse(remaining);
                }
                else
                {
                    minor = int.Parse(remaining[..secondDotIndex]);
                    patch = int.Parse(remaining[(secondDotIndex + 1)..]);
                }
            }

            return new SemanticVersion(major, minor, patch, preRelease, buildMetadata);
        }

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
        /// Compares the current version instance with another version and returns an integer that 
        /// indicates whether the current instance precedes, follows, or occurs in the same position 
        /// in the sort order as the other version.
        /// </summary>
        /// <param name="other">A version to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the versions being compared</returns>
        public int CompareTo(SemanticVersion other)
        {
            if (other is null) return 1;

            if (Major != other.Major) return Major.CompareTo(other.Major);
            if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
            if (Patch != other.Patch) return Patch.CompareTo(other.Patch);

            if (string.IsNullOrEmpty(PreRelease) && !string.IsNullOrEmpty(other.PreRelease)) return 1;
            if (!string.IsNullOrEmpty(PreRelease) && string.IsNullOrEmpty(other.PreRelease)) return -1;
            if (string.Equals(PreRelease, other.PreRelease, StringComparison.Ordinal)) return 0;

            return ComparePreRelease(PreRelease, other.PreRelease);
        }

        /// <summary>
        /// Determines whether two versions are equal.
        /// </summary>
        /// <param name="other">The version to compare with the current version.</param>
        /// <returns>True if the specified version is equal to the current version; otherwise, false.</returns>
        public bool Equals(SemanticVersion other)
        {
            if (other is null) return false;
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified Object is equal to the current Object; otherwise, false.</returns>
        public override bool Equals(object obj) => Equals(obj as SemanticVersion);

        /// <summary>
        /// Serves as a hash function for the entity object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, PreRelease);
        }

        /// <summary>
        /// The version as string
        /// </summary>
        /// <returns>String formatted version</returns>
        public override string ToString()
        {
            var version = string.IsNullOrEmpty(PreRelease)
                ? $"{Major}.{Minor}.{Patch}"
                : $"{Major}.{Minor}.{Patch}-{PreRelease}";

            return string.IsNullOrEmpty(BuildMetadata)
                ? version
                : $"{version}+{BuildMetadata}";
        }

        /// <summary>
        /// Compares two pre-release strings according to SemVer precedence rules.
        /// </summary>
        private static int ComparePreRelease(string left, string right)
        {
            var leftIdentifiers = left.Split('.');
            var rightIdentifiers = right.Split('.');
            var maxLength = Math.Max(leftIdentifiers.Length, rightIdentifiers.Length);

            for (var i = 0; i < maxLength; i++)
            {
                if (i >= leftIdentifiers.Length) return -1;
                if (i >= rightIdentifiers.Length) return 1;

                var result = ComparePreReleaseIdentifier(leftIdentifiers[i], rightIdentifiers[i]);
                if (result != 0) return result;
            }

            return 0;
        }

        /// <summary>
        /// Compares two pre-release identifiers according to SemVer precedence rules.
        /// </summary>
        private static int ComparePreReleaseIdentifier(string left, string right)
        {
            var leftIsNumeric = int.TryParse(left, out var leftNumeric);
            var rightIsNumeric = int.TryParse(right, out var rightNumeric);

            if (leftIsNumeric && rightIsNumeric) return leftNumeric.CompareTo(rightNumeric);
            if (leftIsNumeric && !rightIsNumeric) return -1;
            if (!leftIsNumeric && rightIsNumeric) return 1;

            return string.Compare(left, right, StringComparison.Ordinal);
        }
    }
}