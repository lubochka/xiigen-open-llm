# N60 — READ-ONLY проверка экспериментов P01–P125: покрытие ≠ валидный learning A/B

**Дата:** 2026-10-19. **Папка-объект:** [`llm_mvp_core/docs/plans/replanning-possible-diffs`](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/plans/replanning-possible-diffs), включая `experimets/cards/P01…P125` (имя `experimets` именно так написано в репозитории). **Не изменять вообще.** Результаты этого прохода опубликованы только в `xiigen-planning/.../possibleDifs/`. `RUN_COUNT=0`, `CODE_MUTATIONS=0`, `PROTECTED_MUTATIONS=0`.

## Методика фактически проведённой проверки

Через GitHub read API прочитаны **все 125 существующих файлов карточек P01–P125**, а также родительский метод `experimets/2026-10-19-small-learning-probes/01-method-and-report.md` и сохранённый индекс native результатов за 18.10. Ранее прочитаны документы аудитов N56, N58 и N59. Автоматизированный **structural keyword lint**, не предметное исполнение, показал:

| Проверка структуры файла | Итог | Что это НЕ доказывает |
|---|---:|---|
| Карточка существует и была прочитана через GitHub | **125/125** | Наличие корректного native host/consumer |
| Присутствует обязательная точная формулировка о любом измеренном полезном улучшении | **125/125** (P01–P13 с эквивалентной текстовой проверкой; P14–P125 exact wording) | Фактическое улучшение хоть чего-либо |
| Есть явная оговорка «рост точности не обязателен» | **125/125** | Что регрессии измерены на настоящем run |
| Есть явно сформулированное правило участия в обучении и запрета отдельных report/serving/zero-update опытов | **125/125** | Что каждый Pxx.* следует этому правилу |
| Есть A/B или до/после и указанный владелец/обучаемый механизм | **125/125** на уровне текста/регулярных выражений | Полнота 4 snapshots A0/A1/B0/B1 **в каждом подслучае**; наличие независимого EXAM |
| Примеры ручного чтения полного work-order prompt | P14, P29, P75, P125 — раздел `## Промпт и будущие свидетельства` | Что все 125 промптов независимо воспроизводимы / разрешены |

**Вывод:** глобальные обязательные правила внесены в карточки; **признать все 125 валидными учебными экспериментами нельзя**. Structural lint не устанавливает caller, доказанное происхождение TRAIN vs TEST, корректный A0/B0 baseline, семантически верный gold, модель, фактическую версию кода, точные resources/units и принадлежность фактора к actual update. Эти проверки делаются для **каждого подслучая** перед допуском, а не выводятся из шаблона.

## Дополнительная перекрёстная проверка N60: найденные различия уже представлены в защищённом каталоге

| Различие / source | Уже описано в protected | Вердикт |
|---|---|---|
| Shared `SemanticTokenizerUnit` correction raw-only без SourceLanguage; two lessons/homograph/оба порядка | [P51.8 translation context](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P51-translation-context.md) | **EXACT PRIOR ART**, N60-1 не считать новым открытием и не назначать дублирующий run |
| `ExecutorUnit.TrainOnExample` index keys строит своим bootstrap tokenizer, а normal Flow — learned tokenizer | [P106.4 corrected semantic target](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P106-corrected-semantic-target.md) | **EXACT PRIOR ART**; N28/N60-2 — совместимый sidecar, не новый эксперимент |
| `ExecutorUnit.MeasureAccuracy` retokenizes fresh base вместо learned | [P57.13 semantic measurement](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P57-semantic-measurement.md) | **COVERED, но standalone metric-only опыт вне поручения** |
| Phrase-map `.86` допускается при `Verifier.Pass=false` | [P56.4 verifier context](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P56-verifier-context.md) | **COVERED** для guard, не доказан feedback-mediated training loop H60-3 |
| DPO StepVector теряет позицию/sign/slotted semantics в scalar `activeSlots` | [P06.1 teaching vector](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P06-teaching-vector-adamw.md), [P122](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P122-root-channel-unicode-digits-operators.md) | **COVERED**, но не доказанная specific branch regression |
| Вектор tokenizer/BPE, kNN AST role, negative/signed constraints | [P16](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P16-semantic-order-roles.md), [P26](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P26-structural-features.md), [P27](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P27-structural-codec.md) | **COVERED at family level**, не все языки/домены/учебные hosts достижимы |
| DPO→KnnCorpus→Unit/Flow cross-product recipient | [P92.1–P92.4](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P92-dpo-memory-flow-bridge.md) | **COVERED conditionally**, не автоматически same Student model |
| Learning memory positively affected output EN→RU | [P125](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P125-executor-memory-learning.md) + [native result](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18) | **MEASURED, with limitations**, не proof neural or all directions |
| Wrong learning-admission after mixed `AccuracyAfter`/direction/coverage in **следующем** выборе уроков | P57.3/.4/.5/.8 и P58, но точный **curriculum feedback consumer** не указан этим аудитом | **NEW CONDITIONAL INTERACTION H60-4 / CALLER_PENDING**, не допускать standalone scorer experience |
| Phrase verifier override может стать ошибочным chosen lesson после настоящего feedback admission | P56.4/P58 + [N59-L02](59-03-NEW-INTERACTION-HYPOTHESES-AND-FAST-LEARNING-CARDS-2026-10-19.md) уже формулируют feedback-poisoning chain; она не доказана | **EXACT PRIOR ART / CALLER_PENDING**, НЕ новая находка N60; без запуска |

## Реальные пробелы ВАЛИДНОСТИ: общий header не спасает неучебный подслучай

| Карточка / подслучай | Проверенная суть | Сейчас допуск как самостоятельного learning A/B | Что нужно, прежде чем менять статус |
|---|---|---|---|
| P75.1/.2/.3 — sampler residual/RNG | Fixed sampler/decoder behavior, no demonstrated Train/Unit update from varied sampler branch | **OUT_OF_LEARNING_SCOPE / NO RUN** | Доказать источник dataset/admission/gradient, который **реально** меняется от sampler в конкретном обучении; иначе это serving/component QA |
| P76.1/.3 — UTF-8 output boundary | Decoder continuation; .2 уже локально явно помечен OUT_OF_LEARNING_SCOPE | **OUT_OF_LEARNING_SCOPE / NO RUN**, если training caller нет | Подтверждённый training-material writer зависит от decoded text и actual learner принимает отличающиеся lessons; иначе не запускать |
| P57.1/.2/.3/.4/.5/.8/.13 — scorer/aggregation/route | Смена математического отчёта сама по себе не update Unit/Student; P57.15/.16 уже явно имеют запрет | **OPEN_LEARNING_LINK** | Только если native feedback gate принимает эти metrics и меняет следующий lesson; проверить actual caller, не моделировать фиктивным |
| P56.4 — phrase exception | Learned verifier сохраняет rules, но изменение if-предиката лишь final acceptance | **GUARD-ONLY, НЕ learning A/B** | Настоящая admission chain, где новый accepted output формирует следующий правильный/неправильный урок; иначе не тратить токены |
| P25.1/.2 — cold checkpoint | В самой карточке уже помечены OUT_OF_LEARNING_SCOPE; native 18.10 0 new lessons | **CLOSED как standalone learning task** | Только subordinate cold state check внутри утверждённого fresh training A/B; не нужно повторять P25 result |
| P88.1/.2 — T34 fixed context packet / .92 confidence | Typed parity не равна тренируемому Unit, без подтверждённого native learner | **BLOCKED** | Установить real updated Unit/Flow recipient/feedback; без него параллельный offline сценарий ненужен |
| P95/P96/P97 — graph/async | Не любое изменение порядка исполнения = обучаемое ребро; часть graph owner conditionally absent | **CALLER_PENDING per subcase** | Настоящий edge-weight/graph memory update и subsequent routing decision из learned state |
| P51.8 и P106.4 | В отличие от строк выше, меняют **саму запись** учебного semantic/index state при реальных TrainOnExample; условно годятся | **DESIGN_VALID_CONDITIONAL; NEEDS_INPUT/PENDING** | Distinguishing gold/lesson pair, real unit and consumer, same seed/state/exam, no early-path shadowing |
| P125.1 | Да, обучает `ExecutorUnit`; positive native уже сохранён | **HISTORICAL NATIVE SUCCESS** на ограниченном EN→RU | Не расширять claims до neural или других доменов; не запускать повтор без нового вопроса |

Эти статусы — **предложенная triage-оценка N60**, не правка оригинальных P-card labels. Для карточек, где собственный текст уже запрещает неучебный опыт, **правило оригинала сохраняется**; extra caution не превращать в новый эксперимент. Для P75/P76-подслучаев указан именно **недоказанный training link**, а не доказательство, что такая интеграция вообще невозможна.

## Стандарт соответствия user request: как принимать каждый будущий РЕАЛЬНЫЙ эксперимент

1. **Human gate**: полный исходный запрос, описанные skills, Student и executor/reviewer model, конкретный domain + direction, input data/gold с source IDs, исходный checkpoint/tokenizer/weights, SHA файлов, scope и бюджет **внутри версии карточки**; пользователь лично смотрит и разрешает exact SHA/version. До этого `PENDING`, не исполнять.
2. **Learning gate**: реальный recipient `DpoTrainer.Step/StepVector` с optimizer state **или** `TrainStructuredBatchAsync → Unit.TrainOnExample`, точная writer→state→reader цепь и нормальный consumer. Если меняется только UI, test, score, sampler, decoder, offline KNN без training-identity — `OUT_OF_LEARNING_SCOPE`. Не добавлять dummy lessons.
3. **Paired gate**: одинаковые source, frozen initial state, те же два реальных урока, 4 заранее выбранные новые EXAM (или честно обоснованная domain-specific панель), одинаковые budgets; A0/A1/B0/B1 и отдельные raw OWN outputs. Если меняется сама representation — сохранить другие инварианты, отразить обусловленное отличие. TRAIN recall не называть новым навыком.
4. **Semantic gate**: независимый domain gold до Student output: C# AST typed actual change and valid docs, SQL rows/null/bag semantics, ENRU polarity/roles/word order, RAG cited source/truth, Flow actual decision. Source-family holdout без дубликатов признаков; signs/negations/morphology/AST только релевантному языку или AST.
5. **Metrics gate**: любое положительное изменение **полезного** показателя является локальным успехом по нему — accuracy может остаться/ухудшиться. Показывать всегда accuracy/coverage/correct-answered, critical wrong accepted/retention/regressions, latency/training seconds, CPU/RAM/VRAM, updates/lessons/params/Unit/Flow path. Никакого минимального universal `.02 accuracy` gate, нет quota repeat runs после одного локально убедительного результата.
6. **Evidence gate**: исходный SHA, exact command, run ID, exit codes, raw outputs и state deltas, reproducibility limits, count denominator, independent review if promised. Historical results не переписывать; отличать `SOURCE_CONFIRMED`, `DESIGN_ONLY`, `RUN_MEASURED`, `CAUSAL_NOT_PROVEN`.

## Порядок проверки без лишних расходов

- **Сначала** trace и документальная eligibility (`0` новых runs). Условные связанные модели с несуществующим runtime consumer → `NOT_APPLICABLE`. Известное P51.8/P106.4 не повторять в качестве «новой гипотезы».
- **Затем** выбирать один действительно отличающий реальный учебный A/B на домен, где есть actual learner/gold. Никаких полномасштабных Windows GitHub runs или тысяч regression tests как предварительного условия.
- **Наконец** только собственные native ответы, learning-state changes и все полезные tradeoffs; не зачислять math-only component proof в product learning gain.

## Related docs (only in writable repo)

- [N60 causal evidence crosswalk](60-RP01-INDEPENDENT-EVIDENCE-AND-PRIORITY-CROSSWALK-2026-10-19.md).
- [N60 interaction candidates / correction of false novelty](60-NEW-INTERACTION-HYPOTHESES-AND-DOMAIN-EXPERIMENTS-2026-10-19.md).
- [N56 protected audit](56-02-PROTECTED-EXPERIMENT-COVERAGE-AND-QUALITY-AUDIT-2026-10-19.md), [N58 read-only review](58-PROTECTED-EXPERIMENT-READONLY-REVIEW-2026-10-19.md), [N59 learning path](59-02-EXPERIMENT-LEARNING-PATH-COVERAGE-AND-INTEGRITY-2026-10-19.md).

**Closure status:** structural scan **125/125** and targeted semantic read **selected**; no claim of full independent semantic validation of all ~470 subcases or complete multi-branch git ancestry. `PROTECTED_WRITES=0`, `NEW_RUNS=0`, `USER_APPROVAL_PENDING`.