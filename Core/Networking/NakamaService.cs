using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        // Instancia única para acceder desde Godot (Singleton)
        public static NakamaService Instance { get; } = new NakamaService();

        private IClient _client;
        private ISession _session;
        public ISocket Socket { get; private set; }
        public IMatch CurrentMatch { get; private set; }
        public event Action OnMatchFound;
        private NakamaService() { }

        public async Task LoginUser(string userId)
        {
            if (Socket != null)
            {
                await Socket.CloseAsync();
                Socket = null;
            }
            var config = AppSettings.getNetworkConfig();
            _client = new Client("http", config.host, config.port, config.serverKey);

            try
            {
                _session = await _client.AuthenticateDeviceAsync(userId);

                var nuevoSocket = Nakama.Socket.From(_client);
                await nuevoSocket.ConnectAsync(_session, true);

                Socket = nuevoSocket;

                Console.WriteLine($"Connected, session token: {_session.AuthToken}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"connection error: {e.Message}");
                throw;
            }
        }        
        private IMatchmakerTicket _ticket;

        public async Task FindMatch()
        {
            if (Socket == null) throw new Exception("Socket not connected");

            
            Socket.ReceivedMatchmakerMatched += OnMatchmakerMatched;
                        
            _ticket = await Socket.AddMatchmakerAsync("*", 2, 2);

            Console.WriteLine($"[Matchmaking] Ticket created: {_ticket.Ticket}");
        }
        
        // Update your OnMatchmakerMatched to store the match
        private async void OnMatchmakerMatched(IMatchmakerMatched matched)
        {
            CurrentMatch = await Socket.JoinMatchAsync(matched);
            // subscribe to match state updates
            Socket.ReceivedMatchState += OnReceivedMatchState;
            OnMatchFound?.Invoke();
            Console.WriteLine($"[Network] Match Joined: {CurrentMatch.Id}");

        }

        // Method to send data to the other player
        public async Task SendMatchState(long opCode, string payload)
        {
            if (CurrentMatch == null) return;

            // Convert string to byte array (Nakama sends raw bytes)
            var data = System.Text.Encoding.UTF8.GetBytes(payload);

            await Socket.SendMatchStateAsync(CurrentMatch.Id, opCode, data);
        }

        // Handler for incoming data
        private void OnReceivedMatchState(IMatchState state)
        {
            // In authoritative mode, state.Presence might be null 
            // because the message comes from the SERVER (OpCode 0 or custom)

            if (state.OpCode == MatchOpCodes.ServerInit)
            {
                var config = ServerMatchState.Parser.ParseFrom(state.State);

                this.IsHost = false; // "Host" doesn't exist among clients now
                this.PlayerNumber = config.PlayerNumber;
                this.MatchSeed = config.MapSeed;

                Console.WriteLine($"[Server] Assigned as Player {PlayerNumber}");
                OnMatchFound?.Invoke();
            }
        }
    }
}
