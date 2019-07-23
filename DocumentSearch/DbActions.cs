using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Rest;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

using CsvHelper;
using System.IO;

namespace DocumentSearch
{
    /// <summary>
    /// Allows authentication to the API by using a basic apiKey mechanism
    /// </summary>
    public class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string subscriptionKey;

        /// <summary>
        /// Creates a new instance of the ApiKeyServiceClientCredentails class
        /// </summary>
        /// <param name="subscriptionKey">The subscription key to authenticate and authorize as</param>
        public ApiKeyServiceClientCredentials(string subscriptionKey)
        {
            this.subscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Add the Basic Authentication Header to each outgoing request
        /// </summary>
        /// <param name="request">The outgoing request</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Headers.Add("Ocp-Apim-Subscription-Key", this.subscriptionKey);

            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    public class DbActions
    {
        /* Requirements set by Microsoft Cognitive Services */
        private const int CharacterLimit = 5000;
        private const int MaxDocuments = 100;

        string SubscriptionKey { get; set; }
        string Endpoint { get; set; }
        string MongoEndpoint { get; set; }
        int SleepTime { get; set; }
        string DataLocation { get; set; }

        public DbActions(string SubscriptionKey, string Endpoint, string MongoEndpoint, int SleepTime, string DataLocation)
        {
            this.SubscriptionKey = SubscriptionKey;
            this.Endpoint = Endpoint;
            this.MongoEndpoint = MongoEndpoint;
            this.SleepTime = SleepTime;
            this.DataLocation = DataLocation;
        }

        public void PopulateDatebase(int current)
        {
            var credentials = new ApiKeyServiceClientCredentials(SubscriptionKey);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = Endpoint
            };

            // Change the console encoding to display non-ASCII characters.
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Extraction(client, current).Wait();
            //Console.ReadLine();
        }

        private async Task Extraction(TextAnalyticsClient client, int current)
        {
            List<MultiLanguageBatchInput> inputDocuments;
            List<DocumentEntry> entryDocs;

            (inputDocuments, entryDocs) = GetData(current);

            int i = 0;

            foreach (MultiLanguageBatchInput input in inputDocuments)
            {
                var entityDocuments = new MultiLanguageBatchInput(new List<MultiLanguageInput>());
                var kpResults = await client.KeyPhrasesAsync(false, input);
                for (int j = 0; j < kpResults.Documents.Count; j++)
                {
                    var document = kpResults.Documents[j];
                    string allPhrases = "";
                    foreach (string keyphrase in document.KeyPhrases)
                    {
                        allPhrases += keyphrase + ",";
                    }
                    entityDocuments.Documents.Add(new MultiLanguageInput("en", (j + 1).ToString(), allPhrases));
                }
                var entityResults = await client.EntitiesAsync(false, entityDocuments);

                for (int j = 0; j < entityResults.Documents.Count; j++)
                {
                    var document = entityResults.Documents[j];
                    foreach (var entity in document.Entities)
                    {
                        entryDocs[i].SearchTerms += (entity.Name) + ",";
                    }
                    i++;
                }
                //System.Threading.Thread.Sleep(SleepTime * 1000);
            }

            Console.WriteLine("Data parsed through MSFT Cognitive Services");
            AddToMongoDB(entryDocs);

            Console.WriteLine("Data entered to MongoDB");
        }

        private async void AddToMongoDB(List<DocumentEntry> docs)
        {
            var client = new MongoClient(MongoEndpoint);
            var collection = client.GetDatabase("DocumentSearch").GetCollection<BsonDocument>("Document");

            foreach (DocumentEntry doc in docs)
            {
                await collection.InsertOneAsync(doc.ToBsonDocument());
            }
        }

        private (List<MultiLanguageBatchInput>, List<DocumentEntry>) GetData(int current)
        {
            /*Console.Write("Dataset Entries: ");
            int num = Int32.Parse(Console.ReadLine());*/

            /*Console.Write("Entry starting point <1 more than last time>: ");
            int starting = Int32.Parse(Console.ReadLine());*/

            List<MultiLanguageBatchInput> inputs = new List<MultiLanguageBatchInput>();
            List<DocumentEntry> entryDocs = new List<DocumentEntry>();

            using (var reader = new StreamReader(DataLocation))
            using (var csv = new CsvReader(reader))
            {
                int i = 1;
                int documentLine = 1;
                csv.Read();
                csv.ReadHeader();
                var docs = new MultiLanguageBatchInput(new List<MultiLanguageInput>());
                while (csv.Read() && i <= MaxDocuments) // change back to num
                {
                    Console.WriteLine("Reading csv file... ");
                    Console.ReadLine();
                    if (documentLine < current)
                    {
                    }
                    else
                    {
                        DocumentEntry d = new DocumentEntry()
                        {
                            Title = csv.GetField("title"),
                            Publication = csv.GetField("publication"),
                            Author = csv.GetField("author"),
                            Date = csv.GetField("date"),
                            Year = csv.GetField("year"),
                            Month = csv.GetField("month"),
                            URL = csv.GetField("url"),
                            Content = csv.GetField("content")
                        };

                        if (d.Content.Length > CharacterLimit)
                        {
                            d.Content = d.Content.Substring(0, CharacterLimit);
                        }


                        var line = new MultiLanguageInput("en", i.ToString(), d.Title + " " + d.Content);
                        docs.Documents.Add(line);

                        entryDocs.Add(d);

                        if (i % MaxDocuments == 0 || i == MaxDocuments) // change back to num
                        {
                            inputs.Add(docs);
                            docs = new MultiLanguageBatchInput(new List<MultiLanguageInput>());
                        }

                        i++;
                    }
                    documentLine++;
                }
            }
            Console.WriteLine("Data read from CSV");
            return (inputs, entryDocs);
        }
    }
}
