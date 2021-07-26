MATCH (a:Person {name: 'Tom Hanks'})
OPTIONAL MATCH (a)-[r:ACTED_IN]->(m:Movie)
OPTIONAL MATCH (m)<-[v:REVIEWED]-(rv:Person)
OPTIONAL MATCH (rv)<-[f:FOLLOWS]-(fl:Person)
RETURN a, collect(r), collect(m), collect(v), collect(rv), collect(f), collect(fl)


MATCH (a:Person {name: 'Tom Hanks'})-[r:ACTED_IN]->(m:Movie)
RETURN a, r, m
  LIMIT 5

MATCH (a:Person {name: 'Tom Hanks'})-[r:ACTED_IN]->(m:Movie)
RETURN a, collect(r), collect(m)

MATCH (a:Person {name: 'Tom Hanks'})
OPTIONAL MATCH (a)-[r:ACTED_IN]->(m:Movie)
OPTIONAL MATCH (m)<-[v:REVIEWED]-(rv:Person)
OPTIONAL MATCH (rv)<-[f:FOLLOWS]-(fl:Person)
RETURN a, collect(r), collect(m), collect(v), collect(rv), collect(f), collect(fl)

MATCH (a:Person {name: 'Tom Hanks'})
RETURN a, 42 AS whatever

MATCH (m:Movie)
  WHERE m.title CONTAINS 'Matrix'
RETURN m { .title, .released } AS movie

MATCH path = (p:Person)-[:DIRECTED]->(m:Movie)
return path

MATCH (m:Movie)<-[:ACTED_IN]-(p:Person)
WITH collect(m) AS movies,count(m) AS movieCount, p
UNWIND movies AS movie
RETURN p.name, movieCount, movie.title

MATCH (m:Movie)
WITH m LIMIT 5
MATCH path = (m)<-[:ACTED_IN]-(:Person)
WITH m, collect(path) AS paths
RETURN m, paths[0..2]

