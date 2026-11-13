# Held-Out and Corruption Pairs (007)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-007-001 — HELD-OUT: "security compliance" + approve → QA+SECURITY

Input: "Validate security compliance status and approve the release after the audit."

Correct result:
- "Validate" → QA
- "security" → SECURITY
- "compliance" → QA (also SECURITY context support)
- "approve" → QA
- "audit" → QA (release context) + SECURITY (security context — rule 19: security + audit → SECURITY co-fires)
- PredicateSet: ["QA", "SECURITY"]
- CapabilityGapWarnings: []
- TransitionType: no_contribution
- Phase routing: 3.4 + 7.1

Gate B (held-out assertion): PredicateSet == ["QA","SECURITY"] AND "IMPLEMENT" not in PredicateSet

---

## B38-HO-007-002 — CORRUPTION: SECURITY suppressed

Corrupt input processing:
- "Validate" → QA ✓
- "security" → context word (WRONG — security fires SECURITY predicate)
- "compliance" → QA ✓
- "approve" → QA ✓
- "audit" → QA only (WRONG — with "security" present, audit also fires SECURITY)

Corrupted result:
- PredicateSet: ["QA"] (WRONG — SECURITY should be present)
- Phase routing: 7.1 only (WRONG — 3.4 missing)
- DecisionCount: 1 (WRONG — should be 2)

Why this is wrong:
"security" is a SECURITY predicate trigger. When present, SECURITY fires.
Rule 19: "audit" + "security" together → SECURITY co-fires with QA.
The word "security" in a release-gate sentence fires SECURITY predicate.
There is no B38 rule that suppresses SECURITY.

---

## B38-HO-007-003 — HELD-OUT: "compliance" in multiple contexts

"Ensure compliance with the release standards." → ["QA"] only (compliance fires QA; no "security")
"Ensure security compliance." → ["QA","SECURITY"] (compliance + security → both fire)
"Validate compliance before the go-live audit." → ["QA","SECURITY"] (compliance + audit in security context → SECURITY fires; go-live = gap)
