# DocumentSearch
DocumentSearch was made as a demo for agencies looking to use [Microsoft Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/) to their advantage. By using the Text Analytics API, we were able to create a small console application that allows users to search through the document database using keywords. The keywords are all generated using Key Phrase Extraction and Entity Recognition. The program is able to take a document and store it into a MongoDB Document Database, along with the corresponding keywords generated using the API.

## Getting Started
These instructions will get you a copy of the project up and running on your local machine to see Microsoft's APIs in action.

### Prerequisites
To use this, you must have MongoDB installed. Create a database called `DocumentSearch` with a collection named `Document`. By default, the MongoDB endpoint should be `mongodb://localhost:27017`, but in case you have changed this, make sure to change it endpoint in `DocumentSearch.cs`.

Once you download the source, you should find the set of private variables in `DocumentSearch.cs`

```
private const string SubscriptionKey = "";
private const string Endpoint = "";
private const string MongoEndpoint = "mongodb://localhost:27017";
private const int SleepTime = 60; // Seconds
private const string DataLocation = "";
```
Here you can point the application to the proper data (`DataLocation`), as well as enter your Azure API key and endpoint. If you do not have the default MongoDB endpoint, be sure to change that here as well.

Currently, the application is configured to use the [All the News](https://www.kaggle.com/snapcrack/all-the-news) dataset. Inside the project, the data extraction is done based on the columns in the particular database. In case you are using a different data source, you can change the data extraction function.

### Building and Running
Once you have provided the correct data to the application, you should be able to build and run the console app. Make sure you have a .NET developer environment.

```shell
$ cd DocumentSearch
$ dotnet build
...
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
Once the build succeeds, you can create `DocumentSearch.exe` by running the following command (this assumes Windows10-x64, but other commands will work for different machines.

```shell
$ dotnet publish -c Release -r win10-x64
```
Now you can navigate to the associated location where the program is placed and run the executable.
```shell
$ DocumentSearch.exe
```

## Adding Data to MongoDB
Upon running the application, you should be asked for a search term. Here, you can enter `AdminDb:` followed by the number of documents you want to add to your database. For example: `AdminDb:1000` to add 1000 documents.
The application will add 100 documents at a time (in order to stay within the API's data limit) until it is done.

## Searching with DocumentSearch
Once the documents have been added, you can search through the database by providing terms. The application will return the first 20 documents found and gives you the option to view more.

```
DocumentSearch Demo - Current DB Size: 8000
Enter a search term <QUIT to quit>
You can split multiple terms by a comma (,)
Search: Obama,Medicare
Document Count: 29
Displaying 20 documents. <ENTER to see more, q to quit>
The Guardian view on the US election: the time is right for a female president: https://www.theguardian.com/commentisfree/2016/oct/21/guardian-view-on-us-presidency-time-is-right-for-female-leader-hillary-clinton
Trump's pick for key health post known for punitive Medicaid plan: https://www.theguardian.com/us-news/2016/dec/04/seema-verma-trump-centers-medicare-medicaid-cms
...
What If You Could Take It With You? Health Insurance, That Is: http://www.npr.org/sections/health-shots/2017/02/28/517720563/what-if-you-could-take-it-with-you-health-insurance-that-is
```
