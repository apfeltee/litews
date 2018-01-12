/*
* class HttpListenerResponse : IDisposable
*/

// properties
public Encoding ContentEncoding
public string ContentType
public Stream OutputStream
public string RedirectLocation
public int StatusCode
public string StatusDescription
public CookieCollection Cookies
public bool SendChunked
public bool KeepAlive
public WebHeaderCollection Headers
public long ContentLength64
public Version ProtocolVersion

// methods
public void CopyFrom(HttpListenerResponse templateResponse)

public void AddHeader(string name, string value)
public void AppendHeader(string name, string value)
public void Redirect(string url)
public void AppendCookie(Cookie cookie)
public void SetCookie(Cookie cookie)
public void Abort()
public void Close(byte[] responseEntity, bool willBlock)
public void Close()
