/*
* class HttpListenerContext
*/

// properties
public HttpListenerRequest Request
public HttpListenerResponse Response
public IPrincipal User

// functions
public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol)
public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, TimeSpan keepAliveInterval)
public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval)
public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval, ArraySegment<byte> internalBuffer)
