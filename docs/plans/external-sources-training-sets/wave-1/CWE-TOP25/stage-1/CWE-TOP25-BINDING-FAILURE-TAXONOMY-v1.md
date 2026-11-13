# CWE-TOP25 — Binding Failure Taxonomy
# Stage 1, Document 2 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: MITRE CWE Top 25 2023 — FREE_COMMERCIAL_USE_WITH_NOTICE
# Produced: 2026-05-05

---

## Five named failure modes

### FM-CWE-1: CWE_ID_ABSENT_FROM_VERDICT

**Description:** Node 8 returns a verdict (PASS, CONCERN, REPROCESS) without
naming the specific CWE IDs that were checked. CWE-based verdicts have no
training value without the CWE ID — the ID is the anchor that connects the
behavioral description to the weakness taxonomy.

**Detection:** Search Node 8 output for CWE-XXX format strings. If none present,
this failure mode applies.

**Classification:** Structural gap in Node 8.

**Training correction:** Pairs where Node 8 must always name the CWE ID(s)
relevant to the scenario, whether the verdict is PASS (confirmed absent),
CONCERN (potential weakness), or REPROCESS (weakness confirmed present).

---

### FM-CWE-2: CWE_CATEGORY_CONFUSION

**Description:** The orchestrator uses the wrong CWE ID to describe the weakness.
Most commonly: using CWE-862 (Missing Authorization) when CWE-863 (Incorrect
Authorization) or CWE-269 (Improper Privilege Management) is the correct match.

**Detection for S3:** Node 8 output contains CWE-862 for a scenario where
an authorization check IS present but its scope is wrong. CWE-862 means the
check is absent. If the check is present but wrong, CWE-863 or CWE-269 is
the correct category.

**Detection for general:** CWE-306 (Missing Authentication for Critical Function)
confused with CWE-287 (Improper Authentication). CWE-306 is specifically for
the case where there is NO authentication at all. CWE-287 is for authentication
that is present but can be bypassed or spoofed.

**Classification:** Judgment gap in Node 8.

**Training correction:** Pairs that show side-by-side CWE definitions with
decision examples: "auth check absent → CWE-306/CWE-862; auth check present
but bypassable → CWE-287/CWE-863; auth check present but scope wrong →
CWE-269."

---

### FM-CWE-3: SEVERITY_UNDER_ASSIGNMENT

**Description:** The orchestrator assigns CONCERN to a confirmed CWE-862 or
CWE-306 finding when REPROCESS is required. CWE-862 (Missing Authorization)
and CWE-306 (Missing Authentication for Critical Function) are both in the
CWE Top 25 with high severity — they require remediation, not just a concern
flag.

**Detection:** Node 8 output contains CONCERN (not REPROCESS) for a scenario
where:
- An API endpoint returns data for a resource without checking that the caller
  has permission to access that specific resource (CWE-862), or
- A critical function executes without any authentication check (CWE-306).

**Classification:** Judgment gap in Node 8.

**Training correction:** Pairs that explicitly calibrate CONCERN vs REPROCESS
for CWE Top 25 authentication/authorization weaknesses. CONCERN is appropriate
for CWE-269 (privilege management scope concern) and CWE-798 (hard-coded
credential concern). REPROCESS is required for CWE-862, CWE-863, CWE-287,
CWE-306 when the weakness is confirmed present.

---

### FM-CWE-4: IDOR_PATTERN_MISSED

**Description:** The orchestrator does not identify the IDOR (Insecure Direct
Object Reference) pattern as CWE-862 when the input describes a pattern where
a caller accesses a resource by ID without the endpoint checking that the caller
has permission for that specific resource ID.

The IDOR pattern has a specific signature: "authenticated caller" + "resource
identified by caller-supplied ID" + "no ownership or permission check on that ID."
This pattern is always CWE-862.

**Detection:** Node 8 returns PASS for an input that contains the IDOR signature.

**Classification:** Judgment gap in Node 8.

**Training correction:** Multiple pairs with IDOR variations:
- User accessing another user's data by ID (canonical)
- User accessing an order by order ID without order-to-user binding check
- User accessing a document by document ID without document permission check
All three produce the same CWE-862 verdict.

---

### FM-CWE-5: WEAKNESS_DESCRIPTION_WITHOUT_CHECK

**Description:** The orchestrator describes a CWE weakness in general terms
("IDOR vulnerabilities occur when...") without checking whether the specific
described implementation exhibits the weakness. A description of a weakness
category is not an arbiter verdict.

**Detection:** Node 8 output contains CWE educational content (definition,
explanation, general examples) without a verdict about the specific input
described.

**Classification:** Structural gap in Node 8 (verdict absent) or judgment gap
(description substituted for verdict).

**Training correction:** Pairs where Node 8 must always state a verdict
(PASS/CONCERN/REPROCESS) for the specific implementation described, not just
acknowledge the existence of the weakness category.

---

## Severity ordering

1. FM-CWE-4 first (IDOR missed = CWE-862 present but undetected = high risk)
2. FM-CWE-3 second (severity under-assignment = confirmed weakness downgraded)
3. FM-CWE-2 third (category confusion = wrong CWE = wrong remediation)
4. FM-CWE-1 fourth (CWE ID absent = no traceability)
5. FM-CWE-5 last (description without verdict = output has form but no decision)
