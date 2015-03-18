using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Hanabi
{
    public class GameEntity : TableEntity
    {
        public string game_name { get; set; }
        public int num_players {get;set;}
        public string players {get;set;}
        public int turn {get;set;}
        public string deck {get;set;}
        public int clues {get;set;}
        public int burns {get;set;}
        public string table {get;set;}

        public string users { get; set; }

        public string discards { get; set; }

        public string last_move { get; set; }

        public GameEntity() { }
        public GameEntity(GameData game)
        {
            this.PartitionKey = game.game_id;
            this.RowKey = "";
            this.game_name = game.game_name;
            this.num_players = game.num_players;
            this.turn = game.turn;
            this.clues = game.clues;
            this.burns = game.burns;
            this.deck = JsonConvert.SerializeObject(game.getDeck());
            this.players = JsonConvert.SerializeObject(game.getPlayers());
            this.table = JsonConvert.SerializeObject(game.getTable());
            this.users = JsonConvert.SerializeObject(game.getUsers());
            this.discards = JsonConvert.SerializeObject(game.getDiscards());
            this.last_move = game.last_move;
        }

        public GameData gameData()
        {
            return new GameData(this);
        }
    }
}