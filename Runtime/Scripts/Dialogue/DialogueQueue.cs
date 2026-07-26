using System.Collections.Generic;
using System.Linq;

namespace BranchMaker.Dialogue
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
                if (!StoryEventManager.ValidBlockCheck(block)) continue;
                _blockQueue.Add(block);
            }
        }
    }
}
