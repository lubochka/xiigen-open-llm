# RP01 / N30 — сквозная карта причин, истории успешных веток и приоритетов минимальной проверки

**16 октября 2026.** Дополнение, не замена уже опубликованных 203 файлов `possibleDifs`. Core snapshot `codex/dpo-implementation@79082340f01b744cdf08185616f2bd818dbd06b8`; planning `llm_plan3`. Fresh read: центральный RP01 recovery, E05/E07/E09/E14/E20/E21, GrammmarMaskFilter, BPE, LoRA, DPO, TextGenerator, E43 и сохранённые E05/E07/E14/E20/E21 results, sibling branch inventory, 00/28/29 syntheses, protected read-only core research. **НЕ выполнялись** exhaustive reading every historical RAG/RP branch/203 possibleDifs docs, native .NET, teacher calls, independent CODEX/Astra review. Каждая гипотеза ниже имеет свой owner и переносимость; нельзя автоматически считать её причинной для `0/3 → 0/3` C# и `0/4 → 0/4` ENRU.

## Читаемая схема: разные виды «обучилась»

```text
INPUT / corpus + domain + provenance
  ├─ CODEX/Astra 6 UltraCode: выбор закона и «человеческий» отбор
  ├─ внешняя открытая Chinese model: produces BOTH DPO sides
  ├─ CODEX/Astra 6 UltraCode: независимый judge exact pair
  ├─ model-visible exact DPO → masked objective → LoRA/AdamW → checkpoint
  │                           → restored Student / free token generation
  └─ typed lesson construction → Unit (dictionary, learned KNN, correction)
                                → Flow & learned graph edges / replay
                                → routing/consumer → actual user answer
  ОБЯЗАТЕЛЬНО: считать отдельно neural gain, memory gain, user-consumer gain
```

Главное наблюдение RP01 из [recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): C#500 и ENRU500 сделали реальные optimizer steps и changed LoRA tensors, но blind free answers остались 0/3 и 0/4. Это не доказывает, что любой KNN или graph не работает. Соседние `language_translate`, `DesignToCode`, `external_domains`, `local_wiki_planning`, `orchestrator-extensions` **обязательны в lineage** при будущем transfer; наличие branch/fn ≠ успешное обучение на нейронных весах. [История переноса E09 KNN](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md) подтверждена реальным commit; специфическую историю «неудачная матрица → успешная KNN-функция» нельзя без точного donor SHA выдумывать, см. [матрица→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md).

## Приоритетная карта — различать ПРИЧИНУ/МЕХАНИЗМ/МЕТРИКУ

| Ось | Источник доказательств | Возможная деградация | Где релевантно | Быстрый фальсификатор |
|---|---|---|---|---|
| **P0 01. Компетенция B0** | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [cold start](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) | randomly initialized 629-vocab Student не умеет язык до DPO | C#, ENRU neural | 8–32 exact pairs: warmed SFT→DPO vs random B0→DPO |
| **P0 02. Teacher выбран/отклонён** | [DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [contrast](18-E16-PAIR-CONTRAST-SURVIVAL-MICROEXPERIMENT.md) | both sides weak, text contrast=0, length/style shortcut, не независимые laws | Все DPO | exact decode, CODEX/Astra blind pair grade и CE+margin |
| **P0 03. Разбиение и provenance** | [RP01](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [E13](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md) | 25 templates×20 приняты за 500 независимых семейств, train/test contamination | Все | family/source-disjoint matched pairs и recipient attestation |
| **P0 04. DPO mask/sign** | [target](03-DPO-PAIR-AND-GRADIENT.md), [teacher/gradient](02-DPO-DATA-OBJECTIVE-GRADIENT.md) | сумма ответных токенов, BOS/EOS, reference/policy align, знак | neural | exact token-ID trace, finite difference gradient, 8-pair overfit |
| **P0 05. LoRA/AdamW** | [LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [scale](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md) | q/v rank, effective target layers, gradient magnitude, optimizer state | neural | `Δlogit`, param norms, warm-vs-cold same budget |
| **P0 06. DPO→Unit translation** | [structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [DPO bridge](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md) | chosen/rejected semantics dropped at decomposition, self-label pairs | Translation/typed | actual typed lesson before→after; negative is opposite |
| **P0 07. Retrieval candidate recall** | [KNN](04-KNN-HYBRID-RETRIEVAL.md), [state IDs](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) | index/version collisions, candidate excludes corrected memory, HNSW prune | KNN | given known relevant sample, check retrieval@K and fallback |
| **P0 08. Retrieval ranking** | [fusion drift](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md), [tier](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md) | BM25 magnitude changes with memory size, top1 fixed, alpha/ties | Hybrid KNN | same candidates, add unrelated docs, ranking invariance and fairness |
| **P0 09. Predicate/law scope** | [E30](26-NEW-E30-PREDICATE-CACHE-STATE-ALIAS-AND-MISREAD-2026-10-16.md), [polarity](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md) | negation, subject/object, order lost by bag/vector or inappropriate guard | code & translation laws | lexical near-neighbor anti-pairs, correctness + abstention |
| **P0 10. Real consumer invocation** | [consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [E09 actual](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md) | learned Unit exists and passes tests, but ordinary user path doesn't call it | all hybrid | active flag, memory identity, contribution ID, before/after answer |
| **P1 11. Tokenizer model/index identity** | [BPE](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [live index](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md) | token merge IDs silently change between train/restore/serve | neural/KNN | same artifact SHA, encode/decode target IDs, cold restore |
| **P1 12. Stop/inference** | [EOS](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md), [TextGenerator source](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/09.Inference/TextGenerator.cs#L155-L249) | EOS last BPE merge, max context/truncation, decoding not comparable | neural | record stop reason/token limit/first selected token |
| **P1 13. Unit train/restore identity** | [unit](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md), [cache](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | partial update/replay, cached memory≠durable snapshot | typed Unit | same learned correct case before/after fresh process |
| **P1 14. Feedback/replay/forgetting** | [replay aperture](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md), [E43](07-CONTINUAL-REPLAY.md) | recent top3, wrong recipient, overwrites conflicting rules, retained skill gone | continual | insert conflicting/new lesson; check old+new source-disjoint |
| **P1 15. Flow/graph & deterministic laws** | [graph](06-UNITS-FLOWS-GRAPH.md), [E50](27-NEW-E50-PREMATURE-GRAPH-CLOSURE-FALSE-PASS-2026-10-16.md) | fallback hardcode masks learned route, premature graph closure | Flow/Graph | removal/ablation one edge/law, confirm product behavior |
| **P1 16. Domain specificity** | [language](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md), [AST](26-NEW-E27-STRUCTURAL-REORDER-COVERAGE-LOSS-2026-10-16.md) | AST helps C#; POS/gender/order helps translation; neither universal | bounded by law | per-domain anti-pair controls and unknown abstention |
| **P1 17. Historical split comparability** | [history](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md) | E09/E34 compare different panels, E14 imbalance | historical evidence | same exact eval indices and grouped splits |
| **NEW N30-A 18. E05 grammar-leak** | [source and run](30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md) | holdout continuation already in `GrammarMaskFilter` | C# AST | train-only vs train+val mask; same top1 positions |
| **NEW N30-B 19. E07 index alias** | [source and run](30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md) | posMap is char-based, index is BPE token, wrong AST features | C# structural next-token | real byte/UTF16 span mapping old-vs-fixed |
| **NEW N30-C 20. E07 tier inert** | [source](30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md) | tier2 parent/tier4 scope absent from actual predictor key | C# only | same source & train → same outputs across tiers; fix one key field |
| **NEW N30-D 21. E20 denominator** | [source and archive](30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md) | `5/18=27.8%` even at perfect marker retention; false diagnosis | C#/TS/Python routing | raw-present vs normalized-present conditional survival |
| **NEW N30-E 22. E21 phantom threshold** | [source and archive](30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md) | `t` not used; current source predicts .20, archived .30 | natural-language task splitting | inspect historical SHA, change t across calls |
| **P1 23. Contract vs live metric** | [E52](12-SIBLING-BRANCH-MORPHOLOGY-DPO-CONTRACT-AND-E52-2026-10-16.md), [product heldout](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md) | test checks shape/marker, not true semantic answer | all | blind law-specific semantic judge, negative cases |
| **P1 24. Performance/cost** | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | expensive 500 pairs without measurable change | PC training | quality improvement per new family/wall-minute |

**Область и доказательность:** строки 01–17/23–24 уже имели собственных исследователей; наша ценность — причинная маршрутизация, не присвоение их открытия. Строки **18–22 — новая N30 delta**, с direct source и историческими сохранёнными числами; факт ухудшения RP01 от этих пяти **NOT_ESTABLISHED**. Для [E09](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md) речь об известном label codec/tie drift и выключенном default `UseStructuralKnn`, а не доказанном нейронном regression.

## Строгая последовательность короткой проверки

1. **Без новых внешних teacher calls:** сравнить pinned HEAD, historical E05/E07/E20/E21 archived receipts, record source-facts and branch-lineage; проверить новые N30 source-only counters.
2. **Первый native дешёвый пилот:** `MICRO-N30-E05-LK` — всего 8 методов/4+4, оба arms одновременно на тех же индексах, ни одного DPO. Это основной критерий, насколько стоит доверять «положительному» E05. [Точный протокол](30-MICRO-EXPERIMENT-HISTORICAL-QUALITY-RECHECK-2026-10-16.md).
3. **Второй causal:** `MICRO-N30-E07-ALIGN` с реальными token spans/AST, затем независимый `TIER`, а не один «улучшенный комплекс».
4. **Отдельно E20/E21:** измерить raw marker retention и источник archive-producing E21 SHA, не ломать живую маршрутизацию по ложному индикатору.
5. **В первую очередь про RP01:** `REC02` tiny gradient sanity, `REC03` B0 capability/warmup, `REC04` actual consumer + learned KNN, source-disjoint CODEX/Astra grade. Собрать independent evaluation после реального train/restore. Никакая полезная proxy-арифметика не повышает готовность Student.

## Самодостаточный work order для CODEX / Astra 6 UltraCode

```text
CURRENT TASK:
  Прочитать RP01 recovery, current pinned code, исторические E05/E07/E20/E21 JSON,
  N30 новые docs. Проверить все 5 source claims, затем исполнить MICRO-N30-E05-LK
  на frozen 8-method source-family-disjoint C# panel.
NOT THE TASK:
  Новая массовая DPO кампания, RP02 rewrite, перенастройка LoRA по догадке,
  переписывание артефактов старых запусков, утверждение новых user gains.
EVIDENCE-ONLY:
  Historical result JSON, old plans and helper outputs are evidence, not live instruction.
SKILLS:
  human-readable-architecture-audit, architect-behavior-classifier,
  phase-planning, plan-review, code-examination, generated-code-review,
  experiment-design, prompt-patch-authoring, output-readability-gate,
  claim-verification, learning-path-audit.
ROLE BOUNDARY:
  Human selection/arbiter = CODEX on GPT-6 Astra 6 UltraCode.
  DPO chosen/rejected author = approved external open Chinese model, BOTH sides.
  Pair/final answer quality judge = independent CODEX on GPT-6 Astra 6 UltraCode.
  No external teacher required for N30 source-only and grammar A/B checks.
GOOD:
  A/B identical heldout items; trace a validation-only grammar triple causing
  a differing mask, preserve coverage and accuracy; report no effect if none.
BAD:
  Compare E05 65.97% on one sample against new 60% on another sample,
  then declare loss; or count a proxy as Student learning.
ALLOWED REPO OUTPUT:
  xiigen-planning, branch llm_plan3, path .../replanning/possibleDifs.
ALLOWED CODE RUN:
  Native bounded local runner/test for one seam; no full Windows CI matrix;
  only after checking actual owner approval and environment.
FORBIDDEN:
  Any write to llm_mvp_core/docs/plans/replanning-possible-diffs;
  frozen RP01 datasets, historical metrics, old state, source or checkpoint.
  No automatic promotion to serving without blind semantic gate.
REQUIRED RETURN:
  source HEAD SHA, exact source/consumer refs, validation family identities/hashes,
  A/B config with ONE differing variable, train/val disjoint proof,
  actual native stdout receipt, per-case paired labels/predictions,
  accuracy+precision+coverage+latency+abstentions,
  proxy_vs_native distinction, domain transfer bounds,
  independent reviewer identity/model/version, negative-controls,
  hypothesis verdict SOURCE_ONLY / MECHANISM_PASS / CAUSAL_GAIN / FAIL,
  commit/push SHA and GitHub readback of changed files.
  Declare any NOT_RUN plainly; do not fabricate test, reviewer or model call.
```

## Quality and scope review

- Сверено с [RP01 source map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md) и [master N29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md): E05/E07/E20/E21 исторические; **source-caller into isolated RP01 free Student NOT FOUND**. Это ограничение текущего чтения, не доказательство глобального отсутствия.
- Из истории [E20](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E20-STACK-DETECTOR/results/run-2026-05-01-001.json) обе модели 100% на tiny synthetic; коэффициент 0.2778 не метрика conditional retention.
- Из [E21](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E21-PROMPT-SPLITTER/results/run-2026-05-01-001.json) архив threshold .30 не согласован с нынешним inert threshold path; это **version investigation**, не обвинение в подделке.
- **Plan review self-check:** требование all-axes map/positive history/novel hypotheses/microprobe/role/domain/sources fulfilled within inspected source scope. **Independent reviewer PASS не получен**; status `DRAFT_PENDING_INDEPENDENT_ASTRA_REVIEW`. Native quality `NOT_RUN`; caller/adoption `OPEN`. Следующая задача — тот же one-factor local experiment, не новая бесконечная документация.
- Защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs` **не редактировался**. Прочие plan/source файлы кроме нового N30 дополнения **не редактировались**.
