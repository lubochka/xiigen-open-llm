# Layer 0-C — Knowledge Edge Pairs (022)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-022-001 — Edge: multiple code predicates in AI context all fire

"design the agent platform, build the skill router, integrate the agent API" → ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
Teaching: Three code verbs, each fires its own predicate independently. AI context ("agent platform", "skill router") does not suppress any predicate.

---

## KE-022-002 — Edge: gap vocabulary mixed with SECURITY fires only SECURITY

"orchestrate the agent workflows and prevent prompt injection in user inputs" → ["SECURITY"]
Teaching: "orchestrate" = gap (skipped). "prevent prompt injection" → SECURITY fires. Only predicates are returned; gap vocabulary is silently skipped when any predicate fires.

---

## KE-022-003 — Edge: TEST fires on AI testing vocabulary only when "write tests" is present

"test the agent's few-shot performance" → gap ("test" as observation/evaluation in AI quality context; no "write tests" code verb)
"write tests for the agent's few-shot retrieval" → ["TEST"] ("write tests" = code verb; TEST fires)
Teaching: "test" as an AI evaluation verb → gap. "write tests" as a code-writing verb → TEST fires. The code-writing form is required to fire TEST.

---

## KE-022-004 — Edge: DOCUMENT fires in AI context

"document the agent's self-healing behavior" → ["DOCUMENT"] ("document" = documentation verb; DOCUMENT fires despite AI vocabulary object)
"document the fine-tuning configuration schema" → ["DOCUMENT"] ("document" fires; "fine-tuning configuration" is AI context only)
Teaching: DOCUMENT fires when "document" verb is present regardless of AI vocabulary in the object.

---

## KE-022-005 — Edge: QA abbreviation fires even in AI context

"QA the agent release" → ["QA"] ("QA" abbreviation fires QA predicate; AI context does not suppress)
"quality assurance for the agent deployment" → gap ("quality assurance" spelled out → gap; only "QA" abbreviation fires the QA predicate)
Teaching: "QA" (abbreviation only) → QA fires. "quality assurance" (spelled out) → gap. This is the B33 QA abbreviation rule.
