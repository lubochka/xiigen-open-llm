# Layer 0-A — Compiler Pairs (048)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-048-001 — Complete QA+DOCUMENT reference: 037-048 changelog/communication batch

This file is a QA reference summary for files 037-048 covering changelog automation, release notes, migration/upgrade guides, deprecation, API (Application Programming Interface) breaking changes, backward compatibility, client/internal communication, security advisories, and CVE (Common Vulnerabilities and Exposures) notifications.

### Predicate pattern summary for 037-048
| File | Primary topic | Key predicate patterns |
|---|---|---|
| 037 | ChangelogAutomation | ["QA"] / ["IMPLEMENT"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","SECURITY"] |
| 038 | ReleaseNoteTemplate | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] |
| 039 | MigrationGuide | ["DOCUMENT"] / ["QA","DOCUMENT"] / ["QA","SECURITY"] / ["IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 040 | UpgradeGuide | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] |
| 041 | DeprecationNotice | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","SECURITY"] |
| 042 | APIBreakingChange | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","SECURITY"] |
| 043 | BackwardCompatibilityNote | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] |
| 044 | ClientCommunication | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT"] / ["QA","IMPLEMENT","SECURITY"] |
| 045 | InternalAnnouncement | ["DOCUMENT"] / ["QA"] / ["QA","DOCUMENT"] / ["IMPLEMENT","DOCUMENT"] |
| 046 | SecurityAdvisory | ["DOCUMENT","SECURITY"] / ["QA","SECURITY"] / ["QA","DOCUMENT","SECURITY"] / ["QA","IMPLEMENT","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 047 | CVENotification | ["DOCUMENT","SECURITY"] / ["SECURITY"] / ["QA","SECURITY"] / ["QA","DOCUMENT","SECURITY"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 048 | QA+DOCUMENT combo reference | Reference file |

---

## B38-0A-048-002 — Key teaching points for 037-048

1. "Changelog", "release note", "migration guide", "upgrade guide", "deprecation notice" are context nouns — no predicate fires from them alone.
2. "Document" verb fires DOCUMENT; "publish", "send", "post", "distribute" are gap verbs — no predicate.
3. "Advisory" as a document artifact fires DOCUMENT; co-present "security" fires SECURITY as well.
4. "CVE" is security-domain vocabulary — it fires SECURITY even without explicit "audit" term.
5. "Criteria" fires DOCUMENT for all qualifiers: acceptance, exit, release, upgrade, deprecation, compatibility, communication, announcement, security, vulnerability.
6. "Service" fires IMPLEMENT always (rule 2) — even in "notification service", "advisory service", "changelog service".
7. SecurityAdvisory and CVENotification scenarios are the primary two-predicate DOCUMENT+SECURITY cases without QA.
8. QA is absolutely last in all multi-predicate scenarios for this batch.
9. "Backward compatibility", "breaking change", "deprecation" are context nouns — no predicate fires directly.
10. IMPLEMENT consolidates: "build" + "service" in same sentence = one IMPLEMENT predicate.

---

## B38-0A-048-003 — Anti-corruption for 037-048

v6-01 (QA suppression): QA must fire from review/approve/verify/validate/sign-off even when changelog, advisory, or CVE nouns are co-present. These nouns are context — never QA suppressors.

v6-03 (predicate collapse): ["QA","IMPLEMENT","DOCUMENT","SECURITY"] for file 039 or 047 MUST produce DecisionCount: 4 (4.1 + 2.1 + 3.4 + 7.1).
