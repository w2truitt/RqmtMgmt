using System.Collections.Generic;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for redline comparison results between requirement versions.
    /// Contains the differences and changes between two versions of a requirement.
    /// </summary>
    public class RedlineResultDto
    {
        /// <summary>
        /// Gets or sets the version number of the older requirement version being compared.
        /// </summary>
        public int OldVersion { get; set; }

        /// <summary>
        /// Gets or sets the version number of the newer requirement version being compared.
        /// </summary>
        public int NewVersion { get; set; }

        /// <summary>
        /// Gets or sets the collection of field-level changes between the two versions.
        /// </summary>
        public List<RedlineFieldChangeDto> Changes { get; set; } = new();
    }
}