using System.Collections.Generic;

namespace backend.DTOs
{
    /// <summary>
    /// Represents the result of a redline comparison between two versions.
    /// </summary>
    public class RedlineResultDto
    {
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public List<RedlineFieldChangeDto> Changes { get; set; } = new();
    }
}
