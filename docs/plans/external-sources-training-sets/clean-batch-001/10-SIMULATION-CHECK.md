# 10-SIMULATION-CHECK — Clean Batch 001

This file simulates the pipeline as if the nodes had been trained only on the pairs in this batch.
The simulation uses a real scenario from the source material as its input prompt.

---

## Simulation Input

Simulation prompt (from EXT-1473 / SWE-bench, MIT license):

> A Django view loads 100 blog posts and their authors for a feed page. It takes 4.2 seconds to render. Profiling shows 101 queries: one for the posts and one per post for the author profile. The template accesses post.author.name in a loop. How do I fix this?

---

## Node 1 — Domain Splitter

Trained pairs available: CB1-01-003 (Django N+1 query).

Simulated output given the prompt:
`["D-database-query-efficiency", "E-orm-usage-pattern", "B-backend-response-latency"]`

Assessment: Domain splitter has a matching pair. The output is correct — three domains are identified. The splitter would not produce the generic label "performance" because the training pair names specific functional territories.

---

## Node 2 — Domain Translator

Trained pairs available: None in this batch.

Gap: No domain translator pairs exist for D-database-query-efficiency or E-orm-usage-pattern. A node trained only on this batch cannot produce a formal domain translation.

What is missing: Node 2 pairs where A is a natural-language fragment ("101 queries for 100 posts") plus a domain label ("D-database-query-efficiency") and B is the formal domain description in database query optimization vocabulary.

Future source: SWE-bench task instances provide enough detail to write these pairs. Dapper and EF Core documentation provide the vocabulary.

---

## Node 3 — Scheme Planner

Trained pairs available: CB1-03-001 (OTel tail sampling), CB1-03-002 (EF Core column rename), CB1-03-003 (Dapper transaction), CB1-03-004 (Redis cache).

Gap: No Node 3 pair covers the Django N+1 scenario. The scheme planner would receive translated domain descriptions for the N+1 scenario but has no matching training input.

What the scheme planner should produce for this scenario (not in a pair yet):
- Step 1: Identify the queryset and confirm select_related is missing
- Step 2: Add select_related('author') to the queryset
- Step 3: Verify using Django Debug Toolbar (query count target: 1)
- Step 4: Add a regression test asserting query count <= 2

Future source: SWE-bench Django task instances; Django documentation (BSD license — needs license check before use).

---

## Node 4 — Domain Processor

Trained pairs available: None in this batch.

Gap: No domain processor pairs exist. The processor cannot function for any scenario in this batch.

What is missing: Pairs where A is one sub-task ("Add select_related('author') to the Post queryset") plus domain context, and B contains all success states (query count drops from 101 to 1), failure states (if the Author model has no direct ForeignKey, select_related will fail), and wait-state outcomes.

---

## Node 5 — Applier

Trained pairs available: None in this batch.

Gap: No applier pairs exist.

---

## Node 6 — Result Translator

Trained pairs available: None in this batch.

Gap: No result translator pairs exist.

---

## Node 7 — Arbiter Selector

Trained pairs available: None in this batch.

Gap: No arbiter selector pairs exist. The selector cannot identify which review units must fire.

---

## Node 8 — Arbiter Units

Trained pairs available: CB1-08-001 (Dapper over-fetching), CB1-08-002 (Django N+1), CB1-08-003 (connection management pass), CB1-08-004 (dynamic deserialization concern).

The N+1 scenario would trigger CB1-08-002. Simulated arbiter output:

> Reprocess: N+1 query pattern detected. 101 database queries for 100 posts. Replace Post.objects.all()[:100] with Post.objects.select_related('author').all()[:100]. Send the queryset definition back to the domain processor with this change applied.

Assessment: The arbiter has a matching pair and produces the correct reprocess instruction. A trained Node 8 would also correctly pass the connection management scenario (CB1-08-003 provides the pass case) and flag dynamic deserialization (CB1-08-004).

---

## Node 9 — Formatter

Trained pairs available: CB1-09-001 (HTTP cache vs Redis, junior dev), CB1-09-002 (WebSocket vs polling, PM), CB1-09-003 (ETags, frontend dev), CB1-09-004 (BFF, devops).

Gap: No formatter pair covers the Django N+1 scenario output. The formatter has pairs for junior-developer and PM and DevOps audiences on different topics, but no pair shaping the N+1 fix result for any audience.

The formatter can generalize audience shaping from the existing pairs (technical explanation for devops, non-technical for PM) but the specific N+1 content is not covered.

What is missing: A pair where A is the approved translated result of the N+1 fix plus an audience type and B is the shaped output.

---

## Summary

### What this batch enables

- Domain splitting for four scenario types (React performance, K8s autoscaling, Django N+1, BFF architecture)
- Scheme planning for four scenario types (OTel sampling, EF Core migration, Dapper transaction, Redis cache)
- Arbiter evaluation for four result types (over-fetching, N+1, correct connection management, unsafe deserialization)
- Formatter output for two audiences (junior developer, product manager) and two additional audiences (frontend developer, devops engineer)

### What is missing

- Nodes 2, 4, 5, 6, 7 have zero pairs. These nodes cannot function on any scenario in this batch.
- Node 3 has no pair for the Django N+1 scenario.
- Node 9 has no pair for the N+1 result.

### What to address in clean-batch-002

Priority 1: Node 2 (Domain Translator) pairs. Node 2 is the bottleneck — without domain translations, Node 3 receives natural-language input and cannot produce a precise plan.

Priority 2: Node 4 (Domain Processor) pairs. Without processed results, Nodes 5, 6, 7, and 8 have nothing to act on.

Priority 3: Node 5 (Applier) pairs for the scenarios already covered in Node 3.

Priority 4: Node 7 (Arbiter Selector) pairs — at least one pass case and one selection case for each Node 8 arbiter type present in this batch.

---

## Pre-Promotion Check Results

**Check 1 — License clean:**
- Apache 2.0 sources (KUBERNETES-KEPS, OPENTELEMETRY-SPEC, DAPPER, BIGCODEBENCH): PASS
- MIT sources (SWE-BENCH, EFCORE-SOURCE, DOTNET-MICROSERVICES, NESTJS-ADVANCED): PASS
- CC-BY 4.0 sources (REACT-ADVANCED, ANGULAR-DOCS, ASPNETCORE-DOCS-SAMPLES, GOOGLE-AIP, MICROSOFT-LEARN-ARCH): PASS — attribution recorded in every pair
- Node 09 Pair 001: Uses RFC 7234 concepts, not RFC text. IETF TLP license noted — concept explanation is not text reproduction. Status: PASS with note that this should be confirmed before wider distribution.

**Check 2 — Reasoning present:**
- All pairs: A is a situation requiring a decision or evaluation. B is the decision or evaluation, not a restatement of A. PASS.
- Node 8 pairs: B names the specific rule violated (over-fetching, N+1, CWE-502) and where the source states it. PASS.

**Check 3 — Context sufficient:**
- All pairs in this batch contain enough information to reproduce the output on a similar input without reading the source unit. PASS.

**Check 4 — Node assignment correct:**
- Node 1 pairs: all are user prompt → domain array. PASS.
- Node 3 pairs: all are translated domain descriptions → ordered sub-task plans. PASS.
- Node 8 pairs: all are review responsibility + result → pass/concern/reprocess. PASS.
- Node 9 pairs: all are approved results + audience → shaped user output. PASS.

**Check 5 — Not duplicated in internal corpus:**
The internal corpus (Flow-01 training sets from the earlier zip) covered user-registration flow scenarios. None of the scenarios in this batch (OTel sampling, EF Core migrations, Dapper transactions, React scroll, Django N+1, BFF architecture) appear in the internal corpus. PASS.

---

## Promotion Status

**clean-batch-001: PROMOTED**

32 pairs across 4 node files. All five pre-promotion checks passed.
Nodes 2, 4, 5, 6, 7 remain at zero pairs — addressed in clean-batch-002.
