# Node 07 — Arbiter Selector — Clean Batch 002

Node 7 receives the full collection of formal results from Node 5 plus the flow context.
It produces the exact names of review units that must fire, and an explicit confirmation for units that do not fire.

A is the full result collection plus the flow context as they arrive at runtime.
B is the exact selector output: named review units with their scope, plus an explicit no-review statement for units that do not apply.
A B that says "the selector fires the appropriate arbiters" is wrong — the specific names and reasons must appear.
A B that is silent when no arbiter fires is wrong — the selector must explicitly confirm results are approved for formatting.

The pass-through case must appear in this file. A selector trained only on fire cases dispatches arbiters for every result.

---

Pair CB2-07-001 — Django N+1 scenario: select arbiters
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Full formal results for the Django N+1 blog feed scenario:
[R1] Queryset changed: Post.objects.all()[:100] replaced with Post.objects.select_related('author').all()[:100].
[R2] Query count confirmed by Django Debug Toolbar: 1 query for the full request, down from 101.
[R3] Regression test added: assertNumQueries(1) wrapping the view call for the blog feed endpoint.
[R4] Render time measured: 0.38 seconds, down from 4.2 seconds.

Flow context: Domain labels active in this flow: D-database-query-efficiency, E-orm-usage-pattern, B-backend-response-latency. Available review units: database-query-efficiency-arbiter, orm-usage-pattern-arbiter, security-arbiter, connection-management-arbiter.
B: database-query-efficiency-arbiter: FIRE on [R1] and [R2] together. Review scope: confirm the queryset change reduces query count from 101 to 1 as claimed, and confirm that the change does not introduce a new over-fetching concern (select_related joins the full auth_user row — if the template uses only one or two fields from auth_user, the remaining columns are transferred and discarded, which is a secondary concern at this scale but should be noted).

orm-usage-pattern-arbiter: FIRE on [R1]. Review scope: confirm that select_related is the correct Django ORM mechanism for a ForeignKey relation (correct), not prefetch_related (which applies to ManyToMany or reverse ForeignKey relations). Confirm that the queryset modifier is applied before slicing (correct — select_related is applied before [:100]).

security-arbiter: NO FIRE. [R1] through [R4] contain no authentication logic, no authorization changes, no user-supplied values inserted into queries, and no deserialization of untrusted input. No security review required.

connection-management-arbiter: NO FIRE. No database connection lifecycle changes appear in [R1] through [R4]. The ORM manages connection pooling implicitly. No review required.

B-backend-response-latency (active domain label — no available review unit): [R4] Render time measured: 0.38 seconds, down from 4.2 seconds. The B-backend-response-latency label is active in this flow but has no corresponding available review unit. [R4] is a measurement confirming the fix, not a decision requiring arbitration. No arbiter fires for [R4].
Reason: The selector fires on domain labels present in the flow context. D-database-query-efficiency and E-orm-usage-pattern are active — their corresponding arbiters fire. B-backend-response-latency is informational; its arbiter is not listed as available, and [R4] is a measurement confirming the fix, not a decision requiring arbitration. Security and connection-management arbiters do not fire because no results fall within their review responsibilities.
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT

Pair CB2-07-001 — Phrasing B
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Results from Django blog feed N+1 fix:
[R1] Post.objects.select_related('author').all()[:100] — queryset updated.
[R2] 1 query confirmed via Django Debug Toolbar.
[R3] assertNumQueries(1) regression test added to the test suite.
[R4] Render time: 0.38s, down from 4.2s.
Flow context: Active domains: D-database-query-efficiency, E-orm-usage-pattern, B-backend-response-latency. Available review units: database-query-efficiency-arbiter, orm-usage-pattern-arbiter, security-arbiter, connection-management-arbiter.
B: database-query-efficiency-arbiter: FIRE — review [R1] and [R2]. Confirm query reduction from 101 to 1. Check for new over-fetching from the full auth_user row join.

orm-usage-pattern-arbiter: FIRE — review [R1]. Confirm select_related is correct for ForeignKey (not prefetch_related). Confirm select_related is applied before the slice.

security-arbiter: NO FIRE — no authentication, authorization, input handling, or deserialization in these results.

connection-management-arbiter: NO FIRE — no connection lifecycle changes. ORM handles pooling implicitly.

B-backend-response-latency (active domain label — no available review unit): [R4] Render time: 0.38s, down from 4.2s. The B-backend-response-latency label is active in this flow but has no corresponding available review unit. [R4] is a measurement confirming the fix, not a decision requiring arbitration. No arbiter fires for [R4].

---

Pair CB2-07-002 — Dapper connection reuse scenario: no arbiters fire
Source: https://github.com/DapperLib/Dapper — Dapper connection management patterns
License: Apache 2.0
A: Full formal results for the Dapper batch insert connection reuse scenario:
[R1] SqlConnection opened once before the processing loop: using var conn = new SqlConnection(connStr); await conn.OpenAsync();
[R2] 500 Dapper INSERT operations execute sequentially reusing the same open connection: foreach (var item in items) { await conn.ExecuteAsync(sql, item); }
[R3] The using block disposes the connection when the loop completes and when an exception is thrown.
[R4] Connection pool slot count at the end of the operation: 1 slot checked out, returned to pool when the using block exits.

Flow context: Domain label active in this flow: D-database-connection-management. Available review units: database-query-efficiency-arbiter, orm-usage-pattern-arbiter, security-arbiter, connection-management-arbiter.
B: connection-management-arbiter: NO FIRE — results reviewed inline. [R1] SqlConnection opened once before the loop: correct — one pool slot checked out at loop start. [R2] 500 ExecuteAsync calls reuse the same open connection: correct — no per-iteration pool checkout and return. [R3] The using block disposes the connection when the loop completes and when an exception is thrown: correct — Dispose() called in both success and exception paths. [R4] Connection pool slot count at end: 1 slot returned when the using block exits — correct. Opening a new connection per loop iteration would exhaust the connection pool at 500 concurrent operations; these results do not do that. No concern.

database-query-efficiency-arbiter: NO FIRE — each INSERT is a parameterized operation with no over-fetching risk. No result set is loaded and then discarded. No review required.

orm-usage-pattern-arbiter: NO FIRE — Dapper executes SQL directly, not via an ORM with lazy-loading behavior. No pattern concerns applicable.

security-arbiter: NO FIRE — the INSERT uses parameterized queries, which is Dapper's default. No dynamic SQL concatenation, no deserialization of untrusted input, no authentication or authorization changes.

Results are approved for formatting.
Reason: The selector must explicitly confirm that no arbiter fires — silence is not the correct selector output in the pass-through case. "Results are approved for formatting" is the terminal state that signals the flow to proceed to Node 9. If the selector were silent, the formatter would have no dispatch signal and the flow would stall. The inline review for the connection-management arbiter explains why no concern is raised, providing the evidence that the pass is justified.
Source: https://github.com/DapperLib/Dapper — Dapper connection management patterns
License: Apache 2.0

Pair CB2-07-002 — Phrasing B
Source: https://github.com/DapperLib/Dapper — Dapper connection management patterns
License: Apache 2.0
A: Results from Dapper bulk insert scenario:
[R1] using var conn = new SqlConnection(connStr) wraps the entire operation.
[R2] conn.OpenAsync() called once before the 500-iteration loop.
[R3] conn.ExecuteAsync(sql, item) called inside the loop, reusing the same connection on every iteration.
[R4] Connection returned to pool when the using block exits.
Flow context: Active domain: D-database-connection-management. Available review units: database-query-efficiency-arbiter, orm-usage-pattern-arbiter, security-arbiter, connection-management-arbiter.
B: connection-management-arbiter: NO FIRE — results reviewed inline. [R1] using var conn = new SqlConnection wraps the operation: correct — one pool slot checked out at open. [R2] conn.OpenAsync() called once before the loop: correct — single open for all 500 operations. [R3] conn.ExecuteAsync reusing the same connection on every iteration: correct — no per-iteration checkout. [R4] Connection returned to pool when using block exits: correct. Single connection, correct disposal pattern, no pool exhaustion risk.

database-query-efficiency-arbiter: NO FIRE — parameterized INSERTs, no result set loaded and discarded, no over-fetching.

orm-usage-pattern-arbiter: NO FIRE — Dapper, not ORM. No lazy-load risk.

security-arbiter: NO FIRE — parameterized queries, no dynamic SQL, no untrusted input deserialization.

Results are approved for formatting.
