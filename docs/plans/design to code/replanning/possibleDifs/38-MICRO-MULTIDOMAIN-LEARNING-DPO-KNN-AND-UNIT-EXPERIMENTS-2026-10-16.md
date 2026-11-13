# MICRO-N38 — 4 коротких причинных опыта: Student, KNN, Units, DPO gradient

**16.10.2026 | EXECUTABLE SPEC, NATIVE/DOTNET NOT_RUN, DOMAIN_QUALITY_NOT_MEASURED.**

**Pinned read-only core:** [codex/dpo-implementation@aa284ba2](https://github.com/lubochka/llm_mvp_core/tree/aa284ba21790c5d7601328997cd6174c85cee878). **Разрешённая запись:** только xiigen-planning/llm_plan3/replanning/possibleDifs. **Запрещено:** core/docs/plans/replanning-possible-diffs/**, переписывать старые receipts, запускать широкую Windows CI вместо исследования.

## Общие правила

Интервенция меняет только **одну причину**, а не одновременно dataset, model, Unit route, tokenizer и evaluator. Отложенные задания **source-family-disjoint**, включая near-duplicates. Каждый результат: exact SHA, ID уроков, learning recipient, ordinary consumer path, decoded output, correct/wrong/abstain, checkpoint restore, negative control, latency/RAM. Два независимых ворот: **native mechanism demonstrated** и **domain quality improved**. Green tests, changed LoRA tensors, decreased training loss или KNN.Count сами по себе не второй PASS.

Для финальных новых DPO-пар — только допущенный внешний открытый китайский teacher как автор chosen/rejected; CODEX/Astra 6 UltraCode выбирает задачи и отдельно проверяет pairs и реальные Student outputs. Исторические synthetic pairs не объявлять external. В этом документе teacher и CODEX **не вызывались**; для первого запуска предпочтительнее уже существующие допущенные данные и читаемый one-law fixture.

## EXP-N38-N — P0 нейронная компетенция до DPO

**Вопрос:** способен ли random B0 приобрести полезный язык при малом warmup, или DPO лишь увеличивает маржу без свободной генерации? Проверяются отдельно C#→документация и EN→RU.

**До обучения:** заморозить B0/C50/C100 и выполнить read-only 8/32/64-token decode с exact train/eval framing, first token logits/rank, stop/EOS, actual tokenizer SHA. Сравнить с [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) и [N27 common prefix](27-RP01-DPO-COMMON-PREFIX-GENERATION-GAP-2026-10-16.md). Старые 0/3 и 0/4 не считать новыми независимыми cases.

**Fixture:** в каждом домене 8 train-law families и 4 **непересекающиеся** heldout-law families, по 1–3 разных примера на закон (ориентир 16–32 train и 8–12 heldout на домен). C#: null guard, exception-vs-return, call-order, equality/negation; ENRU: not/never, tense, gender, only-scope. Предварительно независимый gold по исходному коду/значению, а не по ответу модели.

| Рука | База и изменение | Что отличает |
| --- | --- | --- |
| N0 | random B0 + те же допустимые tiny DPO пары, без SFT | DPO-only при отсутствии prior language |
| N1 | та же random B0 + короткий lawful chosen-only supervised warmup, затем тот же DPO | prerequisite SFT vs DPO-only |
| N2 | тот же warm-start, но supervised-only continuation при сопоставимом compute | нужен ли сам DPO после SFT |
| N3 условная | N1 с одинаковой подготовкой, q/v-only vs одно расширение LoRA target | expressivity при доказанном baseline |

**Результат:** correct-law new families, wrong-law, abstain, stop reason, first-token gold-prefix rank, training reproducibility, retention second domain. Не смешивать продолжение с подсказанным ответом и свободную генерацию. **STOP:** даже 8 lawful examples не удаётся overfit после SFT — сначала [N33 autograd](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md), не следующие 500 DPO. Перестановка chosen targets допускается только как isolated test-only negative control; не пополнять production.

## EXP-N38-K — P0 domain-aware KNN против прежнего vector/bag-of-words

**Вопрос:** не утратили ли при переносе E09/E14/E22 в matrix/общий KNN контекстные функции, которые работали только в своём домене? Исторический E14 4/4 не переносить на большой dataset, E22 TypeScript=0 и Tests=0 — отдельные категории [источники в N38 master](38-MASTER-RP01-SUCCESSFUL-HISTORY-NEW-DELTA-DOMAIN-EVIDENCE-2026-10-16.md).

**Frozen memory:** 6–8 законных lessons на домен, по 8 unseen law queries; одни и те же lessons и candidate IDs/seed для всех рук.

| Панель | Содержательная проверка | Функция KNN, которую разрешено испытывать |
| --- | --- | --- |
| K-C# | new method/AST, ADD_NULL_CHECK vs ADD_METHOD vs rare REFRACTOR; без того же файла | AST parent/child, call context и action label |
| K-LANG | ENRU not/never, грамматическая роль, переставленные слова и gender, RUEN отдельно | POS/word order/phrase span и polarity |
| K-SQL | != vs =, parentheses, JOIN direction, alias | operator/AST binding и scope |
| K-ROUTER | Generic, CSharp, BugFix, Orchestration vs TypeScript, Tests | E22 learned label/dataset routing и классозависимый abstain |

**Контрасты:** K0 — обычный consumer и frozen memory без дополнительной структурной feature; K1 — те же реальные learned neighbors с единственной domain-feature (AST/word-order/POS) включённой shadow; K2 — shuffled lawful lesson targets как nonproduction отрицательный контроль; K3 — удалять только одну выбранную feature, не всю архитектуру. Для 8 queries хранить полный trace exactSource→phrase/wordMap→KNN tier→verifier→final answer. Если до KNN ответ уже возвращён, verdict **ROUTE_NOT_REACHED**, нельзя приписать KNN улучшение.

**Критерии:** correct-law coverage, wrongAccepted, abstain, per-domain macro-F1 и class recall, retention after adding one new lawful lesson, cold restored answer parity. Упавшее число ошибочных ответов при 100% abstain — **не** доказательство полезной генерализации. Исправления не распространять с C# AST на natural language или simple numeric rule без независимой панели.

## EXP-N38-U — P0 typed Unit получает урок, но употребляет ли его продукт?

**Вопрос:** действительно ли после правильного DPO/teacher source lesson дошёл до SemanticTokenizerUnit, ExecutorUnit, QualityVerifierUnit, был сохранён, а затем использован в TrainedTranslationAdapter? Это иной recipient, не direct Student LoRA.

**Кейс ENRU:** исходное значение "Do not close the active session" против "Close the active session"; для проверки незнакомое "Never close an active session" плюс положительная версия без not. **Кейс C#:** isNull check vs return, новый method/variable. Это **исследовательские human-authored law examples**, не допущенные DPO chosen/rejected до реальной авторизации.

- U0: empty trained-state → ordinary product path и отдельно exact text/path.
- U1: один admitted structured TrainOnExample в правильный UnitId, экспорт/импорт fresh process → unseen law query.
- U2: U1 same state, shadow missing-critical-modifier check для not/never или AST !=; без изменения teacher/lessons.
- U3: той же формы shuffled target (nonproduction). Если U1≈U3, нет доказанного learned law.

**Receipt:** source example ID → pair admission → UnitId → measured memory changes → cold checkpoint identity → exact/phrase/KNN ranked neighbors → verifier decision → final end-user answer. Проверить специфическую известную ошибку [N21 missing words](21-INDEPENDENT-CLAUSE-DROPOUT-WHITESPACE-AND-LEGACY-SCAFFOLD-2026-10-16.md), [N23 verifier](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md); это **старые**, не новые N38 findings. PASS только при улучшенном correctness на unseen source и отсутствии повышения wrongAccepted.

## EXP-N38-A/B — P2 новые математические seams DPO

### A. Дробная legacy token mask: один native finite-difference тест

**Доказательство формулы:** [N38-A](38-NEW-LEGACY-FRACTIONAL-RESPONSE-MASK-GRADIENT-MISMATCH-2026-10-16.md), [JS source proxy](38-MICRO-DPO-MASK-FINITE-DIFFERENCE-PROXY.mjs), [результат](38-MICRO-DPO-MASK-PROXY-RESULT.json). При логитах [-1,+1] и mask=[0.25,1] верная аналитическая производная score ≈[0.146211716,0.215153137], current backward формула ≈[0.365529289,0.134470711]. **Это не .NET и не quality improvement.**

- A0 native legacy two-token logits, binary mask=[1,1] — finite difference parity должна быть.
- A1 тот же native lawful DpoBatch, дробная mask=[0.25,1] — тест производной по реальному обучаемому LoRA scalar, без изменения batch content.
- A2 exact-v2 response-only binary mask — отрицательный контроль, не смешивать mean/sum outputs.
- A3 trace настоящего вызывающего producer: если fractional legacy масок нет, status LATENT_NOT_REACHED. **Не чинить RP01 exact-v2 из-за этой находки.**

### B. Mixed exact-sum и legacy-mean в одном StepMany

**Доказательство source:** [N38-B](38-NEW-MIXED-EXACT-LEGACY-DPO-OBJECTIVE-IN-SAME-STEP-2026-10-16.md).

- B0 в protected project-LoRA RP01 попытка смешать должна завершиться **PROFILE_REJECTS_MIX до изменения весов**, благодаря execution profile. Это ожидаемый PASS защиты, а не обучаемости.
- B1 только при действительном разрешённом generic trainer: 2 exact vs 1 exact + 1 legacy with matched semantic contrasts, заморозить seed/reference; вывести per-pair loss definition, gradient norm и actual optimizer change. Если разрешения нет — STOP, не обходить permit.
- B2 если native mixed route реально достижим: провести 8–12 source-disjoint C#/ENRU law answers после одного fixed budget; judge не должен сравнивать несовместимые scalar loss как единое качество.

## Выходной пакет и независимая проверка

**Минимальный machine-readable receipt:** experiment_id, pin_sha, domain, recipient, actual_consumer_method, training_family_hashes, heldout_family_hashes, gold_rubric_sha, seed, checkpoint_sha_before_after, intervention, control, baseline_correct, treatment_correct, wrongAccepted, abstain, negativeControl, restoredParity, duration, cpu_ram, status_source, status_native, status_quality.

**ARCH** — проверяет, что обучается нужный recipient, locked decisions и family split. **PLAN** — одна интервенция, бюджет и заранее заданный falsifier. **REVIEW** — обратная проверка на leaks и разные цели. **CODE** — source lines, model mutation integrity, no hardcoded oracle. **PROMPT** — exact task/recipient/domain/negative control, no generic "test until green". **HUMAN** — по-русски коротко объяснить *какой новый навык появился*, у какой модели/Unit и в каком домене.

**Исполнительский prompt:**

> Выбери один EXP-N38 по приоритету P0. Pin SHA; прочитай владеющий code path, source документы и соответствующие skills. Письменно объяви owner/recipient/ordinary consumer, единственную изменяемую переменную, baseline и corrupted negative. Freeze family-disjoint gold. Проведи один узкий локальный native опыт только при законной material/admission/permission, собери реальные ответы before/after/cold restore, не Windows GitHub runs. Верни root-cause with exact source quotes и честные SOURCE / NATIVE / DOMAIN_QUALITY status. Старые успешные experiments и чужие new findings не выдавай за свои. Core protected docs не трогать; все новые findings только planning/possibleDifs. Если выполнена только JS формула, так и напиши NOT_NATIVE, а не PASS обучения.

**Статус публикации:** документ/JS numeric proxy написаны. Ни Student retraining, ни native .NET DPO/Unit/KNN runs, ни реальная оценка учителем/CODEX в этом заходе НЕ ВЫПОЛНЕНЫ.