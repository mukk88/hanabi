using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Hanabi
{
    public class Storage
    {
        private CloudTable table;
        
        public Storage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("hanabi");
            table.CreateIfNotExists();
        }

        public void insert(GameEntity game)
        {
            try
            {
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(game);
                table.Execute(insertOrReplaceOperation);
            }
            catch (Exception ex)
            {
                // do a roll back or something
            }
        }

        public GameData getGame(string game_id)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<GameEntity>(game_id, "");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            GameEntity game = (GameEntity)retrievedResult.Result;
            if (game != null){
                return new GameData(game);
            }else{
                return null;
            }
        }

        public List<GameData> getAllGames()
        {
            TableQuery<GameEntity> query = new TableQuery<GameEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, ""));
            List<GameData> games = new List<GameData>();
            foreach (GameEntity game in table.ExecuteQuery(query))
            {
                games.Add(new GameData(game));
            }
            return games;
        }

        public bool addUser(string game_id, string username)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<GameEntity>(game_id, "");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            GameEntity updateEntity = (GameEntity)retrievedResult.Result;
            if (updateEntity != null)
            {
                GameData gameData = updateEntity.gameData();
                if(gameData.users.Contains(username))
                {
                    return true;
                }
                if (!gameData.users.Contains(""))
                {
                    return false;
                }
                var index = gameData.getUsers().IndexOf("");
                gameData.getUsers().RemoveAt(index);
                gameData.getUsers().Insert(index,username);
                gameData.last_move = username + " just joined the game";

                updateEntity = new GameEntity(gameData);
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(updateEntity);
                table.Execute(insertOrReplaceOperation);
                return true;
            }
            return false;
        }

        public GameData addClue(string game_id, string username, int to, int[] indexes, bool isnum)
        {
            // make bool return type
            TableOperation retrieveOperation = TableOperation.Retrieve<GameEntity>(game_id, "");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            GameEntity updateEntity = (GameEntity)retrievedResult.Result;
            if(updateEntity!=null)
            {
                // check if clues == 0, if so move is illegal
                GameData gameData = updateEntity.gameData();
                int index = gameData.getUsers().IndexOf(username); // not needed now
                for (int i = 0; i < indexes.Length; i++)
                {
                    if (isnum)
                    {
                        gameData.getPlayers()[to].getHand()[indexes[i]].numinfo = true;
                    }
                    else
                    {
                        gameData.getPlayers()[to].getHand()[indexes[i]].suitinfo = true;
                    }
                }
                gameData.last_move = username + " gave " + gameData.getUsers()[to] + " a clue";
                gameData.clues--;
                gameData.turn++;
                updateEntity = new GameEntity(gameData);
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(updateEntity);
                table.Execute(insertOrReplaceOperation);
                return gameData;
            }
            return null;
        }

        public GameData addDiscard(string game_id, string username, int card_index)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<GameEntity>(game_id, "");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            GameEntity updateEntity = (GameEntity)retrievedResult.Result;
            // try catch return false
            if (updateEntity != null)
            {
                GameData gameData = updateEntity.gameData();
                int index = gameData.getUsers().IndexOf(username);
                gameData.clues = gameData.clues == 8 ? gameData.clues : gameData.clues + 1;
                CardData discardCard = gameData.getPlayers()[index].getHand()[card_index];
                gameData.getDiscards().Add(discardCard);
                gameData.getPlayers()[index].getHand().RemoveAt(card_index);
                if(gameData.getDeck().Count > 0)
                {
                    CardData card = gameData.getDeck().Pop();
                    gameData.getPlayers()[index].getHand().Add(card);
                }
                gameData.last_move = username + " discarded a card";
                gameData.turn++;
                updateEntity = new GameEntity(gameData);
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(updateEntity);
                table.Execute(insertOrReplaceOperation);
                return gameData;
            }
            return null;
        }

        public GameData addPlay(string game_id, string username, int card_index)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<GameEntity>(game_id, "");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            GameEntity updateEntity = (GameEntity)retrievedResult.Result;
            // try catch return false
            if (updateEntity != null)
            {
                GameData gameData = updateEntity.gameData();
                int index = gameData.getUsers().IndexOf(username);
                bool valid = false;
                CardData playedCard = gameData.getPlayers()[index].getHand()[card_index];
                for (int i = 0; i < gameData.getTable().Count; i++)
                {
                    if(i == playedCard.suit && gameData.getTable()[i] == (playedCard.num-1))
                    {
                        gameData.getTable()[i]++;
                        // add clue if 5
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                {
                    gameData.getDiscards().Add(playedCard);
                    gameData.burns--;
                    // if burns is 0, lose
                }

                gameData.getPlayers()[index].getHand().RemoveAt(card_index);
                if (gameData.getDeck().Count > 0)
                {
                    CardData card = gameData.getDeck().Pop();
                    gameData.getPlayers()[index].getHand().Add(card);
                }
                gameData.turn++;
                gameData.last_move = username + " played a card";

                updateEntity = new GameEntity(gameData);
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(updateEntity);
                table.Execute(insertOrReplaceOperation);
                return gameData;
            }
            return null;
        }

    }
}