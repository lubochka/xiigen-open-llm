# Held-Out and Corruption Pairs (008)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-008-001 — HO: "rename + restructure" → IMPLEMENT, refactoring

**Request:** "Rename the CustomerController to AccountController and restructure the routing methods to match the new API specification."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1
- Flow: BackendServiceFlow41

**Reasoning:**
- "rename" → IMPLEMENT, refactoring (Rule 19)
- "restructure" → IMPLEMENT, refactoring (Rule 1)
- "routing methods" → 4.1 vocabulary
- "API specification" → NOT API_CONTRACT (it's a reference target, not an integration task)
- No ARCHITECTURE trigger (no "structure", "pattern", "design", "architecture")
- TransitionType: refactoring

---

## B18-HO-008-002 — COR: "restructure" → ARCHITECTURE (wrong)

**Request:** "Rename the CustomerController to AccountController and restructure the routing methods."

**Corrupted classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"] ← WRONG (ARCHITECTURE incorrectly added)
- TransitionType: refactoring
- Sub-phases: 4.1 + 3.1 (3.1 incorrectly added)

**Corruption type:** "restructure" → ARCHITECTURE (treating "restructure" as "structure")
**Corrupted reasoning:** "restructure contains 'structure' → ARCHITECTURE fires"

**Why wrong:** "restructure" is a DIFFERENT TOKEN from "structure". Rule 3 fires on "structure" (the standalone noun), NOT on "restructure" (the compound verb). "restructure" → IMPLEMENT (refactoring vocabulary). "structure" → ARCHITECTURE. They are NOT the same.

---

## B18-HO-008-003 — HO: "restructure" + "architecture" → ARCHITECTURE fires

**Request:** "Restructure the service layer to follow the clean architecture principles."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- Sub-phases: 4.1 + 3.1

**Reasoning:**
- "restructure" → IMPLEMENT, refactoring (the verb)
- "architecture" → ARCHITECTURE fires (Rule 7 — the noun "architecture" fires)
- NOT because of "restructure" containing "structure" — because "architecture" is a separate trigger
- Sub-phase 4.1 (service layer, refactoring vocabulary) + 3.1 (architecture pattern)

---

## B18-HO-008-004 — Disambiguation: "restructure" vs "architecture" in the same request

**The difference:**
- "restructure" → IMPLEMENT (refactoring verb) — does NOT fire ARCHITECTURE
- "architecture" → ARCHITECTURE (architecture noun trigger, Rule 7) — fires ARCHITECTURE

**"Restructure the architecture" → BOTH fire:**
- "restructure" → IMPLEMENT (refactoring)
- "architecture" → ARCHITECTURE (Rule 7)

**"Restructure the service" → only IMPLEMENT fires:**
- "restructure" → IMPLEMENT (refactoring)
- "service" → 4.1 vocabulary, NOT an ARCHITECTURE trigger

---

## B18-HO-008-005 — HO: rename + consolidate + ARCHITECTURE

**Request:** "Rename the payment methods and consolidate validation, following the domain service architecture pattern."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- Sub-phases: 4.1 + 3.1

**Reasoning:**
- "rename" → IMPLEMENT, refactoring
- "consolidate" → IMPLEMENT, refactoring (Rule 20)
- "validation" → 4.1 vocabulary
- "architecture" → ARCHITECTURE fires (Rule 7)
- "pattern" → ARCHITECTURE fires (Rule 7)
- TransitionType: refactoring (rename + consolidate = refactoring verbs)

---

## B18-HO-008-006 — Summary: rename/restructure classification rules

| Request | Predicates | TransitionType | ARCHITECTURE? |
|---|---|---|---|
| "rename class" | [IMPLEMENT] | refactoring | NO (no architecture trigger) |
| "restructure service" | [IMPLEMENT] | refactoring | NO |
| "restructure + architecture" | [IMPLEMENT, ARCHITECTURE] | refactoring | YES ("architecture" fires) |
| "restructure + pattern" | [IMPLEMENT, ARCHITECTURE] | refactoring | YES ("pattern" fires) |
| "restructure + structure" | [IMPLEMENT, ARCHITECTURE] | refactoring | YES ("structure" fires) |
| "rename + document" | [IMPLEMENT, DOCUMENT] | refactoring | NO |
