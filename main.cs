
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;
using LiteWS;

/*
using System;
using System.Net;

class Program
{
    static void Main()
    {
        string a = WebUtility.HtmlEncode("<html><head><title>T</title></head></html>");
        string b = WebUtility.HtmlDecode(a);

        Console.WriteLine("After HtmlEncode: " + a);
        Console.WriteLine("After HtmlDecode: " + b);
    }
}
*/

public partial class FileServer
{
    private string m_rootdir;
    private LiteWS.WebServer m_server;

    /*
    * get the local, relative path (relative to $m_rootdir)
    * for some reason, Path.Combine() ignores the base directory
    * if path starts with a slash...
    * so that needs to be circumvented.
    */
    public string GetPhysicalPath(LiteWS.Client client)
    {
        char first;
        char sec;
        string localpath;
        localpath = client.Request.Url.LocalPath;
        first = localpath[0];
        if(localpath[0] == '/')
        {
            /*
            * check whether or not $.LocalPath is something akin to "c:/blah".
            * reasing being, that Path.Combine() will disregard m_rootdir, if
            * the path is a valid drive spec -- in other words, without this check,
            * anyone could just do something like "http://foo.bar/c:/users/someone/" ...
            * which would be very bad.
            * so, if a drive spec is specified, then do not attempt to parse it,
            * do not combine it, do not collect $200, return null.
            */
            if(localpath.Length >= 2)
            {
                sec = localpath[1];
                if(Utils.IsAlNum(sec))
                {
                    if(localpath.Length >= 3)
                    {
                        if(localpath[2] == ':')
                        {
                            /* this is a strictly bamboozle free zone */
                            return null;
                        }
                    }
                }
            }
            localpath = localpath.Substring(1);
        }
        return Path.Combine(m_rootdir, localpath);
    }


    public string MakeRelativePath(string physpath)
    {
        if(physpath.Length > m_rootdir.Length)
        {
            return physpath.Substring(m_rootdir.Length).Replace('\\', '/');
        }
        return "/";
    }

    public void SendFile(LiteWS.Client client, string path)
    {
        string fmime;
        bool isbin;
        fmime = LiteWS.Client.GuessMime(path);
        isbin = (
            (
                fmime.ToLower().StartsWith("text/") ||
                fmime.ToLower().StartsWith("image/")
            )
            == false
        );
        client.SendFile(HttpStatusCode.OK, path, fmime, isbin);
    }

    void printinfo(Client client)
    {
        var fmtqs = string.Join(", ", client.QueryString.AllKeys.Select((key) =>
        {
            return string.Format("{0}={1}", Utils.QuoteString(key), Utils.QuoteString(client.QueryString[key]));
        }).ToArray());
        Console.WriteLine("** request: method={0}, address={1}, path=[{2}], ua=[{3}], query=[{4}]",
            client.HttpMethod,
            client.RemoteIP,
            client.RequestPath,
            client.UserAgent,
            fmtqs
        );
        /*
        var p = client.Param("__litews.sortmode");
        Console.WriteLine("param(__litews.sortmode)={0}", p == null ? "<null>" : p);
        */
    }
    
    public FileServer(string rootdir)
    {
        m_rootdir = rootdir;
        m_server = new LiteWS.WebServer(rootdir, 20);
        Directory.SetCurrentDirectory(m_rootdir);
        Environment.CurrentDirectory = m_rootdir;
        Console.CancelKeyPress += delegate
        {
            Console.WriteLine("Received ^C, stopping");
            m_server.Stop();
        };

        m_server.OnMatch(@"/(index|default)\.(html?|aspx?)$", true, (client, m) =>
        {
            Console.WriteLine("index matched! m={0}", m);
            client.SendFinalResponse(HttpStatusCode.OK, "text/plain", "No index to be had. sorry :-(\n");
        });

        m_server.OnMatch((client) =>
        {
            string met;
            string physpath;
            string literalpath;
            FileAttributes fattr;
            met = client.HttpMethod;
            printinfo(client);
            if((met == "GET") || (met == "POST") || (met == "HEAD"))
            {
                literalpath = client.Request.Url.LocalPath;
                physpath = GetPhysicalPath(client);
                //Console.WriteLine("literalpath=\"{0}\", physpath=\"{1}\"", literalpath, physpath);
                if((physpath != null) && (Directory.Exists(physpath) || File.Exists(physpath)))
                {
                    try
                    {
                        fattr = File.GetAttributes(physpath);
                        if(fattr.HasFlag(FileAttributes.Directory))
                        {
                            if(client.HttpMethod == "GET")
                            {
                                //Console.WriteLine("Listing contents for {0} ...", physpath);
                                var gh = HTMLPageDirIndex(client, physpath);
                                client.SendFinalResponse(HttpStatusCode.OK, "text/html", gh.ToString());
                            }
                            else
                            {
                                client.SendFinalResponse(
                                    HttpStatusCode.BadRequest, "text/plain",
                                    "directory listing only possible via 'GET'\n"
                                );
                            }
                        }
                        else if(File.Exists(physpath))
                        {
                            SendFile(client, physpath);
                        }
                        else
                        {
                            client.SendFinalResponse(
                                HttpStatusCode.NotFound, "text/plain",
                                String.Format("not a file: {0}\n", Utils.QuoteString(literalpath)));
                        }
                    }
                    catch(Exception ex)
                    {
                        //client.SendErrorInternal(ex);
                        Console.WriteLine(ex.ToString());
                        var errbuf = String.Format("exception: ({0}) {1}\n",
                            ex.GetType().Name,
                            Utils.QuoteString(ex.Message));
                        client.SendFinalResponse(HttpStatusCode.InternalServerError, "text/plain", errbuf);
                    }
                }
                else
                {
                    //client.SendErrorNotFound(literalpath);
                    client.SendFinalResponse(
                        HttpStatusCode.NotFound, "text/plain",
                        String.Format("file not found: {0}\n", Utils.QuoteString(literalpath)));
                }
            }
            else
            {
                client.SendFinalResponse(
                    HttpStatusCode.BadRequest, "text/plain",
                    String.Format("unsupported HTTP method '{0}'\n", Utils.QuoteString(client.HttpMethod)));
            }
        });
    }

    public void Start(int port)
    {
        m_server.Start(port);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        {
            CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
        }
        string curdir;
        FileServer hs;
        curdir = Directory.GetCurrentDirectory();
        if(args.Length > 0)
        {
            curdir = args[0];
        }
        hs = new FileServer(curdir);
        hs.Start(8000);
    }
}

