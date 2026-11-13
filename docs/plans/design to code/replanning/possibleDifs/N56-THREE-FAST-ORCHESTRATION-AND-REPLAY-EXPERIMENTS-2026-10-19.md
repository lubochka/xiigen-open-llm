# N56 — три коротких причинных эксперимента O1–O3: real Unit/Flow, учебные домены, до/после

**19.10.2026.** `PREREGISTRATION-PROPOSAL / NO NEW NATIVE RUN`. Авторизация на реальное обучение и PC mutation этим текстом не выдаётся: будущий исполнитель делает отдельный bounded run только после установления допустимого caller и выделения **одноразового экспериментального** состояния. Нельзя переписывать frozen исторические протоколы/отчёты RP01, P125 или защищённый [`core/docs/plans/replanning-possible-diffs`](https://github.com/lubochka/llm_mvp_core/tree/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs). Детерминированная техническая проба без обновления модели не является доказательством обучения.

## Условия допуска (общие для O1–O3)

1. До запуска назвать `Domain` как изучаемую тему, а **не** алгоритм; `profile` как язык/тип задачи; `SourceOwner→TrainingRecipient→Consumer`. Версии SHA core/planning, contract, checkpoint/model/tokenizer, exact frozen TRAIN/DEV/EXAM material и human-good/bad **до** чтения post результатов.
2. Нащупать **действительно используемый** `LogicalCapabilityTrainingPreferenceProjector.ProjectCurrent`/`ProjectAuthorizedReplay` и реальную запись в обучаемое состояние. Для каждого записать исходник+method, actual run/case ID, число входных raw, accepted, projected, trainer-mutated примеров, `SelectedUnit`, `Flow/Scenario`, state hash. Если ProjectCurrent вызывается лишь audit/proxy, **STOP: NOT_REACHABLE**, не заменять отсутствующий trainer аналогом, исполняющим другое задание.
3. Свежее состояние A и B — независимые копии *одного* base/checkpoint/tokenizer и **той же** конфигурации rank/alpha/dropout/LR/reference/optimizer/randomness. Идентичный вход до train обязан дать `A0=B0` при deterministic decoding; если нет — mismatch/instrumentation failure, остановка до обучения.
4. `EXAM` содержит новые source families и заранее подписанные semantic rules, не вытекающие из TRAIN literal phrase; `gold` хранит независимый evaluator и не передаёт в learner, retrieval, Unit classifier или replay scorer. Любой feedback от EXAM выводится только после всего завершения frozen оценивания.
5. Для каждого отдельного A/B сравнения держать неизменными BPE, length, prompt framing, timing, chosen/rejected author, frozen corpus, recipient, число optimizer steps и разрешения. Изменить **только указанную ось**; если для этого требуется новая версия schema или отсутствующий host, это следует сначала зарегистрировать как dependency и не объявлять результат.

## O1 — шаблонные chosen/rejected против предметного сигнала

**Domain:** оркестрация рабочих операций / workflow-to-subphase selection. **Profile:** текстовая постановка на английском языке с русской рубрикой для эксперта; язык не меняется внутри сравнения. Источник: [`ProjectCurrent`, `BuildChosen/Rejected`](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L118-L155). Цель не перевод, не C# code documentation и не обучение LoRA автоматически.

### Минимальная панель с разделением law family

| Роль | Вход, пример для заморозки | Ожидаемое внешнее правило |
|---|---|---|
| TRAIN 1 | `Draft an OpenAPI contract for a new endpoint including request and response schemas.` | API contract/endpoint class; не просто слово `capability` |
| TRAIN 2 | `Teach a reusable skill from corrected feedback with a recorded training example.` | skill/learning class; не API-contract branch |
| EXAM 1 | `Specify the webhook payload schema and success/error responses.` | API/schema; новый source family `webhook` |
| EXAM 2 | `Set up a retraining example for a feedback-derived agent skill.` | training/skill; новый source family `feedback` |
| EXAM 3 (negative) | `Inspect a request; do not create or deploy a new API endpoint.` | корректный инспекционный/abstain route; запрещено рекомендовать deployment |
| EXAM 4 (edge) | `Document an existing API skill without modifying the service.` | допустимые маршруты определяются reviewer до запуска, с явной границей ambiguity |

Семантический reviewer должен до тренировки связать задания с **существующим** subPhase catalog и подтвердить допустимые маршруты. У `DeterministicSubPhaseBaselineScorer` есть anchors `3.3` API/schema и `7.5` skill/learning, но anchors **не являются oracle**: не копировать их ответы в экзаменационный scorer как доказательство исполнения.

**A:** тот же authentic ProjectCurrent chosen=`Generated capability for sub-phase...`, rejected=`Do not generate...`. **B:** canonical chosen/rejected для той же фазы и того же входного prompt, но различающие действительное валидное/невалидное *handoff действие* или *осознанный отказ*; независимый reviewer проверяет обе стороны по опубликованному task contract, не изменяя prompt и amount. Для DPO нужно реально допустимое legal pair admission, не изменять guardrail; при отсутствующем training consumer — design only.

**Структура:** A0/B0 — frozen ordinary flow outputs до; A1/B1 — после двух legal pairs или минимального обязательного API размера. Budget одинаков по **applied pairs/updates**, не по количеству generated files. Дополнительная *нулевая* диагностическая проба с удалением subPhase ID из model input **не** входит в основной A/B train contrast; ее результат только проверяет зависимость от label echo.

**Measurements:** `correct_route/4`, `correct_action_or_handoff/4`, `false_action/4`, `abstain/4`, loss/margin **вторичные**; prompt conditioning counterfactual, trainer state change, selected learned vs deterministic route, train wall, ordinary response latency, peak RAM/VRAM, tokens, model and Unit levels. Как позитивный эффект — хотя бы один **заранее размеченный** новый ответ из B1 правильнее A1 без потери ранее правильных; один smoke gain не release proof. Отрицательный контроль: shuffle chosen/phase labels на отдельной open canary state; разрешённая нулевая мутация контрольного состояния.

**Фальсификаторы:** одинаковое качество при уверенно пройденном recipient; B материал не был допущен / model не обучалась; не существует meaningful own output (consumer умеет только возвращать canned label); evaluator использовал ID, вставленный в prompt, как gold; частные переменные не совпали. Каждая причина — разный вердикт, `0/4` нельзя ставить вместо `NO_CONSUMER`.

## O2 — 3-decimal model-visible teaching vector

**Domain:** оркестрация и semantic skill projection (не byte-BPE или C# AST). Ссылка [`vectorText` + numeric digest](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L102-L155).

**Stage 0 (≤8 настоящих существующих open TRAIN запросов + целевая synthetic unit probe; zero updates).** Заморозить output компилятора: full 60 координат/length, raw bits/digest, string representation `0.###`, full model-visible `Prompt` и bounded context. Показать точную пару produced tokens, для которой numeric raw различен, а truncated textual representation равен, и назвать **остальные** prompt поля, которые совпали/нет. При отсутствие достижимых non-integer coordinates — `NOT_REACHABLE: precision loss as learner bug`, закончить. Синтетические 0.00041/0.00049 только проверяют формат, не реальный корпус.

**Stage 1 (только если source call реально достигнут и Stage0 подтвердил коллизию).** A = current `0.###`, B = full precision invariant versioned representation; raw floats, sources, chosen/rejected, model, schedule and mutation count fixed. Те же 4 EXAM cases, где преподаватель хочет разную обучаемую область; gold не кодируется в vectors. Перед A/B проверить длину model context: если B превращает материал в overflow, это отдельный риск и эксперимент не сопоставим. Учитывать разные token counts как цену выбранной интервенции; не объявлять улучшение из-за другой тренировочной длины без учета confounder. Проверить baseline output; два минимальных native accepted updates только при тех же caller и contract.

**Primary:** различённые lawful responses и actual selected Unit на одинаковой независимой панели; secondary: exact tokenization, prompt hash, DPO gradient/loss, extra token cost. При `SHA differs`/`vector differs` без нового correct response — `TECHNICAL_DIFFERENCE_ONLY`. Если модель обучается непосредственно **numeric** vector, а не prompt string, проблема текстового представления может не иметь причинного эффекта.

## O3 — Replay .Distinct(Prompt) против источников и частоты feedback

**Domain:** continual-learning of orchestration skills, только recipient `ProjectAuthorizedReplay` → реальный replay consuming Unit. Источник [`L165–214`](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L165-L214).

**Stage 0 (zero native updates).** Собрать current projections Q1, Q1 (два независимых допустимых source bindings S1,S2 при одинаковом literal prompt), Q2. Два сравнения: list `[Q1,Q1]` → один text chunk, `[Q1,Q2]` → два. Проверить sourceBindingSha256s и group seal, resulting replay vector, full digest/version. Прямое равенство text не доказывает потерю самих receipts. Если duplicate input не проходит authentic replay admission, записать **NOT_REACHABLE**; не обходить admission с фальшивыми hashes.

**Stage 1 (только после реального consumer trace).** Главный один фактор — влияние **веса повторённых valid observations**. A: current Distinct text with declared existing downstream source-binding frequency policy. B: явно переданная provenance-aware frequency через официально разрешённый исследовательский versioned projection; сохранить text budget идентичным (например, тот же unique text и отдельный count), same number optimizer steps, same raw lawful laws. Если B нельзя получить без изменения других факторов — остановить и записать `MISSING_WEIGHT_CONSUMER`. Повтор текста vs explicit weight — разные механизмы и требуют **двух** сравнений, не смешивать.

**EXAM:** пара новых tasks, которым полезен корректный repeat feedback, пара source-disjoint conflicting-law tasks и отрицательный policy-violating repeat. Проверить прежде всего `correct/4`, `critical false accept/4`, `retained prior correct`, выбранный replay ID/weight, state delta, time/bytes; один-два repeats — не доверительный интервал. Шам-контроль: повторить Q1 с **одним** source binding ID — не должен начислить второй независимый experience, даже если caller повторен.

**Falsifier:** в действующем trainer multiplicity уже правильно учтена по sourceBindings/weights; реальный caller не дает duplicate distinct messages; либо source family не требует frequency weighting и no quality gain. Экономия времени или index bytes без потери качества может быть **полезным техническим** результатом, не learning regression.

## Унифицированный компактный отчёт M (для каждого O, никаких выдуманных цифр)

| CaseID, law, source family | Gold (sealed) | A0 | A1 | B0 | B1 | Semantic verdict + route |
|---|---|---|---|---|---|---|
| EXAM-1 | записать заранее | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | NOT_MEASURED |
| EXAM-2 | записать заранее | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | NOT_MEASURED |
| EXAM-3 | записать заранее | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | NOT_MEASURED |
| EXAM-4 | записать заранее | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | NOT_MEASURED |

| Ось затрат/качества | A0→A1 | B0→B1 | Единица/источник |
|---|---|---|---|
| Correct / all; correct / answered; false accepted; abstain | NOT_RUN | NOT_RUN | identical sealed denominator |
| Raw / admitted / derived / mutated lessons | NOT_RUN | NOT_RUN | real training receipt, not pair files |
| Source-family train/eval overlap | NOT_RUN | NOT_RUN | sorted hashes/family IDs |
| Changed weights/index/readback / selected consumer and route | NOT_RUN | NOT_RUN | state hashes + live trace |
| Train time, queries time, tokens/s, RAM/VRAM peak, bytes | NOT_RUN | NOT_RUN | measured scope and warmup |
| Neural parameters/layers vs logical Unit/Flow instances/depth | NOT_RUN | NOT_RUN | model topology vs actually traversed |
| Cold restore/retention | NOT_RUN | NOT_RUN | **separate** test, not silently merged into main |

## Привязка других доменов — не подмена получателя

- **Документация C# (RP01):** исполнять [protected D neural](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md) и P10/P81 на собственном actual Student; O1–O3 применимы только при доказанном использовании **того же** capability-projection path.
- **SQL:** проверять NULL, left join ON/WHERE, порядок агрегации через SQL-native/admitted learner; AST/POS признаки только по источнику, не навязывать `TranslationKnnStore`.
- **Перевод EN→RU/RU→EN:** использовать P125/translation `T-compose` для реальных Executor memory; отдельно neural EN→RU DPO профили. `not`/imperative/question/actor-object перемена роли — critical; correct-vs-abstain различать. Не переименовывать 0/24→19/24 memory в LoRA gain.
- **C# AST kNN:** источник исторической пользы E09/E22/E26; только если Unit действительно принимает и использует деревья, измерять changed next-kind/route accuracy на source-disjoint методах с одинаковыми TRAIN. Не обобщать на свободный язык.

**Stop criteria:** `NO_CALLER`, `NO_LEGAL_ADMISSION`, `EMPTY_REACHED_SET`, `NO_MODEL_VERSION_PARITY`, `EXAM_LEAK`, `MEASUREMENT_MISMATCH`, `QUALITY_REGRESSION` — немедленно фиксировать точный blocker/ответы, не перерабатывать тест под алгоритм. **Следующее действие:** только один уместный O1/O2/O3 после read-only gate; никакой Windows CI/DPO500 ради одной гипотезы.