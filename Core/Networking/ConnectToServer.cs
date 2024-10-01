﻿using Core.GameLoop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.Networking
{
    public static class ConnectToServer
    {
        public static void Connect(string player)
        {
            // Connect to the server
            string endpointPLayer1 = "wss://rnfsocket.webpubsub.azure.com/client/hubs/rnfhub?access_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJ3c3M6Ly9ybmZzb2NrZXQud2VicHVic3ViLmF6dXJlLmNvbS9jbGllbnQvaHVicy9ybmZodWIiLCJpYXQiOjE3Mjc3NzE3MDksImV4cCI6MTcyNzgzMTcwOSwicm9sZSI6WyJ3ZWJwdWJzdWIuc2VuZFRvR3JvdXAiLCJ3ZWJwdWJzdWIuam9pbkxlYXZlR3JvdXAiXSwic3ViIjoicGxheWVyMSJ9.YSQAA1Glrfgg-07miz2V7jlTT_-mnoiViSGH8G6BleI";
            string endpointPLayer2 = "wss://rnfsocket.webpubsub.azure.com/client/hubs/RnFHub?access_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJ3c3M6Ly9ybmZzb2NrZXQud2VicHVic3ViLmF6dXJlLmNvbS9jbGllbnQvaHVicy9SbkZIdWIiLCJpYXQiOjE3Mjc0MTkzMDQsImV4cCI6MTcyNzUwMzMwNCwicm9sZSI6WyJ3ZWJwdWJzdWIuc2VuZFRvR3JvdXAiLCJ3ZWJwdWJzdWIuam9pbkxlYXZlR3JvdXAiXSwic3ViIjoicm9vbTFfcGxheWVyMiJ9.AfrGPwXH90oxkQShoObGueYqGnIv2s7qHg9n7DH5kOo";

            // DIRTY AND FAST, at least this token expires in 8 hours

            if (player == "player1")
            {
                PlayerInfoSingleton.Instance.setPlayerSpot(PlayerSpotEnum.PLAYER1);
                PlayerInfoSingleton.Instance.connect(endpointPLayer1, "room1_player1", "room1");
            }
            else if (player == "player2")
            {
                PlayerInfoSingleton.Instance.setPlayerSpot(PlayerSpotEnum.PLAYER2);

                PlayerInfoSingleton.Instance.connect(endpointPLayer2, "room1_player2", "room1");
            }
        }
    }
}
