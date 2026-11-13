# Held-Out and Corruption Pairs (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-004-001 — HO: "ETL pipeline" → IMPLEMENT, feature_addition, 4.4

**Request:** "Build an ETL pipeline to extract order data from the legacy warehouse CSV files, transform it to match the new normalized schema, and load it into the analytics database."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**Reasoning:**
- "build" + "ETL pipeline" → IMPLEMENT (build + ETL)
- ETL vocabulary (extract, transform, load) → 4.4 routing signals
- "legacy CSV", "order data", "analytics database" → data vocabulary → 4.4
- No "structure", "pattern", "design" → ARCHITECTURE does NOT fire
- TransitionType: feature_addition (new ETL pipeline artifact)

---

## B18-HO-004-002 — COR: ETL → 4.3 API_CONTRACT (wrong)

**Request:** "Build an ETL pipeline to extract order data from the legacy warehouse CSV files and load it into the analytics database."

**Corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: **4.3** ← WRONG
- Flow: **ConnectorFlow43** ← WRONG

**Corruption type:** "pipeline" confused with connector/integration
**Corrupted reasoning:** "pipeline" → system integration → 4.3 (Connector/Integration sub-phase)

**Why wrong:** "pipeline" in ETL context = data pipeline (4.4). "pipeline" in integration context would be a connector (4.3), but the ETL vocabulary (extract, CSV, load into analytics DB) clearly signals 4.4. No API endpoint, webhook, or external service vocabulary present → NOT 4.3.

---

## B18-HO-004-003 — HO: ETL + "structure" → IMPLEMENT+ARCHITECTURE

**Request:** "Build an ETL pipeline to transform legacy order data to match the new analytics table structure."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1
- Flows: DataMigrationFlow44 + ArchitectureFlow31

**"structure" fires ARCHITECTURE even in ETL context** (Rule 3, absolute).

---

## B18-HO-004-004 — COR: ETL + "structure" → ARCHITECTURE suppressed (v6-01)

**Request:** "Build an ETL pipeline to transform legacy order data to match the new analytics table structure."

**Corrupted classification:**
- predicates: ["IMPLEMENT"] ← WRONG (ARCHITECTURE missing)
- TransitionType: feature_addition
- Sub-phase: 4.4 only
- Flow: DataMigrationFlow44 only (ArchitectureFlow31 missing)

**Corruption type:** v6-01 — "structure" in "table structure" suppressed
**Corrupted reasoning:** "table structure is describing the database layout, not an architectural design"
**Why wrong:** Rule 3 is absolute — "structure" always fires ARCHITECTURE.

---

## B18-HO-004-005 — HO: ETL + analytics API endpoint → 4.4 + 4.3

**Request:** "Build an ETL pipeline to transform product data and load it via the analytics service REST API."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phases: 4.4 (ETL) + 4.3 (REST API endpoint)
- Flows: DataMigrationFlow44 + ConnectorFlow43

**Reasoning:** ETL + "REST API" → both 4.4 (ETL pipeline) and 4.3 (API integration for loading data via the API). Both sub-phases fire when both vocabulary types are present.

---

## B18-HO-004-006 — ETL summary: held-out routing rules

**ETL held-out classification rules:**

| Request pattern | Predicates | Sub-phase | Notes |
|---|---|---|---|
| "build ETL pipeline" alone | [IMPLEMENT] | 4.4 | Standard ETL |
| "ETL + table structure" | [IMPLEMENT, ARCHITECTURE] | 4.4 + 3.1 | "structure" fires |
| "ETL + write tests" | [IMPLEMENT, TEST] | 4.4 + 5.x | Tests after ETL |
| "ETL + REST API loading" | [IMPLEMENT] | 4.4 + 4.3 | API endpoint signal |
| "ETL + document" | [IMPLEMENT, DOCUMENT] | 4.4 + 2.1 | Documentation |
| "ETL pipeline integration" | [IMPLEMENT] | 4.4 (NOT 4.3) | "integration" alone insufficient for 4.3 |
