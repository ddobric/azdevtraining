using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDataUploaderSample
{
    internal class SimpleSample
    {
        private GraphConnection m_Conn;

        private DocumentCollection m_Collection;
        private string endpointUrl;
        private string primaryKey;
        private string databaseId;
        private string collectionName;

        private DocumentClient m_Client;



        public SimpleSample(string endpointUrl, string primaryKey, string databaseId, string collectionName)
        {
            this.endpointUrl = endpointUrl;
            this.primaryKey = primaryKey;
            this.databaseId = databaseId;
            this.collectionName = collectionName;

            m_Client = new DocumentClient(new Uri(endpointUrl), primaryKey);

            Database database = m_Client.CreateDatabaseQuery("SELECT * FROM d WHERE d.id = \"" + databaseId + "\"").AsEnumerable().FirstOrDefault();
            List<DocumentCollection> collections = m_Client.CreateDocumentCollectionQuery(database.SelfLink).ToList();
            m_Collection = collections.FirstOrDefault(x => x.Id == collectionName);

            m_Conn = new GraphConnection(m_Client, m_Collection);
        }

        public async Task RunNative()
        {
            GraphCommand graphCommand = new GraphCommand(this.m_Conn);

            var resFeed = await executeQuery("g.V().hasLabel('planet').count()");

            Console.WriteLine($"Number of planets in system: {resFeed.FirstOrDefault()}");




            resFeed = await executeQuery("g.V().hasLabel('satellite').count()");

            Console.WriteLine($"Number of planets in system: {resFeed.FirstOrDefault()}");

            resFeed = await executeQuery("g.V().hasLabel('satellite').limit(100)");



            resFeed = await executeQuery("g.E().hasLabel('compatiblewith').count()");

            Console.WriteLine($"Number of connections to planet: {resFeed.FirstOrDefault()}");



            // Enlist all planets.
            resFeed = await executeQuery("g.V().hasLabel('planet').values('planetName', 'id')");

            foreach (var item in resFeed)
            {
                Console.WriteLine($"Planet: {item}");
            }

            Console.WriteLine($"Number of planets in system: {resFeed.FirstOrDefault()}");

            // Remove all planets from graph.
            Console.WriteLine("Deleting planets");
            await graphCommand.g().V().HasLabel("planet").Drop().NextAsync();

            Console.WriteLine("Deleting satellites");
            await graphCommand.g().V().HasLabel("satellite").Drop().NextAsync();

            Console.WriteLine("Creating edges");
            await graphCommand.g().E().HasLabel("compatiblewith").Drop().NextAsync();


            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("Creating planets");
            for (int i = 0; i < 10; i++)
            {
                resFeed = await executeQuery($"g.addV('planet').Property('id','P{i}').Property('name', 'Planet {i}')");
                Console.Write(".");
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Creating satellites");
            for (int i = 0; i < 100; i++)
            {
                resFeed = await executeQuery($"g.addV('satellite').Property('id','S{i}').Property('name', 'SAT {i}')");
                Console.Write(".");
            }


            Random rnd = new Random(42);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Binding sattelites and planets");
            for (int i = 0; i < 10; i++)
            {
                for (int y = 0; y < 10; y++)
                {
                    var j = rnd.Next(100);
                    resFeed = await executeQuery($"g.V().hasLabel('planet').has('id','P{i}').addE('bindedTo').Property('year','2018').to(g.V().hasLabel('satellite').has('id','S{j}'))");
                    Console.Write($"[P{i}->S{j}], ");
                }
                Console.WriteLine();
            }
                       
        }


        public async Task RunApi()
        {
            GraphCommand graphCommand = new GraphCommand(this.m_Conn);

            // Enlist all items.
            var qT = graphCommand.g().V();

            await traceOut("All", qT);

            // Remove all planets from graph.
            await graphCommand.g().V().HasLabel("planet").Drop().NextAsync();
            await graphCommand.g().V().HasLabel("sattelite").Drop().NextAsync();
            await graphCommand.g().E().HasLabel("bindedTo").Drop().NextAsync();

            // Add some planets
            await AddPlanets();

            // Add some satellites
            await AddSatellites();

            // Add some satellites
            await AddRelations();

            await AddLoops();

            // Enlist all planets
            await traceOut("Planets", graphCommand.g().V().HasLabel("planet"));

            //
            // Enlist all satellites
            await traceOut("Satellites", graphCommand.g().V().HasLabel("satellite"));

            await traceOut("Satellites", graphCommand.g().V().Has("name", "PLANET P1").Out("followup").Out("followup").Values("name"));

        }


        public async Task AddPlanets()
        {
            for (int i = 0; i < 10; i++)
            {
                GraphCommand cmd = new GraphCommand(this.m_Conn);

                var traversal = cmd.g().AddV("planet");
                traversal = traversal.Property("name", $"PLANET P{i}");
                traversal = traversal.Property("size", DateTime.Now.Ticks);

                var res = await traversal.NextAsync();

                if (res.Count == 0 || res[0] == "[]")
                {
                    throw new Exception("Could not insert element");
                }
            }
        }


        public async Task AddSatellites()
        {
            for (int i = 0; i < 100; i++)
            {
                GraphCommand cmd = new GraphCommand(this.m_Conn);

                var traversal = cmd.g().AddV("satellite")
                .Property("name", $"SAT {i}")
                .Property("size", DateTime.Now.Ticks);

                var res = await traversal.NextAsync();

                if (res.Count == 0 || res[0] == "[]")
                {
                    throw new Exception("Could not insert element");
                }
            }
        }


        public async Task AddRelations()
        {
            for (int i = 0; i < 10; i++)
            {
                GraphCommand cmd = new GraphCommand(this.m_Conn);

                for (int j = 0; j < 5; j++)
                {
                    Random rnd = new Random(42);

                    var traversal = cmd.g().V().HasLabel("planet").Has("name", $"PLANET P{i}").AddE("compatiblewith").
                       To(cmd.g().V().HasLabel("satellite").Has("name", $"SAT {rnd.Next(100)}"));

                    var res = await traversal.NextAsync();

                    if (res.Count == 0 || res[0] == "[]")
                    {
                        throw new Exception("Could not insert element");
                    }
                }
            }
        }


        public async Task AddLoops()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = i + 1; j < 10; j++)
                {

                    GraphCommand cmd = new GraphCommand(this.m_Conn);

                    Random rnd = new Random(42);

                    var traversal = cmd.g().V().HasLabel("planet").Has("name", $"PLANET P{i}").AddE("followup").
                        To(cmd.g().V().HasLabel("planet").Has("name", $"PLANET P{j}"));

                    var res = await traversal.NextAsync();

                    if (res.Count == 0 || res[0] == "[]")
                    {
                        throw new Exception("Could not insert element");
                    }
                }

            }
        }
        private async Task<FeedResponse<dynamic>> executeQuery(string query)
        {
            var gQuery = this.m_Conn.DocumentDBClient.CreateGremlinQuery(this.m_Collection, query, feedOptions: new Microsoft.Azure.Documents.Client.FeedOptions() { MaxItemCount = 100, });

            return await gQuery.ExecuteNextAsync();

        }

        /// <summary>
        /// Outputs traversal
        /// </summary>
        /// <param name="traversal"></param>
        /// <returns></returns>
        private static async Task traceOut(string title, GraphTraversal traversal)
        {
            Console.WriteLine("-------------------------");
            Console.WriteLine(title);
            foreach (var d in await traversal.NextAsync())
            {
                Console.WriteLine(d);
            }
        }
    }
}
