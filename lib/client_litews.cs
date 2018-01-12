
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using LiteWS;

namespace LiteWS
{
    public partial class Client
    {
        private string m_rootdir;
        protected HttpListenerContext m_context;
        protected HttpListenerRequest m_request;

        public HttpListenerContext Context {
            get{ return m_context; }
        }

        public HttpListenerRequest Request {
            get{ return m_request; }
        }

        public HttpListenerResponse Response {
            get{ return m_context.Response; }
        }

        public string RequestPath {
            get{ return m_request.Url.LocalPath; }
        }

        public IPEndPoint RemoteIP {
            get{ return m_request.RemoteEndPoint; }
        }

        public NameValueCollection QueryString {
            get{ return m_request.QueryString; }
        }

        public string HttpMethod {
            get{ return m_request.HttpMethod; }
        }

        public bool IsLocal {
            get{ return m_request.IsLocal; }
        }

        public bool IsHEAD {
            get{ return (this.HttpMethod == "HEAD"); }
        }

        public bool IsHeaderSent {
            get{ return (this.Response.Headers.Count > 0); }
        }

        public string UserAgent {
            get{ return m_request.UserAgent; }
        }

        public Uri UrlReferrer {
            get{ return m_request.UrlReferrer; }
        }

        public Client(string rootdir, HttpListenerContext ctx)
        {
            m_rootdir = rootdir;
            m_context = ctx;
            m_request = m_context.Request;
        }

        public void AddHeader(string name, string value)
        {
            this.Response.AddHeader(name, value);
        }

        public string Param(string key)
        {
            return this.QueryString.Get(key);
        }

        public string[] Params(string key)
        {
            return this.QueryString.GetValues(key);
        }

        public void Write(byte[] data, int offset, int length)
        {
            this.Response.OutputStream.Write(data, offset, length);
        }

        public void Write(byte[] data, int length)
        {
            this.Write(data, 0, length);
        }

        public void Write(byte[] data)
        {
            this.Write(data, 0, data.Length);
        }

        public void WriteByte(byte data)
        {
            this.Response.OutputStream.WriteByte(data);
        }

        public void Write(string data)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);
            this.Write(bytes, bytes.Length);
        }

        public void BeginResponse(HttpStatusCode httpcode, string mime)
        {
            this.Response.StatusCode = (int)httpcode;
            this.Response.ContentType = mime;
        }

        public void EndResponse()
        {
            m_context.Response.OutputStream.Close();
        }

        public void ResponseDo(HttpStatusCode httpcode, string mime, Action cb)
        {
            BeginResponse(httpcode, mime);
            try
            {
                cb();
            }
            finally
            {
                EndResponse();
            }
        }

        public void SendFinalResponse(HttpStatusCode httpcode, string mime, byte[] data, Int64 length)
        {
            //Console.WriteLine("Client::SendFinalResponse(httpcode={0}, mime=\"{1}\", data=<omitted>, length={2})",
            //    httpcode, mime, length);
            ResponseDo(httpcode, mime, () =>
            {
                this.Response.ContentLength64 = length;
                this.Response.OutputStream.Write(data, 0, data.Length);
            });
        }

        public void SendFinalResponse(HttpStatusCode httpcode, string mime, string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            SendFinalResponse(httpcode, mime, bytes, bytes.Length);
        }

        public void SendFile(HttpStatusCode httpcode, string path, string mime, bool attachment)
        {
            int read;
            string filename;
            byte[] buffer;
            Utils.LogC("Client.SendFile", httpcode, path, mime, attachment);
            //using(FileStream fs = File.OpenRead(path))
            using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                ResponseDo(httpcode, mime, () =>
                {
                    filename = Path.GetFileName(path);
                    this.Response.ContentLength64 = fs.Length;
                    this.Response.SendChunked = false;
                    buffer = new byte[64 * 1024];
                    if(attachment)
                    {
                        //AddHeader("Content-disposition", "attachment; filename=" + filename);
                    }
                    using(var bw = new BinaryWriter(m_context.Response.OutputStream))
                    {
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, read);
                            bw.Flush();
                        }
                        bw.Close();
                    }
                });
            }
        }

        public Match MatchPath(string pattern, bool icase=false)
        {
            int i;
            char ch;
            string path;
            StringBuilder npat;
            Regex rx;
            RegexOptions opts = RegexOptions.CultureInvariant;
            if(pattern[0] != '/')
            {
                throw new FormatException("pattern must start with a '/'");
            }
            npat = new StringBuilder();
            for(i=0; i<pattern.Length; i++)
            {
                ch = pattern[i];
                if(ch == '/')
                {
                    npat.Append('\\').Append('/');
                }
                else
                {
                    npat.Append(ch);
                }
            }
            path = m_request.Url.LocalPath;
            if(icase)
            {
                opts |= RegexOptions.IgnoreCase;
            }
            //Console.WriteLine("Client.PathMatch(npat={0})", Utils.QuoteString(npat.ToString()));
            rx = new Regex(npat.ToString(), opts);
            return rx.Match(path);
        }

        public bool PathMatches(string pat, bool icase)
        {
            return MatchPath(pat, icase).Success;
        }

        public void OnPathMatch(string pat, bool icase, Action<Match> fn)
        {
            Match m;
            if((m = MatchPath(pat, icase)).Success)
            {
                fn(m);
            }
        }

        public void Redirect(HttpStatusCode code, string newUrl)
        {
            this.Response.StatusCode = (int)code;
            this.Response.RedirectLocation = newUrl;
        }

        public void Redirect(string newUrl)
        {
            Redirect(HttpStatusCode.Redirect, newUrl);
        }

        public void RedirectPermanent(string newUrl)
        {
            Redirect(HttpStatusCode.MovedPermanently, newUrl);
        }
    }
}
