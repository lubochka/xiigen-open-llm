# Аудит исполнения переносов 26 октября 2026

Дата аудита: **27 октября 2026**, Asia/Jerusalem. Проверенный source: `4ce8bdd0dd3942a20b32c569c2b0f0c37894dd23`, ветка `codex/dpo-implementation`. Объём — **17 переносов из сообщения Любы и классификация трёх соседних экспериментальных коммитов**. Это аудит исходников, тестов и сохранённых свидетельств; сборки, тесты, discovery, inference и обучение здесь не запускались. Не заявляется отсутствие всех возможных ошибок продукта.

Практический результат: переносы действительно содержат изменения кода, но завершение полного контракта проверки не подтверждено. Найдены два конкретных дефекта тестов памяти, расхождение перенесённого ATS-фактора с опытом, разрыв между проверяемым SVD helper и реальным Run, а также недостающее подтверждение обучения/состояния/потребителя. Порядок исправлений — [65](65-transfer-repair-plan-2026-10-27.md). Остальные применимые карточки усилены контрпримерами; общий договор — [03§24](03-test-and-execution-contract.md#transfer-audit-20261027).

## Что проверено и что считается доказательством

Родитель прочитал product diff десяти переносов WC/KHE/KS/IM/KR/BPE/BW/AST/HNQ/PQ, соответствующие тесты, адресные фрагменты владельцев планов, реальные вызовы и первичные отчёты PQ/HNQ/AST/BW/MKSC/WC/KR. Для KHE/IM/BPE проверены code/test и evidence pins владельцев; все их raw runs заново не разобраны. Независимый исследователь `/root/transfer_math_audit` проверил ATS/MB/RRD/CS/LC/SVD/HCL; его точные ссылки, математический разбор и ограничения — [отдельный пакет](audits/2026-10-27-math-transfers-review.md). Это один независимый пакет по семи переносам, а не независимая проверка всех семнадцати или N циклов.

Наличие исходника теста не считается его прохождением. Сообщения transfer commits заявляют успешные сборки, тесты и некоторые mutation checks; они прочитаны как отчёты автора, а не независимо воспроизведённые здесь результаты. Например, KHE проверял мутацию очистки памяти, KS — снятие target projection/спуск во вложенные тела, PQ — арифметические и структурные мутации. Это не те контрпримеры R1/F5, для которых ниже выявлены пробелы. В проверенных назначенных местах отсутствовали `artifacts/fbe/` и `Tests.Support/ExperimentTransfer/`; в `Tests/ExperimentFixes/` обнаружен `WikiFeatureReuseLearningTests.cs`. Это отсутствие назначенных подтверждений в этих путях, а не доказательство, что никто нигде не запускал тесты. Чужие untracked learning-sets/training-artifacts и появлявшиеся во время аудита изменения Tests/Fixtures не использованы как завершённые результаты переноса.

Skills прочитаны адресно: архитектура, planning startup, plan review, generated-code review, readability и словарь человеческой коммуникации; огромные AGENTS/CLAUDE и skills не заявляются полностью загруженными. Активная фаза — чтение и Markdown. Никакие выводы ниже не являются fresh runtime failure или runtime PASS.

## Матрица фактического переноса

«Код внесён» означает найденный конкретный commit и реализацию. «Проверка неполна» не означает автоматически неверный алгоритм.

| FIX / опыт | Commit | Куда внесено и что видно в коде | Незакрытая часть |
|---|---|---|---|
| WC / WC01 | `4cb23f0f7e8` | ProjectWikiRequestRoutingTrainableModel.Train готовит features один раз на строку до эпох | Есть независимый historical state oracle; acquisition/readout, cold restore, public gate и costs явно отложены самим тестом |
| KHE / KH-EXT-01 | `73a86de4658` | SemanticCompilerFacade.TrainMethodKnn допускает ≥4 настоящих adjacent same-file pairs | Отказ проверяется, но positive replacement-test повторяет тот же набор и не обнаруживает no-op; F2 |
| KS / MKSC01 | `4f85ac9c7d7` | Отдельные ExtractWithTargetScope / TrainWithTargetScope / facade ingress; только Next получает outer-body scope | Тест отказа затем переобучает память; F1. Opt-in k=1 намеренный, default k=5 не квалифицирован |
| IM / IDENTITY-M1-01 | `e2b844f5068` | FindLearnedPair ограничивает kNN уже выбранным FlowRef; PlanMachine вызывает этот метод | Нет полного assigned restore→GeneratePlanAsync→wrapper доказательства; direct reader tests остаются частичными |
| KR / KR01 | `a88c019acce` | Reader использует те же C#/ .NET replacements, что writer | Тест вручную создаёт profiles вместо загрузки pinned trained payload; F5 |
| HCL / HCL01 | `f5c5049337b` | Исправлен local-history reader модуля34; Run использует общий механизм | Новая synthetic панель не покрывает всю историческую 7/152/16 панель и её provenance; не neural RP01 |
| SVD / SVD01 | `3852e774fa9` | Разделены directed и symmetric readers в модуле34; текущий Run вызывает directed | FitFixedTraining дублирует training core Run; новые tests обходят реальный Run; F4 |
| LC / LC-REF-CACHE | `8056157ed88` | Reference-score cache подключён к DpoTrainer | Метрики не заменяют полного state parity; negative lifecycle/authority/rollback требуют покрытия |
| CS / CS01 | `91154df3311` | Causal-score path внесён в SimdOps/TensorOps/CausalSelfAttention | Локальные numerical/gradient tests не заменяют назначенные restored SFT steps и полный state/consumer contract |
| RRD / RD01 | `416a64d7e0e` | Последняя ось Sum читается непрерывной строкой | Нужен прежний last-axis/keepDims scope и native restored SFT qualification |
| MB / MB01 | `39330a83a58` | Mean backward последней оси использует строковую адресацию | Нужны полный state parity и preservation fallback; primitive test сам по себе не qualified transfer |
| ATS / AT01 | `046e0ded876` | Изменён generic Transpose: повторно используется int[rank] | Это другая реализация относительно rank4 swap1↔2/direct formula опыта; F3 |
| BPE / BPE-NO-MATCH-COPY-01 | `f317cd7ee32` | MergeChunk возвращает тот же список при отсутствии пары | Нет строгого полного old→new tokenizer golden и native resource receipt; одинаковые candidate fit не доказывают legacy parity |
| BW / BW01 | `8565b6cdb32` | Дубликаты chunks представлены multiplicity; first-occurrence order сохранён | Частные weight/tie tests есть; полное old state/ordered merges и два native resource профиля не подтверждены |
| AST / AST01 | `d28c925795a` | Кэш sibling fields per tree применён в TrainFromSource и multi-file facade | Direct vectors/ParentWalks не защищают весь facade batch/readout и оба narrow/wide resource профиля |
| HNQ / HNQ01 | `a37d0b9f52a` | Норма query вычисляется один раз в GreedySearch, SearchLayer, SelectNeighbours | Нет всего пути genuine lesson insertion→trained representation→index→старые/новые ответы HNQ |
| PQ / PQ01 | `4ce8bdd0dd3` | Query bigram set создаётся после empty guard один раз в Predict | Private signature не доказывает стоимость; отсутствует полная PQ128/32 acquisition/readout/cost qualification |

Побочные E05 fits в Compile уже учтены в владельцах26/35/37; это проверенная существующая оговорка, не новая находка и не основание объявлять MethodKNN EXAM обучающим для его таблицы.

Для PQ/HNQ/BW/AST сверены условия математической эквивалентности: query/input неизменен внутри вызова; та же последовательность float accumulation для нормы; сумма multiplicity равна числу исходных вхождений; sibling count/position зависят от одного immutable tree. Это аргументы о соответствующих вычислениях, а не новые измерения скорости или качества.

### Три соседних коммита — экспериментальные результаты

| Commit | Содержание | Статус в этом аудите |
|---|---|---|
| `a23026550e6` | Опыт144, насыщение дозы | Не дополнительный production transfer |
| `e585a3290e5` | KNN-DOMAIN-PRINCIPLES-01 | Отдельный эксперимент; production constructor этим списком переносов не подтверждён |
| `f1309ffb219` | KNN-DOMAIN-CONSTRUCTOR-ROUTE-01 | Отдельный эксперимент; маршрутизация конструктора не объявляется подключённой к ProductHost |

При просмотре изменений этих трёх коммитов вне experimental source каталогов найден required-changes.md, а не новый production constructor. Их полная экспериментальная методология в этот аудит исполнения семнадцати переносов не входила. Положительные результаты сохраняются; отсутствие production интеграции не отменяет их.

## Конкретные находки и исправления

<a id="f1-ks-retention"></a>
### F1. Проверка сохранения памяти KS маскирует её возможную потерю

**Высокий приоритет, дефект теста.** [MethodKnnTargetScopeLearningTests.cs](../../../../Tests/MethodKnnTargetScopeLearningTests.cs), исходные строки148–172: после отказа на159–167 снова вызывается TrainMethodKnnWithTargetScope на170. Даже если отказ очистил таблицу, это обучение вернуло бы её к четырём парам, и assertions171–172 прошли бы. Комментарий о сохранённом scoped answer не подкреплён чтением ответа.

Ремонт: сохранить injected predictor, снять его четыре правильных ответа до refusal, затем проверить те же ответы и ordered memory **сразу после** refusal. Не делать repair-fit. После этого отдельно пройти настоящий Compile на заданных C#-маркерных query. Это не установленная потеря памяти текущим продуктом — текущая guard-ветвь выходит до Train; установлен тест, который такую регрессию пропустит.

<a id="f2-khe-replacement"></a>
### F2. Проверка KHE не отличает повторное обучение от его пропуска

**Высокий приоритет, дефект теста.** [MethodKnnAdmissionLearningTests.cs](../../../../Tests/MethodKnnAdmissionLearningTests.cs):99–116 дважды предъявляет TrainableBatchSource и ждёт те же counts/ответ. Реализация «после первой загрузки больше не обучать» удовлетворила бы этому сценарию.

Ремонт: второе допустимое предъявление должно иметь четыре Guard target вместо четырёх Loop. По [CanonicalType](../../../../12.SemanticCompiler/Prediction/MethodKnn.cs):297–309 If+Ret даёт Guard. Ожидается Loop→Guard через Compile, по-прежнему четыре пары. Для refusal проверять буквальный правильный Loop, а не лишь non-null. Историческая одноклассовая панель остаётся проверкой удержания, не обобщения.

<a id="f3-ats-factor"></a>
### F3. ATS переносит не тот узкий фактор, который был измерен

**Высокий приоритет относительно точности переноса; численный дефект не установлен.** AT01 измерял rank4 swap1↔2 с прямой адресной формулой и неизменным fallback. В [TensorOps.cs](../../../../00.TensorCore/Core/TensorOps.cs):868–898 переработан общий Transpose: массив координат переиспользуется, но generic division/modulo и FlatIndex остаются. [План45](45-attention-transpose-strides.md) сохранял общую ветвь.

Ремонт: вернуть предусмотренный bounded fast path и прежний fallback. Если сохранить generic-оптимизацию как отдельную идею, ей нужен собственный небольшой опыт, а не присвоение успеха AT01. Точные доказательства и математика индексов — в [независимом пакете](audits/2026-10-27-math-transfers-review.md). Её самостоятельный выигрыш этой сессией не измерен.

<a id="f4-svd-consumer"></a>
### F4. SVD тестирует другой training вход, чем реальный Run

**Средний приоритет, пробел интеграции и дублирование кода.** E10 содержит FitFixedTraining и отдельный counts→PPMI путь в Run; новые tests используют первый. Общим остаётся TruncatedSvd. Текущий Run выбирает ArgmaxDirected верно, но будущий возврат в нём к symmetric reader не сделает helper tests красными.

Ремонт: общий training core и один явный directed consumer для фиксированной панели и Run; symmetric API сохраняется отдельно. Проверить non-symmetric directed matrix и вход настоящего Run. Не приписывать этому модулю работу neural RP01 или общего prompt→domain router. Ссылки на точные строки — [пакет семи переносов](audits/2026-10-27-math-transfers-review.md).

<a id="f5-evidence"></a>
### F5. Часть тестов даёт более узкое доказательство, чем требуется карточкой

- [BpeNoMatchCopyTests.cs](../../../../Tests/BpeNoMatchCopyTests.cs):56–67 сравнивает два fit новой версией. Они могут одинаково изменить merge-order. Round-trip32–51 тоже не доказывает старую таблицу merges. Нужен pinned historical tokenizer и exact IDs.
- [RouterLearnedKeyReadingTests.cs](../../../../Tests/RouterLearnedKeyReadingTests.cs):145–166 конструирует profiles вручную. Это допустимый reader contract, но не проверка restore выученного payload KR01. Название source в строковом поле не заменяет provenance.
- [PosQueryBigramTests.cs](../../../../Tests/PosQueryBigramTests.cs):192–205 проверяет private parameter type. Сбор HashSet внутри каждой итерации сохранил бы эту сигнатуру. Нужны actual operation/cost observations в учебном readout.
- [WikiFeatureReuseLearningTests.cs](../../../../Tests/ExperimentFixes/WikiFeatureReuseLearningTests.cs):24–27 **честно** оставляет readout/cold restore/gate/receipts вне реализованного объёма. Его fixture SHA проверен чтением: `e684017bcf44cb96c2951e2a49926d08cc9a869f38e5f229bedcda8cc23f8210`; это положительный пример независимого state oracle, который нужно дополнить, а не удалить.
- Для LC/CS/RRD/MB/ATS остаётся недоказанным назначенное обучение с восстановленного state и полные последствия update; для HCL/SVD — исходная historical панель. Равные средние метрики могут скрывать разные параметры или компенсирующие ошибки строк.

Здесь не утверждается, что перечисленные microtests бесполезны или продукт обязательно неправилен. Устанавливается точная граница их доказательства и соответствующая незакрытая часть исходных планов.

<a id="f6-plan-state"></a>
### F6. Старые команды могут ошибочно назвать уже оптимизированный HEAD baseline

**Высокий приоритет для следующего исполнения.** README/registry и карточки сохраняли «внедрение ещё не выполнено» и рецепт baseline до product diff. В [03](03-test-and-execution-contract.md) исходные§9–11 и [51](51-pos-query-bigram-reuse.md):167–202 фиксировали baselineCommit=HEAD до изменения. После перечисленных commits эта команда выберет уже B.

Ремонт: текущие статусы и ссылки внесены в README/registry/карточки, общий§24 отделяет первую реализацию от последующей квалификации. Старые результаты и бюджеты не стираются. Конкретная последующая qualification использует явную пару source revisions и общий test harness; A=B недопустимо. Отсутствующий старый native receipt не «восстанавливается» переименованием исторического experimental отчёта.

## Ограничения потребителей и ширины аудита

Проверены ближайшие места, где успех мог оказаться ложным: KS меняет только Next, не query features; IM фильтрует уже выбранный FlowRef, не добавляя его в query; KR сохраняет прежние confidence floors, отказ при слабом/tied evidence и известные DomainHint/substring границы. MethodKNN Mixed fallback уже даёт2/4 в исходной панели; это не приобретение с нуля. AST/POS caches не являются новым обученным доменным selector.

HCL/SVD — действующие алгоритмы экспериментального модуля34. KS сознательно opt-in при k=1. IM найден в реальном OrchestrationPlanMachine, но тест helper не доказывает весь final rendering/receiver. Полная публичная цепочка prompt→domain→модель→обновление только выбранного state, внешнее выполнение специалистов и общая export-accuracy политика **не подтверждены** этим локальным аудитом.

Более широкий аудит может обнаружить ошибки в маршрутизации, публикации/rollback checkpoints, конкуренции владельцев и объединении нескольких оптимизаций. Здесь проверены только указанные ближайшие границы; это не заявление о достаточности всей архитектуры.

## Первичные источники для повторного чтения

- Product/test изменения — перечисленные17 commits и source HEAD выше.
- [PQ raw/report](../../../../experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md), [HNQ](../../../../experiments-v2/RP01-MICRO/HNQ01/results/report.md), [AST](../../../../experiments-v2/RP01-MICRO/AST01/results/attempt-0001/report-for-user.md), [BW](../../../../experiments-v2/RP01-MICRO/BW01/results/attempt-0001/report-for-user.md).
- [MKSC](../../../../experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/report-for-user.md), [WC](../../../../experiments-v2/RP01-MICRO/WC01/results/attempt-001/report-for-user.md), [KR](../../../../experiments-v2/RP01-MICRO/KR01/results/report.md).
- KHE/IM/BPE — source pins и payload paths в прежних владельцах26/48/12; математические семь — independent packet. Ссылки на опыт не означают его повторный запуск.

Новые гипотезы или показатели качества здесь не придуманы. Изменены только документы аудита, плана ремонта и применимых карточек. Будущее выполнение требует отдельного прямого поручения, остаётся на конкретном ID и не запускает всю очередь автоматически.
