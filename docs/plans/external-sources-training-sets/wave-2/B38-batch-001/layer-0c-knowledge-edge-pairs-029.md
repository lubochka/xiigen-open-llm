# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-029-001 — HotfixTag context boundary

### HotfixTag terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| hotfix | Delivery noun | No predicate |
| tag (noun) | Release label | No predicate |
| emergency release | Release descriptor | No predicate |
| critical fix | Fix descriptor | No predicate |
| expedited | Process adjective | No predicate |
| out-of-band | Release descriptor | No predicate |
| fix | Fix noun | No predicate in isolation |
| rollback | Operational noun | No predicate (context) |
| revert | Operational verb | No predicate (context) |

### HotfixTag inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Tag the hotfix release as critical." | "Tag" (verb) not in lexicon; "critical" context → [] |
| "Flag this release as emergency." | "Flag" not in lexicon → [] |
| "Escalate the hotfix to production." | "Escalate" not in lexicon; "production" context → [] |

---

## B38-0C-029-002 — PatchRelease context boundary

### PatchRelease terms: context only
| Term | Category | Notes |
|---|---|---|
| patch | Fix descriptor | No predicate |
| maintenance release | Release type | No predicate |
| maintenance window | Operational window | No predicate |
| workaround | Fix approach | No predicate |
| regression | Quality issue type | No predicate |
| remediation | Fix action | No predicate (confirmed in extension 013-024) |
| incident | Event noun | No predicate |

### PatchRelease inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Schedule the patch release for next Monday." | "Schedule" not in lexicon → [] |
| "Open the maintenance window for patch 1.0.1." | "Open" not in lexicon → [] |
| "Apply the patch to the production environment." | "Apply" not in lexicon; "production" context → [] |

---

## B38-0C-029-003 — Hotfix and patch: "fix" ambiguity

"fix" as a noun is context — no predicate.
"fix" as a verb is also context — no predicate.
Only surrounding QA/IMPLEMENT vocabulary fires predicates.

| Sentence | PredicateSet |
|---|---|
| "Fix the bug in the payment service." | ["IMPLEMENT"] (service fires — rule 2; "fix" is context) |
| "Review the fix before release." | ["QA"] ("review" fires; "fix" is context) |
| "Fix the deployment." | [] (fix = context; deployment = gap) + gap: deployment |
| "Apply the fix." | [] (apply not in lexicon; fix is context) |
