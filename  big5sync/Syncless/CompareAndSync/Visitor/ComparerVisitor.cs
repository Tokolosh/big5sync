﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompareAndSync.CompareObject;

namespace CompareAndSync.Visitor
{
    public class ComparerVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, int level, string[] currentPaths)
        {
            ProcessFileMetaData(file, level, currentPaths);
            CompareFiles(file, level, currentPaths);
        }

        public void Visit(FolderCompareObject folder, int level, string[] currentPaths)
        {
            //Delete, New, Copy, Rename
            ProcessFolderMetaData(folder, level, currentPaths);
            CompareFolders(folder, level, currentPaths);
        }

        public void Visit(RootCompareObject root)
        {
            // Do nothing
        }

        #endregion

        #region Files

        private void ProcessFileMetaData(FileCompareObject file, int level, string[] currentPaths)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.Exists[i] && !file.MetaExists[i])
                    file.ChangeType[i] = MetaChangeType.New; //Possible rename
                else if (!file.Exists[i] && file.MetaExists[i])
                    file.ChangeType[i] = MetaChangeType.Delete; //Possible rename
                else if (file.Exists[i] && file.MetaExists[i])
                {
                    if (file.Length[i] != file.MetaLength[i] || file.Hash[i] != file.MetaHash[i])
                        file.ChangeType[i] = MetaChangeType.Update;
                    else
                        file.ChangeType[i] = MetaChangeType.NoChange;
                }
                else
                    file.ChangeType[i] = null;
            }
        }

        private void CompareFiles(FileCompareObject file, int level, string[] currentPaths)
        {
            //Delete will only occur if all other changes are NoChange
            List<int> deletePos = new List<int>();

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.ChangeType[i] == MetaChangeType.Delete)
                    deletePos.Add(i);
                else if (file.ChangeType[i] != MetaChangeType.NoChange)
                {
                    deletePos.Clear();
                    break;
                }
            }

            if (deletePos.Count > 0)
            {
                foreach (int i in deletePos)
                    file.Priority[i]++;
                return;
            }

            //Update and create handled in the same way
            //Rename is handled in a weird way, think about it later
            int mostUpdatedPos = 0;
            bool diff = false;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.Exists[i])
                {
                    mostUpdatedPos = i;
                    break;
                }
            }

            for (int i = mostUpdatedPos + 1; i < currentPaths.Length; i++)
            {
                if (!file.Exists[i])
                    continue;

                if (file.Length[mostUpdatedPos] != file.Length[i] || file.Hash[mostUpdatedPos] != file.Hash[i])
                {
                    diff = true;
                    if (file.LastWriteTime[i] > file.LastWriteTime[mostUpdatedPos])
                    {
                        mostUpdatedPos = i;
                    }
                    else if (file.LastWriteTime[i] == file.LastWriteTime[mostUpdatedPos])
                    {
                        //Handle conflicts here?
                    }
                }
            }
            if (diff)
                file.Priority[mostUpdatedPos]++;
        }

        #endregion

        #region Folders

        private void ProcessFolderMetaData(FolderCompareObject folder, int level, string[] currentPaths)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (folder.Exists[i] && !folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.New; //Possible rename
                else if (!folder.Exists[i] && folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.Delete; //Possible rename
                else if (folder.Exists[i] && folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.NoChange;
                else
                    folder.ChangeType[i] = null;
            }
        }

        private void CompareFolders(FolderCompareObject folder, int level, string[] currentPaths)
        {
            //Delete will only occur if all other changes are NoChange
            List<int> deletePos = new List<int>();

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (folder.ChangeType[i] == MetaChangeType.Delete)
                    deletePos.Add(i);
                else if (folder.ChangeType[i] != MetaChangeType.NoChange)
                {
                    deletePos.Clear();
                    break;
                }
            }

        }

        #endregion

    }
}
