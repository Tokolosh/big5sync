﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Manual.CompareObject;

namespace SynclessUI.Visitor
{
    public interface IVisitor
    {
        void Visit(FileCompareObject file, int numOfPaths);
        void Visit(FolderCompareObject folder, int numOfPaths);
        void Visit(RootCompareObject root);
    }
}
