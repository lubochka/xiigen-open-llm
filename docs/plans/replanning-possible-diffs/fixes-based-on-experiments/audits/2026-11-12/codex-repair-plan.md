# Адресный план исправлений после аудита 12 ноября 2026

**Назначение:** довести экспериментальные переносы до проверяемого обычного поведения и прекратить повторение ошибок доказательства. Это план реализации, а не утверждение, что перечисленный ремонт уже выполнен. Продуктовый код этой аудиторской поставкой не меняется.

Исходники проверяемого среза: core `dcb931773147267c8864a532351d22b4c351265b`, planning `405e7e5bcfc245c4f68980175e57912dfd108b96`. [Итог аудита](codex-audit.md), [числа](codex-experiment-review.md), [обычный переводческий путь](codex-product-review.md), [новый конкретный confidence witness](codex-flow-confidence-review.md). Сохраняются детали прежнего [плана 11 ноября](../2026-11-11/repair-plan.md), если они не уточнены ниже; готовый PlanAdmissible повторно не писать.

## Порядок и независимые потоки работы

| Очередь | Работа | Почему сейчас | Зависимость и условие окончания |
|---|---|---|---|
| 1 | R01: владелец доказательств и внешний terminal | Следующий запуск должен сохранить пригодные доказательства даже при падении | Малые infrastructure checks до нового обучения; immutable raw целы |
| 1 | R02: обязательные guards P50 | Сейчас обнаруженное нарушение может сосуществовать с пригодным итогом | Один outcome builder управляет JSON и фактическим exit; проверка процесса без learner |
| 2 | R03: подключить готовый admissible planner | Реальный caller всё ещё теряет допустимую альтернативу | Два существующих ordinary-path red становятся green; обратная мутация возвращает red |
| 2 | R04: свой confidence witness | Чужой урок открывает допуск выбранного flow | Контрпример A/B через GeneratePlanAsync; оба положительных контроля и body identity сохранены |
| 2, параллельно | R05: ceiling, novelty, полный расход | Производные сводки пересчитываются без Train; приёмка изменённого runtime writer имеет отдельный ledger | Reporter и объяснение согласованы с независимым пересчётом; raw неизменен |
| 3 | R06: active target revision; R07: actual-answer provenance | Для них нужен явный продуктовый договор состояния и восстановления | Typed ownership, atomic state publication и адресные отрицательные сценарии |
| 3 | R08: согласовать loss/gradient | Нынешняя reported loss не соответствует используемому gradient в saturation | Независимые finite differences и finite/stable loss на заданных logits |
| 4 | R09: точная ресурсная приёмка; R10: batch inventory | Прогресс есть, но текущие доказательства не закрывают заявленный охват | Matched A/B и fail-closed discovery; без арифметического «вычитания красных» |

Документные R05 и подготовка R03/R04 не требуют ждать длительных прогонов. R03/R04 имеют собственную приёмку после общего исправления сохранения доказательств. R06/R07 не прячутся в «однострочный bugfix» и не блокируют завершение уже готового узкого planner.

## R01. Один владелец новой попытки; окончательный статус после окончания процесса

**Доска:** B05/B06, прежние F03/F04. **Файлы:** `Tests/ExperimentFixes/ExperimentEvidenceSink.cs`, его tests/callers, внешний runner; конкретно добавить writer `TranslationRequiredWholeTermOrdinaryPathTests` и уже отмеченный `Learn07RollbackRunnerTests` в существующий реестр writers. У P56 новый run-subdirectory полезен, но static Lazy+File.WriteAllText не даёт исключительного владения.

До setup/Train получить run-local handle атомарным созданием owner marker (`CreateNew` или эквивалентная существующая primitive). В handle: run ID, source/dirty identity, project/filter/config, start, owner. Не выбирать каталог по отсутствию tracked files. Непустая чужая/прерванная попытка сохраняется и даёт отказ или отдельно назначенный новый адрес. Пути writers определяются одним handle, не каждый своим timestamp. Окончательная расписка не перезаписывается; progress имеет отдельно определённый единственный writer и атомарное обновление.

Отделить научные наблюдения от результата test run. Тест может записать строки, выполненную дозу и локальное решение, но не фактический exit ещё не завершённого процесса. Внешний runner связывает process handle, source/DLL identity, команду, stdout/stderr, TRX, точные test IDs и действительный terminal. Crash/abort/no-TRX/поздний assertion — INCOMPLETE/FAILED, даже если inner status был положительным.

**Приёмка с Train=0:** два конкурентных claim одного адреса — ровно один владелец; два invocation одного процесса — разные адреса; untracked nonempty root не считается свободным; ошибка claim происходит до fake training callback; чужие frozen bytes не меняются; late-fail после полезных измерений даёт failed terminal; убитый дочерний процесс без finally даёт incomplete, не PASS. Mutation «ordinary WriteAllText вместо исключительного final commit» обязана обнаруживаться. После этого один уже назначенный regression использует ту же оснастку; весь набор научных опытов повторять не требуется.

## R02. P50: пригодность должна управлять итогом и кодом возврата

**Доска:** B14/F05. **Владелец:** `experiments-v2/RP01-MICRO/P5001/attempt-0002/Program.cs`, существующий builder/writer итогов. Предыдущий source-level вывод и новый пересчёт здорового saved2/4→4/4 описывают разные границы; здоровый исторический опыт не отменять.

Вычислять решение один раз: stages complete + mandatory guards clear → validity; затем, только для valid, оценка гипотезы. Сериализация и process exit читают тот же объект решения. Добавить read-only replay режим saved inputs в том же host/outcome path для проверки без обучения; это будущая реализация, готовый CLI не заявляется.

**Приёмка:** healthy fixture остаётся valid; изменение только main lessons8→9, неизвестный step ID и missing stage дают invalid, ненулевой реальный exit и learner_calls=0 в replay. Изменение только serializer при старом exit0 не проходит. Endpoint gain не называть monotonicity: либо переименовать поле, либо для нового требования проверять каждый переход и различать ряд2,4,1,4. Frozen attempt-0002 не переписывать, нового научного Train для этих проверок не нужно.

## R03. B01: соединить написанный помощник с обычным потребителем

**Файлы:** `27.TranslationDomain/TranslationTrainedUnits.cs::TryComposeFromPhraseMappings`, готовый `TranslationPhraseCompositionPlanner.PlanAdmissible`, `Tests/ExperimentFixes/TranslationPhraseAdmissionAlignmentTests.cs`.

В разрешённой ветви EN→RU, ≤8 нормализованных source tokens, без accepted word maps вызвать готовый PlanAdmissible. Использовать текущие coverage/span условия: непустые parts/spans и coverage≥0.85 либо spans≥2 вместе с phrase coverage≥0.50. Проверка совместимости target/direction после композиции и остальные fallback сохраняются. Не переносить target-dependent predicate внутрь сжатого DP без доказательства достаточности состояния.

**Приёмка:** существующие wide-rejected и equal-coverage ProductPath-тесты уже вызывают `TranslationMachine.GetOrchestratorAsync().RunAsync` через `TranslateEnRuAsync`. Сохранить именно этот публичный фасад: прямые `ExecutorUnit.Execute` или helper не заменяют уже имеющуюся более сильную приёмку. Сохранить остальные положительные, ties, nonempty, нормализацию и 8/9 boundary проверки из уже написанного класса. Подключить принятый ordinary translation regression; phrase/word/exact preemption записывать, не угадывать. Обратная мутация только PlanAdmissible→Plan должна снова уронить два различающих product-path assertions. Не менять gold, confidence или порог покрытия ради этого.

Заявление о расходе памяти закрывается R09. Пока нет matched ordinary A/B, нельзя считать ресурсный допуск доказанным из прежней компонентной дроби. Приёмка корректности и ресурсный компромисс имеют отдельные поля.

## R04. B12-CW: подтверждающий сосед принадлежит выбранному flow

**Файл/метод:** `42.ProjectWikiDomain/PromptFlowGroundingModel.TryGround`, вызов на строке115 текущего pin. **Минимальная форма:** передать `best.FlowRef` третьим аргументом существующего `FindNearestWithScore`. Сохранить старый двухаргументный API, ranking, query, finite/positive guards, формулу/thresholds и уже правильный filter в FindLearnedPair. Отсутствие своего подтверждения не заменять глобальным fallback.

В существующем владельце `Tests/OrchestrationPlanTests.cs` добавить адресный ordinary regression выбранного инварианта; не дублировать production algorithm в expected. Новый proposed Fact назвать `LearnedGrounding_ConfidenceWitnessBelongsToSelectedFlow`, фактический FQN подтвердить discovery перед исполнением. Подготовка и сравнение состояний подробно разложены в [confidence review](codex-flow-confidence-review.md).

Фиксированные пары A=(FLOW-01,alpha,ALPHA_BODY_ONLY), B=(FLOW-02,beta,BETA_BODY_ONLY), отдельные PairId. Запрос `beta FLOW-01` проходит настоящий classifier. На исходном коде A-only даёт Mode2, A+B даёт learnedA с confidence29/30. После ремонта обе отрицательные ситуации дают прежний Mode2; `alpha FLOW-01` и `beta FLOW-02` продолжают выдавать полный собственный payload. Сравнить RouteMode, FlowRef, literal body/title, warnings; confidence не выдавать за калиброванную вероятность.

**Предложенный бюджет:**2 одноэлементных batches,17 Generate,2 Save,5 Load; точный breakdown в review. **Порядок baseline:** сохранить S-A/S-AB и observed outputs до различающего failing assertion на исходном коде. Иначе ожидаемое падение лишит candidate его подготовленного checkpoint. После load Train=0. Обратная мутация только scoped→global вновь нарушает отрицательный oracle. После восстановления повторить negative+оба positive. Дополнительные regression-классы имеют собственный счёт вызовов.

Этот шаг не решает ambiguous active revision и не доказывает смысловую релевантность всех same-flow уроков. R06 отдельно связывает witness с membership активной редакции. Исторический FIX-IM не отменяется: он решал источник тела и исполнил прежнюю карточку правильно.

## R05. Исправить вычислимые заявления; отдельно принять изменённые writers

| Владелец | Что менять | Различающий oracle |
|---|---|---|
| `P5201RejectedAnnotationAddressArmsTests` writer | Answer ceiling из gold histogram и того же comparer; tense ceiling отдельно; cold сохранить отдельным полем | Cold0 при ceiling2/4; gold3+1→3/4; allsame→4/4; empty явно invalid; изменение cold при прежнем gold не меняет ceiling |
| `P4901/src/FinitenessGovernedVerdict.cs::ComputeOverlap` | Три взаимно исключающие группы exact pair / seen input,new target / unseen input относительно TRAIN плеча | Текущий EXAM cold2+1+1, warm4+0+0; новая метка не делает input новым |
| P49 reporter / карточка107 | Полная панель и NEW_PROJECT_B/NEW_ONLY отдельно, с собственными ceilings | 6/16→11/16; 6/8→7/8 только projectB; NEW_ONLY3/4 обоих=ceiling3/4 |
| `P5801CorrectiveLessonCounterExampleArmsTests` | Фактические main/arm setup, correction, readout counters и внешний incomplete terminal | Main105TrainOnExample/56reads на нынешнем здоровом пути; missing stage/early-fail не становятся complete |
| `P4801WeightingCurriculumCeilingTests` budget/report | Учесть дополнительный настоящий gradient-oracle TrainStep | 4800arm+1oracle=4801 main; full class отдельно |

Разделить две работы: derived-пересчёт сохранённых строк имеет Train=0; native приёмка изменённого runtime writer выполняет назначенные Facts с отдельным фактическим ledger, включая их обучение. Для исторической поправки публиковать новый derived report с raw hashes и версией вычисления. Не дописывать в старый receipt поля как будто они наблюдались тогда. Для текущего producer создать новое execution evidence по назначенному тесту, если меняется runtime writer; Python-пересчёт сам по себе его не исполняет. Имена current ceiling helper P58 и finite guards P48/P49 уже есть — не назначать их создание заново.

## R06. Активная редакция цели, а не вечный запрет старой историей

**Доска:** B11/F08. **Владельцы:** PromptFlowGroundingModel, OrchestrationKnnStore, checkpoint contracts/OrchestrationPlanMachine, существующие `ExperimentBasedPromptFlowRetentionTests` и tests карточки48. Это сохранённый открытый контракт прежнего плана, а не новая сегодня реализованная сущность.

Typed active revision связывает FlowRef/RevisionId/target/body/MemberPairIds. Накопление input features идёт только внутри текущей совместимой редакции. История A остаётся историей; B1 создаёт новую редакцию, B2 той же B объединяет только B features. Выбор тела и confidence witness используют её membership. Не угадывать «последнюю» цель по PairId, JSON order или времени файла.

Export/import переносит weights, pairs и active revision вместе, проверяет ссылки и неизменность PairId payload. Для старого неоднозначного A+B snapshot — явный ambiguity, без тихого присвоения active target. Приёмка обычным consumer проверяет B1/B2/B3 retention, правильный bodyB и competitorC, включая restore; oldPairId-before-new и renaming IDs не меняют выбор.

Сохраняется прежняя точная доза одного порядка:5 batches,25readouts,1Save/1Load; обратный порядок — отдельный такой же run, вместе10/50/2/2. Новые structural rejects имеют Train0. R04 использует два разных flow без смены редакций и не расходует этот бюджет.

## R07. Коррекция использует фактически выданный ответ

**Доска:** B10/F09. **Владельцы:** `TranslationMachine.InlineTranslationOrchestrator.RunAsync`, `TranslationCorrectionLearningAdapter`, existing conditional `TrainStructuredBatchAsync`, publication/checkpoint state и `TranslationCorrectiveLearningTests`.

Производитель создаёт receipt из реально выданного successful flow result, захваченного request/state/version, direction/topic/receiver. Включать корректируемый low-confidence/source-echo; не требовать успешного семантического verdict для происхождения ответа. Связанная verifier receipt удостоверяет проверку того же actual. Caller-owned строка «проверено» не подменяет provenance.

Порядок: authenticity+immutable payload → поиск уже committed outcome → для новой доставки проверка captured state → conditional training со старым expectedStateVersion → atomic publish state+outcome. Повтор принятой receipt после роста версии возвращает прежний исход без Train. Новая stale или изменённый payload при старом ключе отвергаются до мутации. Нельзя исправить это одним сравнением rejected!=source: законное echo тоже бывает фактическим ответом, выдуманный третий текст тоже бывает неравным.

**Приёмка:** true echo с genuine receipt, чужой request/direction/topic/receiver/state, поддельный actual, duplicate до/после saved restore, CAS loser. Существующий учебный сценарий10уроков/60flowcalls/8verifier/2Save/3Load сохраняет область; новые structural/commit probes имеют отдельный ledger. Crash до durable сохранения state+outcome остаётся UNKNOWN_COMMIT_STATE, автоматический retrain запрещён. In-memory CAS не объявлять exactly-once через произвольный crash.

## R08. Loss и gradient описывают одну функцию

**Доска:** B04/F10. **Файл:** `35.ExperimentsV2/Core/ProbeNeuralNetwork.TrainStep`; существующие gradient tests и loss consumers.

Вернуть stable CE в logit domain: `(maxLogit-targetLogit)+log(sum(exp(logits-maxLogit)))`. Нужный для отображения clip вынести в отдельную диагностическую величину. Не обнулять правильный p−y gradient ради совпадения с ошибочным clipped loss.

**Приёмка:** logits[0,-20] дают CE≈20.000000002, не16.11809565; saturated и обычная область, finite extremes, independent central finite differences по всем обучаемым блокам вдали от ReLU/argmax boundaries; dtype/tolerance назначить до исполнения. Один W2 oracle P48 в обычной области не закрывает все блоки и saturation. Старые loss raw не пересчитывать задним числом как наблюдённый новый objective.

## R09. Доказать ресурсный компромисс на нужном пути

**B13/F02, planner:** matched old/new ordinary caller, одинаковые loaded state, panel, build/config, warmup и границы allocation/time counters, чередование arms. Публиковать raw per-repeat values, helper отдельно от whole-path. Формула overhead=(new−old)/old только на сопоставимых окнах. Caps10400/14300 относительно9864/13552 не являются строгими5%; рассчитывать целочисленный лимит по объявленному правилу округления. Median не охраняет регресс в меньшинстве повторов; либо назвать ограничение, либо заранее определить дополнительный tail/max критерий.

**Wiki:** принятый вынос Featurize сохраняется. Его rollback mutation, оставшаяся green по quality, показывает отсутствие защиты ресурса, а не ошибку семантической приёмки. Для ресурсного claim нужен собственный counter/измерение вызываемой операции и одинаковые данные.

**B02/F11, epochs:** на WC01 updates2,2,2,1 затем14нулей. Остановиться после первой полной zero-update эпохи, сохранив maximum cap; не зашивать «4эпохи» и не выводить runtime speed из Python replay. Приёмка: точные weights/predictions и presented/update counts WC01, fixture с поздним zero-update, fixture без zero-update до cap, явный empty TRAIN договор. Если обучающий цикл получает shuffle, stochastic updates или scheduler, аргумент о повторяемом неизменном состоянии перепроверить.

## R10. QA: точная принадлежность классов и свежая полнота прогона

**B08/F12:** `tools/test-batches/batches.tsv`, `Tests/TestSuiteBatchManifest.cs`, `TestSuiteBatchInventoryGuardTests`. Для119 перечисленных классов назначить смысловые семьи и resource/serialization ownership. Проверить положительный маршрут и first-match ровно одной семьи по фактическому discovery. Не складывать их в weak baseline и не накрывать широким prefix ради зелёного счётчика.

**B09:** recursive fallback различает явный synthetic mode без solution и существующий непригодный solution. При solution parsed0, missing declared project и неподдержанной записи — diagnostic fail, не поиск чужих вложенных copies. Нормальная действующая solution-ветвь и уже принятый21-test fix сохраняются. Для malformed/foreign-copy fixtures не нужен научный Train.

**B07:** historical full run имеет13751passed,501failed,94not-executed recorded rows и abort.501 — не текущий baseline;501−21 нельзя назвать нынешним480.342fail одного coordinator class — группа triage, не доказанная единая причина. До нового full run разобрать её representative failures по stack/input/state; repairs проверять на финальной сборке. Полный прогон, когда назначен, сопоставляет discovered test IDs с executed/skipped/failed/not-run и фактическим terminal, не только суммой счётчиков TRX.

## Команды и выходной пакет исполнителя

Ни одна команда ниже в этом аудите не выполнена. Это последовательность для среды с SDK; проектные tests назначенного ремонта могут обучать свои fixtures, поэтому отдельный actual ledger обязателен.

```powershell
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~TranslationPhraseAdmissionAlignmentTests" --logger "trx;LogFileName=phrase-admission.trx" --results-directory <owned-new-run>/phrase
```

Остальные фильтры заполняются точными discovered names владельца соответствующего ремонта. Для R04 новый proposed Fact сначала должен реально появиться в discovery; существующий broad OrchestrationPlanTests не включается скрыто в17calls. P50 replay команда публикуется после реализации его CLI, а не выдумывается сейчас. `--no-build` применяется только после успешной сборки того же source/dirty/config состояния.

Выход одного ремонта: проблема человеческими словами; old/new source и DLL identity; scope; настоящий red/green и reverse mutation с названной причиной; полный inventory выбранных tests; commands/terminal/TRX; RowId input/expected/actual; фактическая доза; что закрыто и что осталось. Красный соседний тест сначала классифицируется как product/fixture/contract/infrastructure — gold, Skip и threshold не подгоняются к actual. Публикация кода и публикация доказательств проверяются readback отдельно.


---

## Применимость плана после A07 — 12 ноября 2026

Историческое R03 выше («подключить готовый admissible planner») **выполнено и принято в назначенной узкой области** в `b64ba198`; на `f089a7d5` подключение подтверждено чтением обычного caller. R03 нельзя исполнять повторно или использовать как текущий блокер. Действующие новые бюджеты `PlanAdmissible` уже существуют; оставшаяся работа по памяти — применимость карточки, охват размеров входа и сопоставимый обычный путь.

[Обновлённый адресный план](post-admission/codex-repair-plan.md) задаёт текущий порядок и отрицательные случаи. Он сохраняет незакрытые R01/R02/R04–R10, уточняет уже существующий launcher с исключительным захватом, разделяет P58 corrected-item recall и остальные буквальные Source; независимость holdout ещё не доказана, задаёт стоимость **выбранного класса**, а не только основного Fact, и различает подтверждённые старые результаты от новых аудиторских пересчётов. Старые бюджеты, команды и наблюдения остаются историческими и не подменяют свежий discovery/фактический счёт вызовов.


**Поздняя сверка перед публикацией:** в `f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64` общий sink уже получил claim/occupied-check/single-shot запись. Прежнее описание отсутствия этих механизмов относится к старому срезу. [Точная дельта и остатки](post-admission/codex-late-ownership-review.md) и [актуальный план](post-admission/codex-repair-plan.md) сохраняют полезную реализацию и назначают ordinary timing, закрытие, platform containment и внешний итог.
