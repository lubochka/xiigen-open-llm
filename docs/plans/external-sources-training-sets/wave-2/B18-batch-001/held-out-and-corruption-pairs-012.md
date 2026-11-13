# Held-Out and Corruption Pairs (012)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-012-001 — HO: "reorganize" → IMPLEMENT, refactoring, 4.1

**Request:** "Reorganize the domain service layer to separate order fulfillment logic from payment processing."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1
- Flow: BackendServiceFlow41

**Reasoning:**
- "reorganize" → IMPLEMENT, refactoring (refactoring vocabulary)
- "domain service layer" → 4.1 vocabulary
- "order fulfillment" + "payment processing" → service vocabulary (NOT 4.4 data vocabulary)
- No "structure", "architecture", "pattern", "design" → ARCHITECTURE does NOT fire
- TransitionType: refactoring

**Note:** "reorganize" is a refactoring verb. It does not create new functionality — it restructures existing code organization.

---

## B18-HO-012-002 — COR: "reorganize" → feature_addition (v6-02)

**Request:** "Reorganize the domain service layer to separate order fulfillment logic from payment processing."

**Corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: **feature_addition** ← WRONG
- Sub-phase: 4.1 (accidentally correct)

**Corruption type:** v6-02 — "reorganize" treated as creating new service structure
**Corrupted reasoning:** "separating order fulfillment from payment processing creates two new logical components — this is adding new structure (feature_addition)"

**Why wrong:** "reorganize" is in the refactoring vocabulary. Separating existing logic into distinct modules is reorganization/extraction — not feature addition. TransitionType: refactoring.

---

## B18-HO-012-003 — HO: "move" (code) → IMPLEMENT, refactoring, 4.1

**Request:** "Move the payment validation logic from the OrderService to the PaymentService class."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1
- Flow: BackendServiceFlow41

**Reasoning:**
- "move" (code/logic target) → IMPLEMENT, refactoring
- "payment validation logic" → code artifact → 4.1
- "OrderService", "PaymentService" → service vocabulary → 4.1
- No data vocabulary → NOT 4.4
- No architecture triggers → ARCHITECTURE does NOT fire
- TransitionType: refactoring

**Critical distinction:** "move code/logic" → refactoring (4.1). "move data/records" → feature_addition (4.4). The target noun determines routing.

---

## B18-HO-012-004 — HO: "clean up" → IMPLEMENT, refactoring, 4.1

**Request:** "Clean up the legacy payment service by removing dead code and unused imports."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1
- Flow: BackendServiceFlow41

**Reasoning:**
- "clean up" → IMPLEMENT, refactoring (cleanup vocabulary)
- "legacy payment service" → 4.1 service vocabulary
- "dead code" + "unused imports" → code artifact → 4.1
- No migration, no data vocabulary → NOT 4.4
- "legacy" alone (without data vocabulary) → does NOT trigger 4.4

**Note:** "legacy" is a modifier, not a routing signal. It requires data vocabulary ("legacy flat table", "legacy records", "legacy data") to route to 4.4.

---

## B18-HO-012-005 — HO: "clean up" + "migrate" → feature_addition wins for sub-phase

**Request:** "Clean up the legacy order service and migrate the order archive data to the new cold storage schema."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring (Rule 9: "clean up" = refactoring verb; no feature_addition verbs present)
- Sub-phases: 4.1 (service cleanup) + 4.4 (data migration)
- Flows: BackendServiceFlow41 + DataMigrationFlow44

**Reasoning:**
- "clean up" → IMPLEMENT, refactoring → 4.1
- "migrate" + "order archive data" + "cold storage schema" → IMPLEMENT, feature_addition vocabulary → 4.4
- Both sub-phases fire (compound request)
- TransitionType: refactoring (cleanup verb is the refactoring signal; "migrate" does not override Rule 9)
- Both flows run: BackendServiceFlow41 (cleanup) then DataMigrationFlow44 (migration)

---

## B18-HO-012-006 — Summary: Complete B18 held-out vocabulary quick reference

**Refactoring verbs (→ TransitionType: refactoring):**
refactor, extract (code), rename, consolidate (code), restructure, reorganize, move (code), clean up, remove dead code

**Feature addition verbs (→ TransitionType: feature_addition when no refactoring verbs):**
write migration, migrate (data), seed, create fixture, build ETL, implement (new)

**ARCHITECTURE absolute triggers (Rule 3 + Rule 7):**
structure, architecture, pattern, design (verb)

**4.4 routing vocabulary (→ DataMigrationFlow44):**
migrate data/records/rows, ETL, seed, fixture, legacy flat table, legacy data, cold storage, archive data, schema (as migration target)

**4.1 routing vocabulary (→ BackendServiceFlow41):**
service, class, method, repository, controller, domain logic, handler, validator, processor

**SECURITY absolute triggers (Rule 14):**
encrypted, PII, GDPR, credentials, authentication, authorization, access control, sensitive data, token (auth context)

**Rule 9 override:** "refactor" wins over all — even when migration vocabulary is present, the refactoring verb sets TransitionType: refactoring.
