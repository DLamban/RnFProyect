using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Aranfee;
using Core.Networking.config;
using Core.Units;
using Google.Protobuf;
using Nakama;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Core.Networking
{
    public static class MatchOpCodes
    {
        public const long ServerInit = 1;
        public const long UnitPosition = 2;      
    }    
    public class NakamaService
    {
        public static NakamaService Instance { get; } = new NakamaService();

        private IClient _client;
        private ISession _session;
        public ISocket Socket { get; private set; }
        public IMatch CurrentMatch { get; private set; }

        // actions
        public event Action OnMatchFound;
        public event Action<ServerInit> OnReceiveMatchState;
        public event Action<UnitPosition> OnReceiveUnitPosition;
        private NakamaService() { }
        #region LOGIN_AND_MATCHMAKING
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
        #endregion
        #region RECEIVE_DATA_SERVER
        // We build EVENTS by opCODE
        // this is for initial server data and matching
        private void OnReceivedMatchState(IMatchState state)
        {
            Console.WriteLine($"Received Match State OpCode: {state.OpCode}");
            OpCode opCode = (OpCode)state.OpCode;

            switch (opCode)
            {
                case OpCode.ServerInit:
                    ServerInit data = ServerInit.Parser.ParseFrom(state.State);
                    OnReceiveMatchState?.Invoke(data);
                    break;
                case OpCode.UnitPosition:
                    UnitPosition unitPosition = UnitPosition.Parser.ParseFrom(state.State);
                    OnReceiveUnitPosition?.Invoke(unitPosition);
                    break;

                default:
                    Console.WriteLine($"Unhandled OpCode: {state.OpCode}");
                    break;
            }
        }
        #endregion

        #region SEND_DATA_SERVER
        public async void sendUpdatedUnitPosition(BaseUnit unit)
        {
            var vectordirector = unit.Transform.getVectorDirector();
            var updatedPosition = new UnitPosition
            {
                Guid = unit.UnitGuid.ToString(),
                Position = new Vector2Proto
                {
                    X = (float)unit.Transform.offsetX,
                    Y = (float)unit.Transform.offsetY                    
                },                
                Director = new Vector2Proto
                {
                    X = (float)vectordirector.X,
                    Y = (float)vectordirector.Y
                }
            };
            byte[] data = updatedPosition.ToByteArray();

            await Socket.SendMatchStateAsync(CurrentMatch.Id, (long)OpCode.UnitPosition, data);
        }
        public async void sendEnemyUnits(List<BaseUnit> listUnits)
        {
            //MAP units to protobuf data objects

        }
        #endregion
    }
}
