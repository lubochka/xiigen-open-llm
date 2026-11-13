# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-009-001 — v6 corruption types in B17: overview

**Three v6 corruption types appear in B17:**

| Code | Name | Description |
|---|---|---|
| v6-01 | Predicate invention | Model invents a predicate not triggered by the input (e.g., IMPLEMENT from "integrate", or non-existent INTEGRATION predicate) |
| v6-02 | Gap suppression | Model detects a trigger but fails to fire the predicate (e.g., "token" present but SECURITY not fired) |
| v6-03 | Wrong sub-phase | Model fires the correct predicate but routes to wrong sub-phase (e.g., IMPLEMENT → 4.1 when 4.3 is correct) |

---

## B17-0C-009-002 — v6-01: predicate invention in B17

**Common v6-01 errors:**

1. "integrate" → IMPLEMENT invented (Rule 1 violation: integrate fires API_CONTRACT only)
2. "integrate" → INTEGRATION predicate invented (INTEGRATION predicate does not exist in B17)
3. "connect" → CONNECTIVITY predicate invented (no such predicate)
4. "payment gateway" → PAYMENT predicate invented (no such predicate)

**Gate B for v6-01:** The only valid predicates in B17 are: IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA. Any output containing other predicate names is v6-01.

---

## B17-0C-009-003 — v6-02: gap suppression in B17

**Common v6-02 errors:**

1. "token" present but SECURITY not fired (most common)
2. "OAuth" present but AUTHORIZATION not fired
3. "webhook" present but API_CONTRACT not fired (connector fires API_CONTRACT but webhook not recognized)
4. "authentication" present but SECURITY not fired
5. SECURITY present but not firing SecurityAuditFlow34 (router-level suppression)

**Detection:** v6-02 can be detected by checking that every trigger word in the input has a corresponding predicate in the output. If any trigger word is unmatched, v6-02 occurred.

---

## B17-0C-009-004 — v6-03: wrong sub-phase in B17

**Common v6-03 errors:**

1. IMPLEMENT → 4.1 when "connector" present (should be 4.3)
2. IMPLEMENT → 4.1 when "webhook" present (should be 4.3)
3. API_CONTRACT → 4.3 (should be 3.3; 4.3 requires IMPLEMENT)
4. SECURITY → 4.3 (should be 3.4; SECURITY always → 3.4)
5. TEST → 3.3 (should be 5.1)
6. DOCUMENT → 4.3 (should be 2.1)

**Gate B for v6-03:**
- IMPLEMENT + integration vocabulary → must be 4.3
- API_CONTRACT alone → must be 3.3
- SECURITY → must be 3.4
- AUTHORIZATION → must be 3.4
- TEST → must be 5.1
- DOCUMENT → must be 2.1
- QA → must be 7.x
