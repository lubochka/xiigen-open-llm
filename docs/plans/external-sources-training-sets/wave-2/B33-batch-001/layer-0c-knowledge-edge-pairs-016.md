# Layer 0-C — Knowledge Edge Pairs (016)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-016-001 — Edge: "prompt engineering" vs "build prompt system"

"prompt engineering for the agent" → gap (AI vocabulary; no code verb)
"build the prompt engineering system" → IMPLEMENT (code verb; "system" = software artifact)
"invest in prompt engineering" → gap ("invest" does not fire a code predicate)

---

## KE-016-002 — Edge: "prompt" alone vs "prompt injection"

"prompt engineering" → gap (AI design vocabulary)
"system prompt configuration" → gap (AI configuration vocabulary)
"prompt injection prevention" → SECURITY (injection = attack vector → SECURITY fires)
Teaching: "prompt" alone = gap. Only "prompt injection" (with "injection") → SECURITY. No other "prompt" use fires SECURITY.

---

## KE-016-003 — Edge: "prompt template" as code artifact vs AI configuration

"write a prompt template for customer support" → gap ("write a prompt template" = AI configuration, not code)
"create a prompt template rendering service" → IMPLEMENT (code verb + "service" = software artifact)
Teaching: Writing prompts = AI configuration → gap. Creating/building a service that handles prompts = IMPLEMENT.

---

## KE-016-004 — Edge: "prompt engineering" + DOCUMENT

"document the prompt engineering guidelines for the team" → ["DOCUMENT"] (document = documentation verb → DOCUMENT fires; "prompt engineering" is the subject matter, not a gap trigger since DOCUMENT fires)
Teaching: Code verbs override AI vocabulary in object position. "document the guidelines" fires DOCUMENT; gap is silently skipped.

---

## KE-016-005 — Edge: "prompt engineering" does not suppress TEST

"write tests to validate prompt engineering outputs" → ["TEST"] (code verb fires; "prompt engineering" is the test subject)
"test the prompt engineering workflow" → ["TEST"] ("test" as action verb fires TEST; AI context does not suppress)
Teaching: TEST fires when "write tests" / "test" appears regardless of AI vocabulary in the subject.
