# Node 02 — Domain Translator — Examination Domain Pairs
# Produced by gap closure pass across five experiment translations
# Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
# Version: 1.0 — 2026-05-04

Node 2 receives one prompt fragment plus one domain label.
It produces a formal domain statement in the vocabulary of that domain.
These pairs teach the five examination domains introduced by the experiment
translation work. Each B is the formal examination statement — not a
description of what such a statement would contain.

---

## Experiment 1 Domain — scheme-planner-examination

Pair EX1-02-001 — Scheme planner: parallel independence claim contradicted by content cross-reference
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB1-03-001 Step 3 transformational prompt states "positioned after the memory_limiter (which will be added in Step 4)" while the step header declares Step 3 parallel with Step 4 and the Reason field asserts "processor configuration files are independent" and "applied together at deployment time."
Label: scheme-planner-examination
B: scheme-planner-examination: The scheme plan pair contains a parallel independence claim contradicted by a content cross-reference. Pathology: the Reason field declares Steps 3 and 4 parallel and independent; the Step 3 transformational prompt body references Step 4's output as a positional constraint ("positioned after the memory_limiter which will be added in Step 4"). Observable evidence: the transformational prompt body contains a forward reference to a sibling step's output using the form "which will be added in Step [n]" while the Reason field asserts concurrent execution is valid. Required state: the transformational prompt body describes the final pipeline configuration without referencing a sibling step's output as a positional dependency — "the tailsampling processor appears after the memory_limiter in the final configured pipeline" rather than "after the memory_limiter which will be added in Step 4." The parallel claim and the prompt body must not contain contradictory ordering implications.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX1-02-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB1-03-001 scheme plan — Steps 3 and 4 labeled "runs in parallel with Step 4 / runs in parallel with Step 3" in their step headers. Reason field states "configuration files are independent" and "applied together at deployment time." Step 3 transformational prompt body contains: "positioned after the memory_limiter (which will be added in Step 4)."
Label: scheme-planner-examination
B: scheme-planner-examination: Parallel independence claim present in Reason field; cross-reference to concurrent step present in transformational prompt body. The two statements contradict each other. Pathology: a step declared independent of its parallel sibling step references that sibling's output in its own transformational prompt body, creating an apparent ordering dependency. Observable evidence: search transformational prompt body for "will be added in Step [n]" or any phrase that names a parallel step's output as a prerequisite — any such reference contradicts a stated parallel independence claim. Required state: transformational prompt body uses final-state description language only, without forward-referencing the sibling step — "the tailsampling processor appears after the memory_limiter in the final pipeline" describes the configuration outcome without implying Step 3 depends on Step 4.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

## Experiment 2 Domain — domain-translator-examination

Pair EX2-02-001 — Domain translator: implementation syntax in required outcome
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB2-02-002 B required outcome contains "Target state: 1 database query, generating SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100."
Label: domain-translator-examination
B: domain-translator-examination: The formal domain description's required outcome contains implementation syntax rather than a structural description. Pathology: the required outcome field contains an exact SQL SELECT clause with specific field lists, JOIN keywords, and ON conditions. A formal domain statement of the required outcome names the join relationship type, the cardinality, and the round-trip count — it does not specify SQL clause syntax. Observable evidence: the required outcome contains SQL keywords (SELECT, FROM, INNER JOIN, ON, LIMIT) with specific field names or table names from the implementation layer. Required state: the required outcome names the relationship between tables (JOIN between blog_post and auth_user on the author foreign key), the cardinality (one query returning 100 posts and their corresponding authors), and the round-trip count (1), without specifying SELECT clause syntax or column lists. SQL vocabulary belongs in Node 3 (scheme planner) or Node 4 (domain processor), not in Node 2 (domain translator) output.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX2-02-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB2-02-002 node-02 pair — the B output's required outcome reads "Target state: 1 database query, generating SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100."
Label: domain-translator-examination
B: domain-translator-examination: Implementation syntax present in required outcome of a formal domain description. Domain translation stops at the structural level — required outcome should describe what the corrected state looks like structurally. Observable evidence: SQL syntax (SELECT clause, specific column lists, JOIN keyword with table names) appears in the required outcome section. This is implementation vocabulary — it belongs in Node 3 or Node 4, not in a formal domain description from Node 2. Required state: required outcome describes query structure (single round-trip, JOIN on the author foreign key) and cardinality (100 rows with related data in one result set), without SELECT clause syntax.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

## Experiment 3 Domain — domain-processor-examination

Pair EX3-02-001 — Domain processor: silent failure signal absent
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB2-04-002 Failure state 2 reads "If the load balancer cannot parse the traceparent header to extract the traceId, it cannot perform consistent routing. Confirm that the load balancer can match and route on a substring of a custom header before attempting this configuration." No observable signal at the load balancer layer is named.
Label: domain-processor-examination
B: domain-processor-examination: The failure state names the condition and the consequence but provides no observable signal at the point where the failure occurs. Pathology: a failure state with a silent onset — the failure produces no direct observable signal at the component where it occurs and is only detectable downstream. Observable evidence: the failure state text contains no statement of the form "observable as: [specific check or metric]" and the named consequence (incorrect routing) is not visible at the load balancer — it manifests as downstream Collector behavior (incomplete-trace metrics). Required state: the failure state explicitly names either a direct observable signal at the point of failure (if one exists) or explicitly acknowledges the silence and names the downstream signal — "this failure is silent at the load balancer; it appears downstream as: Collector metrics show 'incomplete trace' or 'spans from unknown trace' counters incrementing after the routing change is deployed." A failure state without an observable signal leaves the executor without a direct check to run.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX3-02-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB2-04-002 node-04 pair Failure state 2 — "TraceId is propagated in the W3C traceparent header. If the load balancer cannot parse the header to extract the traceId, it cannot perform consistent routing." Failure state contains no observable signal statement.
Label: domain-processor-examination
B: domain-processor-examination: Silent failure state — condition and consequence named; observable signal absent. A failure state in a domain processor output enables an executor to determine whether the failure has occurred. If the failure produces no direct observable signal at the failing component, the failure state must name this explicitly and identify the downstream signal instead. Observable evidence: failure state text contains no phrase of the form "visible as [metric or log entry]" or "observable when [check] returns [result]." Required state: the failure state names either the direct signal (load balancer logs a parsing error) or the downstream signal with explicit acknowledgment that the failure is silent at the origin — "this failure is not visible at the load balancer; it appears downstream as [Collector metric]."
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

## Experiment 4 Domain — arbiter-selector-examination

Pair EX4-02-001 — Arbiter selector: active domain label with no review unit — result disposition implicit
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB2-07-001 B arbiter selector output names fire and no-fire verdicts for all four available review units. The B-backend-response-latency domain label is active in the flow context. [R4] (render time: 0.38 seconds) is a result in the B-backend-response-latency domain. No entry in the selector output names the disposition of [R4] or explains why B-backend-response-latency has no corresponding review unit.
Label: arbiter-selector-examination
B: arbiter-selector-examination: Active domain label with no available review unit — result disposition implicit rather than explicit. Pathology: an active domain label appears in the flow context, a result belongs to that label, but the selector output contains no entry explaining the label's status or the result's disposition. Observable evidence: count active domain labels in the flow context (5) and count domain labels addressed in the selector output (4 — only those with available review units); the difference (1) is the count of silently omitted active labels. Required state: the selector output includes an explicit entry for every active domain label regardless of whether a review unit exists — either a fire verdict for an available unit covering it, or an explicit no-review entry naming the label, stating no review unit is available, and naming the result(s) in that domain: "B-backend-response-latency (active domain label — no available review unit): [R4] is a measurement confirming the fix, not a decision requiring arbitration. No arbiter fires for [R4]."
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX4-02-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB2-07-001 arbiter selector output — fire and no-fire verdicts for database-query-efficiency-arbiter, orm-usage-pattern-arbiter, security-arbiter, connection-management-arbiter. Flow context lists five active domain labels. One label (B-backend-response-latency) has no corresponding available review unit. One result ([R4]) belongs to this label. The selector output contains no entry for this label or this result.
Label: arbiter-selector-examination
B: arbiter-selector-examination: Selector output is silent on one active domain label and its result. Pathology: the selector produces verdicts for all available review units but omits any mention of an active-but-unrepresented domain label, leaving an implicit disposition gap. Observable evidence: compare active domain label count in flow context (5) against distinct domain labels addressed in selector output (4) — any shortfall indicates at least one active label is silently omitted. Required state: selector explicitly states the disposition of every active domain label — for labels without available review units, name the label, state no review unit is available, and name the result(s) in that domain so downstream nodes know the result was seen and cleared, not missed.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

## Experiment 5 Domain — cascade-plan-examination

Pair EX5-02-001 — Cascade plan: prerequisite check without passing and failing forms
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB3-03-001 Step 1 transformational prompt — "Confirm Post.author is a ForeignKey (e.g., author = models.ForeignKey(User, on_delete=models.CASCADE))." The prompt names the target condition but adds "or equivalent" without naming the equivalent forms, does not name the file to check, and does not name the failing forms explicitly.
Label: cascade-plan-examination
B: cascade-plan-examination: Prerequisite check condition named without passing form, failing form, or check location. Pathology: a prerequisite confirmation step names the condition to confirm but does not specify where to check it, what a passing outcome looks like with no ambiguity, or what a failing outcome looks like and what to do in that case. Observable evidence: transformational prompt contains "or equivalent" without naming the equivalent forms; no file location named; no failing form listed with its alternative action path. Required state: the transformational prompt names (1) the file to check (models.py or equivalent model definition file), (2) the passing forms with no ambiguity (models.ForeignKey with any on_delete value; models.OneToOneField with any on_delete value), (3) the failing forms with their corrective action paths (models.ManyToManyField → use prefetch_related; reverse relation via related_name → use prefetch_related). A check without explicit passing and failing forms leaves the executor without a binary confirmation mechanism.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX5-02-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Fragment: CB3-03-001 Step 1 — "Confirm Post.author is a ForeignKey" is the prerequisite check. Adds "or equivalent" without naming what is equivalent. Names one failing case (ManyToManyField) but not reverse relations. Names no file location.
Label: cascade-plan-examination
B: cascade-plan-examination: Prerequisite check with underspecified confirmation criteria. Pathology: a scheme plan prerequisite step must name what a passing check looks like and what a failing check looks like — not just name the target condition. Observable evidence: prompt contains "or equivalent" without defining equivalents; file location absent; "reverse ForeignKey" failing form not listed. Required state: prerequisite check names location (models.py), passing forms (ForeignKey or OneToOneField with any on_delete), failing forms (ManyToManyField; reverse relation via related_name) with the alternative action for each failing case.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
