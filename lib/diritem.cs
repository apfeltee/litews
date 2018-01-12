
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using LiteWS;

namespace LiteWS
{
    public class DirItem
    {
        public string Name { get; }
        public string FullPath { get; }
        public string RootDir { get; }
        public string ThisDir { get; }
        public FileInfo FInfo { get; }

        public bool IsDir {
            get {
                return (bool)(this.FInfo.Attributes == FileAttributes.Directory);
            }
        }

        public DirItem(string rootdir, string dname, string fpath)
        {
            this.RootDir = rootdir;
            this.ThisDir = dname;
            this.FInfo = new FileInfo(fpath);
            this.Name = this.FInfo.Name;
            this.FullPath = this.FInfo.FullName;
        }

        //! returns the basename of the path. directories have a slash appended.
        public string FormattedName()
        {
            if(this.IsDir)
            {
                return this.Name + "/";
            }
            return this.Name;
        }

        /**
        * return the "proper" item url by turning an absolute path
        * into a relative "path" (actually Uri).
        * previous method was a hacky mess. this works way better, and more reliably.
        */
        public string ItemURL()
        {
            string tmp;
            string full = Path.Combine(this.ThisDir, this.Name);
            // only cut if the full path is longer than the rootdir
            if(full.Length > this.RootDir.Length)
            {
                tmp = full.Substring(this.RootDir.Length);
            }
            else
            {
                tmp = full;
            }
            return tmp.Replace('\\', '/');
        }


        public string ItemShortDescription()
        {
            if(this.IsDir)
            {
                return "[dir]";
            }
            return "[file]";
        }

        public string FormattedSize()
        {
            if(this.IsDir)
            {
                return "-";
            }
            try
            {
                // .Length is a property that may throw an exception.
                // this might very well happen for symbolic links.
                return Utils.ReadableFilesize(this.FInfo.Length);
            }
            catch(Exception ex)
            {
                Console.WriteLine("DirItem::FormattedSize(): ({0}) {1}", ex.GetType().Name, ex.Message);
                return "(unreadable)";
            }
        }

        public string FormattedLastChanged()
        {
            return this.FInfo.LastAccessTime.ToString();
        }
    }
}
