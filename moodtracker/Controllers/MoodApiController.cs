using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using moodtracker.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace moodtracker.Controllers
{
    public class MoodApiController : ApiController
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;


        public async Task<IHttpActionResult> Get()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("moodtracker");
            var collection = _database.GetCollection<BsonDocument>("moods");
            var moods = new List<MoodItem>();
            int count = 0;
            using (var cursor = await collection.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        moods.Add(BsonSerializer.Deserialize<MoodItem>(document));
                        count++;
                    }
                }
            }

            return Ok(moods);
        }

        public async Task<IHttpActionResult> Post([FromBody] MoodItem moodItem)
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
                    { "Scale", moodItem.Scale }
                };
                await collection.InsertOneAsync(document);
                return Ok(moodItem);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

        }

        public async Task<IHttpActionResult> Put([FromBody] MoodItem moodItem)
        {
            try
            {
                _client = new MongoClient();
                _database = _client.GetDatabase("moodtracker");
                var collection = _database.GetCollection<BsonDocument>("moods");
                var document = new BsonDocument {
                    { "MoodId", moodItem.MoodId },
                    { "Mood", moodItem.Mood},
                    { "Notes", moodItem.Notes },
                    { "Scale", moodItem.Scale }
                };
                var filter = Builders<BsonDocument>.Filter.Eq("MoodId", moodItem.MoodId);
                var result = await collection.UpdateOneAsync(filter, document);
                if(result.IsAcknowledged) return Ok(moodItem);

                return NotFound();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

        }

        public async Task<IHttpActionResult> Delete(Guid moodId)
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
                    return NotFound();
                }
                {
                    await collection.DeleteOneAsync(filter);
                    return Ok(moodId);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
