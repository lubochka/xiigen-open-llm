# CWE-TOP25 — Experiment Evidence
# Stage 2, Document 1 of 1
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Execution date: 2026-05-05
# Pipeline state: phase 64 — InMemoryLogicalOrchestrator, T34 flow only

---

## Section 1 — Baseline

Pre-run: 1160 passing, 0 failing. Floor: 1160.
Post-run: 1160 passing, 0 failing. Baseline holds.

---

## Section 2 — Scenario traces

Same pipeline failure as OWASP-ASVS: no registered flow for
'security-verification-3.4'. All nine nodes produce no output.

---

## Section 3 — Comparison against oracle

All mandatory checks fail (structural gap).

CWE-TOP25 specific critical mandatory checks failed:
- S2-M01: Node 8 verdict must be REPROCESS for CWE-862 IDOR — NOT PRODUCED
- S2-M02: CWE-862 must be named — NOT PRODUCED
- S3-M02: CWE-269 (not CWE-862) must be named for privilege scope — NOT PRODUCED

---

## Section 4 — Verdict per scenario

S1: CONCERN (structural)
S2: CONCERN (structural)
S3: CONCERN (structural)

---

## Section 5 — Gap catalogue

All nine nodes: structural gap.

**Gap-CWE-01 (primary):** Node 8 — CWE-Authz-Arbiter not registered.
The primary training contribution: distinguishing absent authorization check
(CWE-862) from wrong-scope check (CWE-269). This arbiter does not exist.

**Gap-CWE-02 (primary):** Node 8 — CWE-Authn-Arbiter not registered.
The authentication weakness arbiter (CWE-287, CWE-306) does not exist.

**Gap-CWE-03:** Node 7 — CWE arbiter selection not registered.

**Gaps-CWE-04 through -10:** Nodes 1–6, 9 — same structural absence.

---

## Section 6 — Next action

Stage 3 Action 2 — gap closure pass. No existing CWE pairs to correct.

Primary pairs to produce:
1. Node 8 CWE-Authz-Arbiter: S1 PASS — CWE-862, CWE-269, CWE-287, CWE-306 confirmed absent
2. Node 8 CWE-Authz-Arbiter: S2 REPROCESS — CWE-862 (IDOR, missing resource check)
3. Node 8 CWE-Authz-Arbiter: S3 CONCERN — CWE-269 (privilege scope) not CWE-862
4. Node 7: CWE-Authz-Arbiter selection from access-control vocabulary
