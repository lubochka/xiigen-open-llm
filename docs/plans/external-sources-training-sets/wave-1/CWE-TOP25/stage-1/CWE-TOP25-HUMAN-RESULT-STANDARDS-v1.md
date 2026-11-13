# CWE-TOP25 — Human Result Standards
# Stage 1, Document 4 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: MITRE CWE Top 25 2023 — FREE_COMMERCIAL_USE_WITH_NOTICE
# Produced: 2026-05-05

---

## Scenario S1 — Clean authorization model

**S1-M01** (mandatory): Node 8 final verdict must be PASS.

**S1-M02** (mandatory): Node 8 output must name at least three CWE IDs from
(CWE-306, CWE-862, CWE-269, CWE-287, CWE-863) explicitly in CWE-XXX format,
confirmed as absent. "No weaknesses found" without CWE IDs fails.
Failure mode: FM-CWE-1 (CWE ID absent).

**S1-M03** (mandatory): Node 9 output verdict is PASS, not qualified or hedged.

**S1 passes if:** All three mandatory checks pass.

---

## Scenario S2 — IDOR / CWE-862

**S2-M01** (mandatory): Node 8 final verdict must be REPROCESS.
PASS is the most serious failure — FM-CWE-4 (IDOR pattern missed).
CONCERN instead of REPROCESS — FM-CWE-3 (severity under-assignment).

**S2-M02** (mandatory): Node 8 output must name CWE-862 explicitly. Any other
CWE ID as the primary finding is FM-CWE-2 (category confusion).

**S2-M03** (mandatory): Node 8 output must identify the specific missing check:
the endpoint does not verify that the JWT subject has permission to access the
requested userId. The diagnosis must be specific to the described endpoint —
"IDOR vulnerability" without the specific check name does not pass.
Failure mode if absent: FM-CWE-5 (weakness description without check).

**S2-M04** (mandatory): Node 8 output must state that authentication and
authorization are distinct — authentication does not substitute for authorization.
This is the core training statement for the CWE-862 pattern.
Failure mode if absent: structural gap.

**S2-M05** (mandatory): Node 9 output must contain REPROCESS and name CWE-862
with its CWE Top 25 rank (#11) or equivalent severity signal.
Failure mode: FM-CWE-3 if severity is downgraded.

**S2 passes if:** All five mandatory checks pass.

---

## Scenario S3 — Privilege escalation / CWE-269

**S3-M01** (mandatory): Node 8 final verdict must be CONCERN.
REPROCESS fails — FM-CWE-3 (over-escalation; authorization check exists).
PASS fails — missed the privilege scope concern.

**S3-M02** (mandatory): Node 8 output must name CWE-269, not CWE-862.
CWE-862 in the output for S3 is FM-CWE-2 (category confusion — CWE-862 means
authorization check absent; in S3 the check is present).
Failure mode: FM-CWE-2.

**S3-M03** (mandatory): Node 8 output must confirm CWE-862 is ABSENT (the
authorization check is present). The concern is about privilege scope, not
missing authorization. Omitting CWE-862 absent confirmation is a structural gap.

**S3-M04** (mandatory): Node 8 output must describe the principle of least
privilege violation: the editor can assign admin role, exceeding their own
role level.
Failure mode if absent: FM-CWE-5 (weakness description without check).

**S3-M05** (mandatory): Node 9 output must not contain REPROCESS. CONCERN
must be propagated with the role-ceiling fix recommendation.

**S3 passes if:** All five mandatory checks pass.

---

## Aggregate scoring

| Scenario | Mandatory checks | Passes if |
|----------|-----------------|-----------|
| S1 | M01–M03 | All 3 pass |
| S2 | M01–M05 | All 5 pass |
| S3 | M01–M05 | All 5 pass |

Most critical check: S2-M01 (REPROCESS for confirmed CWE-862) and S3-M02
(CWE-269 not CWE-862 for S3). These two checks together train the primary
CWE training distinction: absent check vs. wrong-scope check.
