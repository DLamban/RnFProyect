using System;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Messaging.WebPubSub;
using Azure.Messaging.WebPubSub.Clients;
using Core.List;
using Core.Networking;
using Core.Units;
using Server;
using Server.config;
using ServerSocket;


class Program
{
    // server string
    private static string connectionStringServer = AppSettings.endpointAzureWebPubSub;


    // client string, does not matter if it's in the github, it's temporal
    private static string connectionStringClient = "wss://rnfsocket.webpubsub.azure.com/client/hubs/rnfhub?access_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJ3c3M6Ly9ybmZzb2NrZXQud2VicHVic3ViLmF6dXJlLmNvbS9jbGllbnQvaHVicy9ybmZodWIiLCJpYXQiOjE3Mjc0MTY1MjcsImV4cCI6MTcyNzUwMDUyNywicm9sZSI6WyJ3ZWJwdWJzdWIuc2VuZFRvR3JvdXAiLCJ3ZWJwdWJzdWIuam9pbkxlYXZlR3JvdXAiXSwic3ViIjoic2VydmVyX3Jvb20xIn0.WuXkXRRgLMqIVyTWzK-0wI8HHfULWGDfvXeGkiA8070";
    private static string hubName = "RnFHub";
    private static string lobbyNameTest = "room1";
    //private static string groupClientDefaultName = "room1"; // DEV ONLY PHASE , CHANGE FOR DINAMIC GROUPS
    //private static string groupServerDefaultName = "serverRoom1"; // DEV ONLY PHASE , CHANGE FOR DINAMIC GROUPS



    static async Task Main(string[] args)
    {        
        MockList MockList = new MockList(1);
        // We create the service client and the client
        // so we can use both worlds
        // arquitecture is in a room or group, is server, player1 and player2
        // server is the one who can send messages to all the players
        
        
        
        var serviceClient = new WebPubSubServiceClient(connectionStringServer, hubName);
        var client = new WebPubSubClient(new Uri(connectionStringClient));
        Console.WriteLine("Servidor de juego iniciado...");
        ServerInstance serverInstance = new ServerInstance(serviceClient, client, lobbyNameTest);       
       
        // ????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
        //ServerEventsHandler serverEventsHandler = new ServerEventsHandler(serviceClient);
       






        while (true)
        {
            Console.WriteLine("Opciones:");
            Console.WriteLine("1. Publicar mensaje a todos los jugadores de room1");
            Console.WriteLine("2. Generar URL de conexión para un nuevo jugador");
            Console.WriteLine("3. Enviar mensaje a un jugador en binario");
            Console.WriteLine("4. Enviar lista de unidades del jugador");
            Console.WriteLine("Selecciona una opción: ");
            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    // Publicar mensaje a todos los jugadores
                    Console.Write("Escribe el mensaje para los jugadores: ");
                    string message = Console.ReadLine();
                    await serviceClient.SendToGroupAsync("room1", message);
                    Console.WriteLine("Mensaje publicado a todos los jugadores de room1.");
                    break;

                case "2":
                    // Generar una URL de conexión para un jugador
                    var url = serviceClient.GetClientAccessUri();
                    Console.WriteLine($"URL de conexión generada: {url}");
                    break;
                case "3":
                    //Vector3 vector3 = new Vector3(1.0f, 2.3f, 3.1f);
                    //Guid guid = Guid.NewGuid();
                    //// Enviar un mensaje binario a un jugador con guid y eso
                    //guid.ToByteArray();

                    //// Serializar el Vector3
                    //using (var stream = new MemoryStream())
                    //{

                    //    using (var writer = new BinaryWriter(stream))
                        
                    //    {
                    //        byte msgtype = 1;
                    //        writer.Write(msgtype); // Tipo de mensaje
                    //        writer.Write(guid.ToByteArray());

                    //        writer.Write(vector3.X);
                    //        writer.Write(vector3.Y);
                    //        writer.Write(vector3.Z);
                    //    }

                    //    // Enviar los datos serializados
                    //    var binaryData = new BinaryData(stream.ToArray());

                    //    await serviceClient.SendToUserAsync("player1", binaryData, ContentType.ApplicationOctetStream);
                    //}
                    break;
                case "4":
                    List<MinimumUnitTransferInfo> player1units = MockList.unitManagerCore.getPlayer1TransUnits();
                    List<MinimumUnitTransferInfo> player2units = MockList.unitManagerCore.getPlayer2TransUnits();
                    var objectToSend = new
                    {                        
                        player1_units = player1units,
                        player2_units = player2units
                    };
                    string json = JsonSerializer.Serialize(objectToSend);
                    await serviceClient.SendToGroupAsync(lobbyNameTest, json,ContentType.ApplicationJson);
                    break;
                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }

            Console.WriteLine(); // Espaciado para la siguiente operación

        }
    }
}