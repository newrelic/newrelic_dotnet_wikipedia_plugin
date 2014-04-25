using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Processors;
using NewRelic.Platform.Sdk.Utils;

namespace NewRelic.Platform.Wikipedia.Plugin
{
    class WikipediaAgent : Agent
    {
        public override string Guid 
        { 
            get 
            { 
                return "com.newrelic.dotnet.wikipedia"; 
            } 
        }

        public override string Version { 
            get 
            { 
                return "1.0.0"; 
            } 
        }

        private const String WIKIPEDIA_URL = "/w/api.php?action=query&format=json&meta=siteinfo&siprop=statistics";

        private string name;
        private string host;
        private Uri uri;
        private IProcessor articleCreationRate;

        private Logger log = Logger.GetLogger(typeof(WikipediaAgent).Name);

        public WikipediaAgent(string name, string host)
        {
            this.name = name;
            this.host = host;
            this.uri = new Uri(string.Format("http://{0}{1}", host, WIKIPEDIA_URL));

            this.articleCreationRate = new EpochProcessor();
        }

        /// <summary>
        /// Returns a human-readable string to differentiate different hosts/entities in the New Relic UI
        /// </summary>
        public override string GetAgentName()
        {
            return this.name;
        }

        /// <summary>
        // This is where logic for fetching and reporting metrics should exist.  
        // Call off to a REST head, SQL DB, virtually anything you can programmatically 
        // get metrics from and then call ReportMetric.
        /// </summary>
        public override void PollCycle()
        {
            int? numArticles = GetNumberOfArticles();

            if (numArticles.HasValue)
            {
                ReportMetric("Articles/Count", "articles", numArticles);
                ReportMetric("Articles/Created", "articles/sec", articleCreationRate.Process(numArticles));
            }
            else
            {
                log.Error("Unable to fetch information from the Wikipedia host '{0}'", this.host);
            }
        }

        private int? GetNumberOfArticles()
        {
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                }

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseJson = reader.ReadToEnd();
                    IDictionary<string, object> responseObject = JsonHelper.Deserialize(responseJson) as IDictionary<string, object>;

                    if (responseObject != null)
                    {
                        IDictionary<string, object> query = (IDictionary<string, object>)responseObject["query"];
                        IDictionary<string, object> statistics = (IDictionary<string, object>)query["statistics"];

                        return Convert.ToInt32(statistics["articles"]);
                    }
                }
            }

            return null;
        }
    }
}
