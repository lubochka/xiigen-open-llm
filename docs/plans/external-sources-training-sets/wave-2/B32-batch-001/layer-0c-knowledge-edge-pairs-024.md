# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-024-001 — Complete style qualifier taxonomy for B32

**Knowledge edge reference: all B32 style qualifier categories and their gap IDs**

### Category 1: Quality-bar framing (low-polish intent)
Gap ID: gap-vibe-coding-style-001
Terms: scrappy, rough, low-fi, lo-fi, good enough, quick win, fast and loose, scaffold (alone), creative mode

### Category 2: Exploratory framing (investigation intent)
Gap ID: gap-vibe-coding-exploratory-001
Terms: exploratory, explore

### Category 3: Artifact-type without action (noun-only)
Gap ID: gap-vibe-coding-prototype-001
Terms: prototype (alone, no action verb)

### Category 4: Technical investigation framing
Gap ID: gap-vibe-coding-spike-001
Terms: spike, technical spike, run a spike

### Category 5: Casual-assembly framing
Gap ID: gap-vibe-coding-hack-001
Terms: hack together, cobble together, throw together, hack (as verb)

### Category 6: Demonstration framing
Gap ID: gap-vibe-coding-demo-001
Terms: demo (standalone)

### Category 7: Non-UI display artifact
Gap ID: gap-vibe-coding-dashboard-001
Terms: dashboard

### Category 8: Low-fidelity cut framing
Gap ID: gap-vibe-coding-rough-cut-001
Terms: rough cut, rough-cut

### Category 9: Vibe/intuition framing
Gap ID: gap-vibe-coding-intuition-001
Terms: intuition-driven, feel right, feels right, intuitive

### Category 10: Disposability framing
Gap ID: gap-vibe-coding-throwaway-001
Terms: throwaway, throwaway code

### Category 11: Low-quality-bar framing (compound)
Gap ID: gap-vibe-coding-quick-dirty-001
Terms: quick and dirty

### Category 12: Minimal-correctness framing
Gap ID: gap-vibe-coding-just-works-001
Terms: just make it work, just get it working

### Category 13: Low-ambition framing
Gap ID: gap-vibe-coding-nothing-fancy-001
Terms: nothing fancy

---

## B32-0C-024-002 — Style qualifier rule: definitive statement

**The rule in full:**

Style qualifiers in B32 are vocabulary items that describe HOW the developer wants to work (quality bar, methodology, formality level). They do NOT describe WHAT technical action to perform.

The system routes on WHAT (action verbs that fire predicates). It does not route on HOW.

Therefore:
1. Style qualifiers alone → zero predicates → knowledge_edge
2. Style qualifiers + action verbs → predicates fire on action verbs; style qualifiers emit warnings
3. No style qualifier can suppress a registered action predicate
4. Multiple style qualifiers produce multiple gap IDs; SCHEDULE_CAPABILITY_LEARNING still emits once

DomainId for all B32 outputs: "software-development" (never "project-management" or "vibe-coding")

---
