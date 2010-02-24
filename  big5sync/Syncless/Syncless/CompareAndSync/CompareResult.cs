﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public abstract class CompareResult : Result
    {

        //Files: Used for deletions. Folders: Used for deletio
        public CompareResult(FileChangeType changeType, string from) :
            base(changeType, from)
        {
        }
        
        //Files: Used for rename
        public CompareResult(FileChangeType changeType, string from, string to) :
            base(changeType, from, to)
        {
        }

        public override string ToString()
        {
            String s = null;
            switch (ChangeType)
            {
                case FileChangeType.Create:
                    s = "Create:";
                    break;
                case FileChangeType.Delete:
                    s = "Delete:";
                    break;
                case FileChangeType.Rename:
                    s = "Rename:";
                    break;
                case FileChangeType.Update:
                    s = "Update";
                    break;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(s);
            sb.AppendLine("Source: " + base.From);
            sb.AppendLine("Destination: " + base.To);
            //sb.Append(s + " Source: " + base.From + " Destination: " + base.To);
            return sb.ToString();
        }
    }
}
