using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace TrialsOfNeo
{
    public class Grapher
    {
        private readonly IQueryExecutor _queryExecutor;

        public Grapher(IQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        }

        public async Task<List<T>> ReadAs<T>(string query) where T : new()
        {
            var records = await _queryExecutor.Read(query);

            return Translate<T>(records);
        }

        // Notes: update flow to be like:
        //  Start with passed in class
        //  For each record
        //  Match first value from values and convert
        //  If converted object is a node, add to nodes dictionary <id[string], node[Class]>
        //  If first converted class references another, keep going.
        //  Investigate IRelationship to ensure we traverse properly.
        //  Once everything is done, go through each relationship and tie the references
        private static List<T> Translate<T>(List<IRecord> records) where T : new()
        {
            var result = new List<T>();

            var type = typeof(T);
            var attributes = Attribute.GetCustomAttributes(type);

            var nodeAttributes = attributes
                .OfType<NodeAttribute>();

            var labels = new HashSet<string>();

            foreach (var attribute in attributes)
            {
                if (attribute is NodeAttribute nodeAttribute)
                {
                    labels.Add(nodeAttribute.Label);
                }
            }

            foreach (var record in records)
            {
                foreach (var recordValue in record.Values)
                {
                    if (recordValue.Value is INode node && node.Labels.Any(label => labels.Contains(label)))
                    {
                        var newThing = new T();

                        var properties = type.GetProperties();

                        foreach (var propertyInfo in properties)
                        {
                            var neoPropName = Attribute.GetCustomAttributes(propertyInfo)
                                .OfType<NeoPropertyAttribute>()
                                .SingleOrDefault()
                                ?.Name;

                            if (neoPropName != null && node.Properties.TryGetValue(neoPropName, out var neoProp))
                            {
                                propertyInfo.SetValue(newThing, neoProp);
                            }
                        }

                        result.Add(newThing);
                    }
                }
            }

            return result;
        }
    }
}