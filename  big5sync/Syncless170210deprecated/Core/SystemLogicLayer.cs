﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Tagging;
using Syncless.CompareAndSync;
using Syncless.Monitor;
using Syncless.Profiling;
namespace Syncless.Core
{
    public class SystemLogicLayer : IUIControllerInterface,IMonitorControllerInterface
    {
        private static SystemLogicLayer _instance;
        public static SystemLogicLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SystemLogicLayer();
                }
                return _instance;
            }
        }
        private SystemLogicLayer()
        {

        }


        #region IUIControllerInterface Members

        public List<Tag> GetAllTags()
        {
            return null;    
        }

        public List<Tag> GetAllTags(FileInfo file)
        {
            return null;
        }

        public List<Tag> GetAllTags(DirectoryInfo info)
        {
            return null;
        }

        public FileTag CreateFileTag(string tagname)
        {
            return TaggingLayer.Instance.CreateFileTag(tagname);
        }

        public FolderTag CreateFolderTag(string tagname)
        {
            return TaggingLayer.Instance.CreateFolderTag(tagname);
        }

        public FileTag TagFile(string tagname, FileInfo file)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(file.FullName,true);
            return TaggingLayer.Instance.TagFile(path, tagname);
        }

        public FileTag TagFile(FileTag tag, FileInfo file)
        {
            return TagFile(tag.TagName, file);
        }

        public FolderTag TagFolder(string tagname, DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            return TaggingLayer.Instance.TagFolder(path, tagname);
        }

        public FolderTag TagFolder(FolderTag tag, DirectoryInfo folder)
        {
            return TagFolder(tag.TagName, folder);
        }

        public int UntagFile(FileTag tag, FileInfo file)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(file.FullName, true);
            return TaggingLayer.Instance.UntagFile(path, tag.TagName);
        }

        public int UntagFolder(FolderTag tag, DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            return TaggingLayer.Instance.UntagFolder(path, tag.TagName);
        }

        public bool DeleteTag(FolderTag tag)
        {
            FolderTag folderTag =  TaggingLayer.Instance.RemoveFolderTag(tag.TagName);
            if (folderTag != null)
            {
                return true;
            }
            return false;
        }

        public bool DeleteTag(FileTag tag)
        {
            FileTag fileTag = TaggingLayer.Instance.RemoveFileTag(tag.TagName);
            if (fileTag != null)
            {
                return true;
            }
            return false;
        }

        public bool DeleteAllTags()
        {
            return false;
        }

        public bool DeleteAllTags(FileInfo file)
        {
            return false;
        }

        public bool DeleteAllTags(DirectoryInfo folder)
        {
            return false;
        }

        public bool StartManualSync(FileTag tagname)
        {
            //For Safety purpose , get the latest from tagging Layer
            return false;
        }

        public bool StartManualSync(FolderTag tagname)
        {
            return false;
        }

        public bool MonitorTag(FileTag tag, bool mode)
        {
            //may need to return a list of error.
            List<string> pathList = new List<string>();
            foreach (TaggedPath path in tag.PathList)
            {
                pathList.Add(path.Path);
            }
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(pathList);
            if (mode)
            {
                foreach (string path in convertedPath)
                {
                    MonitorLayer.Instance.MonitorPath(path);
                }
            }
            else
            {
                foreach (string path in convertedPath)
                {
                    MonitorLayer.Instance.UnMonitorPath(path);
                }
            }
            return true;
        }

        public bool SetTagBidirectional(FileTag tag)
        {
            return false;
        }

        public bool SetTagBidirectional(FolderTag tag)
        {
            return false;
        }

        public CompareResult PreviewSync(FolderTag tag)
        {
            return null;
        }

        public CompareResult PreviewSync(FileTag tag)
        {
            return null;
        }

        public bool PrepareForTermination()
        {
            return true;
        }

        public bool Terminate()
        {
            return false;
        }

        public bool Initiate()
        {
            return ProfilingLayer.Instance.Init(ProfilingLayer.RELATIVE_PROFILING_ROOT_SAVE_PATH);
        }

        #endregion



        #region IMonitorControllerInterface Members

        public void HandleFileChange(FileChangeEvent fe)
        {
            
        }

        public void HandleFolderChange(FolderChangeEvent fe)
        {
            
        }

        public void HandleDriveChange(DriveChangeEvent dce)
        {
            
        }

        #endregion
    }
}
