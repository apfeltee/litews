/*
* class HttpListenerRequest
*/

// properties -- sorted by usefulness
public string UserAgent
public string UserHostAddress
public string UserHostName
public string HttpMethod
public CookieCollection Cookies
public string RawUrl
public string[] AcceptTypes
public Uri Url
public Uri UrlReferrer
public NameValueCollection QueryString
public bool IsAuthenticated
public bool IsLocal
public bool IsSecureConnection
public bool IsWebSocketRequest
public NameValueCollection Headers
// -- perhaps internals or something ... ?
public unsafe Guid RequestTraceIdentifier
public Encoding ContentEncoding
public long ContentLength64
public string ContentType
public Stream InputStream
public string ServiceName
public string[] UserLanguages
public int ClientCertificateError
public TransportContext TransportContext
public Version ProtocolVersion
public bool HasEntityBody
public bool KeepAlive
public IPEndPoint RemoteEndPoint
public IPEndPoint LocalEndPoint


// methods
public X509Certificate2 GetClientCertificate()
public IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state)
public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult)
public Task<X509Certificate2> GetClientCertificateAsync()
