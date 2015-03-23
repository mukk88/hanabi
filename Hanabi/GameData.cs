using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Hanabi
{
    public class GameData
    {
        private static int num_suits = 5;
        private static int num_ones = 3;
        private static int num_fives = 1;
        private static int num_others = 2;

        public string game_id {get;set;}
        public string game_name { get; set; }
        public int num_players {get;set;}
        public List<PlayerData> players = new List<PlayerData>();
        public int turn {get;set;}
        public Stack<CardData> deck;
        public int clues {get;set;}
        public int burns {get;set;}
        // bridge + joker
        public List<int> table = new List<int>();

        public List<CardData> discards = new List<CardData>();
        public List<string> users;

        public string last_move;

        public int last_turn_count;


        public GameData(GameEntity entity)
        {
            this.game_id = entity.PartitionKey;
            this.game_name = entity.game_name;
            this.num_players = entity.num_players;
            this.players = JsonConvert.DeserializeObject <List<PlayerData>>(entity.players);
            this.turn = entity.turn;
            this.deck = JsonConvert.DeserializeObject<Stack<CardData>>(entity.deck);
            this.clues = entity.clues;
            this.burns = entity.burns;
            this.table = JsonConvert.DeserializeObject<List<int>>(entity.table);
            this.users = JsonConvert.DeserializeObject<List<string>>(entity.users);
            this.discards = JsonConvert.DeserializeObject<List<CardData>>(entity.discards);
            this.last_move = entity.last_move;
            this.last_turn_count = entity.last_turn_count;
        }

        public GameData(int num_players, string game_id, string game_name){
            this.game_id = game_id;
            this.game_name = game_name;
            this.num_players = num_players;
            int hand_size = num_players > 3 ? 4 : 5;
            for(int i =0;i<num_players;i++){
                this.players.Add(new PlayerData("player" + i, i, hand_size));
            }
            this.turn = 0;
            this.clues = 8;
            this.burns = 3;
            // add cards to deck 3 1s, 2 2s, 2 3s, 2 4s, 1 5, 5 suits
            List<CardData> tempDeck = new List<CardData>();
            for(int i=0;i<num_suits;i++){
                for (int j = 0; j < num_ones; j++) { tempDeck.Add(new CardData(i, 1)); }
                for (int j = 0; j < num_others; j++) { tempDeck.Add(new CardData(i, 2)); }
                for (int j = 0; j < num_others; j++) { tempDeck.Add(new CardData(i, 3)); }
                for (int j = 0; j < num_others; j++) { tempDeck.Add(new CardData(i, 4)); }
                for (int j = 0; j < num_fives; j++) { tempDeck.Add(new CardData(i, 5)); }
            }
            this.Shuffle(tempDeck);
            deck = new Stack<CardData>(tempDeck);
            for (int i = 0; i < num_players; i++)
            {
                for (int j = 0; j < hand_size; j++)
                {
                    CardData card = deck.Pop();
                    players[i].draw(card);
                }
            }
            for (int i = 0; i < 5; i++)
            {
                table.Add(0);
            }
            users = new List<string>();
            for (int i = 0; i < num_players; i++)
            {
                users.Add("");
            }
            last_move = "";
            last_turn_count = 0;
        }

        public List<PlayerData> getPlayers()
        {
            return players;
        }

        public Stack<CardData> getDeck()
        {
            return deck;
        }

        public List<int> getTable()
        {
            return table;
        }
        public List<string> getUsers()
        {
            return users;
        }

        public List<CardData> getDiscards()
        {
            return discards;
        }

        private void Shuffle(List<CardData> list)  
        {  
            Random rng = new Random();  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                CardData card = list[k];  
                list[k] = list[n];  
                list[n] = card;  
            }  
        }


    }

    public class CardData
    {
        public int suit {get;set;}
        public int num {get;set;}

        public bool suitinfo { get; set; }
        public bool numinfo { get; set; }

        public CardData() { }
        public CardData(int suit, int num)
        {
            this.suit = suit;
            this.num = num;
            this.suitinfo = false;
            this.numinfo = false;
        }
    }

    public class PlayerData
    {
        public int player_index { get; set; }
        public string name {get;set;}
        public int hand_size {get;set;}
        public List<CardData> hand { get; set; }
        public PlayerData(string name, int player_index, int hand_size)
        {
            this.name = name;
            this.player_index = player_index;
            this.hand_size = hand_size;
            hand = new List<CardData>();
        }

        public CardData discard(int card_index)
        {
            // todo implement
            return new CardData(0,1);
        }

        public void draw(CardData card)
        {
            hand.Add(card);
        }

        public List<CardData> getHand()
        {
            return hand;
        }

        public override string ToString()
        {
            String result = String.Format("player index = {0} hand size = {1} name = {2} ", this.player_index, this.hand_size, this.name);
            for (int i = 0; i < hand_size; i++)
            {
                result += String.Format(" CARD suit {0} num {1}", this.hand[i].suit, this.hand[i].num);
            }
            return result;
        }

    }
}