# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-034-001 — SecurityAdvisory context boundary

### SecurityAdvisory terms that fire SECURITY (not QA)
| Term | Category | Notes |
|---|---|---|
| security advisory | Advisory noun | Fires SECURITY; advisory is DOCUMENT artifact if "document" verb present |
| vulnerability | Security noun | Fires SECURITY |
| CVE (Common Vulnerabilities and Exposures) | Security identifier | Fires SECURITY |
| patch | Fix noun | No predicate (fix context) |
| mitigation | Fix noun | No predicate |
| exposure | Risk noun | No predicate |

### SecurityAdvisory without QA trigger: SECURITY alone
| Input | Analysis |
|---|---|
| "Publish the security advisory." | "Publish" gap; "security advisory" → SECURITY → ["SECURITY"] |
| "Register the CVE in the database." | "Register" not in lexicon; CVE → SECURITY → ["SECURITY"] |
| "Issue the security patch." | "Issue" not in lexicon; "patch" context → [] → knowledge_edge |

---

## B38-0C-034-002 — "audit" + "security" vs. "audit" alone

"Audit" alone fires QA only (no SECURITY without "security" co-present).
"Audit" + "security" (or CVE) fires both QA and SECURITY.

| Sentence | PredicateSet |
|---|---|
| "Audit the release process." | ["QA"] ("audit" fires QA; no "security" present) |
| "Audit the security controls." | ["QA","SECURITY"] (audit fires QA; "security" fires SECURITY — rule 19) |
| "Audit the CVE notification." | ["QA","SECURITY"] (audit fires QA; CVE fires SECURITY) |
| "Audit the migration guide." | ["QA"] (audit fires QA; no security context) |

---

## B38-0C-034-003 — SecurityAdvisory: "publish" is a gap verb

"Publish" is confirmed gap in B38 across all contexts.
Even when co-present with SECURITY-domain nouns, "publish" does not fire any predicate.
SECURITY fires from the domain noun, not from "publish".

| Sentence | PredicateSet |
|---|---|
| "Publish the security advisory." | ["SECURITY"] (security fires; publish is gap) |
| "Publish the CVE notification." | ["SECURITY"] (CVE fires SECURITY; publish is gap) |
| "Publish the release criteria." | [] (publish gap; criteria is a DOCUMENT-trigger only with "document" verb) |
