using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Hanabi
{
    public class HanabiHub : Hub
    {
        public void JoinGame(string gameID)
        {
            // Call the broadcastMessage method to update clients.
            Groups.Add(Context.ConnectionId, gameID);
        }

        public static void notifyGame(string gameID, string gameData)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<HanabiHub>();
            hubContext.Clients.Group(gameID).update(gameData);
        }
    }
}