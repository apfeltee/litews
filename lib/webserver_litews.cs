
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LiteWS;

/*
...
 private bool disposed = false;

  void Dispose() 
  { 
    Dispose(true); 
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if(!disposed)
    {
      if(disposing)
      {
        // Manual release of managed resources.
      }
      // Release unmanaged resources.
      disposed = true;
    }
  }

  ~MyClass() { Dispose(false); }
...

ws.OnPath("/:!(index\.html)", (LiteWS.Client cl) =>
{
    
})


-------
    public enum HttpStatusCode
    {
        // Informational 1xx
        Continue = 100,
        SwitchingProtocols = 101,

        // Successful 2xx
        OK = 200,
        Created = 201,
        Accepted = 202,
        NonAuthoritativeInformation = 203,
        NoContent = 204,
        ResetContent = 205,
        PartialContent = 206,

        // Redirection 3xx
        MultipleChoices = 300,
        Ambiguous = 300,
        MovedPermanently = 301,
        Moved = 301,
        Found = 302,
        Redirect = 302,
        SeeOther = 303,
        RedirectMethod = 303,
        NotModified = 304,
        UseProxy = 305,
        Unused = 306,
        TemporaryRedirect = 307,
        RedirectKeepVerb = 307,

        // Client Error 4xx
        BadRequest = 400,
        Unauthorized = 401,
        PaymentRequired = 402,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        ProxyAuthenticationRequired = 407,
        RequestTimeout = 408,
        Conflict = 409,
        Gone = 410,
        LengthRequired = 411,
        PreconditionFailed = 412,
        RequestEntityTooLarge = 413,
        RequestUriTooLong = 414,
        UnsupportedMediaType = 415,
        RequestedRangeNotSatisfiable = 416,
        ExpectationFailed = 417,

        UpgradeRequired = 426,

        // Server Error 5xx
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        HttpVersionNotSupported = 505,
    }
*/
namespace LiteWS
{
    public partial class WebServer: IDisposable
    {
        internal struct MatchFnInfo
        {
            public bool IgnoreCase;
            public Action<Client, Match> Callback;

            public MatchFnInfo(bool ic, Action<Client, Match> fn)
            {
                this.IgnoreCase = ic;
                this.Callback = fn;
            }
        }

        public event Action<Client> ProcessRequest;

        private string m_rootdir;
        private HttpListener m_listener;
        private Thread m_listenerThread;
        private Thread[] m_workers;
        private ManualResetEvent m_stop;
        private ManualResetEvent m_ready;
        private Queue<HttpListenerContext> m_queue;
        private Dictionary<string, MatchFnInfo> m_onmatches;
        private Action<Client> m_ondefaultmatch;

        public AuthenticationSchemes AuthenticationSchemes {
            get{ return m_listener.AuthenticationSchemes; }
            set{ m_listener.AuthenticationSchemes = value; }
        }

        private void HandleRequests()
        {
            while(m_listener.IsListening)
            {
                var context = m_listener.BeginGetContext(ContextReady, null);
                if(WaitHandle.WaitAny(new[]{m_stop, context.AsyncWaitHandle}) == 0)
                {
                    return;
                }
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock(m_queue)
                {
                    m_queue.Enqueue(m_listener.EndGetContext(ar));
                    m_ready.Set();
                }
            }
            catch
            {
                return;
            }
        }

        private Client MakeClient(HttpListenerContext context)
        {
            var client = new Client(m_rootdir, context);
            return client;
        }

        private void CloseContext(HttpListenerContext context)
        {
            Console.WriteLine("closing context (endpoint={0})",
                context.Request.RemoteEndPoint);
            context.Response.OutputStream.Dispose();
            context.Response.Close();
        }

        private void InformException(Exception ex, string doingwhat, bool printtb, string fmt="({0}) {1}")
        {
            var msg = string.Format(fmt, ex.GetType(), ex.Message);
            Console.WriteLine("exception during {0}: {1}", doingwhat, msg);
            if(printtb)
            {
                Console.WriteLine("{0}", ex.StackTrace);
            }
        }

        private void SendContextServerError(HttpListenerContext context, Exception e)
        {
            var client = new Client(m_rootdir, context);
            var resp = string.Format("<b>internal server error</b>\n<pre>{0}</pre>\n\n", e);
            try
            {
                if(context.Response.Headers.Count > 0)
                {
                    Console.WriteLine("SendContextServerError: cannot send context, because a response was already sent");
                }
                else
                {
                    client.SendFinalResponse(HttpStatusCode.InternalServerError, "text/html", resp);
                }
            }
            catch(Exception ex)
            {
                InformException(ex, "SendFinalResponse() in SendContextServerError()", true);
            }
        }

        private void Worker()
        {
            bool hadmatch;
            Match m;
            WaitHandle[] wait = new[]{ m_ready, m_stop};
            while(WaitHandle.WaitAny(wait) == 0)
            {
                HttpListenerContext context;
                lock(m_queue)
                {
                    if(m_queue.Count > 0)
                    {
                        context = m_queue.Dequeue();
                    }
                    else
                    {
                        m_ready.Reset();
                        continue;
                    }
                }
                try
                {
                    var client = MakeClient(context);
                    // if any OnMatch is declared ...
                    if(m_onmatches.Count > 0)
                    {
                        // walk through matches ...
                        hadmatch = false;
                        foreach(var kvp in m_onmatches)
                        {
                            // if MatchPath succeeded, then call it, and return,
                            // so to ensure that this is the only response in this context
                            if((m=client.MatchPath(kvp.Key, kvp.Value.IgnoreCase)).Success)
                            {
                                kvp.Value.Callback(client, m);
                                hadmatch = true;
                                return;
                            }
                        }
                        // if none matched, and a default response is defined,
                        // call it. same as above, it will *not* fall through, to
                        // ensure it's the only response in this context.
                        if(hadmatch == false)
                        {
                            if(m_ondefaultmatch != null)
                            {
                                m_ondefaultmatch(client);
                            }
                        }
                    }
                    // otherwise, use ProcessRequest event
                    else
                    {
                        ProcessRequest(client);
                    }
                }
                catch(Exception ex)
                {
                    Console.Write("LiteWS::WebServer/try-catch: Exception({0}) \"{1}\"", ex.GetType().Name, ex.Message);
                    InformException(ex, "ProcessRequest()", true);
                    SendContextServerError(context, ex);
                }
                finally
                {
                    CloseContext(context);
                }
            }
        }

        public WebServer(string rootdir, int maxThreads)
        {
            m_rootdir = rootdir;
            m_workers = new Thread[maxThreads];
            m_queue = new Queue<HttpListenerContext>();
            m_stop = new ManualResetEvent(false);
            m_ready = new ManualResetEvent(false);
            m_listener = new HttpListener();
            m_listenerThread = new Thread(HandleRequests);
            m_onmatches = new Dictionary<string, MatchFnInfo>();
            m_ondefaultmatch = null;
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start(int port, string[] hosts)
        {
            foreach(var h in hosts)
            {
                AddPrefix(h, port);
            }
            Start(port);
        }

        public void Start(int port)
        {
            if(m_listener.Prefixes.Count == 0)
            {
                AddPrefix(null, port);
            }
            Console.WriteLine("listening at port {0}: http://0.0.0.0:{0} ...", port);
            Console.WriteLine("declared prefixes:");
            foreach(var pre in m_listener.Prefixes)
            {
                Console.WriteLine("   \"{0}\"", pre);
            }
            m_listener.Start();
            m_listenerThread.Start();
            Console.WriteLine("initiating {0} workers ...", m_workers.Length);
            for (int i = 0; i < m_workers.Length; i++)
            {
                m_workers[i] = new Thread(Worker);
                m_workers[i].Start();
            }
        }

        public void Stop()
        {
            Console.WriteLine("stopping server (joining {0} workers) ...", m_workers.Length);
            m_stop.Set();
            m_listenerThread.Join();
            foreach(Thread worker in m_workers)
            {
                Console.WriteLine("joining worker {0} ...", worker);
                worker.Join();
            }
            m_listener.Stop();
            Console.WriteLine("done");
        }


        public void AddPrefix(string uripat)
        {
            m_listener.Prefixes.Add(uripat);
        }

        public void AddPrefix(string hostip, int port, bool https=false)
        {
            string uripat = "http";
            if(hostip == null)
            {
                hostip = "+";
            }
            if(https)
            {
                uripat += "s";
            }
            uripat += string.Format("://{0}:{1}/", hostip, port);
            AddPrefix(uripat);
        }

        public void OnMatch(string pat, bool ignorecase, Action<Client, Match> fn)
        {
            m_onmatches.Add(pat, new MatchFnInfo(ignorecase, fn));
        }

        public void OnMatch(Action<Client> fn)
        {
            m_ondefaultmatch = fn;
        }
    }
}


