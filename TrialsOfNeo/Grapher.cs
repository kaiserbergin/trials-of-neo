using System;
using System.Collections;
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
        
        #region Paper Napkin Plan - Premapping

        private INode _anchorNode;
        private Dictionary<long, INode> _nodesById = new Dictionary<long, INode>();
        private ILookup<string, IRelationship> _relationshipLookup;

        public void AssignAnchorNode<T>(List<IRecord> records) where T : new()
        {
            var type = typeof(T);
            var attributes = Attribute.GetCustomAttributes(type);
            var labels = GetNodeLabels(attributes);

            var isAnchorFound = false;
            
            foreach (var record in records)
            {
                foreach (var (_, recordValue) in record.Values)
                {
                    if (recordValue is INode node) 
                    {
                        foreach (var label in node.Labels)
                        {
                            if (labels.Contains(label))
                            {
                                _anchorNode = node;
                                isAnchorFound = true;
                                break;
                            }
                        }
                    }

                    if (isAnchorFound) break;
                }

                if (isAnchorFound) break;
            }
        }

        private HashSet<string> GetNodeLabels(Attribute[] attributes)
        {
            var labels = new HashSet<string>();

            foreach (var attribute in attributes)
            {
                if (attribute is NodeAttribute nodeAttribute)
                {
                    labels.Add(nodeAttribute.Label);
                }
            }

            return labels;
        }
        
        public void PopulateNodes(List<IRecord> records)
        {
            foreach (var record in records)
            {
                foreach (var (_, recordValue) in record.Values)
                {
                    if (recordValue is INode node)
                    {
                        _nodesById.TryAdd(node.Id, node);
                    }
                    else if (recordValue is List<object> objectList && objectList.FirstOrDefault() != null && objectList.FirstOrDefault() is INode)
                    {
                        foreach (var obj in objectList)
                        {
                            var nodeFromObj = obj as INode;
                            if (nodeFromObj?.Id != null)
                                _nodesById.TryAdd(nodeFromObj.Id, nodeFromObj);
                        }
                    }
                }
            }
        }

        public void PopulateRelationships(List<IRecord> records)
        {
            var distinctRelationships = GetDistinctRelationships(records);

            _relationshipLookup = distinctRelationships.ToLookup(rel => rel.Type, rel => rel);
        }

        private List<IRelationship> GetDistinctRelationships(List<IRecord> records)
        {
            var relationships = new Dictionary<long, IRelationship>();
            
            foreach (var record in records)
            {
                foreach (var (_, recordValue) in record.Values)
                {
                    if (recordValue is IRelationship relationship)
                    {
                        relationships.TryAdd(relationship.Id, relationship);
                    }
                    else if (recordValue is List<object> objectList && objectList.FirstOrDefault() != null && objectList.FirstOrDefault() is IRelationship)
                    {
                        foreach (var obj in objectList)
                        {
                            var relationshipFromObj = obj as IRelationship;
                            if (relationshipFromObj?.Id != null)
                                relationships.TryAdd(relationshipFromObj.Id, relationshipFromObj);
                        }
                    }
                }
            }

            return relationships.Values.ToList();
        }
        
        #endregion
        
        #region First Attempt

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
        
        #endregion
    }
}