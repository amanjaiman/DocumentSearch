using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentSearch
{
    public class DocumentEntry
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("publication")]
        public string Publication { get; set; }

        [BsonElement("author")]
        public string Author { get; set; }

        [BsonElement("date")]
        public string Date { get; set; }

        [BsonElement("year")]
        public string Year { get; set; }

        [BsonElement("month")]
        public string Month { get; set; }

        [BsonElement("url")]
        public string URL { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("search_terms")]
        public string SearchTerms { get; set; }
    }
}
