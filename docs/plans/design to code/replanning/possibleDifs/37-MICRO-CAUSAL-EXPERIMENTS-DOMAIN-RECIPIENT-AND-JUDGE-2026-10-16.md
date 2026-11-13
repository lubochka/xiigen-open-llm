# N37-MICRO — быстрые опровержимые эксперименты по доменам, без подмены успешного обучения тестами

**Дата:** 16.10.2026. **STATUS:** EXECUTABLE_PROTOCOL_ONLY; CODEX/ASTRA_NATIVE_SEMANTIC_RUN=NOT_RUN; CHINESE_TEACHER_AUTHORING=NOT_RUN; STUDENT_NATIVE_QUALITY=NOT_RUN.  
Scope разрешённых правок сейчас: только `xiigen-planning/llm_plan3/.../replanning/possibleDifs`. `llm_mvp_core/docs/plans/replanning-possible-diffs` — **READ ONLY**. Кодовые A/B предлагаются будущему CODEX executor, **не запускались здесь**.

## Обязательные роли (не спутать с историческим simulation)

| Responsibility | Единственный разрешённый исполнитель в реальной DPO кампании |
|---|---|
| Human selection of training cases and choice of domain/desired law | **CODEX с моделью Astra 6 UltraCode**; документирует критерий выбора и blind source-family split. |
| Generation of **BOTH** `chosen` AND `rejected` | **Внешняя разрешённая открытая китайская модель** (Qwen/DeepSeek/Kimi/GLM с проверкой конкретной модели, лицензии, hosted terms); оба raw outputs + model/provider/terms/license provenance; не CODEX и не закрытая judge модель. |
| Independent DPO pair validity/semantic law quality assessment | **CODEX с Astra 6 UltraCode**, в отдельном непересекающемся контексте, не видящем sealed exam и без самопроверки собственного авторства. |
| Independent blind evaluation of trained student's real answers | **CODEX с Astra 6 UltraCode** в отдельном grading run; domain oracle/compilation where possible. |

Если новый experiment **не строит DPO** (исторический E17/E28/E31/E42 KNN) — teacher не вызывается, а CODEX с Astra 6 UltraCode всё равно выбирает meaningful source cases и независимо оценивает actual answer quality. `ASTRA_SIMULATED_TEACHER` не выдавать за `CHINESE_TEACHER`. Ни один из model-specific вызовов не выполнен этим source-only аудитом.

## Нулевая стадия: зафиксировать, что именно учится и кто потом отвечает

Для каждого опыта до первого патча создать `recipient-map.json`:

```json
{
  "domain": "CSharp_AST|SQL|EN_RU|RU_EN|Figma_DOM|Orchestrator",
  "source_sha": "<exact core commit>",
  "teacher_model_id": "NONE_FOR_KNN_OR_EXTERNAL_CHINESE_MODEL",
  "codex_selection_judge_model": "Astra 6 UltraCode",
  "input_source_family_sha": "<frozen>",
  "train_ids_sha": "<frozen>",
  "sealed_ids_sha": "<hidden>",
  "learner_owner": "<DpoTrainer/LoRA OR KNN store OR Unit OR Flow>",
  "representation_version": "<tokenizer/parser/encoder hash>",
  "memory_checkpoint_version": "<hash or NOT_APPLICABLE>",
  "ordinary_consumer": "<actual ProductHost/TranslationMachine/Unit/etc>",
  "scoring_contract": "same gold laws / same thresholds / paired source-disjoint"
}
```

**Стоп:** нельзя выполнить настоящий caller или идентифицировать owner learned state → `ROUTE_UNKNOWN`; не объявлять «алгоритм исправлен» после standalone proxy. Запрет изменять historical JSON, archived frozen law panels и текущее RP01 checkpoint.

## Панель для коротких прогонов

- **Сначала** 8–12 TRAIN примеров и 8–12 *SEALED source-family-disjoint* запросов на каждый существенный домен, либо 16–24 synthetic rule transitions для parser-KNN с независимыми проверяемыми доменными законами. Это discovery, **не** статистически окончательный benchmark. Учитывать уникальные *семантические* families, не число переименований.
- Для C#: `static int Count(string? s) => s?.Length ?? 7;` → для `null` возврат 7; plus typed `?.`, `??`, `await`, `return`; compare semantics and compile/run where safe.
- Для SQL: `LEFT JOIN` keep unmatched users; ON vs WHERE filter; `NULL` 3-valued logic, distinct/cardinality; dialect pinned.
- Для EN→RU/RU→EN: отрицание + порядок subject/recipient, числа, modality, referent ambiguity; reversed direction stored explicitly.
- Для DTC/Figma: дерево DOM, constraints/responsive and visual QA, не C# lexical token law.
- Для Orchestrator: typed phase dependency, edge source, state version, lawful transition and incorrect role.
- Неправильные/gold ambiguity cases не отбрасывать задним числом: marker `AMBIGUOUS`/multi-valid gold.

## Обязательная таблица контрастов (минимум изменений, максимум информации)

| Trial | Baseline A | Enhanced B | Primary blind semantic metric | Domain/negative control | Механизм, который может быть опровергнут |
|---|---|---|---|---|---|
| **RP01-1 B0 capability** | random B0 + unchanged DPO-only | same initial B0 + 8–32 chosen-only supervised warm-up, then budget-matched DPO | real `Q_law` and free-answer correct after cold reopen + chosen token probability | C#/ENRU, same source families/compute and an untrained negative | B0 insufficient / LoRA doesn't train |
| **RP01-2 gradient** | analytic `DpoTrainer` gradient | finite-difference independent directional derivative | relative/error sign for chosen-rejected objective; no new product score | single pair equal/unequal length, shared prefix, zero masks | maths implementation invalid |
| **N37-E28** | `genericPairs(TRAIN)`, within-file only | project KNN trained on same TRAIN, route by real project ID | `lawful_next@1` paired on test files | DTC/C#/SQL; leaked `genericPairs(TRAIN+SEALED)` **diagnostic only**; permute file order | E28 negative due to test leak / invalid project proxy |
| **N37-E17** | Level2 KNN | Level3 KNN, same source TRAIN and queries | correct parsed next-law@1 / macro critical error | C#/DTC/SQL; `all tokens=CONST` gives false-perfect perplexity | E17 metric not evidence of useful representation |
| **N37-E31** | global unconditioned `f(x)` | true `f(x,domain,role)` or scoped KNN, same data | correct conflicting domain-law@1 macro | C# vs SQL typed role; ENRU/RUEN with direction; label shuffle control | E31 order-only didn't test domain conditioning |
| **N37-E42** | FIFO capacity K | value-based eviction same K/lesson order | `correct_retained_law@1` after cold restore | SQL/C# ENRU direction; same markers but opposite target as negative | 100% predicate markers mask lost actual knowledge |
| **E09 rescue** | exact token/label baseline on equal file panel | AST feature KNN with same available memory, valid label codec | next-kind accuracy with fixed denominator + old-law retention | C# source-disjoint, sibling/parent and symbol roles | historically positive AST KNN portability |
| **E26 rescue** | word map only | direction+role-aware dictionary/KNN, identical correct source pairs | polarity/roles/word order semantic clauses | ENRU, RUEN separate; swap subject/recipient; no literal-text exact assumption | old contextual gain survives modern Unit |
| **RP01 vs deterministic rules** | learner OFF, rule OFF | learner ON, rule OFF; separately rule ON/OFF arms | factual correctness, critical errors, coverage/abstain | SQL/null law, ENRU negation, DTC structure | deterministic success misreported as learned neural improvement |

Для каждого Tier1: **один изменённый фактор на контраст**. Если B & A имеют разный набор TRAIN, разные gold или различное вычислительное ограничение — `INVALID_EVIDENCE`. Векторные и грамматические функции не применяются к доменам без подходящего парсера/feature schema.

## Точное выполнение и сохраняемые evidence

1. Закрепить `core_SHA`, `planning_SHA`, `sibling_SHA`, оригинальные исходники historical positive, hash корпуса, source-family split. Владелец CODEX/Astra пишет хеши до доступа judge к sealed.
2. Доказать идентичность объектов: `tokenizer SHA / feature schema`, `LoRA tensor bindings / AdamW state`, `KNN index and memory`, `UnitId`, `FlowId`, policy routing selection. Отдельно учесть state каждого subunit из одной пары: many→many неделимо по смыслу без provenance.
3. Построить два неизменяемых A/B learned состояния (не перезаписывать checkpoint, не смешивать optimizer/RNG/replay cursor); применять `train_ids` только в TRAIN. При no DPO tests teacher/model cost = 0.
4. На одинаковых frozen SEALED вопросах выполнить обычный consumer до и после; сохранить сырые outputs, route trace, retrieval candidates, chosen law, rejected hypotheses и stop reason. Ответ должен реально принадлежать tested learner.
5. Независимый CODEX/Astra 6 UltraCode judge пишет per-case `valid_law_clauses`, `critical_error`, `correct`, `abstain`, `why_wrong`, `version`, `confidence`; допускается compile/SQL safe oracle, но он не заменяет семантический judge.
6. Считать `paired n01/n10`, `Q_law`, `correct@1`, macro by independent family/domain, new/old law retention, coverage/abstain, latency, max resident RAM on PC. Paired uncertainty; N мал, поэтому «signal only», не statistical confirmation.
7. Cold restart: открыть policy+adapter/tokenizer+optimizer (где применимо), memory/encoder/index/Unit/Flow, выполнить те же ordinary calls; если результат пропал после reopen, verdict `RESTORE_OR_CONSUMER_GAP`, не «learning failed».
8. Стоп/решение: `SOURCE_CLAIM_CONFIRMED` (по коду), `CAUSE_SUPPORTED_SMALL_SAMPLE` (реальные ответы), `NO_OBSERVED_GAIN`, `REGRESSION`, `NOT_APPLICABLE`, `INVALID_EVIDENCE`. Никакой `PRODUCTION_READY` по одному toy / synthetic shadow. Сохранить и ошибки, и выигрыш, а не только последнюю позитивную итерацию.
9. Только для подтверждённого consumer-bound gain — кандидат изменения core в *отдельной будущей authorization*; минимальный focused integrity check вместо Windows CI marathon; code review + independent plan review с exact SHA после исправления.

## 5 наиболее дешёвых проверок перед реальным обучением

- **Micro A:** открыть E28 и посчитать пересечение file IDs generic TRAIN и test: если пересечение не пусто, архивный comparison invalid без model-call.
- **Micro B:** построить из E17 два hash списка validation sourceFamily ID: различие между Level2 и Level3 или single `combined` tokenized string не допускает «same panel».
- **Micro C:** проверить `TrainOnPairs` в E31; если domain discarded, нельзя относить FAIL к настоящему conditional learner.
- **Micro D:** в E42 сохранить два stores с идентичным marker coverage и различными correct law bindings — marker retention не определяет learnt knowledge.
- **Micro E:** на RP01 saved checkpoint сравнить до/после **реальные first-8/16/32 tokens + route IDs** и teacher-forced chosen-token likelihood; никакого ещё одного 500 batch до различающего сигнала.

## Поручение CODEX/Astra UltraCode (готовый prompt)

> Ты — CODEX, работающий моделью **Astra 6 UltraCode** и исполняющий человеческий отбор, архитектурную проверку и независимую оценку качества. Прочти `AGENTS.md`, `CLAUDE.md`, живой RP01 handoff, сквозной RAG и skills architect/planning/plan-review/code-review/prompt-engineering/human-communication/experiment-design. Твоё право сейчас — read-only аудит `llm_mvp_core` и отдельная permission-based Tier1 микропроба; **НИКОГДА не изменяй** `llm_mvp_core/docs/plans/replanning-possible-diffs/**`. Проверь каждое source claim N37-A..D против точного commit SHA. Для настоящих новых DPO-пар отдай генерацию **обеих** сторон chosen/rejected **внешней лицензированной открытой китайской модели**, не Astra. Сохрани raw authoring provenance. Отдельный CODEX/Astra 6 UltraCode judge оценивает DPO-pair correctness и blind ordinary-consumer outputs. Не выдавай synthetic proxy, margins, repeat-trained pairs или code-only success за semantic learning. Выполни одну выбранную domain-проверку по frozen A/B и опубликуй только имеющиеся реальные receipts / найденные ошибки и `NOT_RUN` для остальных. Один гипотетический патч = один механизм. Никакой принудительной Windows CI кампании.

**Ревью итогов:** проверить source SHA, валидность сравнения, отсутствие sample leakage, exact consumer, roles + license, old-law regressions, rollback/cold parity, доменные negative controls, честный статус и линк на raw receipt. На исходном коде **не производилось** ни одной train-модификации в этой сессии.