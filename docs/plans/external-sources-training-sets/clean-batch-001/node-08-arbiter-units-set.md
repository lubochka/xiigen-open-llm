# Node 08 — Arbiter Units — Clean Batch 001

Node 8 receives one review responsibility plus one result or result group.
It produces exactly one of three outputs: pass (no explanation — silence is the pass signal), a concern naming the specific rule violated and where the source states that rule, or a reprocess instruction naming the exact fragment to send back and what must change.

The pass case must appear in this file. A node with no pass training flags everything.

A is the exact input the arbiter unit receives: the review responsibility plus the result or result group.
B is the exact arbiter output: pass, concern, or reprocess instruction.

---

Pair CB1-08-001 — Dapper over-fetching detection: concern
Source: https://github.com/DapperLib/Dapper — Dapper usage patterns
License: Apache 2.0
A: Review responsibility: Detect over-fetching — confirm that database queries return only the rows required by the caller, not a superset that is filtered in application code.
Result to review: The order repository fetches all orders for a customer using the query SELECT * FROM Orders WHERE CustomerId = @id and returns the full result set to the service layer. The service layer then applies .Where(o => o.Status == "Pending") in C# to filter the list. The Orders table has 500,000 rows; the typical customer has 10,000 orders, of which 50 have Pending status.
B: Concern: Over-fetching. The query retrieves 10,000 rows from the database and transfers them to the application layer, where 9,950 are immediately discarded. The correct approach is to push the status filter to the database: SELECT * FROM Orders WHERE CustomerId = @id AND Status = 'Pending'. This reduces network transfer by 99.5% and eliminates the deserialization of 9,950 objects that will not be used. Reprocess the query definition with the Status filter added to the WHERE clause before applying this result.
Reason: Over-fetching is the anti-pattern of retrieving more data than the caller needs and discarding the excess in application code. Pushing predicates to the database is the standard correction — the filter runs at the storage layer, before network transfer and deserialization.
Source: https://github.com/DapperLib/Dapper — Dapper usage patterns
License: Apache 2.0

Pair CB1-08-001 — Phrasing B
Source: https://github.com/DapperLib/Dapper — Dapper usage patterns
License: Apache 2.0
A: Review responsibility: Confirm queries do not load more rows than required and then filter in the application layer.
Result to review: Dapper query: SELECT * FROM Orders WHERE CustomerId = @id. Returns 10,000 rows. C# code applies .Where(Status == "Pending") and gets 50 rows. Orders table: 500K rows total.
B: Concern: Application-layer filtering after full result retrieval. 10,000 rows are loaded and transferred; 9,950 are discarded. The Status predicate belongs in the SQL WHERE clause: WHERE CustomerId = @id AND Status = 'Pending'. Add the filter to the query and send the query back for reprocessing with this change applied.

---

Pair CB1-08-002 — Django N+1 query detection: reprocess
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Review responsibility: Detect N+1 query patterns — confirm that related objects are not loaded by issuing one query per parent object.
Result to review: A Django blog view issues the following queries during a request for 100 posts: one query to load all 100 Post objects using Post.objects.all()[:100], then one query per post to load the author profile (post.author accessed in the template, triggering a lazy load). Total queries: 101. Each author query is SELECT * FROM auth_user WHERE id = %s with a different id value.
B: Reprocess: N+1 query pattern detected. 101 database queries are issued for a single page request. The correct query is Post.objects.select_related('author').all()[:100], which generates one SQL JOIN query: SELECT post.*, user.* FROM blog_post JOIN auth_user ON blog_post.author_id = auth_user.id. This reduces 101 queries to 1. Send the queryset definition back to the domain processor with the instruction: replace Post.objects.all()[:100] with Post.objects.select_related('author').all()[:100].
Reason: N+1 is the ORM anti-pattern of loading related objects with one query per parent. select_related instructs Django to perform a JOIN at the SQL level, retrieving parent and related objects in a single query. The N+1 pattern is detectable by counting queries where one query generates N follow-up queries of the same shape with different parameter values.
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT

Pair CB1-08-002 — Phrasing B
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Review responsibility: Check for N+1 query patterns in ORM usage.
Result to review: Django view for blog feed. Post.objects.all()[:100] loads posts. Template accesses post.author.name for each. 101 queries issued: 1 for posts, 100 for authors (one per post, same shape, different WHERE id = N each time).
B: Reprocess: N+1 pattern confirmed. Replace Post.objects.all()[:100] with Post.objects.select_related('author').all()[:100]. This joins the User table in a single query and eliminates the 100 per-author lookups. Return the queryset to the domain processor with this replacement applied.

---

Pair CB1-08-003 — Memory-safe connection management: pass
Source: https://github.com/DapperLib/Dapper — Dapper connection management
License: Apache 2.0
A: Review responsibility: Confirm that database connections are disposed correctly and that connection pool exhaustion cannot occur from this code path.
Result to review: The order ingestion service opens a single SqlConnection outside the processing loop, executes 500 Dapper INSERT commands reusing the same open connection, and disposes the connection in a using block after the loop completes. The connection is wrapped in a using block: using var conn = new SqlConnection(connStr); await conn.OpenAsync(); foreach (var item in items) { await conn.ExecuteAsync(sql, item); } The connection is not opened inside the loop.
B: Pass.
Reason: A single connection opened outside the loop and reused for all 500 operations checks out one connection pool slot and returns it after the loop. This is the correct pattern. The using block guarantees disposal in both the success and exception paths. No pool exhaustion risk from this code path.
Source: https://github.com/DapperLib/Dapper — Dapper connection management
License: Apache 2.0

Pair CB1-08-003 — Phrasing B
Source: https://github.com/DapperLib/Dapper — Dapper connection management
License: Apache 2.0
A: Review responsibility: Check for database connection pool exhaustion risk.
Result to review: Dapper service: using var conn = new SqlConnection(connStr) wraps a loop of 500 conn.ExecuteAsync() calls. Connection is opened once before the loop, disposed when the using block exits.
B: Pass.

---

Pair CB1-08-004 — Dynamic deserialization: security concern
Source: https://github.com/dotnet/efcore — ASP.NET Core security patterns; also OWASP Deserialization guide
License: MIT
A: Review responsibility: Check for unsafe deserialization that allows untrusted input to influence type resolution or object graph construction in ways that could execute code or expose internal state.
Result to review: An ASP.NET Core 8 API endpoint deserializes a partner integration payload using: var data = JsonConvert.DeserializeObject<dynamic>(Request.Body). The dynamic type is then used directly: data.SettingName, data.ConfigValue. The Newtonsoft.Json version in use is 13.0.1. TypeNameHandling is not explicitly configured.
B: Concern: CWE-502 (Deserialization of Untrusted Data) risk. Deserializing to dynamic from an untrusted source accepts any JSON shape and allows arbitrary property access without type validation. Although TypeNameHandling.All is not explicitly set, dynamic deserialization still bypasses input shape enforcement. The correct approach is: define a strongly typed DTO (Data Transfer Object) with explicit properties matching the expected partner payload, and deserialize using System.Text.Json.JsonSerializer.Deserialize<PartnerConfigDto>(body). System.Text.Json does not support $type-based polymorphic resolution from JSON metadata by default, removing the gadget chain attack surface. Reprocess the endpoint implementation to use a typed DTO with System.Text.Json.
Reason: Deserializing to dynamic accepts all JSON shapes without validation. The correct mitigation is strong typing via a DTO — the input shape is enforced by the model binder, and unexpected fields are ignored rather than accepted. System.Text.Json is preferred over Newtonsoft.Json for new code because it does not resolve polymorphic types from JSON metadata by default.
Source: https://github.com/dotnet/efcore — ASP.NET Core security docs; OWASP Deserialization Cheat Sheet
License: MIT

Pair CB1-08-004 — Phrasing B
Source: https://github.com/dotnet/efcore — ASP.NET Core security docs
License: MIT
A: Review responsibility: Identify unsafe deserialization from untrusted input.
Result to review: ASP.NET Core endpoint: JsonConvert.DeserializeObject<dynamic>(requestBody) from a partner integration. Newtonsoft.Json 13.0.1. TypeNameHandling not explicitly set. The dynamic result is accessed for two properties: SettingName and ConfigValue.
B: Concern: Unsafe deserialization from untrusted source. Dynamic type accepts arbitrary JSON shapes with no schema enforcement. Define a PartnerConfigDto class with SettingName and ConfigValue as typed properties. Deserialize using System.Text.Json.JsonSerializer.Deserialize<PartnerConfigDto>(body). Validate the DTO with FluentValidation before use. Reprocess the endpoint to use this typed model.
