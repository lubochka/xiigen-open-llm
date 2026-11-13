# CWE-TOP25 — Executor Contract
# Stage 1, Document 5 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: MITRE CWE Top 25 2023 — FREE_COMMERCIAL_USE_WITH_NOTICE
# Produced: 2026-05-05

---

## Section 1 — Pre-run checklist

Same as OWASP-ASVS Executor Contract Section 1. Baseline ≥ 1153 required.

---

## Section 3 — Scenario inputs (verbatim)

### S1 Input

"Our REST API implements the following authorization controls for the user
profile service:
- Every request requires a valid JWT, validated for signature (RS256), exp,
  nbf, and aud claims before any processing occurs.
- Profile retrieval (GET /api/users/{userId}/profile) checks that the JWT
  subject claim ('sub') matches the requested userId, OR that the JWT contains
  an 'admin' scope claim. No other combination is accepted.
- Privilege assignment (POST /api/users/{userId}/role) checks that the JWT
  subject matches the requesting user AND that the requesting user's current
  role is 'admin'. An editor cannot grant any role.
- All admin functions log the caller identity and action before execution.
Please assess whether this implementation exhibits any CWE Top 25
authentication or authorization weaknesses."

### S2 Input

"Our profile service endpoint retrieves user profile data:
GET /api/v1/users/{userId}/profile
The endpoint requires a valid JWT token (authentication is required). The
endpoint handler retrieves the profile for the userId in the URL path and
returns it. The handler does not perform any check relating the JWT subject
to the requested userId. The team's reasoning: since authentication is required,
only logged-in users can make this request.
Please assess whether this endpoint has any security weaknesses."

### S3 Input

"Our workspace management API has a member-add endpoint:
POST /api/workspaces/{workspaceId}/members
The endpoint accepts a JSON body with { userId, role } where role can be
'viewer', 'editor', or 'admin'. The endpoint checks that the calling user
has 'editor' or 'admin' role in the workspace before proceeding. An editor
can add a new member with any role, including 'admin'. The endpoint was
implemented this way because editors manage workspace membership in our product.
Is there a security concern with this authorization check?"

---

## Section 4 — What the executor must not do

**Must not use CWE definitions to explain instead of assess.** If the pipeline
produces CWE educational content instead of a verdict for the described endpoint,
record this as FM-CWE-5 and mark S-M as applicable FAIL.

**Must not accept CWE-862 for S3.** If the pipeline names CWE-862 for the S3
scenario, record this as FM-CWE-2 — the authorization check is present, making
CWE-869 or CWE-269 correct.

---

## Section 5 — Evidence file location

`wave-1/CWE-TOP25/stage-2/CWE-TOP25-EXPERIMENT-EVIDENCE-v1.md`

---

## Section 6 — Source authority

| CWE ID | Name | Rank | Source URL |
|--------|------|------|------------|
| CWE-862 | Missing Authorization | #11 | https://cwe.mitre.org/data/definitions/862.html |
| CWE-287 | Improper Authentication | #13 | https://cwe.mitre.org/data/definitions/287.html |
| CWE-306 | Missing Authentication for Critical Function | #20 | https://cwe.mitre.org/data/definitions/306.html |
| CWE-269 | Improper Privilege Management | #22 | https://cwe.mitre.org/data/definitions/269.html |
| CWE-863 | Incorrect Authorization | #24 | https://cwe.mitre.org/data/definitions/863.html |

License attribution for all pairs: "Copyright © The MITRE Corporation. CWE and
CAPEC are registered trademarks of The MITRE Corporation."

---

## Section 7 — Pair production for Stage 3

**Pair ID format:** CB4-3.4-[SEQ] (continuing from NIST-SSDF pairs)
**Primary training goal:** Node 8 CWE-Authz-Arbiter ability to distinguish
absent authorization checks (CWE-862, CWE-306) from incorrect scope checks
(CWE-269, CWE-863), and to name the specific CWE ID in every verdict.

**Node coverage priorities:**
1. Node 8 — CWE-Authz-Arbiter: S2 REPROCESS with CWE-862 named (IDOR pattern)
2. Node 8 — CWE-Authz-Arbiter: S3 CONCERN with CWE-269 named (privilege scope)
3. Node 8 — CWE-Authn-Arbiter: S1 PASS with CWE IDs confirmed absent
4. Node 7 — Arbiter selector: CWE-Authz-Arbiter selected when access control
   vocabulary present

Node 8 pass case (S1) must precede CONCERN and REPROCESS cases per batch
CB4 Node 8 pass-case-first rule.
