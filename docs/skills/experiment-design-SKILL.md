# experiment-design-SKILL.md — Design and run experiments correctly

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## When to load

Для RP01 действует [договор45](../plans/replanning-possible-diffs/experimets/45-learning-claim-and-native-acceptance-contract-2026-11-03.md): отдельно задавать TRAIN-предусловие, усвоение предъявленного материала, перенос на отложенные случаи и наблюдённую ошибку. Любой измеренный полезный выигрыш фиксировать по показателю; отсутствие роста общей точности не обнуляет его. Общий перенос требует отдельной квалификации регрессий. Один фактор на опыт: отрицание и вопросительная форма не смешиваются в одном объяснении причины. Нельзя автоматически масштабировать Tier или повторять обучение сверх карточки; нижний исторический Tier-escalation не расширяет бюджет и текущую фазу.
Load for any EXPERIMENT session. Load when a SCALE_UP result is found.
Load when designing a new E-N experiment from scratch.

## Experiment structure — every experiment must have all five

    ExperimentId:         "E-N-NAME" (unique, from EXPERIMENT-RESULTS-MAP.md)
    Baseline:             what the system does without the change
    Enhanced:             what the system does with the change
    Metric:               one named, measurable number (not "better")
    Tier:                 1 (1-10 files), 2 (11-100), 3 (100+)

## Failure classification

    PASSED:      enhanced > baseline, Gate B met
    FAILED:      enhanced <= baseline after Tier 3
    SCALE_UP:    needs more data — algorithm correct, re-run at higher Tier
    ARCH:        architectural observation — algorithm not applicable here
    DATA:        test data problem — fix generator, re-run
    TOOL:        wrong tool for the hypothesis

## Tier escalation rule
FAILED at Tier 2 does not mean the hypothesis is wrong.
It means: escalate to Tier 3 before final verdict.
SCALE_UP flag in EXPERIMENT-RESULTS-MAP = "needs more data, algorithm correct."

## Three critical rules — always active (from DECISIONS-LOCKED.md)

### D-07 — StackDetector (E20) before normalisation
    CORRECT:   StackDetector → RouteClassifier → SemanticNormalizer → FamilyCollapser → LogicExtractor
    WRONG:     SemanticNormalizer → StackDetector (destroys C#/TS/PY markers)

### D-09 — max_weight is dynamic, never a constant ×10
    maxWeight = totalExamples / (numClasses × rareExamples)
    // Example: 1000 examples, 5 domains (D-04), 50 security → maxWeight = 4.0
    // NEVER: const double maxWeight = 10.0;
    // Reason: E44v2 −98% — constant ×10 caused catastrophic forgetting

### D-08 — StratifiedSplit (stratified sampling) for all experiments
    Use StratifiedSplit, not RandomSplit(seed:42).
    Reason: E40 / E42 failed because seed-42 put all rare classes in val-set.
    StratifiedSplit.cs exists in 35.ExperimentsV2/Infrastructure/.

## ProbeNeuralNetwork (ProbeNN) rules
    vocabSize MUST be 22 — not 12, not 10.
    LogicExtractor returns predicates 0-21 (22 total).
    Wrong vocabSize → IndexOutOfRangeException on first TrainStep.

## Locked decisions quick reference (D-01..D-12)
Load docs/DECISIONS-LOCKED.md for full table.
D-01: OperationResult<T>  D-02: Fabric First  D-03: 5-level compression
D-04: Domain names (CODE_Q/BUG/PLAN/ARCH/REFACTOR — short form in code)
D-05: LoRA (Low-Rank Adaptation) 2 MB per project  D-06: 70/30 Replay Buffer  D-07: StackDetector first
D-08: StratifiedSplit  D-09: dynamic max_weight  D-10: BM25 (Best Match 25)+K-NN (K-Nearest Neighbours) fusion
D-11: No external ML (Machine Learning) frameworks  D-12: Domain labels — short in code, long in comments

## E28-E50 lessons
- Bigram (B-gram) models cannot show gradient-based signal (E28-E31, E44, E45 → SCALE_UP)
- Synthetic data with fixed seed can collapse class distribution (E40, E42 → use StratifiedSplit)
- Baseline must be non-trivial: alphabetical sort is not a real baseline (E38 → fixed to E38v2)
- ProbeNN ≠ LoRA proxy: K-NN (K-Nearest Neighbours) has no base/adapter separation
- Self-healing distribution fix can flip PASSED→FAILED if both reach 1.00 trivially (E33)
