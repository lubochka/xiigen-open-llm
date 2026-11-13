# Layer 3 — Teaching Unit Pairs (017)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-017-001 — "low-fi" and "lo-fi": fidelity-level qualifiers

**Teaching unit:** "Low-fi" (and its abbreviated form "lo-fi") communicates that the user wants a low-fidelity result. Neither form is an action verb. Both map to gap-vibe-coding-style-001.

**Core rule:**
```
"low-fi" alone → gap-vibe-coding-style-001 → zero predicates
"lo-fi" alone → gap-vibe-coding-style-001 → zero predicates
"low-fi" + action verb → predicate fires + gap-style-001 warning
```

---

## B32-L3-017-002 — Examples: low-fi alone vs low-fi + action verb

**Zero-predicate (low-fi alone):**

| Request | Predicates | Notes |
|---|---|---|
| "Low-fi version" | [] | "version" is not an action |
| "Low-fi onboarding flow" | [] | Domain noun; no action |
| "Low-fi prototype" | [] | prototype alone + low-fi = two gap IDs |
| "Keep it lo-fi" | [] | "keep" is not IMPLEMENT |

**Predicate-firing:**

| Request | Predicate | Notes |
|---|---|---|
| "Build a low-fi version" | IMPLEMENT | "build" fires |
| "Lo-fi implementation" | IMPLEMENT | "implementation" root fires |
| "Low-fi + page build" | IMPLEMENT + UI | "build" + "page" fire |
| "Low-fi wireframe design" | ARCHITECTURE | "design" fires |

---

## B32-L3-017-003 — Fidelity framing never changes routing logic

**Teaching:** The system routes identically whether the user asks for "high-quality implementation" or "low-fi implementation". Both fire IMPLEMENT. The fidelity qualifier is a gap warning for learning purposes only.

**Analogy:** A request for "build a car" and "build a rough car" both tell a factory to build a car. The "rough" qualifier does not change the factory's process — it just flags a quality preference.

---
