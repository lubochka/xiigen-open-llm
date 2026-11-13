# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-023-001 — SubscriptionBilling: "billing data" → 4.4 routing edge

**SubscriptionBilling routing edge map:**

| Token | Edge type | Target |
|---|---|---|
| "migrate" | predicate edge | IMPLEMENT |
| "subscription billing records" | routing vocabulary edge | 4.4 sub-phase |
| "billing_history table" | routing vocabulary edge | 4.4 sub-phase |
| No "structure" | (absent) | ARCHITECTURE edge inactive |

**TransitionType edge:** feature_addition

---

## B18-0C-023-002 — SubscriptionBilling: "billing API contract" fires 4.3 routing edge in addition to 4.4

**When both 4.4 and 4.3 vocabulary appear:**
- "billing data" → 4.4 routing edge
- "billing API contract" → 4.3 routing edge (API contract is integration vocabulary)
- Both edges fire; router activates both flows
- Neither edge suppresses the other

**Knowledge graph rule:** routing edges are additive. Multiple sub-phase edges result in multiple flow activations.

---

## B18-0C-023-003 — SubscriptionBilling: "write integration tests" fires TEST edge (verb form required)

**TEST predicate edge firing:**
- "write integration tests" → TEST predicate edge fires (Rule 12, verb form)
- "write" + "tests" = required verb+noun pattern
- "integration tests" alone (noun) = no TEST edge

**Result for SubscriptionBillingMigration + write integration tests:**
- IMPLEMENT + TEST predicates
- DataMigrationFlow44 + UnitTestingFlow51 routes
- UnitTestingFlow51 hard dependency on DataMigrationFlow44

---

## B18-0C-023-004 — SubscriptionBilling: "restructure" on billing tables → refactoring + 4.4

**"restructure billing tables" edge trace:**
- "restructure" → IMPLEMENT predicate edge
- "restructure" → refactoring TransitionType edge (Rule 1)
- "billing tables" → 4.4 routing edge (tables = database vocabulary)
- "merge monthly and annual" → consolidation vocabulary; refactoring reinforcement

**Result:** IMPLEMENT, refactoring, 4.4 — DataMigrationFlow44 is the target flow even for refactoring-typed data table restructuring.

---

## B18-0C-023-005 — SubscriptionBilling: knowledge graph complete edge summary

**SubscriptionBilling knowledge graph edge summary (all scenarios):**

| Request type | Predicate edges | TransitionType edge | Sub-phase routing edges |
|---|---|---|---|
| Migrate billing records | IMPLEMENT | feature_addition | 4.4 |
| Migrate + API contract | IMPLEMENT | feature_addition | 4.4 + 4.3 |
| Schema structure present | IMPLEMENT + ARCHITECTURE | feature_addition | 4.4 + 3.1 |
| Write integration tests | IMPLEMENT + TEST | feature_addition | 4.4 + 5.x |
| Restructure billing tables | IMPLEMENT | refactoring | 4.4 |
| Migrate + restructure | IMPLEMENT | refactoring (wins) | 4.4 |
