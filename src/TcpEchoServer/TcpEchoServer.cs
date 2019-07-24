using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TcpEchoServer
{
    public class TcpEchoServer : BackgroundService
    {
        private readonly ILogger<TcpEchoServer> _logger;
        private readonly IConnectionListenerFactory _factory;

        public TcpEchoServer(ILogger<TcpEchoServer> logger, IConnectionListenerFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = await _factory.BindAsync(new IPEndPoint(IPAddress.Loopback, 5000), stoppingToken);
            // In an actual server you call Accept in a loop
            var connection = await listener.AcceptAsync(stoppingToken);

            while (true)
            {
                var result = await connection.Transport.Input.ReadAsync(stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                {
                    connection.Abort();
                    break;
                }

                if (result.IsCompleted)
                {
                    break;
                }

                await connection.Transport.Output.WriteAsync(result.Buffer.ToArray());
                connection.Transport.Input.AdvanceTo(result.Buffer.End);
                // To demo the echo server I use `nc` under WSL
                // `nc localhost 5000`
            }

        }
    }
}
