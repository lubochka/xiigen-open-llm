# Layer 3 — Teaching Unit Pairs (011)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU011-001 — Teach: "input validation" fires SECURITY in enforcement; gap in general description
  "Implement strict input validation." → ["SECURITY", "IMPLEMENT"]
  "The service validates input." → [] (descriptive — gap without enforcement/audit verb)
  Rule: "input validation" fires SECURITY with enforce/implement/audit action verb.

## TU011-002 — Teach: "race condition" alone is gap; TOCTOU vulnerability fires SECURITY
  "Fix the race condition in the InventoryService." → [] (gap — concurrency domain)
  "Audit for TOCTOU race condition vulnerabilities." → ["SECURITY"]
  Rule: "race condition" is concurrency gap. "TOCTOU vulnerability" fires SECURITY.

## TU011-003 — Teach: "security architecture" fires both SECURITY and ARCHITECTURE
  "Review the security architecture of the platform." → ["SECURITY", "ARCHITECTURE"]
  "Design the security architecture." → ["SECURITY", "ARCHITECTURE", "DOCUMENT"] (design → documentation transitionType)
  Rule: "security architecture" fires both predicates simultaneously.

## TU011-004 — Teach: "bug bounty" is gap (organizational process)
  "Set up a bug bounty program." → [] (gap — organizational initiative)
  "Implement a vulnerability disclosure endpoint." → ["SECURITY", "IMPLEMENT"] (code-level)
  Rule: Bug bounty programs are organizational — gap. Code implementations of security features fire SECURITY.

## TU011-005 — Teach: "password policy" fires SECURITY with define/enforce/implement
  "Define a password policy." → ["SECURITY", "ARCHITECTURE"]
  "Enforce the password complexity policy." → ["SECURITY"]
  "Check our password policy." → [] (gap — "check" is gap verb)
  Rule: "password policy" fires SECURITY with security-action verbs. "check" is gap.
