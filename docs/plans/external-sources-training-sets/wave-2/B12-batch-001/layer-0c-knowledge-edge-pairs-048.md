# Layer 0-C — Knowledge Edge Pairs (048)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-048-001 — SECURITY multiplicity rule: multiple auth triggers = ONE SECURITY predicate

```
source_id: xiigen-b12-0c-048-001
pair_kind: knowledge_edge
```

### Rule B12-R27 (full statement): SECURITY multiplicity

When multiple SECURITY triggers appear in a single request, CompiledPredicateIds includes
"SECURITY" exactly once. This applies to any combination of:
  - authentication
  - oauth / oauth2
  - secure (as modifier in security context)

Examples:
  "authentication" alone → SECURITY (once)
  "oauth2 authentication" → SECURITY (once — two triggers, one predicate)
  "secure REST API with authentication and OAuth2" → SECURITY (once — three triggers, one predicate)

DecisionCount rule (B12-R23): DecisionCount = COUNT(CompiledPredicateIds).
If CompiledPredicateIds = [API_CONTRACT, DOCUMENT, SECURITY], then DecisionCount = 3.
Even if "authentication" and "OAuth2" both fired SECURITY, it's still ONE predicate = ONE decision.

---

## B12-0C-048-002 — Round 3 knowledge edge summary: rules exercised in files 037–048

```
source_id: xiigen-b12-0c-048-002
pair_kind: knowledge_edge_reference
```

Rules exercised in knowledge-edge files 037–048:

| File | Primary rule |
|------|-------------|
| 037 | rate limiting = domain qualifier; connect ≠ IMPLEMENT; expose = not IMPLEMENT |
| 038 | caching = domain qualifier; ETag contract fires API_CONTRACT via "contract"; design dual-fire |
| 039 | idempotency fires API_CONTRACT (B12-R18); idempotency-key = contract artifact; connect ≠ IMPLEMENT |
| 040 | versioned = qualifier; upgrade ≠ IMPLEMENT; design alone ≠ DOCUMENT |
| 041 | deprecated = lifecycle label (not suppressor); update in doc context = DOCUMENT; implement redirect = IMPLEMENT |
| 042 | API key alone ≠ SECURITY; authentication fires SECURITY (Core Rule 6); connect + auth fires both |
| 043 | OAuth fires SECURITY; SECURITY multiplicity; connect + OAuth = API_CONTRACT + SECURITY (not IMPLEMENT) |
| 044 | tenant-scoped = qualifier; authentication unconditional even in tenant context; tenant alone ≠ SECURITY |
| 045 | routing fires API_CONTRACT; build ≠ design; versioning qualifiers summary |
| 046 | backward compatible = qualifier; upgrade ≠ IMPLEMENT; DOCUMENT multiplicity |
| 047 | auto-generate ≠ IMPLEMENT; build fires IMPLEMENT; integrate ≠ IMPLEMENT; DOCUMENT fires from object |
| 048 | SECURITY multiplicity rule full; Round 3 summary |

---

## B12-0C-048-003 — New rules confirmed in Round 3 (files 037–048)

```
source_id: xiigen-b12-0c-048-003
pair_kind: knowledge_edge_reference
```

New rules confirmed in Round 3:

B12-R28 (rate limiting qualifier): "rate limiting", "throttle", "throttling" → domain qualifiers
only. API_CONTRACT fires from accompanying API vocabulary.

B12-R29 (caching qualifier): "caching", "cache-control", "ETag", "cache invalidation" → domain
qualifiers only. API_CONTRACT fires from API vocabulary co-occurrence.

B12-R30 (versioned endpoint qualifier): "versioned", "multi-version", "backward compatible",
"deprecated" → domain/lifecycle qualifiers. API_CONTRACT fires from endpoint/api/contract terms.

B12-R31 (upgrade ≠ IMPLEMENT): "upgrade" is a lifecycle/migration term. Does NOT fire IMPLEMENT.
Explicit construction verb required.

B12-R32 (API key ≠ SECURITY alone): "API key" does not fire SECURITY independently.
"Authentication" keyword required for SECURITY (Core Rule 6: unconditional).

DOCUMENT multiplicity: multiple DOCUMENT triggers in one request → ONE DOCUMENT predicate.
Same rule as SECURITY multiplicity (B12-R27).
