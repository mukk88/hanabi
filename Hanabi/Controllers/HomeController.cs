using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using System.Configuration;
using System.Web;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Hanabi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //GameData gameData = new GameData(4, "0");
            //GameEntity gameEntity = new GameEntity(gameData);
            //Storage storage = new Storage();
            //storage.insert(gameEntity);
            //ViewBag.gameData = gameData.ToString();
            return View();
        }

        [HttpGet]
        [Route("games")]
        public ContentResult Games()
        {
            Storage storage = new Storage();
            List<GameData> games = storage.getAllGames();
            string jsonGames = JsonConvert.SerializeObject(games);
            return Content(jsonGames, "application/json");
        }

        [HttpGet]
        [Route("game/{gameID}")]
        public ActionResult Game()
        {
            var gameID = (string)Request.RequestContext.RouteData.Values["id"];
            var user = Request.QueryString.Get("user");
            Storage storage = new Storage();
            
            if (user != null && gameID !=null && storage.addUser(gameID, user))
            {
                ViewBag.gameid = gameID;
            }
            return View();
        }

        [HttpGet]
        [Route("data/{gameID}")]
        public ContentResult data()
        {
            var gameID = (string)Request.RequestContext.RouteData.Values["id"];
            Storage storage = new Storage();
            GameData game = storage.getGame(gameID);
            string jsonGame = JsonConvert.SerializeObject(game);
            return Content(jsonGame, "application/json");
        }

        [HttpPost]
        [Route("game")]
        public HttpResponseMessage Create()
        {
            string guid =  Guid.NewGuid().ToString();
            GameData gameData = new GameData(4, guid, "my game");
            GameEntity gameEntity = new GameEntity(gameData);
            Storage storage = new Storage();
            storage.insert(gameEntity);
            var res = new HttpResponseMessage
            {
                Content = new StringContent(guid, Encoding.UTF8, "application/json")
            };
            return res;
        }

        [HttpPost]
        [Route("game/discard")]
        public HttpResponseMessage Discard()
        {
            var form = Request.Form;
            var game_id = form["game_id"];
            var user = form["username"];
            int card_index = Convert.ToInt32(form["card"]);
            Storage storage = new Storage();
            // check if failed 
            GameData gameData = storage.addDiscard(game_id, user, card_index);
            this.Notify(game_id, JsonConvert.SerializeObject(gameData));
            var res = new HttpResponseMessage
            {
                Content = new StringContent(game_id, Encoding.UTF8, "application/json")
            };
            return res;
        }

        [HttpPost]
        [Route("game/play")]
        public HttpResponseMessage Play()
        {
            var form = Request.Form;
            var game_id = form["game_id"];
            var user = form["username"];
            int card_index = Convert.ToInt32(form["card"]);
            Storage storage = new Storage();
            GameData gameData = storage.addPlay(game_id, user, card_index);
            this.Notify(game_id, JsonConvert.SerializeObject(gameData));
            var res = new HttpResponseMessage
            {
                Content = new StringContent(game_id, Encoding.UTF8, "application/json")
            };
            return res;
        }

        [HttpPost]
        [Route("game/clue")]
        public HttpResponseMessage Clue()
        {
            var form = Request.Form;
            var game_id = form["game_id"];
            var user = form["username"];
            int to = Convert.ToInt32(form["to"]);
            int[] indexes = form["indexes"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            bool isnum = Convert.ToBoolean(form["isnum"]);
            Storage storage = new Storage();
            GameData gameData = storage.addClue(game_id, user, to, indexes, isnum);
            this.Notify(game_id, JsonConvert.SerializeObject(gameData));
            return new HttpResponseMessage();
        }

        private void Notify(string gameID, string gameData)
        {
            HanabiHub.notifyGame(gameID, gameData);
        }

    }
}