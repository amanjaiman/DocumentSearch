using System;
using System.Collections.Generic;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace DocumentSearchApp
{
    class Program
    {
        private const string MongoEndpoint = "mongodb://localhost:27017";
        static void Main(string[] args)
        {
            var client = new MongoClient(MongoEndpoint);
            var collection = client.GetDatabase("DocumentSearch").GetCollection<BsonDocument>("Document");

            string term = "";
            while (!term.Equals("QUIT"))
            {
                Console.WriteLine("DocumentSearch Demo - Current DB Size: " + collection.EstimatedDocumentCount());
                Console.WriteLine("Enter a search term <QUIT to quit>");
                Console.WriteLine("You can split multiple terms by a comma (,)");
                Console.Write("Search: ");
                term = Console.ReadLine();

                if (term == "QUIT")
                {
                    continue;
                }
                else
                {
                    var results = QueryDatabase(term, collection);
                    int resultsCount = results.Count;

                    Console.WriteLine("Document Count: " + resultsCount);
                    if (resultsCount > 20)
                    {
                        Console.WriteLine("Displaying 20 documents. <ENTER to see more, q to quit>");
                        int curr = 0;
                        int i;
                        string response = "";
                        while (response != "q")
                        {
                            i = curr;
                            while (i < resultsCount && i < curr + 20)
                            {
                                DocumentEntry doc = BsonSerializer.Deserialize<DocumentEntry>(results[i]);
                                Console.WriteLine(doc.Title + ": " + doc.URL);
                                i++;
                            }
                            curr += 20;
                            response = Console.ReadLine();
                        }

                    }
                    else
                    {
                        foreach (BsonDocument result in results)
                        {
                            DocumentEntry doc = BsonSerializer.Deserialize<DocumentEntry>(result);
                            Console.WriteLine(doc.Title + ": " + doc.URL);
                        }
                        Console.ReadLine();
                    }
                }
                Console.Clear();
            }
        }

        public static List<BsonDocument> QueryDatabase(string terms, IMongoCollection<BsonDocument> collection)
        {
            List<FilterDefinition<BsonDocument>> listOfFilters = new List<FilterDefinition<BsonDocument>>();

            string[] search = terms.Split(',');
            foreach (string term in search)
            {
                listOfFilters.Add(Builders<BsonDocument>.Filter.Regex("search_terms", new BsonRegularExpression(term)));
            }

            var filter = Builders<BsonDocument>.Filter.And(listOfFilters);

            var result = collection.Find(filter).ToList();

            return result;
        }
    }
}
