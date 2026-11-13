# Held-Out and Corruption Pairs (024)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-024-001 — Complete held-out corruption reference: v6-01 (ARCHITECTURE suppressed)

**v6-01 corruption pattern: "structure" present but ARCHITECTURE absent**

**All of these should fire ARCHITECTURE (and a v6-01 classifier misses them):**

| Request fragment | ARCHITECTURE trigger | v6-01 error |
|---|---|---|
| "address data structure" | "structure" | Treats as data vocabulary |
| "index structure" | "structure" | Treats as database context |
| "session storage structure" | "structure" | Treats as infrastructure |
| "preference schema structure" | "structure" | Treats as data schema |
| "billing schema structure" | "structure" | Treats as financial domain |
| "key rotation schema structure" | "structure" | Treats as key management |
| "audit log schema structure" | "structure" | Treats as log format |
| "normalized structure" | "structure" | Treats as data normalization |

**Rule 3 is absolute. No domain context suppresses "structure" → ARCHITECTURE.**

---

## B18-HO-024-002 — Complete held-out corruption reference: v6-02 (wrong TransitionType)

**v6-02 corruption: refactoring verb present but TransitionType: feature_addition output**

| Request fragment | Refactoring verb | Correct TransitionType | v6-02 error reason |
|---|---|---|---|
| "consolidate product_variant tables" | consolidate | refactoring | "new table created" |
| "restructure billing tables" | restructure | refactoring | "new table created" |
| "rename audit columns" | rename | refactoring | "column renamed = new schema" |
| "refactor AddressService + migrate" | refactor | refactoring | migrate overrides refactor |
| "consolidate repository classes" | consolidate | refactoring | "GenericRepository<T> is new" |

**Rule 9 (and Rule 1 for non-refactor verbs): action verb determines TransitionType, not output artifact.**

---

## B18-HO-024-003 — Complete held-out corruption reference: v6-03 (wrong sub-phase routing)

**v6-03 corruption: data vocabulary routed to 4.1 (BackendServiceFlow41) instead of 4.4**

| Request fragment | Vocabulary | Correct sub-phase | v6-03 error |
|---|---|---|---|
| "seed user_preferences" | seed | 4.4 | Routed to 4.1 (all IMPLEMENT = 4.1) |
| "fixture for search_index" | fixture | 4.4 | Routed to 5.x (fixture = test data) |
| "ETL pipeline for events" | ETL | 4.4 | Routed to 4.3 (ETL = API pipeline) |
| "populate product_variants" | populate | 4.4 | Routed to 4.1 (unknown vocabulary) |
| "rebuild search index" | rebuild data | 4.4 | Routed to 4.1 (rebuild = refactor) |

---

## B18-HO-024-004 — Complete held-out reference: disambiguation test questions

**Ten disambiguation test questions covering all 013-024 domains:**

1. "normalize address data" → ARCHITECTURE fires? **No.** "normalize" is not a trigger.
2. "address data structure" → ARCHITECTURE fires? **Yes.** "structure" absolute trigger.
3. "ETL analytics events" → sub-phase 4.3 or 4.4? **4.4.** ETL is data migration vocabulary.
4. "fixture for integration testing" → TEST fires? **No.** Noun form; no "write/add/create tests".
5. "credentials hashed" → SECURITY fires? **Yes.** "credentials" + "hashed" both fire SECURITY.
6. "API keys" alone → SECURITY fires? **No.** "API keys" alone is not a security trigger.
7. "structured audit log" → ARCHITECTURE fires? **No.** "structured" is adjective, not "structure" noun.
8. "refactor + migrate" → TransitionType? **refactoring.** Rule 9 absolute.
9. "seed + write tests" → how many predicates? **Two.** IMPLEMENT (seed) + TEST (write tests).
10. "billing API contract" + "billing records" → sub-phases? **4.4 + 4.3.** Both vocabulary sets fire.

---

## B18-HO-024-005 — Complete held-out reference: five critical rules (never forget)

**The five rules that cause the most held-out failures:**

1. **"structure" (noun) ALWAYS fires ARCHITECTURE.** No exception. No compound noun suppresses it.
2. **"refactor"/"consolidate"/"restructure"/"rename" ALWAYS sets TransitionType: refactoring.** The result artifact does not override the action verb.
3. **"seed"/"fixture"/"populate" ALWAYS routes to 4.4.** Never 5.x (test), never 4.1 (service).
4. **"ETL"/"transform"/"load" (data) ALWAYS routes to 4.4.** Never 4.3 (API connector).
5. **SECURITY triggers (PII/GDPR/encrypted/credentials/authentication/compliance) ALWAYS fire.** No context suppresses them. Not the domain. Not the operation type.
