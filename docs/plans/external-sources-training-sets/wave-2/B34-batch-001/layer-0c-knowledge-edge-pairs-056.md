# Layer 0-C — Knowledge Edge Pairs (056)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-056-001 — "explain" never fires: even with direct code nouns

"Explain the implementation" → gap (explain = always gap; implementation noun = topic)
"Explain the test suite" → gap (explain + test suite noun = topic only)
"Explain the integration architecture" → gap (explain + integration architecture = topic only)
"Explain the security vulnerabilities" → gap (explain + security noun = topic only)
"Explain the deployment pipeline" → gap (explain + pipeline noun = topic only)
Rule: "explain [any noun phrase]" = always gap. No noun phrase, topic, or code artifact noun triggers a predicate through "explain."

---

## KE-056-002 — "present" never fires: even with direct code nouns

"Present the test results" → gap (present = guidance; test results = topic)
"Present the security audit findings" → gap (present + security findings = topic)
"Present the architecture diagram" → gap (present + architecture diagram = presentation act → gap)
"Present the API (Application Programming Interface) contract to clients" → gap (presenting contract ≠ integrating/publishing it)
"Present the implementation roadmap" → gap (present + roadmap = guidance act)
Rule: "present [any noun phrase]" = always gap. No code artifact noun fires a predicate through "present."

---

## KE-056-003 — "discuss" never fires: even with direct code topics

"Discuss the implementation plan" → gap
"Discuss the security vulnerabilities" → gap (discuss ≠ audit; security topic = discussion content)
"Discuss the API contract" → gap (discuss ≠ publish/integrate)
"Discuss the test strategy" → gap (discuss ≠ write tests)
"Discuss the architecture options" → gap (discuss ≠ architect)
Rule: "discuss [any topic]" = always gap. Discussion of any technical topic ≠ performing that technical action.

---

## KE-056-004 — "consult on" never fires: even with direct code topics

"Consult on the implementation approach" → gap (consult ≠ implement)
"Consult on security vulnerabilities" → gap (consult ≠ audit/harden)
"Consult on the API versioning strategy" → gap (consult ≠ publish/integrate)
"Consult on the test architecture" → gap (consult ≠ write tests)
"Consult on the system design" → gap (consult ≠ architect)
Rule: "consult on [any topic]" = always gap. Advisory vocabulary never fires topic predicate alone.

---

## KE-056-005 — complete "guidance vocabulary never fires topic predicate alone" rule

Extended to all Round 3 advisory vocabulary:
seek advice on [X] → gap (not X-predicate)
get feedback on [X] → gap (not X-predicate)
sanity check [X] → gap (not X-predicate)
second opinion on [X] → gap (not X-predicate)
rubber duck [X] → gap (not X-predicate)
sounding board for [X] → gap (not X-predicate)
Rule: X-predicate only fires when requester's DIRECT ACTION on X appears in the same request, not from advisory vocabulary alone.
