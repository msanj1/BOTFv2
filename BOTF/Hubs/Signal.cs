using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
namespace BOTF.Hubs
{
    [HubName("messagesHub")]
    public class Signal:Hub
    {
        public void UpdateVoteHistory(){
            Clients.updateVoters();
        }

        public void UpdateCommentHistory() {
            Clients.updateComments();
        }

        public void UpdateRankings()
        {
            Clients.updateRanks();
        }
    }
}