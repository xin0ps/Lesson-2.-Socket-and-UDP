using System.Net.Sockets;
using System.Net;


var ip = IPAddress.Broadcast;
var port = 27001;
var listener = new UdpClient(port); // Bind to the specified port
listener.EnableBroadcast = true; // Enable broadcasting

var broadcastEP = new IPEndPoint(ip, port);
Console.WriteLine($"{broadcastEP} Listener Started...");

while (true)
{
    IPEndPoint? remoteEp = null;

    var maxLen = ushort.MaxValue - 29;
    var len = 0;
    var buffer = new byte[maxLen];
    var list = new List<byte>();

    try
    {
        // Receive data
        var result = listener.Receive(ref remoteEp);
        buffer = result;
        len = buffer.Length;
        list.AddRange(buffer);

        var imgBytes = list.ToArray();
        var chunks = imgBytes?.Chunk(ushort.MaxValue - 29);

        foreach (var chunk in chunks!)
        {
            // Send back the received data
            await listener.SendAsync(chunk, chunk.Length, broadcastEP);
        }

        list.Clear();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}
