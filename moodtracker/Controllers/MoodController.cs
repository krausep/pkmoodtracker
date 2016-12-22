using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using moodtracker.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using WebGrease.Css.Extensions;

namespace moodtracker.Controllers
{
    public class MoodController : Controller
    {

        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        // GET: Mood
        public ActionResult Index()
        {
            return View("../MoodTracker/Moods");
        }

        public async Task<JsonResult> GetMoods(int top = 30)
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("moodtracker");
            var collection = _database.GetCollection<BsonDocument>("moods");
            var moods = new List<MoodItem>();
            using (var cursor = await collection.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        moods.Add(BsonSerializer.Deserialize<MoodItem>(document));
                    }
                }
            }

            moods = moods.OrderByDescending(m => m.CreatedDate).Take(top).ToList();
            return Json(moods, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetMoodScales()
        {
            var moods = await GetAllEntries();


            moods = moods.OrderBy(m => m.CreatedDate).Skip(Math.Max(0, moods.Count - 30)).ToList();
            return Json(moods.Select(m => new { m.CreatedDate, m.Scale}), JsonRequestBehavior.AllowGet);
        }


        public async Task<JsonResult> GetMoodAveragesByDayOfWeek()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("moodtracker");
            var moods = await GetAllEntries();
            var dictionary = new Dictionary<string, MoodAverage>();

            moods = moods.OrderBy(m => m.CreatedDate.DayOfWeek).ToList();
            foreach (var mood in moods)
            {
                if (!dictionary.ContainsKey(mood.CreatedDate.DayOfWeek.ToString()))
                {
                    dictionary.Add(mood.CreatedDate.DayOfWeek.ToString(), new MoodAverage { DayOfWeek = mood.CreatedDate.DayOfWeek, TotalScale = mood.Scale, Count = 1});
                }
                else
                {
                    var entry = dictionary[mood.CreatedDate.DayOfWeek.ToString()];
                    entry.Count++;
                    entry.TotalScale += mood.Scale;
                }
            }

            dictionary.Keys.ForEach(dkey => dictionary[dkey].AverageScale = dictionary[dkey].TotalScale / dictionary[dkey].Count);

            return Json(dictionary.Keys.Select(d => new {DayOfWeek = d, Average = dictionary[d].AverageScale}), JsonRequestBehavior.AllowGet);

        }

        private static async Task<List<MoodItem>> GetAllEntries()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("moodtracker");
            var collection = _database.GetCollection<BsonDocument>("moods");
            var moods = new List<MoodItem>();
            using (var cursor = await collection.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        moods.Add(BsonSerializer.Deserialize<MoodItem>(document));
                    }
                }
            }
            return moods;
        }


        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> AddMood([FromBody] MoodItem moodItem)
        {
            try
            {
                _client = new MongoClient();
                _database = _client.GetDatabase("moodtracker");
                if (moodItem.MoodId == Guid.Empty) moodItem.MoodId = Guid.NewGuid();
                var collection = _database.GetCollection<BsonDocument>("moods");
                var document = new BsonDocument {
                    { "MoodId", moodItem.MoodId },
                    { "Mood", moodItem.Mood},
                    { "Notes", moodItem.Notes },
                    { "Scale", moodItem.Scale },
                    { "CreatedDate", DateTime.UtcNow },
                    { "ModifiedDate", DateTime.UtcNow }
                };
                await collection.InsertOneAsync(document);
                return Json(moodItem);
            }
            catch (Exception e)
            {

                return Json(new {_result = "REQUEST FAILED", _error = e.Message});
            }
        }

        [System.Web.Mvc.HttpDelete]
        public async Task<JsonResult> DeleteMood(Guid moodId)
        {
            try
            {
                _client = new MongoClient();
                _database = _client.GetDatabase("moodtracker");
                var collection = _database.GetCollection<BsonDocument>("moods");
                var filter = Builders<BsonDocument>.Filter.Eq("MoodId", moodId);
                var result = await collection.FindAsync(filter);
                if (result == null)
                {
                    return Json(new {_result = "NOT FOUND"});
                }
                {
                    var deleteResult = await collection.DeleteOneAsync(filter);
                    return Json(moodId);
                }
            }
            catch (Exception e)
            {
                return Json(new {_result = "BAD REQUEST", _message = e.Message});
            }
        }
    }
}