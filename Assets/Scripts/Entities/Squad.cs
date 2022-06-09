using System.Collections.Generic;

namespace Entities
{
    public class Squad
    {
        private List<Unit> Units { get; }
        public bool IsMoved { get; } = false;
        public bool IsOrdered { get; } = false;
        public int LevelIndex { get; } = 0;

        public Squad(List<Unit> squad)
        {
            this.Units = squad;
        }
    }
}