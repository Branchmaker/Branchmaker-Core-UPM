using System.Collections.Generic;
using System.Linq;

namespace BranchMaker
{
    public class DialogueQueue
    {
        private readonly List<BranchNodeBlock> _blockQueue = new();

        public int Count()
        {
            return _blockQueue.Count;
        }

        public void Clear()
        {
            _blockQueue.Clear();
        }

        public BranchNodeBlock PopFirst()
        {
            if (_blockQueue.Count == 0) return null;
           var current = _blockQueue.First();
           _blockQueue.RemoveAt(0);
           return current;
        }

        public void LoadBlocks(List<BranchNodeBlock> storyBlocks)
        {
            foreach (var block in storyBlocks)
            {
                _blockQueue.Add(block);
            }
        }
    }
}
