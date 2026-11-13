# Layer 0-C — Knowledge Edge Pairs (025)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-025-001 — "show how to" knowledge edge

"show how to [infinitive]" = gap (infinitive is learning target, not requester's action).
"show [person] [existing thing]" = gap (showing = demonstrating = teaching act).
"show and [code verb]" = code predicate fires; "show" becomes gap warning.

Boundary: "show" fires code predicate ONLY when it means "display/output" in a code context ("show the results of the query") — not teaching sense. In human-directed guidance contexts, "show" always = gap.

---

## KE-025-002 — "demonstrate" knowledge edge: no exceptions

"demonstrate" = always gap in human-directed guidance sense.
Unlike "integrate" (which fires even inside guidance frames), "demonstrate" NEVER fires any predicate.
"demonstrate security scanning" → gap (not SECURITY).
"demonstrate writing tests" → gap (not TEST).
"demonstrate integrating APIs" → gap EXCEPT integrate fires separately if the request also contains direct integration action.

---

## KE-025-003 — "introduce" ambiguity: person vs software artifact

Person sense: "introduce [person/team] to [topic]" → gap.
Software sense: "introduce [component/feature/change] into [system]" → IMPLEMENT.
Ambiguous: "introduce [technology] to [system]" → IMPLEMENT (technology = software artifact + "to system" = insertion).
Ambiguous: "introduce [concept] to [team]" → gap (concept = knowledge + "to team" = teaching).

Distinguishing rule: if object is a person/group/concept → gap; if object is a software artifact/feature/change → IMPLEMENT.

---

## KE-025-004 — "brief" knowledge edge

"brief [person/team] on [topic]" → gap always (information sharing = teaching act).
"brief on security" → gap (not SECURITY); "brief on tests" → gap (not TEST); "brief on architecture" → gap (not ARCHITECTURE).
"brief and [code verb]" → code predicate fires; "brief on" = gap warning.

No exceptions: "brief" is never a code-action verb.

---

## KE-025-005 — "familiarize with" knowledge edge: onboard vs general gap

"familiarize [new hire/new team] with" → gap-skills-guidance-onboard-001 (new-hire/orientation context confirmed).
"familiarize [any person] with [system/process]" → gap-skills-guidance-onboard-001 (familiarize = orientation vocabulary).
"familiarize" never fires any code predicate regardless of what follows.
