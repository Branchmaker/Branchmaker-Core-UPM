#if UNITY_EDITOR
namespace BranchMaker.Editor.InstallWizard
{
    public interface IBranchMakerInstallStep
    {
        int Order { get; }
        string Title { get; }
        string Description { get; }

        StepState GetState();
        void Run();

        // Optional: for “Select” button when done.
        UnityEngine.Object GetSelectTarget();
    }

    public enum StepState
    {
        NotDone,
        Done
    }
}
#endif