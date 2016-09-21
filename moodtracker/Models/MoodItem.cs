using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace moodtracker.Models
{
    [BsonIgnoreExtraElements]
    public class MoodItem
    {
        //[BsonIgnore]
        //public BsonObjectId Id { get; set; }
        public Guid MoodId { get; set; }
        public string Mood { get; set; }
        public string Notes { get; set; }
        public int Scale { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}