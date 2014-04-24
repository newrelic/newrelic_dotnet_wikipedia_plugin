using System;
using System.Collections.Generic;
using NewRelic.Platform.Sdk;

namespace NewRelic.Platform.Wikipedia.Plugin
{
    class WikipediaAgentFactory : AgentFactory
    {
        public override Agent CreateAgentWithConfiguration(IDictionary<string, object> properties)
        {
            string name = (string)properties["name"];
            string host = (string)properties["host"];

            if (name == null || host == null)
            {
                throw new ArgumentNullException("'name' and 'host' cannot be null. Do you have a 'config/plugin.json' file?");
            }

            return new WikipediaAgent(name, host);
        }
    }
}
