
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using LiteWS;


namespace LiteWS
{
    public static class Utils
    {
        //! turns a NameValueCollection into an ordinary dictionary.
        public static IDictionary<string,string> ToDictionary(this NameValueCollection col)
        {
            return col.AllKeys.ToDictionary(x => x, x => col[x]);
        }

        /*!
        * logs a function call and its arguments.
        * for example:
        *
        *  public void Foo(string blah, Something stuff)
        *   {
        *       LogC("Foo", blah, stuff); 
        *   }
        *
        * arguments are ToString()'d, obviously.
        */
        public static void LogC(string fn, params object[] args)
        {
            int i;
            Console.Write("call:{0} (", fn);
            for(i=0; i<args.Length; i++)
            {
                Console.Write("{0}={1}", i, args[i]);
                if((i+1) < args.Length)
                {
                    Console.Write(", ");
                }
            }
            Console.WriteLine(")");
        }

        //! matches characters that are upper/lowercase alphabetical, or numbers
        public static bool IsAlNum(char c)
        {
            return (
                ((c >= 'a') && (c <= 'z')) ||
                ((c >= 'A') && (c <= 'Z')) ||
                ((c >= '0') && (c <= '9'))
            );
        }

        //! matches characters that can be displayed as ascii, and are NOT control characters.
        public static bool IsPrint(char c)
        {
            return ((c >= 32) && (c <= 126));
        }

        //! encases a string in double quotes, and escapes non-printable characters
        public static string QuoteString(string inp)
        {
            int i;
            char c;
            StringBuilder buf;
            buf = new StringBuilder();
            buf.Append('"');
            for(i=0; i<inp.Length; i++)
            {
                c = inp[i];
                if(Utils.IsPrint(c))
                {
                    switch(c)
                    {
                        case '"':
                            buf.Append('\\').Append('"');
                            break;
                        default:
                            buf.Append(c);
                            break;
                    }
                }
                else
                {
                    switch(c)
                    {
                        case '\0':
                            buf.Append('\\').Append('0');
                            break;
                        case '\n':
                            buf.Append('\\').Append('n');
                            break;
                        case '\r':
                            buf.Append('\\').Append('r');
                            break;
                        case '\t':
                            buf.Append('\\').Append('t');
                            break;
                        case '\a':
                            buf.Append('\\').Append('a');
                            break;
                        default:
                            buf.AppendFormat(@"\u{0:x4}", c);
                            break;
                    }
                }
            }
            buf.Append('"');
            return buf.ToString();
        }

        public static String ReadableFilesize(long byteCount, string fmt="{0}{1}")
        {
            string[] suf = {"B", "KB", "MB", "GB", "TB", "PB", "EB"};
            if (byteCount == 0)
            {
                return "0" + suf[0];
            }
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return string.Format(fmt, (Math.Sign(byteCount) * num), suf[place]);
        }

        public static List<DirItem> GetSortedFiles(string dir, string rootdir)
        {
            //Console.WriteLine("GetSortedFiles(\"{0}\")", dir);
            var ret = new List<DirItem>();
            var dirs = Directory.GetDirectories(dir);
            var files = Directory.GetFiles(dir);
            Action<string, bool> addItem = delegate(string path, bool isdir)
            {
                var fullpath = Path.Combine(dir, path);
                try
                {
                    ret.Add(new DirItem(rootdir, dir, fullpath));
                }
                catch(Exception ex)
                {
                    Console.WriteLine("GetSortedFiles: DirItem(\"{0}\"): ({1}) {2}",
                        fullpath, ex.GetType().Name, ex.Message);
                }
            };
            foreach(var d in dirs)
            {
                addItem(d, true);
            }
            foreach(var f in files)
            {
                addItem(f, false);
            }
            return ret;
        }
    }
}
