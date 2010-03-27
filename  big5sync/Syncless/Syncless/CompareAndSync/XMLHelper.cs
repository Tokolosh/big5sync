﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.XMLWriteObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync
{
    class XMLHelper
    {

        private const string MetaDir = ".syncless";
        private const string XmlName = "syncless.xml";
        private const string Metadatapath = MetaDir + "\\" + XmlName;
        private const string XpathExpr = "/meta-data";
        private const string NodeName = "name";
        private const string NodeSize = "size";
        private const string NodeHash = "hash";
        private const string NodeLastModified = "last_modified";
        private const string NodeLastCreated = "last_created";
        private const string FOLDER = "folder";
        private const string FILE = "file";
        private const string TodoName = "todo.xml";
        private const string Todopath = MetaDir + "\\" + TodoName;
        private const string Deleted = "Deleted";
        private const string Renamed = "Renamed";
        private const string Action = "action";
        private const string NodeLastUpdated = "last_updated";
        private const string LastKnownState = "last_known_state";
        private static long dateTime = DateTime.Now.Ticks;
        private static readonly object SyncLock = new object();

        #region Main method
        public static void UpdateXML(BaseXMLWriteObject xmlWriteList)
        {
           
            if (xmlWriteList is XMLWriteFolderObject)
            {
                HandleFolder(xmlWriteList);
                return;
            }

            switch (xmlWriteList.ChangeType)
            {
                case MetaChangeType.New:
                     CreateFile((XMLWriteFileObject)xmlWriteList);
                     break;
                case MetaChangeType.Delete:
                     DeleteFile((XMLWriteFileObject)xmlWriteList);
                     break;
                case MetaChangeType.Rename:
                     RenameFile((XMLWriteFileObject)xmlWriteList);
                     break;
                case MetaChangeType.Update:
                     UpdateFile((XMLWriteFileObject)xmlWriteList);
                     break;
            }
        }
        #endregion

        #region File operations
        private static void CreateFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, Metadatapath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            DoFileCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText hashText = xmlDoc.CreateTextNode(xmlWriteObj.Hash);
            XmlText sizeText = xmlDoc.CreateTextNode(xmlWriteObj.Size.ToString());
            XmlText createdTimeText = xmlDoc.CreateTextNode(xmlWriteObj.CreationTime.ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(xmlWriteObj.LastModified.ToString());

            XmlElement nameElement = xmlDoc.CreateElement(NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(NodeHash);
            XmlElement sizeElement = xmlDoc.CreateElement(NodeSize);
            XmlElement createdTimeElement = xmlDoc.CreateElement(NodeLastCreated);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(NodeLastModified);
            XmlElement fileElement = xmlDoc.CreateElement(FILE);

            nameElement.AppendChild(nameText);
            hashElement.AppendChild(hashText);
            sizeElement.AppendChild(sizeText);
            createdTimeElement.AppendChild(createdTimeText);
            lastModifiedElement.AppendChild(lastModifiedText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(sizeElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(createdTimeElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode(XpathExpr);
            rootNode.AppendChild(fileElement);
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFileTodoByName(xmlWriteObj);
        }

        private static void UpdateFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, Metadatapath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" +FILE + "[name=" + CommonMethods.ParseXpathString(xmlWriteObj.Name) + "]");
            if (node == null)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath); 
                CreateFile(xmlWriteObj);
                return;
            }

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];
                if (nodes.Name.Equals(NodeSize))
                {
                    nodes.InnerText = xmlWriteObj.Size.ToString();
                }
                else if (nodes.Name.Equals(NodeHash))
                {
                    nodes.InnerText = xmlWriteObj.Hash;
                }
                else if (nodes.Name.Equals(NodeName))
                {
                    nodes.InnerText = xmlWriteObj.Name;
                }
                else if (nodes.Name.Equals(NodeLastModified))
                {
                    nodes.InnerText = xmlWriteObj.LastModified.ToString();
                }
                else if (nodes.Name.Equals(NodeLastCreated))
                {
                    nodes.InnerText = xmlWriteObj.CreationTime.ToString();
                }
            }

            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFileTodoByName(xmlWriteObj);
        }


        private static void RenameFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode tempNode = null;
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, Metadatapath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE +"[name=" + CommonMethods.ParseXpathString(xmlWriteObj.Name) + "]");
            if (node == null)
                return;
            tempNode = node.Clone();
            node.FirstChild.InnerText = xmlWriteObj.NewName;
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            GenerateFileTodo(xmlWriteObj, tempNode);
        }

        private static void DeleteFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode tempNode = null;
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, Metadatapath);
            if (File.Exists(xmlFilePath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);
                XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE + "[name=" + CommonMethods.ParseXpathString(xmlWriteObj.Name) + "]");
                if (node == null)
                    return;
                tempNode = node.Clone();
                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            }

            GenerateFileTodo(xmlWriteObj , tempNode);
        }

        #endregion

        #region Misc Operations

        private static string GetLastFileIndex(string filePath)
        {
            string[] splitWords = filePath.Split('\\');
            string folderPath = "";
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (i == splitWords.Length - 1)
                    return splitWords[i];
            }

            return folderPath;
        }

        private static void DoFileCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE + "[name=" + CommonMethods.ParseXpathString(name) + "]");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }

        private static void DoFolderCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FOLDER + "[name=" + CommonMethods.ParseXpathString(name) + "]");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }
        #endregion

        #region Folder State processor
        private static void HandleFolder(BaseXMLWriteObject xmlWriteObj)
        {
            switch (xmlWriteObj.ChangeType)
            {
                case MetaChangeType.New:
                    CreateFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
                case MetaChangeType.Rename:
                    RenameFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
                case MetaChangeType.Delete:
                    DeleteFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
            }
        }
        #endregion

        #region Folder Operations
        private static void CreateFolder(XMLWriteFolderObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, Metadatapath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            DoFolderCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlElement nameOfFolder = xmlDoc.CreateElement(NodeName);
            XmlElement folder = xmlDoc.CreateElement(FOLDER);
            nameOfFolder.AppendChild(nameText);
            folder.AppendChild(nameOfFolder);

            XmlNode rootNode = xmlDoc.SelectSingleNode(XpathExpr);
            rootNode.AppendChild(folder);
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFolderTodoByName(xmlWriteObj);
        }

        private static void RenameFolder(XMLWriteFolderObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(xmlWriteObj.FullPath , Metadatapath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);

            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FOLDER + "[name=" + CommonMethods.ParseXpathString(xmlWriteObj.Name) + "]");
            if (node == null)
                return;
            node.FirstChild.InnerText = xmlWriteObj.NewName;
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);

            XmlDocument subFolderXmlDoc = new XmlDocument();
            string subFolder = Path.Combine(xmlWriteObj.FullPath, xmlWriteObj.NewName);
            string subFolderXmlPath = Path.Combine(subFolder, Metadatapath);
            CommonMethods.CreateFileIfNotExist(subFolder);
            CommonMethods.LoadXML(ref subFolderXmlDoc, subFolderXmlPath);

            XmlNode subFolderNode = subFolderXmlDoc.SelectSingleNode(XpathExpr + "/name");
            if (subFolderNode == null)
                return;
            subFolderNode.InnerText = xmlWriteObj.NewName;
            CommonMethods.SaveXML(ref subFolderXmlDoc, subFolderXmlPath);
            GenerateFolderTodo(xmlWriteObj);
        }

        private static void DeleteFolder(XMLWriteFolderObject xmlWriteObj)
        {
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, Metadatapath);
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xmlFilePath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);
                XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FOLDER + "[name=" + CommonMethods.ParseXpathString(xmlWriteObj.Name) + "]");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            }

            GenerateFolderTodo(xmlWriteObj);
        }
        #endregion

        #region Todo Operations

        private static void CreateTodoFile(string path)
        {
            string todoXML = Path.Combine(path, Todopath);
            if (File.Exists(todoXML))
                return;
            DirectoryInfo di = Directory.CreateDirectory(Path.Combine(path, MetaDir));
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            XmlTextWriter writer = new XmlTextWriter(todoXML, null);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement(LastKnownState);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        private static void GenerateFileTodo(XMLWriteFileObject xmlWriteObj , XmlNode deletedNode)
        {
            if (deletedNode == null)
                return;
            string fullPath = xmlWriteObj.FullPath;
            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(fullPath , Todopath);
            CreateTodoFile(fullPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFileTodo(xmlTodoDoc, xmlWriteObj, Deleted , deletedNode);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }
        
        private static void GenerateFolderTodo(XMLWriteFolderObject xmlWriteObj)
        {
            string parentPath = xmlWriteObj.FullPath;
            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(parentPath, Todopath);
            CreateTodoFile(parentPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFolderTodo(xmlTodoDoc, xmlWriteObj, Deleted);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }

        private static void AppendActionFileTodo(XmlDocument xmlDoc,XMLWriteFileObject xmlWriteObj,string changeType , XmlNode node)
        {
            string hash = string.Empty;
            string lastModified = string.Empty;
            XmlNodeList nodeList = node.ChildNodes;
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNode childNode = nodeList[i];
                switch (childNode.Name)
                {
                    case NodeHash:
                        hash = childNode.InnerText;
                        break;
                    case NodeLastModified:
                        lastModified = childNode.InnerText;
                        break;
                }
            }

            XmlText hashText = xmlDoc.CreateTextNode(hash);
            XmlText actionText = xmlDoc.CreateTextNode(changeType);
            XmlText lastModifiedText = xmlDoc.CreateTextNode(lastModified);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(dateTime.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(FILE);
            XmlElement nameElement = xmlDoc.CreateElement(NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(NodeHash);
            XmlElement actionElement = xmlDoc.CreateElement(Action);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(NodeLastModified);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(NodeLastUpdated);

            hashElement.AppendChild(hashText);
            actionElement.AppendChild(actionText);
            lastModifiedElement.AppendChild(lastModifiedText);
            lastUpdatedElement.AppendChild(lastUpdatedText);
            nameElement.AppendChild(nameText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(actionElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(lastUpdatedElement);
            
            XmlNode rootNode = xmlDoc.SelectSingleNode("/" + LastKnownState);
            rootNode.AppendChild(fileElement);
        }

        private static void DeleteFileTodoByName(XMLWriteFileObject xmlWriteObj)
        {
            string todoXmlPath = Path.Combine(xmlWriteObj.FullPath, Todopath);
            if (!File.Exists(todoXmlPath))
                return;

            XmlDocument todoXmlDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXmlDoc, todoXmlPath);
            XmlNode fileNode = todoXmlDoc.SelectSingleNode("/" + LastKnownState + "/" +FILE + "[name=" + CommonMethods.ParseXpathString(xmlWriteObj.Name) + "]");
            if (fileNode != null)
                fileNode.ParentNode.RemoveChild(fileNode);
            CommonMethods.SaveXML(ref todoXmlDoc, todoXmlPath);
        }

        private static void AppendActionFolderTodo(XmlDocument xmlDoc, XMLWriteFolderObject folder , string changeType)
        {
            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlText action = xmlDoc.CreateTextNode(changeType);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(dateTime.ToString());

            XmlElement folderElement = xmlDoc.CreateElement(FOLDER);
            XmlElement nameElement = xmlDoc.CreateElement(NodeName);
            XmlElement actionElement = xmlDoc.CreateElement(Action);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(NodeLastUpdated);

            nameElement.AppendChild(nameText);
            actionElement.AppendChild(action);
            lastUpdatedElement.AppendChild(lastUpdatedText);

            folderElement.AppendChild(nameElement);
            folderElement.AppendChild(actionElement);
            folderElement.AppendChild(lastUpdatedElement);
            XmlNode rootNode = xmlDoc.SelectSingleNode("/" + LastKnownState);
            rootNode.AppendChild(folderElement);
        }

        private static void DeleteFolderTodoByName(XMLWriteFolderObject xmlWriteObj)
        {
            string todoXmlPath = Path.Combine(xmlWriteObj.FullPath, Todopath);
            if (!File.Exists(todoXmlPath))
                return;

            XmlDocument todoXmlDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXmlDoc, todoXmlPath);
            XmlNode folderNode = todoXmlDoc.SelectSingleNode("/" + LastKnownState + "/" + FOLDER + "[name=" + CommonMethods.ParseXpathString(xmlWriteObj.Name) + "]");
            if (folderNode != null)
                folderNode.ParentNode.RemoveChild(folderNode);
            CommonMethods.SaveXML(ref todoXmlDoc, todoXmlPath);
        }

        #endregion
    }
}
