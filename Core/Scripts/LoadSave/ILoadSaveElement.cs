using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoadSaveElement
{
    public void WriteData(BranchMakerCloudSave saveFile);
    public void ReadData(BranchMakerCloudSave saveFile);
}
