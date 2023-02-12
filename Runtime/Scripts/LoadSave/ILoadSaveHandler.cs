using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.LoadSave
{
    public interface ILoadSaveHandler
    {
        public void UpdateSaveFile();
        public bool CheckForSaveFile();
        public string Resume();
    }
}
