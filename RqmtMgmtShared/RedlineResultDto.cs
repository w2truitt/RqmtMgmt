using System.Collections.Generic;

namespace RqmtMgmtShared
{
    public class RedlineResultDto
    {
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public List<RedlineFieldChangeDto> Changes { get; set; } = new();
    }
}
