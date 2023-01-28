namespace BranchMaker
{
    public interface IOptionHandler
    {
        public bool CanHandleBlock(BranchNodeBlock block);
        public void ProcessNode(BranchNode node);

        public void Cleanup();
    }
}
