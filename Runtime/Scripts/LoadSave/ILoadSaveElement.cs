using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.LoadSave
{
    public interface ILoadSaveElement
    {
        public void WriteData(BranchMakerCloudSave saveFile);
        public void ReadData(BranchMakerCloudSave saveFile);
    }
}