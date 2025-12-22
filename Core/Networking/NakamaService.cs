using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aranfee;
using Core.Networking.config;
using Nakama;

namespace Core.Networking
{
    public static class MatchOpCodes
    {
        public const long ServerInit = 1;
        public const long Chat = 2;
        public const long Message = 3;
        public const long UnitMove = 4;
        public const long UnitAttack = 5;
    }
    public class NakamaService
    {
        public static NakamaService Instance { get; } = new NakamaService();

        private IClient _client;
        private ISession _session;
        public ISocket Socket { get; private set; }
        public IMatch CurrentMatch { get; private set; }

        // This event notifies your UI (Home.cs)
        public event Action OnMatchFound;
        public event Action<ServerInit> OnReceiveMatchState;
        private NakamaService() { }

        public async Task LoginUser(string userId)
        {
            var config = AppSettings.getNetworkConfig();
            _client = new Client("http", config.host, config.port, config.serverKey);
            _session = await _client.AuthenticateDeviceAsync(userId);

            Socket = Nakama.Socket.From(_client);

            Socket.ReceivedMatchmakerMatched += OnMatchmakerMatched;
            Socket.ReceivedMatchState += OnReceivedMatchState;
            await Socket.ConnectAsync(_session, true);
            Console.WriteLine("Socket Connected and Events Registered.");
        }
        public async Task FindAuthoritativeMatch()
        {
            await Socket.AddMatchmakerAsync("*", 2, 2);
            Console.WriteLine("Searching for match...");
        }

        private async void OnMatchmakerMatched(IMatchmakerMatched matched)
        {
            Console.WriteLine("Matchmaker found opponents! Joining authoritative match...");

            CurrentMatch = await Socket.JoinMatchAsync(matched);

            // Notify the UI
            OnMatchFound?.Invoke();
        }
        // We build EVENTS by opCODE
        // this is for initial server data and matching
        private void OnReceivedMatchState(IMatchState state)
        {
            Console.WriteLine($"Received Match State OpCode: {state.OpCode}");
            OpCode opCode = (OpCode)state.OpCode;

            if (opCode == OpCode.ServerInit)
            {
                ServerInit data = ServerInit.Parser.ParseFrom(state.State);
                Console.WriteLine($"Server Assigned Player: {data.PlayerNumber}");
                OnReceiveMatchState?.Invoke(data);
            }
        }
    }
}
