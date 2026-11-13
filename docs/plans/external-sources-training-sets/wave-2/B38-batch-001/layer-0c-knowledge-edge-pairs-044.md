# Layer 0-C — Knowledge-Edge Pairs (044)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-044-001 — SECURITY lexicon additions for 037-060

SECURITY fires from the following terms in the 037-060 range:
| Term | Fires SECURITY | Notes |
|---|---|---|
| security | Yes | Direct lexicon member |
| CVE (Common Vulnerabilities and Exposures) | Yes | Security identifier |
| vulnerability | Yes | Security noun |
| security advisory | Yes | Security-domain compound |
| security controls | Yes | Security-domain compound |
| security compliance | Yes | Security-domain compound |
| signing keys | No | Context — no SECURITY alone |
| attestation | No | Context |
| provenance | No | Context |
| SBOM | No | Context |

---

## B38-0C-044-002 — SECURITY does not fire from "policy" or "criteria" alone

"Policy" and "criteria" are DOCUMENT-triggering artifacts, not SECURITY triggers.
SECURITY fires from the "security" root word and its domain compounds.

| Sentence | PredicateSet |
|---|---|
| "Document the attestation policy." | ["DOCUMENT"] (document fires; policy is DOCUMENT artifact — no SECURITY) |
| "Audit the attestation security policy." | ["QA","SECURITY"] (audit fires QA; "security" fires SECURITY) |
| "Document the security criteria." | ["DOCUMENT","SECURITY"] (document fires; security fires SECURITY) |
| "Document the compliance criteria." | ["DOCUMENT"] (compliance is not SECURITY; criteria fires DOCUMENT) |

---

## B38-0C-044-003 — "advisory" as dual-predicate trigger

"Security advisory" fires both DOCUMENT (if "document" verb is present) and SECURITY.
"Advisory" alone (without "security") fires only DOCUMENT if "document" verb is present.

| Sentence | PredicateSet |
|---|---|
| "Document the advisory." | ["DOCUMENT"] (advisory = document artifact; no security) |
| "Document the security advisory." | ["DOCUMENT","SECURITY"] (document + security) |
| "Review the security advisory." | ["QA","SECURITY"] (review fires QA; "security" fires SECURITY) |
| "Publish the advisory." | [] → knowledge_edge (publish is gap; advisory alone doesn't fire) |
