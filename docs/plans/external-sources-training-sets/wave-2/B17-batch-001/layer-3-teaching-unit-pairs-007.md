# Layer 3 — Teaching Unit Pairs (007)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-007-001 — SECURITY + AUTHORIZATION both to 3.4: DecisionCount counts predicates

**The core insight:** Two predicates routing to the same sub-phase are still two decisions.

**Rule 9:** SECURITY → 3.4. AUTHORIZATION → 3.4. When both fire, DecisionCount += 2 (not +1).

**Analogy:** Two students submitting to the same classroom are two submissions. The classroom (3.4) receives both. The submission count is 2, not 1.

---

## B17-TU-007-002 — DecisionCount examples showing predicate count ≠ sub-phase count

**Example A:** ["SECURITY", "AUTHORIZATION"]
- Predicates fired: 2
- Sub-phases activated: 1 (3.4)
- DecisionCount: 2
- UniqueSubPhases: 1

**Example B:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
- Predicates fired: 4
- Sub-phases activated: 3 (4.3, 3.3, 3.4)
- DecisionCount: 4
- UniqueSubPhases: 3

**Example C:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
- Predicates fired: 3
- Sub-phases activated: 3 (4.3, 3.3, 3.4)
- DecisionCount: 3
- UniqueSubPhases: 3

Example B has DecisionCount = 4 but UniqueSubPhases = 3. Example C has DecisionCount = UniqueSubPhases = 3. **The two metrics coincide only when no two predicates share a sub-phase.**

---

## B17-TU-007-003 — Why two flows in sub-phase 3.4?

**SecurityAuditFlow34** and **AuthorizationFlow34** are separate flows because:
- They address different concerns (security artifacts vs authorization protocol)
- They produce different outputs (security checklist vs OAuth flow diagram)
- They have different reviewers in a real project (security engineer vs identity engineer)

**Sub-phase 3.4 is a zone, not a single task.** Multiple flows can operate within a zone simultaneously.

---

## B17-TU-007-004 — Practical test: compute DecisionCount correctly

**Input:** "Build the Auth0 OAuth connector, implement token refresh, secure credentials, and write tests."

**Predicates fired:**
1. IMPLEMENT (build, implement) → 1
2. API_CONTRACT (connector, OAuth context) → 1
3. SECURITY (token, secure) → 1
4. AUTHORIZATION (OAuth) → 1
5. TEST (write tests) → 1

**DecisionCount = 5**
**UniqueSubPhases = 4** (4.3, 3.3, 3.4, 5.1 — 3.4 counted once)

**Common wrong answer:** DecisionCount = 4 (miscounting SECURITY+AUTHORIZATION as one decision). Correct: 5.
