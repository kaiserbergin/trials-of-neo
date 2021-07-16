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

        public async Task<List<T>> ReadAs<T>(string query) where T : class, new()
        {
            var records = await _queryExecutor.Read(query);

            return Translate<T>(records);
        }

        #region Paper Napkin Plan - Premapping

        private Dictionary<long, INode> _nodesById = new Dictionary<long, INode>();
        private ILookup<string, IRelationship> _relationshipLookup;

        private void AssignNeoLookups(List<IRecord> records)
        {
            AssignNodes(records);
            AssignRelationships(records);
        }

        private INode GetAnchorNode(IRecord record, Type targetType)
        {
            var attributes = Attribute.GetCustomAttributes(targetType);
            var labels = GetNodeLabels(attributes);

            foreach (var (_, recordValue) in record.Values)
            {
                if (recordValue is INode node)
                {
                    foreach (var label in node.Labels)
                    {
                        if (labels.Contains(label))
                        {
                            return node;
                        }
                    }
                }
            }

            throw new Exception("Anchor node not found sucka");
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

        private void AssignNodes(List<IRecord> records)
        {
            foreach (var record in records)
            {
                foreach (var (_, recordValue) in record.Values)
                {
                    if (recordValue is INode node)
                    {
                        _nodesById.TryAdd(node.Id, node);
                    }
                    else if (recordValue is List<object> objectList && objectList.FirstOrDefault() != null &&
                             objectList.FirstOrDefault() is INode)
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

        private void AssignRelationships(List<IRecord> records)
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
                    else if (recordValue is List<object> objectList && objectList.FirstOrDefault() != null &&
                             objectList.FirstOrDefault() is IRelationship)
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

        private List<T> Translate<T>(List<IRecord> records) where T : class, new()
        {
            AssignNeoLookups(records);

            var result = new List<object>();

            var targetType = typeof(T);

            foreach (var record in records)
            {
                var translatedNode = TranslateNode(GetAnchorNode(record, targetType), targetType);
                result.Add(translatedNode);
            }

            return result.Select(obj => (T)obj).ToList();
        }

        private object TranslateNode(INode neoNode, Type targetType)
        {
            if (targetType.GetConstructor(Type.EmptyTypes) == null)
                throw new Exception($"You need a paramless ctor bro. Class: {targetType.Name}");
            
            var target = Activator.CreateInstance(targetType);

            var targetProperties = targetType.GetProperties();

            foreach (var propertyInfo in targetProperties)
            {
                NeoPropertyAttribute neoPropertyAttribute = null;
                NeoRelationshipAttribute neoRelationshipAttribute = null;

                foreach (var customAttribute in Attribute.GetCustomAttributes(propertyInfo))
                {
                    if (customAttribute is NeoPropertyAttribute propertyAttribute)
                    {
                        neoPropertyAttribute = propertyAttribute;
                        break;
                    }

                    if (customAttribute is NeoRelationshipAttribute relationshipAttribute)
                    {
                        neoRelationshipAttribute = relationshipAttribute;
                        break;
                    }
                }

                if (neoPropertyAttribute?.Name != null && neoNode.Properties.TryGetValue(neoPropertyAttribute.Name, out var neoProp))
                {
                    propertyInfo.SetValue(target, neoProp);
                    continue;
                }

                if (neoRelationshipAttribute?.Type != null)
                {
                    // need to determine target type when wrapped in IEnumerable
                    // Also we populated ALL the relationships and nodes instead of the ones in our current record.
                    var nodeTargetType = propertyInfo.PropertyType;
                    var targetTypeCustomAttributes = Attribute.GetCustomAttributes(nodeTargetType);
                    var targetTypeLabels = GetNodeLabels(targetTypeCustomAttributes);

                    var targetNodes = new List<object>();
                    
                    var relationshipsOfTargetType = _relationshipLookup[neoRelationshipAttribute.Type];

                    if (neoRelationshipAttribute.Direction == RelationshipDirection.Outgoing)
                    {
                        foreach (var relationship in relationshipsOfTargetType)
                        {
                            if (relationship.StartNodeId == neoNode.Id)
                            {
                                if (_nodesById.TryGetValue(relationship.EndNodeId, out var candidateTargetNode))
                                {
                                    foreach (var label in candidateTargetNode.Labels)
                                    {
                                        if (targetTypeLabels.Contains(label))
                                        {
                                            var translatedTargetNode = TranslateNode(candidateTargetNode, nodeTargetType);
                                            targetNodes.Add(translatedTargetNode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (neoRelationshipAttribute.Direction == RelationshipDirection.Incoming)
                    {
                        
                    }
                }
            }

            return target;
        }

        #endregion
    }
}