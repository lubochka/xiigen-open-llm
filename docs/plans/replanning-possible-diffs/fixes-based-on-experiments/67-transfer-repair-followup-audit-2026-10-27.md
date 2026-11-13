# Повторный аудит исполнения переносов: что исправлено и что ещё пропускают проверки

Дата: **27 октября 2026**. Проверенный срез: `455398b082ddd1a88e6eb6ffe1daba56ea289611`. Проверены 26 названий коммитов из присланного списка и дополнительный последний коммит KR `455398b082d`. Область — 17 переносов из [аудита64](64-transfer-execution-audit-2026-10-27.md), ремонты R1–R4 и их реальные потребители. Это чтение исходников, Git diff, карточек и сохранённых свидетельств; **сборки, тесты, мутации, inference и обучение в этом аудите не запускались**. Продуктовые файлы не исправлялись.

**Основной результат:** прежние дефекты KS/KHE, границы ATS и training core SVD исправлены в исходниках. Однако в R4 остаются четыре конкретных дефекта доказательства: случайная модель названа восстановленной обученной моделью; RNG сравнивается через неполный `ToString()`; конечный текст плана заменён проверкой идентификатора; относительная стоимость BW используется как защита другого фактора BPE. Это не четыре доказанные поломки продуктовых алгоритмов. Это четыре случая, когда зелёная проверка или запись «закрыто» не подтверждает требуемое поведение.

Следующая работа — [план68](68-transfer-repair-followup-plan-2026-10-27.md), сначала учебное состояние и RNG. Уже полученные положительные эффекты исходных опытов сохраняются. Новых результатов обучения этот аудит не создаёт.

**Дополнительно по следующему сообщению Любы:** проверен новый `846144464a856cd3f118bcfc0165c8117b06c167` / TTQ02 (§5 ниже). Его результат прочитан, не воспроизведён. Это новый экспериментальный commit, не ещё один production-перенос. Новое правило обязательного обучения диагностик применено в живых contracts/index/passports каталогаexperimets.

## 1. Новые находки

### FU-01 · P1 · Общая учебная квалификация подменена другим опытом с 0/4 → 0/4

Источник: [RestoredStateLearningQualificationTests.cs](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/RestoredStateLearningQualificationTests.cs:32), строки32–75,116–133,136–215; заявление о закрытии — [66](66-r4-row-evidence-2026-10-27.md), раздел «CS, RRD, MB — учебная квалификация с восстановленным состоянием», исходные строки589–648.

`SavedState()` заново генерирует случайные числа seed4242, а `Restored()` переносит плоский массив в свежую модель. Конфигурация V64/H16/L2 без LoRA; оптимизатор новый, step0→4. Ни сохранённый обученный checkpoint, ни накопленные моменты, ни RNG исходного опыта не загружаются. Контракт владельца [41-neural, H64](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md#h64-rd-at-mb-rpe-silu-cs-ac) требует V256/H64/L1, LoRA q/v rank4, восстановленные моменты step400→404 и RNG своей семьи.

Gold последних токенов четырёх Lessons: `[34,35,36,37]`. Закреплённое before `[42,37,43,46]`, after `[22,22,22,22]`. Поэлементное сравнение даёт **0/4→0/4**. Это арифметический вывод из констант теста, не наш прогон. Перемещение неправильного ответа не заменяет приобретённый навык. Тесты полезны как отдельная численная регрессия и повторяемость, но не закрывают перенос CS/RRD/MB на уже обученное состояние.

Дополнительные пробелы того же доказательства:

- `Assert.NotEqual(fresh.ToBytes(), run.Optimizer)` может различать только step0 и step4. Оно не доказывает наличие и правильность массивов моментов.
- Полные итоговые параметры и optimizer bytes сравниваются между двумя одинаковыми запусками текущей реализации. Это повторяемость B/B, а не независимый A/B-эталон.
- Logits/losses до последнего update не определяют всё состояние после него. Логический контрпример: изменить только embedding неиспользуемого токена63 после четвёртого update; ни Lessons, ни последующие запросы его не используют, embedding и lm_head раздельные. Повторяемая порча не нарушает сравнение B/B. Контрпример **не выполнялся**.
- Равенство optimizer bytes не доказывает RNG: stream может изменить cursor без влияния на уже вычисленные веса.

**Исправление:** восстановить назначенные S0, сохранить полезные синтетические проверки под честным назначением, отдельно квалифицировать фактор на прежнем корпусе и полном typed state. В документе66 снять закрытие этой оси; не повышать LR, не менять gold и не запускать новый bootstrap ради ненулевого результата.

### FU-02 · P1 · LC не сравнивает внутреннее состояние RNG

Источник: [DpoReferenceScoreCacheTests.cs:209](C:/Projects/llm_mvp_core/Tests/DpoReferenceScoreCacheTests.cs:209), [LlmModelRandomState.cs:29](C:/Projects/llm_mvp_core/03.TransformerModel/Randomness/LlmModelRandomState.cs:29).

После двух DPO-шагов тест сравнивает `CaptureRandomState(...).ToString()`. Snapshot — record с полем `Entries: IReadOnlyList<LlmModelRandomStateEntry>` без собственной глубокой сериализации. Строковое представление списка не перечисляет `ConsumedCursor`/seeds каждого элемента. Поэтому равные внешние поля и тип списка не означают равные состояния потоков. Dropout=0 дополнительно не упражняет потребление случайных чисел.

Правильные части ремонта сохраняются: сравниваются policy parameters, неизменность reference, optimizer bytes и step2. Но запись66 о полном RNG-паритете преждевременна. Контрпример для будущего теста: скопировать snapshot и изменить только один `ConsumedCursor`, сохранив topology, step и остальные поля. Глубокий comparator обязан отказать; нынешний `ToString()` это изменение не представляет. Мы snapshot не изменяли и тест не запускали.

**Исправление:** сравнивать typed header и все entries по стабильному ключу, включая количество/уникальность ключей, seeds и cursor; проверять удалённую/лишнюю запись и изменение одного поля отдельно. Не включать dropout и не добавлять DPO-шаги автоматически: это отдельное изменение профиля, а comparator можно проверить без обучения.

### FU-03 · P2 · IM: идентификатор маршрута принят за выученное тело ответа

Источник: [OrchestrationPlanTests.cs:266](C:/Projects/llm_mvp_core/Tests/OrchestrationPlanTests.cs:266), [TrainedRouteComposerUnit.cs:19](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs:19), [SerializeChosen:84](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs:84), [OrchestrationPlanMachine.cs:104](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanMachine.cs:104).

Новый `Mode1Grounding_AfterRestore_DeliversTheBodyOfTheGroundedRoute` требует FLOW-33 и отсутствие чужих FlowRef в `result.Value.PlanText`. Composer сам печатает `flow_ref` из grounding, независимо от learned body. Собственно `chosen.PlanText` добавляется отдельно в `SerializeChosen`.

Если убрать только `chosen.PlanText`, сохранив идентификаторы и заголовок, все перечисленные assertions нового Fact остаются удовлетворимыми. Тест не отличает пустое тело от правильного, а также не обнаруживает устаревшее тело с тем же FlowRef. Это контрпример из структуры программы, **не выполненная мутация и не утверждение, что текущий composer уже теряет тело**. Прямые reader-тесты с точным ShortPlanText/LongPlanText сохраняют ценность, но не проверяют итоговое форматирование после restore.

**Исправление:** на уже назначенном donor сравнить независимые полные тела10-a/00-b и title/route/metadata после Generate и настоящего orchestrator consumer. Новое обучение не нужно. [Карточка48](48-learned-route-body-identity.md) уже запрещала проверку одного заголовка; теперь задаётся конкретная ошибочная реализация, которую oracle должен отклонить.

### FU-04 · P2 · BPE: относительная цена повторов BW не защищает от лишней копии no-match

Источник: [BpeWeightedChunkTests.cs:222](C:/Projects/llm_mvp_core/Tests/BpeWeightedChunkTests.cs:222), [BpeTokenizer.cs:618](C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:618), [BpeNoMatchCopyTests.cs](C:/Projects/llm_mvp_core/Tests/BpeNoMatchCopyTests.cs).

Ресурсный тест сравнивает реальный Train на 1×/8× тексте и требует `eight < once*1.5`. Это защита BW — обучения на взвешенных представителях. Семантические BPE-тесты закрепляют old-state hashes и IDs, но не требуют сохранения ссылки на no-match список и не измеряют отдельно цену его копии.

Математическая причина нечувствительности: пусть R≥S>0 — цены 8× и1× после группировки, а возврат `new List<int>(ids)` вместо `ids` добавляет одинаковую Δ≥0 на одинаковых представителях. Тогда `(R+Δ)/(S+Δ) ≤ R/S`: дефект делает относительное условие **легче**, а не труднее. Ordered vocabulary/merges/IDs при этом сохраняются. Равенство добавок относится к одинаковым уникальным фрагментам и merge-порядку данного корпуса; это не утверждение об абсолютной цене любого корпуса. Мутация не запускалась.

**Исправление:** сохранить BW ratio как отдельную проверку; для FIX-BPE добавить no-match identity + match control и парное измерение native Train с одним изменённым фактором, одинаковым BW в обеих руках. Не объявлять BPE закрытым на основании BW. Само старое положительное наблюдение BPE не опровергнуто.

## 2. Текущее состояние всех 17 переносов

«Защита видна» ниже означает чтение конкретного кода/теста. Это не новый успешный запуск. Исторические количества passed и mutation-fail из66 — свидетельства автора ремонта, если нет отдельного inspectable raw artifact.

| Перенос / текущий владелец | Что теперь видно в коде | Что ещё нельзя объявлять закрытым |
|---|---|---|
| KS / [35](35-method-knn-target-scope.md) | После отказа память и ответы проверяются сразу; нет маскирующего repair-Train; есть facade queries | Перенос узкого opt-in k=1 на default k=5 без своего основания |
| KHE / [26](26-method-knn-training-admission.md) | Второй допустимый набор Guard отличается от Loop; no-op повторного Train становится видим | Универсальное обобщение по одному классу второго набора |
| ATS / [45](45-attention-transpose-strides.md) | R2 ограничивает прямую формулу rank4/swap1↔2, сохраняя generic fallback | Собственная H64/LoRA restored qualification и ресурсная ось по owner |
| SVD / [57](57-native-transfer-contract-gaps.md) | R3 соединяет FitFixedTraining с реальным Run и закрепляет directed readout | Не расширять узкий корпус до доказательства общего качества/готовности prompt routing |
| LC / [13](13-dpo-reference-cache.md) | Policy/reference/optimizer и step проверяются существенно полнее | FU-02; typed RNG; остальные lifecycle пункты прежнего owner не исчезают |
| CS / [57](57-native-transfer-contract-gaps.md) | Causal consumer и WithPast защищены; старая гипотеза о нечувствительности consumer отозвана по repair evidence | FU-01; learned S0 и A/B полного состояния |
| RRD / [44](44-rms-row-reduce.md) | Scalar/axis и consumer проверки дополнены чувствительными случаями | FU-01; восстановленный RD01, не новый random H16 |
| MB / [57](57-native-transfer-contract-gaps.md) | Backward/consumer проверки сохраняют полезность | FU-01; восстановленный MB01 и полный state |
| BPE / [12](12-bpe-no-match-copy.md) | Независимые старые canonical hashes/IDs вместо одной повторяемости | FU-04: no-match resource axis |
| BW / [31](31-bpe-weighted-chunks.md) | Старые hashes, веса/порядок и relative allocation bound | Отделить собственный эффект от BPE; native timing не выводить из ratio allocations |
| AST / [38](38-structural-knn-sibling-cache.md) | Multi-file facade memory, narrow/wide allocations, 16-query gold panel | Direct predictor panel не является всем пользовательским prompt→domain E2E |
| HNQ / [50](50-hnsw-training-query-norm.md) | Настоящие8 уроков: старые4 сохраняются, новые4 усваиваются; owner cases норм | 56→14 — расчёт числа операций, не CPU measurement; allocations +56 B не скрывать |
| PQ / [51](51-pos-query-bigram-reuse.md) | Exact128/32 panel,11/32→28/32, порядок памяти и allocation check реального reader | Не приписывать ресурсу дополнительный прирост accuracy; остальные E2E пределы owner |
| WC / [33](33-wiki-training-feature-reuse.md) | Cold/trained hashes, snapshot restore, публичное предупреждение gate | Учебный before/after readout за gate; public50/50 runner нельзя подменить повторением8/4 |
| KR / [49](49-router-learned-key-normalization.md) | Реальный writer, целый TrainableRoutingFlow; закреплённые bytes/SHA/provenance; guard cases | Авторские сообщения о broad-suite failures не являются нашим прогоном; привязать raw receipts |
| IM / [48](48-learned-route-body-identity.md) | After-restore mode/FlowRef и отсутствие чужих IDs | FU-03: полное body/title и состояние реального recipient |
| HCL / [53](53-hierarchical-local-history.md) | Настоящий E05 Run закреплён accuracy0.8693467, отделён от baseline | Историческая панель7 событий/152 токена/16 запросов остаётся открытой, как и заявлено66 |

Отдельно: KR `RouterLearnedKeyReadingTests.Decide` уже вызывает настоящий `TrainableRoutingFlow`; прежнее подозрение о ручной imitation guard к текущему коду не применяется. Не создавать из него новый дефект.

## 3. Связь с присланными коммитами

Все 26 названий из вложения сопоставлены Git-коммитам. Три относятся к соседней документной/экспериментальной работе, а не являются новым production-переносом. Группировка ниже не означает 26 независимых аудитов.

| Область | Коммиты из вложения |
|---|---|
| R1 KS/KHE | `71ca436a236e` |
| R2 ATS; R3 SVD | `4a7839d4c0bd`, `f9694321424a` |
| LC, CS, RRD/MB | `dc8eb29bd3a8`, `f04ba4dfe895`, `850b767904b6` |
| Общая qualification; status reconciliation | `76c3b027280c`, `d847a109cc51`, `9f4e1bd2df2a` |
| BPE/BW | `9faff7c8947a`, `69d8ad11e25d` |
| AST | `ae9bd860b982`, `70f01980bacd`, `49a380c05604`, `b4fb340fb5cf` |
| HNQ | `10da6c84bd1f`, `17fd1446360b` |
| PQ | `bebc44f5d920`, `c913304a9647` |
| WC, KR, IM, HCL | `fe8ba12f3563`, `52d654fff811`, `5c4c55b18093`, `261bf0022387` |
| Соседние работы, без нового production-переноса в этом аудите | `48c324a94793` (каталог272), `8cfa50580f3e`, `6e58d068db4d` (N62) |
| Дополнительно текущий HEAD, после присланного списка | `455398b082d` (KR whole flow/payload provenance) |

Источники не заменены сообщениями коммитов: находки выше привязаны к прочитанным assertions и product consumers. Для численной группы получен отдельный пакет от единственного worker `/root/repair_numerical_audit` (Astra ultra): [проверка численных переносов и review IM/BPE](audits/2026-10-27-r4-numerical-followup.md). Родитель проверял остальные строки и интегрировал выводы. Это один worker-пакет, не множество «циклов».

## 4. Границы доказательства и правка карточек

В ограниченно просмотренных `TestResults`, `.tmp`, `.codex-test-temp` не обнаружены raw logs/TRX, адресуемые из заявлений ремонта о мутациях. Это **не доказательство отсутствия логов где-либо ещё**. Числа7/7,17/17,32/32,52/52 и результаты mutation runs из66 не переименовываются в самостоятельно воспроизведённые. Будущий ремонт обязан приложить source SHA, точную команду/фильтр, terminal exit, TRX и raw observations; одна итоговая строка не заменяет их.

Усилены общий [контракт03§25](03-test-and-execution-contract.md#repair-followup-20261027) и15 действующих карточек-владельцев для17 переносов; CS/MB/SVD разделяют57. В них заданы конкретные хорошие/плохие проверки, область успеха и остаток работы. Старые опыты и отчёты сохранены. Эти документные изменения не являются разрешением запускать весь план, менять production или считать отсутствующее runtime evidence полученным.

## 5. Дополнительный commit TTQ02: настоящий учебный результат и ограничения учёта

Источники: [Program.cs](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TTQ02/Program.cs:347), [metrics](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TTQ02/results/attempt-0001/metrics.json), [status](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TTQ02/results/attempt-0001/status.json), [авторский report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TTQ02/results/attempt-0001/report.md). Новые результаты не получались: ниже сверка raw с путём кода и арифметикой счётчиков.

Вместо прежних0updates выполнены60native SFT updates T1 из W0, LR.003, AdamW .9/.95/decay.01/eps1e−8. Raw подтверждает T1 **0→22/23**, Q1 **0→0/29**, Q2 **0→2/21**; последний token верен только наT1. Поэтому локальный успех обучения есть, а primary qualification2из3 не пройдена. Формулировка об ошибке в серединеT1 соответствует сохранённому readout; отсутствие переноса на Magnitude не превращается в общий успех.

**FТ-01 · P2 · стоимость обучения недосчитана названием счётчика.** `forwards_performed=24` растёт только в явном ReadRow. В `Trainer.TrainSupervisedNextTokenStep` есть ещё один native Forward на каждый из60updates. Полный исправленный прогон — **84Forward =60training+24diagnostic**. Сохранённый `before-correction` содержит10updates/19diagnostic, то есть ещё29Forward. Совокупность этих двух попыток —70updates/113Forward; это производный учёт по коду и raw, не новый runtime-счётчик. В карточке разделение исправлено. Будущий точный ремонт experiment-reporting — поля `training_forward_calls`, `diagnostic_forward_calls`, `total_forward_calls`, отдельные attempt totals. Само исправление metadata не требует повторять обучение.

**FТ-02 · P2 · «побайтово» шире сохранённого доказательства.** `StateDigest` в Program:185–195 хэширует последовательность float bytes `GetCheckpointParameterEntries`. Raw показывает одинаковый digest trained/saved_auth `9dc0eb81…`; это сильное свидетельство равенства этой проекции model parameters при том же порядке. Она не включает имена/shapes, optimizer, RNG и все bytes checkpoint-файла. Не объявлять проверенным сохранение полного training state. Для будущих receipts добавить typed keys/shapes и отдельные optimizer/RNG, если заявляется continuation; здесь повтор исходной подготовки, continuation Wauth не заявлен.

**Исправление критерия остановки принято по source:** joint-LF ввод нужен обучению, но stop каждые10updates читает exact-DPO response. Первый сохранённый вариант остановился на10; исправленный прошёл60. Это разные наблюдения, не один прогон. Старый output сохранён. Не утверждать, что любой frozen reader бесполезен: он способен локализовать ошибку, но **по прямому правилу Любы не удовлетворяет новой программе учебного эксперимента без её реального Train**.

Паспорт[TTQ02](../experimets/cards/DPO-TERMINAL-TARGET-QUALIFIED-02.md), оба его статуса в[индексе](../experimets/cards/00-execution-index.md) и продолжение обновлены: RESULT_RECORDED, qualification FAIL, NEXT Q002/P01-PRIMARY-01. Повтор только ради смены текста паспорта не назначается. Подробный отдельный пакет — [аудит diagnostic learning](../experimets/reviews/2026-10-27-diagnostic-learning-contract-review.md).

## 6. Последующее уточнение карточек и граница проверки документов

По прямому уточнению Любы устранены нулевые учебные бюджеты десяти новых P-паспортов; точные training/data/admission/Forward budgets —[36](../experimets/36-diagnostic-learning-programmes.md). У исторического P25 был только0Train readback, поэтому отдельно назначен[P25-LEARNED-RESTORE-02/Q125](../experimets/cards/P25-cold-restore.md#p25-learned-restore-02):8уроков native памяти и72Flow до/после/нового процесса restore. Старые результаты сохранены. Обе KNN-конструкторные карточки получили конкретные отрицательные случаи потерянного payload, неверной модели/массива функций и записи урока чужим получателям.

Статическая сверка документов: проверены пути1790локальных ссылок в45 затронутых документах, отсутствующих целей не найдено; это existence-check путей, не проверка всех anchors или работоспособности referenced code. После последних правок повторно проверены69ссылок нового пакета и P25. Индекс содержит Q001–Q125 ровно по одному, без пропусков; у TTQ02 больше нет текущего DESIGNED_NOT_RUN. В десяти исправленных PRIMARY видны ссылки на36 и нет прежнего нулевого полного учебного бюджета. `git diff --check` на двух документных каталогах завершился без сообщений. Это статические проверки, **не тесты/сборки/обучение**.

Во время работы другой исполнитель продолжал общий checkout: после846144464a8 появился25fa8a1aca5 с P01, затем75107af930a1d4c55d60fa8a4ef7c01643f13cb0. Публикационные команды этот аудитор и worker не выполняли. Scope проверки17переносов остаётся закреплённым455398b082d, TTQ02 —846144464a8; появление новых коммитов не означает их полный аудит. Для навигации дополнительно прочитан P01 terminal receipt:800SFT выполнены, DPO admission заблокирован, ось DPO открыта. Живой индекс выбирает первый Q без свежего terminal учебного receipt, а не навечно фиксирует следующий номер. Все изменения этого назначения — Markdown; чужие product/runtime результаты не присваиваются аудиту.
