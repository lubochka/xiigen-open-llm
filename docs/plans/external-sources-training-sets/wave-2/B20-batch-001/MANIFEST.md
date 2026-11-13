# MANIFEST — B20-batch-001
# Scenario: B20 — Unit test design for billing calculation
# Batch type: STANDARD (TEST predicate; nine-node flow)
# Production date: 2026-05-05
# Gate B status: ACCEPTED
# Expansion date: 2026-05-05 | Expansion status: COMPLETE (101 files)
# Extension date: 2026-05-06 | Extension status: COMPLETE (201 files)

## File inventory

### Layer 0-A: Compiler pairs (24 files)
| File | Type |
|------|------|
| layer-0a-compiler-pairs.md | compiler (original) |
| layer-0a-compiler-pairs-002.md through -012.md | compiler (expansion wave-1) |
| layer-0a-compiler-pairs-013.md through -024.md | compiler (extension wave-2: CartCalculation through predicate boundary) |

### Layer 0-B: Router pairs (24 files)
| File | Type |
|------|------|
| layer-0b-router-pairs.md | router (original) |
| layer-0b-router-pairs-002.md through -012.md | router (expansion wave-1) |
| layer-0b-router-pairs-013.md through -024.md | router (extension wave-2: CartCalculation through run≠TEST boundary) |

### Layer 0-C: Knowledge edge pairs (24 files)
| File | Type |
|------|------|
| layer-0c-knowledge-edge-pairs.md | knowledge_edge (original) |
| layer-0c-knowledge-edge-pairs-002.md through -012.md | knowledge_edge (expansion wave-1) |
| layer-0c-knowledge-edge-pairs-013.md through -024.md | knowledge_edge (extension wave-2: run≠TEST, check/confirm/ensure gaps) |

### Layer 0-D: Flow factory pairs (24 files)
| File | Type |
|------|------|
| layer-0d-flow-factory-pairs.md | factory_selection (original) |
| layer-0d-flow-factory-pairs-002.md through -012.md | factory_selection (expansion wave-1) |
| layer-0d-flow-factory-pairs-013.md through -024.md | factory_selection (extension wave-2: zero-predicate factory behaviour) |

### Layer 1: Transition orchestrator pairs (24 files)
| File | Type |
|------|------|
| layer-1-transition-orchestrator-pairs.md | transition_orchestrator (original) |
| layer-1-transition-orchestrator-pairs-002.md through -012.md | transition_orchestrator (expansion wave-1) |
| layer-1-transition-orchestrator-pairs-013.md through -024.md | transition_orchestrator (extension wave-2: documentation vs feature_addition) |

### Layer 2: Nine-node flow pairs (24 files)
| File | Type |
|------|------|
| layer-2-nine-node-5-1-pairs.md | nine_node_flow UnitTestingFlow51 (original) |
| layer-2-nine-node-5-1-pairs-002.md through -012.md | nine_node_flow (expansion wave-1, 11 services) |
| layer-2-nine-node-5-1-pairs-013.md through -024.md | nine_node_flow (extension wave-2: 10 new services + probationary rejection + rule summary) |

### Layer 3: Teaching unit pairs (24 files)
| File | Type |
|------|------|
| layer-3-teaching-unit-pairs.md | teaching_unit (original) |
| layer-3-teaching-unit-pairs-002.md through -012.md | teaching_unit (expansion wave-1) |
| layer-3-teaching-unit-pairs-013.md through -024.md | teaching_unit (extension wave-2: new billing domains) |

### Held-out and corruption pairs (24 files)
| File | Type |
|------|------|
| held-out-and-corruption-pairs.md | held_out_corruption (original) |
| held-out-and-corruption-pairs-002.md through -012.md | held_out_corruption (expansion wave-1) |
| held-out-and-corruption-pairs-013.md through -024.md | held_out_corruption (extension wave-2: new domains + run≠TEST corruptions) |

### Cross-layer synthesis (8 files)
| File | Type |
|------|------|
| cross-layer-synthesis-001.md through -004.md | cross_layer_synthesis (wave-1) |
| cross-layer-synthesis-005.md through -008.md | cross_layer_synthesis (wave-2: documentation, run≠TEST, feature_addition, four-predicate) |

## File count
Total files: 201 | Original: 9 | Expansion wave-1: 88 | Synthesis wave-1: 4 | Extension wave-2: 96 | Synthesis wave-2: 4

## Approximate pair count
Layer 0-A: ~110 pairs | Layer 0-B: ~110 pairs | Layer 0-C: ~120 pairs
Layer 0-D: ~110 pairs | Layer 1: ~110 pairs | Layer 2: ~24 flows (avg 7–14 tests each)
Layer 3: ~110 teaching units | Held-out: ~110 pairs | Cross-layer: 8 traces
**Total pairs: ~910**

## Nine-node flows covered
| File | Service | Tests | Verdict |
|------|---------|-------|---------|
| -pairs.md (original) | PaymentCalculationService | 12 | PASS_WITH_WARNINGS |
| -002 | DiscountCalculationService | 14 | PASS |
| -003 | CouponService | 10 | PASS |
| -004 | TaxCalculationService | 12 | PASS |
| -005 | InventoryService | 11 | PASS_WITH_WARNINGS |
| -006 | UserRegistrationService | 13 | PASS |
| -007 | RefundService | 11 | PASS |
| -008 | OrderValidationService | 16 | PASS |
| -009 | ShippingCostCalculator | 14 | PASS |
| -010 | EmailDispatcherService | 10 | PASS |
| -011 | LoyaltyPointsService | 12 | PASS |
| -012 | SubscriptionRenewalService | 10 (TDD) | PASS |
| -013 | CartCalculationService | 5 | PASS |
| -014 | InventoryAllocationService | 5 | PASS |
| -015 | DiscountEngine | 7 | PASS |
| -016 | TaxCalculationService (multi-jurisdiction) | 7 | PASS |
| -017 | ShippingCostCalculator (weight-tier) | 7 | PASS_WITH_WARNINGS |
| -018 | LoyaltyPointsService (tier multiplier) | 7 | PASS |
| -019 | FraudScoreCalculator (TEST+IMPLEMENT) | 7 | PASS |
| -020 | PasswordValidator (TEST+SECURITY) | 9 | PASS |
| -021 | JwtTokenBuilder (TEST+SECURITY) | 7 | PASS |
| -022 | PriceFormatter | 7 | PASS |
| -023 | CouponApplicator — PROBATIONARY REJECTED | 1 (partial) | REJECTED |
| -024 | Rule summary (no service) | — | reference |

## Extension wave-2 teaching points (new in 013–024)
1. "run tests" / "run unit tests" / "run the test suite" → NEVER fires TEST (execution verb boundary)
2. "execute", "trigger", "launch", "invoke", "start" are all execution verbs — same boundary as "run"
3. TEST alone → TransitionType documentation (no IMPLEMENT construction verb present)
4. TEST + IMPLEMENT → feature_addition (construction verb elevates TransitionType)
5. TEST + SECURITY → feature_addition when two predicates combine (multi-predicate = feature_addition)
6. "security tests" = two predicates simultaneously: TEST (via "tests") + SECURITY (via "security")
7. SECURITY in testing context routes to 5.2, not Phase 1-3 design sub-phases
8. "fraud", "compliance", "risk" are domain nouns — they do NOT fire SECURITY
9. "confirm", "ensure", "make sure", "confirms" are informal verification — gap (same family as "check")
10. Zero predicates → zero flow instances → factory returns empty list → knowledge_edge
11. Probationary flow: QA+TEST mixture rejected at ArbiterUnit quality gate
12. "testable" (adjective) does not fire TEST; only noun/verb/gerund forms fire TEST

## Key teaching points (expanded)
1. "check" does NOT fire TEST; "verify", "assert", "validate" always fire TEST
2. "validate" fires TEST in ALL contexts — data validation, business rules, test assertions — universally
3. "fix" does NOT fire IMPLEMENT; "apply", "write", "create", "build", "develop", "extend" do
4. "testable" (adjective) does NOT fire TEST; "test" (noun/verb/gerund) does
5. "regression"/"mutation"/"acceptance"/"flaky" alone are gap; compound with "test" fires TEST
6. "code coverage" is gap; "test coverage" fires TEST via "test"
7. "QA" abbreviation fires QA; "quality assurance" spelled out is gap
8. BDD (Behaviour-Driven Development) Given/When/Then syntax without "test" fires zero predicates
9. Multiple TEST triggers → ONE TEST predicate (deduplication rule)
10. All morphological forms of "test" (test, tests, testing, tested, test-driven) fire TEST
11. TransitionType belongs to IMPLEMENT only: feature_addition / refactoring / documentation
12. SECURITY in test-writing context → 5.2; SECURITY in audit context → 6.3
