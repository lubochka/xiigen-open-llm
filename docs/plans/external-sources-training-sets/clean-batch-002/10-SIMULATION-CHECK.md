# 10-SIMULATION-CHECK — Clean Batch 002

This file simulates the pipeline using the same scenario as the clean-batch-001 check.
All pairs from both clean-batch-001 and clean-batch-002 are treated as available.

---

## Simulation Input

Simulation prompt (from EXT-1473 / SWE-bench, MIT license):

> A Django view loads 100 blog posts and their authors for a feed page. It takes 4.2 seconds to render. Profiling shows 101 queries: one for the posts and one per post for the author profile. The template accesses post.author.name in a loop. How do I fix this?

---

## Node 1 — Domain Splitter

Trained pairs available: CB1-01-003 (Django N+1 query).

Simulated output:
`["D-database-query-efficiency", "E-orm-usage-pattern", "B-backend-response-latency"]`

Assessment: PASS. No change from clean-batch-001.

---

## Node 2 — Domain Translator

Three domain labels arrive from Node 1. Node 2 is an array node — each label is processed separately.

**Label: D-database-query-efficiency**
Trained pairs available: CB2-02-002 (Django N+1 fragment → D-database-query-efficiency translation).

Simulated output:
> D-database-query-efficiency: The N+1 query pattern is active. 1 query loads 100 Post objects; 100 queries each load one auth_user row by primary key. Total round trips: 101. Required outcome: replace with a single JOIN query. Target: 1 database query, SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100.

Assessment: PASS. CB2-02-002 matches the fragment and label. Node 3 now receives a formal query-domain description instead of the natural-language symptom. **This is the primary bottleneck resolved by clean-batch-002.**

**Label: E-orm-usage-pattern**
Trained pairs available: None in either batch.

Gap: No Node 2 pair exists for E-orm-usage-pattern. A node trained on these two batches cannot produce a formal ORM-domain translation for this label. Node 3 would receive the raw natural-language fragment for this territory.

What is missing: A pair where Fragment is "the template accesses post.author.name in a loop" + Label is "E-orm-usage-pattern" and B is the formal ORM description (lazy load trigger mechanism, select_related vs prefetch_related distinction, the ForeignKey traversal pattern).

**Label: B-backend-response-latency**
Trained pairs available: None in either batch.

Gap: No Node 2 pair exists for B-backend-response-latency. This label is informational for this scenario — the latency is a symptom of the N+1 pattern, not an independent domain requiring translation. However, the absence means a trained Node 2 cannot produce a formal latency-domain description.

---

## Node 3 — Scheme Planner

Trained pairs available from both batches: CB1-03-001 (OTel tail sampling), CB1-03-002 (EF Core column rename), CB1-03-003 (Dapper transaction), CB1-03-004 (Redis cache).

Gap: No Node 3 pair covers the Django N+1 scenario. The scheme planner receives the formal domain description from Node 2 for D-database-query-efficiency, but has no matching training input for this scenario type.

What the scheme planner should produce for this scenario (not yet in a pair):
- Step 1: Confirm Post.author is a ForeignKey field (not ManyToMany) — determines whether select_related or prefetch_related is correct
- Step 2: Add select_related('author') to the Post queryset
- Step 3: Verify with Django Debug Toolbar — query count target: 1
- Step 4: Add assertNumQueries(1) regression test wrapping the view call

Assessment: GAP. Node 3 cannot produce a plan for the Django N+1 scenario. This cascades to Nodes 4, 5, and 6 — they receive no sub-tasks and cannot function.

Future source: SWE-bench Django task instances. Django documentation (BSD license — requires license check before use).

---

## Node 4 — Domain Processor

Trained pairs available from clean-batch-002: CB2-04-001 (add select_related sub-task), CB2-04-002 (consistent-hash routing sub-task), CB2-04-003 (nullable column migration sub-task), CB2-04-004 (Redis CacheModule sub-task).

CB2-04-001 covers the specific sub-task for the Django N+1 scenario. However, Node 4 depends on Node 3's output for this scenario, and Node 3 has no Django N+1 plan pair. The sub-task "Add select_related('author') to the Post queryset" does not arrive at Node 4 because Node 3 cannot produce it.

Assessment: PAIR EXISTS but flow is blocked by Node 3 gap. CB2-04-001 is the correct pair for the sub-task — when Node 3 is trained on the Django N+1 plan, CB2-04-001 will activate immediately.

---

## Node 5 — Applier

Trained pairs available: None in either batch.

Gap: No Applier pairs exist. The cascade from Node 3 and Node 4 also blocks this node.

---

## Node 6 — Result Translator

Trained pairs available: None in either batch.

Gap: No Result Translator pairs exist.

---

## Node 7 — Arbiter Selector

Trained pairs available from clean-batch-002: CB2-07-001 (Django N+1 scenario — fire two arbiters), CB2-07-002 (Dapper connection reuse — no arbiters fire).

CB2-07-001 covers the Django N+1 selector case. However, Node 7 receives results from Node 5, which has no pairs and cannot produce results for this scenario.

Simulated output if Node 5 results were present (from CB2-07-001):
> database-query-efficiency-arbiter: FIRE on [R1] and [R2].
> orm-usage-pattern-arbiter: FIRE on [R1].
> security-arbiter: NO FIRE.
> connection-management-arbiter: NO FIRE.

Assessment: PAIR EXISTS but flow is blocked by Node 5 gap. CB2-07-001 is correct — when Node 5 is trained, this pair activates immediately.

---

## Node 8 — Arbiter Units

Trained pairs available from clean-batch-001: CB1-08-001 (Dapper over-fetching), CB1-08-002 (Django N+1), CB1-08-003 (connection management pass), CB1-08-004 (dynamic deserialization).

CB1-08-002 covers the Django N+1 arbiter case. Node 8 is dispatched by Node 7, which also depends on Node 5 results. However, if the arbiters were dispatched, CB1-08-002 would produce:

> Reprocess: N+1 query pattern detected. 101 database queries for a single page request. Replace Post.objects.all()[:100] with Post.objects.select_related('author').all()[:100]. Send the queryset definition back to the domain processor with this replacement applied.

Assessment: PAIR EXISTS and correct. No new Node 8 pairs are needed for this scenario.

---

## Node 9 — Formatter

Trained pairs available from clean-batch-001: CB1-09-001 (HTTP cache vs Redis, junior developer), CB1-09-002 (WebSocket vs polling, PM), CB1-09-003 (ETags, frontend developer), CB1-09-004 (BFF, devops engineer).

Gap: No formatter pair covers the Django N+1 result for any audience. The formatter can generalize audience-shaping behavior from existing pairs, but no pair shapes the N+1 fix result for any specific audience.

What is missing: A pair where A is the approved translated result of the N+1 fix plus an audience type and B is the shaped output. Minimum: one developer-facing audience (junior developer or backend engineer) and one non-technical audience (product manager).

---

## Summary

### What clean-batch-002 adds

- **Node 2 now covers D-database-query-efficiency** for the Django N+1 scenario — the primary gap from clean-batch-001 is closed. Node 3 now receives a formal domain description instead of natural-language symptoms for this label.
- **Node 4 now has the correct pair for the select_related sub-task** (CB2-04-001) — it will activate as soon as Node 3 is trained on the Django N+1 plan.
- **Node 7 now has the correct pair for the Django N+1 arbiter selection** (CB2-07-001) — it will activate as soon as Node 5 results flow through.
- **Node 7 pass-through case** (CB2-07-002) is covered — the selector will not dispatch arbiters for results that do not require review.

### What is still missing

- Node 2: no pairs for E-orm-usage-pattern or B-backend-response-latency labels
- Node 3: no Django N+1 scheme plan pair — this is the remaining bottleneck. Nodes 4, 5, and 6 have pairs for sub-tasks but receive nothing from Node 3 for this scenario.
- Node 5: zero pairs in either batch
- Node 6: zero pairs in either batch
- Node 9: no pairs for the Django N+1 result for any audience

### What to address in clean-batch-003

Priority 1: Node 3 — Django N+1 scheme plan pair. This resolves the cascade block on Nodes 4 and 7 for the simulation scenario, and allows CB2-04-001 and CB2-07-001 to activate.

Priority 2: Node 5 (Applier) — at least one pair for the sub-tasks covered in Node 4. Source: SWE-bench task instances.

Priority 3: Node 6 (Result Translator) — at least one pair translating a formal result into user-readable natural language.

Priority 4: Node 2 — pairs for E-orm-usage-pattern and B-backend-response-latency labels.

---

## Pre-Promotion Check Results

**Check 1 — License clean:**
- MIT sources (SWE-BENCH, EFCORE-SOURCE, NESTJS-ADVANCED, DOTNET-MICROSERVICES): PASS
- Apache 2.0 sources (KUBERNETES-KEPS, OPENTELEMETRY-SPEC, DAPPER): PASS
- CC-BY 4.0 sources (REACT-ADVANCED): PASS — attribution recorded in every pair

**Check 2 — Reasoning present:**
- Node 2 pairs: B is the formal domain description itself, not a restatement of the fragment. PASS.
- Node 4 pairs: B names specific success conditions with measurable states, specific failure conditions with specific causes, and specific wait states with specific blocking conditions. PASS.
- Node 7 pairs: B names which arbiters fire and which do not, with the reason for each decision. The pass-through case explicitly confirms approval — it does not use silence. PASS.

**Check 3 — Context sufficient:**
- All pairs contain enough information to reproduce the output on a similar input without reading the source unit. PASS.

**Check 4 — Node assignment correct:**
- Node 2 pairs: all are one prompt fragment + one domain label → formal domain description. PASS.
- Node 4 pairs: all are one sub-task + domain context → enumerated success, failure, and wait-state outcomes. PASS.
- Node 7 pairs: all are full result collection + flow context → named review units or explicit approval. PASS.

**Check 5 — Not duplicated in internal corpus:**
No scenarios in this batch (React scroll, Django N+1, BFF topology, K8s provisioning, OTel tail sampling, EF Core column rename, Redis caching) appear in the internal corpus (Flow-01 user-registration scenarios). PASS.

---

## Promotion Status

**clean-batch-002: PROMOTED**

20 pairs across 3 node files. All five pre-promotion checks passed.
Nodes 5 and 6 remain at zero pairs. Node 3 lacks a Django N+1 plan pair — addressed in clean-batch-003.
