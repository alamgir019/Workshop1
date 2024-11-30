using SimpleWS_Server;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
ConfigurationService.ConfigureServices(builder);
//builder.WebHost.UseUrls("http://localhost:6969");
var app = builder.Build();
app.UseWebSockets();
app.UseResponseCaching();
var publicConnections = new List<WebSocket>();
var privateConnections = new ConcurrentDictionary<string, WebSocket>();
var _sockets = new ConcurrentDictionary<string, WebSocket>();
//string AddSocket(WebSocket socket)
//{
//    var connectionId = Guid.NewGuid().ToString(); // Unique ID for each client
//    _sockets.TryAdd(connectionId, socket);
//    return connectionId;
//}

//async Task RemoveSocket(string connectionId)
//{
//    _sockets.TryRemove(connectionId, out WebSocket socket);
//    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();
app.Map("/ws", async context => 
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var curName = context.Request.Query["name"];
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        publicConnections.Add(ws);
        await Broadcast($"{curName} joined the room");
        await Broadcast($"{publicConnections.Count} users connected");
        await ReceiveMessage(ws,
            async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await Broadcast(curName + ": " + message);
                }
                else if (result.MessageType == WebSocketMessageType.Close || ws.State == WebSocketState.Aborted)
                {
                    publicConnections.Remove(ws);
                    await Broadcast($"{curName} left the room");
                    await Broadcast($"{publicConnections.Count} users connected");
                    await ws.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
                }
            });
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

app.Map("/private-ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var sender = context.Request.Query["sender"];
        var receiver = context.Request.Query["receiver"];
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        privateConnections.TryAdd(sender, ws);
        await Broadcast_Private(string.Empty, receiver, $"{sender} joined the room");
        //await Broadcast_Private(ws, $"{privateConnections.Count} users connected");
        await ReceiveMessage(ws,
            async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await Broadcast_Private(sender, receiver, sender + ": " + message);
                }
                else if (result.MessageType == WebSocketMessageType.Close || ws.State == WebSocketState.Aborted)
                {
                    _ = privateConnections.TryRemove(sender, out WebSocket socket);
                    await Broadcast_Private(string.Empty, receiver, $"{sender} left the room");
                    //await Broadcast_Private(ws, $"{privateConnections.Count} users connected");
                    await ws.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
                }
            });
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});
async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
{
    var buffer = new byte[1024 * 4];
    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        handleMessage(result, buffer);
    }
}

async Task Broadcast(string message)
{
    var bytes = Encoding.UTF8.GetBytes(message);
    foreach (var socket in publicConnections)
    {
        if (socket.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}


async Task Broadcast_Private(string sender, string receiver, string message)
{
    var sockets = privateConnections.Where(x => x.Key.Equals(sender, StringComparison.OrdinalIgnoreCase) || x.Key.Equals(receiver, StringComparison.OrdinalIgnoreCase));
    var bytes = Encoding.UTF8.GetBytes(message);
    foreach (var ws in sockets)
    {
        if (ws.Value.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await ws.Value.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
await app.RunAsync();
