# Два математически обоснованных ремонта обученных моделей E05/E10

<!-- transfer-entry-reload -->
**Вход исполнителя.** [R05](../fixes-based-on-experiments/01-registry.md#work-fix-svd). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](../fixes-based-on-experiments/03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](../fixes-based-on-experiments/01-registry.md#executor-next).

## Уточнение допуска FIX-SVD — 3 ноября 2026

**Приостановить общий перенос.** Вошёл по точности направленных связей и математическому обоснованию: симметричное LLᵀ не представляет произвольную направленную матрицу. Однако удвоение состояния превышает ресурсный предел. Математика объясняет успех, но не заменяет допуск.

- Точность: 2/5 → 5/5 (+60 п.п.); направленные случаи 0/3 → 3/3.
- Полезный результат: Все три направленные связи различаются.
- Цена: Удержанное состояние 120 000 → 240 000 Б (+100%); выделения обучения 7 577 464 → 7 697 488 Б (+1,58%). Время не квалифицировано из-за прогрева.

[Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-svd). Старое общее разрешение ниже не переопределяет этот вывод. Это уточнение только FIX-SVD; остальные механизмы документа имеют собственные выводы. Сначала разобраться с измеренным регрессом на сохранённых данных. Нового запуска, изменения продукта или обхода проверки этим дополнением не назначено.


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN/STATE:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно. Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять чтение именно памяти, изменённой назначенным Train, с реальными ключами и payload до потребителя и после restore. Законный kNN retrieval сохранён; запрещена подстановка gold/архивного oracle вместо текущего поиска в обученной памяти. Отдельный PASS структуры или счётчик-константа не доказывает этот consumer либо новые updates.

Для FIX-SVD исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](../fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Сохранять построчные исходы и независимый gold выбранного домена вместе с итоговой долей. Одинаковая общая точность может скрыть потерю прежнего урока и случайный выигрыш другой строки; изменение scorer/support само по себе не доказывает обучение. Для данной карточки подтверждать назначенный Train и чтение его результата реальным потребителем; существующую проверку усиливать сохранённым learned state вместо дублирования искусственного теста.

Применимость: FIX-SVD. Основание и адресные проверки — [план85](../fixes-based-on-experiments/85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](../fixes-based-on-experiments/03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Панель должна сохранять точное множество уникальных RowId, Kind и gold. Контрпример P05: [T1,T2]→[T1,T1] имеет тот же размер, но прячет потерянный урок. Контрпример P11: 2/4→2/4 скрывает E1 gain и E2 loss; constant-answer baseline считать отдельно. 1→0 после настоящего обучения остаётся валидной регрессией. Сдвиг весов сам по себе не квалифицирует результат, PARTIAL и нулевой назначенный Train не закрывают учебную цель.

Применимость: FIX-SVD. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](../fixes-based-on-experiments/88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Сверка с текущим native ремонтом, 27 октября 2026.** FIX-SVD уже использует общий [FitFixedTraining](C:/Projects/llm_mvp_core/34.Experiments/Experiments/E10SvdPmiEmbedding.cs:33): Run вызывает его на строке109 и ArgmaxDirected на117; directed reader связывает SourceFactors и TargetFactors на220–221. Поэтому раздел «Native tests и compatibility» ниже описывает исходное назначение и оставшиеся критерии, а не повторное выделение core или повторную реализацию R3. Сам source defect закрыт в узкой области; новые tests здесь не запускались.

Оракул направленного чтения должен отвергать замену R на L на той же уже обученной несимметричной PPMI и сохранять wait controls; повтор factorization ради assertion не нужен. Source-source similarity — отдельный прежний контракт.4 factor fits/2 reference fits/30 queries исходного назначения не расширяются. Это обучение счётчиков и факторизация с собственным seed, без AdamW; сохранённые factors/PPMI не объявлять native checkpoint restore. Дополнительные120 024 allocated bytes и удвоение retained factors остаются регрессией: правильная формула сама по себе не даёт исключения P1-L и не квалифицирует общий resource перенос. FIX-HCL в этом файле — историческое дополнение к актуальному [owner53](../fixes-based-on-experiments/53-hierarchical-local-history.md).

Дата: 23.10.2026. Новые кандидаты FIX-HCL и FIX-SVD. [Общий контракт 02](02-native-transfer-and-test-repair.md) обязателен. Приоритет высокий по силе объяснения и измерению качества, но получатели находятся в существующем проекте `34.Experiments`. Это **не доказательство**, что именно эти классы сейчас исполняют RP01 или выбирают предметный домен. Их перенос в другой production learner без traced consumer не назначен.

## FIX-HCL — локальный номер осмыслен только в своём кластере

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN/STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание [HIERARCHICAL-LOCAL-HISTORY-01](../../../../experiments-v2/RP01-MICRO/HCL01/results/attempt-0001/report.md), [metrics](../../../../experiments-v2/RP01-MICRO/HCL01/results/attempt-0001/metrics.json), [predictions](../../../../experiments-v2/RP01-MICRO/HCL01/results/attempt-0001/predictions.jsonl). Тема оркестрация, переходы синтетических событий проверки/сохранения; получатели cluster и within-cluster BigramMarkov. Native counts обучены; не neural DPO.

На 16 запросах A6→10, B6→16; cross-cluster after6/12→12/12;4same-cluster controls сохраняются. Перестановка локальных ID меняет 12 из 16 ответов A, ни одного B. Trained table hashes A/B равны для каждого порядка. Отсутствие истории целевого кластера не было проверено в исторической панели:0 срабатываний из 128. Allocation fit совпал; дополнительная маленькая history-map и время не доказаны как улучшения.

В [E05HierarchicalPrediction.cs:108–147](../../../../34.Experiments/Experiments/E05HierarchicalPrediction.cs) within learner обучается на отфильтрованной подпоследовательности **своего** кластера, а reader139–140 подаёт локальный ID текущего токена из другого кластера. Исправление: перед чтением очередного target обновить `lastObservedLocal[curCluster]` из текущего наблюдаемого токена; query к within[predCluster] получает последнее наблюдение **predCluster**. На independent sequence очищать history, не переносить историю от другой EXAM строки или следующего gold token. Если history цели нет, сохранить прежний явный fallback local0; записать no-history count. Не подставлять чужой curLocal. Выбор кластера, counters и tie order прежние.

Доказательная опора: локальный словарь задаёт пару (cluster,localId), а не глобальный localId. Перенумерация одного кластера должна согласованно переставить его таблицу и query, сохраняя семантический ответ. Передача номера из другого пространства не обладает этой инвариантностью. HCL проверил это на canonical/permuted вариантах и показал, что сумма 10/16 скрывала разные неверные запросы. Инвариантность не доказывает верность kMeans или обобщение на новые темы.

### Native test без скопированного learner

Файл `Tests/ExperimentFixes/HierarchicalLocalHistoryTransferTests.cs`; fixture HierarchicalLocalHistory01 получает [train.json](../../../../experiments-v2/RP01-MICRO/HCL01/train.json) и [exam.json](../../../../experiments-v2/RP01-MICRO/HCL01/exam.json) побайтно. Словарь 7 событий, фиксированное cluster-members соответствие,152after tokens,16query histories. Canonical и permuted используют одинаковые семантические labels; gold отдельно. Не запускать kMeans/BPE для этого micro-test.

Нужен конкретный доступ к native вычислению: в том же E05 файле выделить существующие блоки 108–123 и 125–149 в переиспользуемые `FitWithinClusterState` и `EvaluateFixedPanel`; основной Run должен вызывать их. Это **планируемые методы**, не существующий API. Сигнатура seam принимает train tokens, tokenToCluster, ordered clusterMembers и observed histories; gold поступает только scorer. Cluster counts, local counts и predictions возвращаются как read-only result. В common baseline подготовке допустим только этот механический extraction без изменения формул/curLocal/fallback. Зафиксировать отдельным refactor diff и сравнить его statements/callers с текущим кодом; A захватывается **после одинакового seam**, до смены history reader. Тест не содержит альтернативную реализацию Bigram/reader; B меняет лишь reader внутри native E05. Это явное исключение к tests-only common harness02, не разрешение «заодно починить» алгоритм до A.

Матрица исходного опыта: arm×2member orders×2stages=8 условий; before fit2native Train, after fit4native Train на условие/порядок. Итого 24native Train,128exam queries,128cluster Predict+80within Predict=208. Старые A/B модельные fits не повторяются ради каждой assertion. State snapshots/count tables сохраняются в output; при отсутствии native restore повторные assertions читают receipts, не запускают fit. Сравнить 4/4controls, направления 6/6, after10→16, semantic permutation equality4/16→16/16; не считать нечисловой успех Run доказательством обучения.

При будущем разборе падений адрес [Tests/ExperimentsTests.cs:353–390](../../../../Tests/ExperimentsTests.cs): `E05_Run_WithSufficientTokens_Succeeds`, `E05_Run_BaselineAndEnhancedAccuracy_AreInValidRange`, `E05_Run_NotesContainKValue`, `E05_Run_TooFewTokens_Fails`, `E05_Run_SmallVocab_FailsVocabSizeGuard`. Не менять их guard/range expected и не запускать большие Run ради micro factor; первые 3 — отдельный integration smoke при явно выбранном полном E05 scope, не часть 24fits. После B назначены два failure guards. TooFewTokens отказывает до обучения. SmallVocab получает RepeatingPattern(5,300), а native Run сначала делает flat Bigram Train на 240 tokens и 59 Predict и лишь затем отказывает на seenCount<50 (E05:35–52). Этот guard добавляет 1 Train/59 Predict вне A/B; вместе с основным capture 25 Train/267 Predict. Не называть его проверкой с нулевым расходом и не передвигать guard в рамках history-factor.

## FIX-SVD — направленный переход требует правой стороны разложения

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN/STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание [SVD-DIRECTED-TARGET-01](../../../../experiments-v2/RP01-MICRO/SVD01/results/attempt-0001/report.md), [metrics](../../../../experiments-v2/RP01-MICRO/SVD01/results/attempt-0001/metrics.json), [native-copy.diff](../../../../experiments-v2/RP01-MICRO/SVD01/results/attempt-0001/native-copy.diff). Тема оркестрация, направленные события receive→validate→persist; learner — counts→PPMI→power iteration factorization. A3/5→2/5, B3/5→5/5; directed after0/3→3/3, wait controls2/2. Before случайная ничья на нулевой строке не считается выученным переходом. Native count reference после обучения 5/5.

Исторический baseline [E10SvdPmiEmbedding.cs](../../../../34.Experiments/Experiments/E10SvdPmiEmbedding.cs) вычислял u и v, но сохранял только L=u√σ и читал dot(L[source],L[candidate]). После R3 уже сохраняются SourceFactors=L и TargetFactors=R=v√σ. Directed consumer использует L[source]·R[candidate], а отдельно именованный ArgmaxSourceSimilarity сохраняет прежний смысл похожести. Текущий Run на строке117 явно выбирает directed score; повторная реализация этого изменения не назначена.

Математика: M≈UΣVᵀ=(U√Σ)(V√Σ)ᵀ=LRᵀ. LLᵀ симметрична, поэтому не может в общем представить несимметричную M переходов. Для любого симметричного S: ||M−S||F≥||M−Mᵀ||F/2, поскольку антисимметричная часть ортогональна симметричной. В raw after max asymmetry2.58436, Frobenius asymmetry6.32742; фактическая ошибка source-source6.327445, source-target2.0012·10⁻¹⁵. Это математическое подкрепление **направленного** reader, не доказательство качества embeddings для similarity kNN. Параметры vocab500/rank30/iterations10/seed42, PPMI, deflation и строгий tie `>` остаются.

Цена: дополнительный double[500×30] =120,000payload bytes; allocation+120,024, retained120,000→240,000. Время одной пары не причинный вывод. Это сознательный обмен памяти на корректность направления.

### Native tests и compatibility

Файл `SvdDirectedTargetTransferTests.cs`, fixture SvdDirectedTarget01 из [train.json](../../../../experiments-v2/RP01-MICRO/SVD01/train.json)/[exam.json](../../../../experiments-v2/RP01-MICRO/SVD01/exam.json);600before/2400after tokens,5queries. Не применять дополнительный 80/20 split, clamp или BPE поверх fixture. Общие native `FitFixedTraining` и `EvaluateFixedPanel` уже внесены; Run использует тот же core и сохраняет прежний split полного входа. При доработке оставшихся tests использовать эти native методы и отдельно именованные source-source/directed readers, без второй копии counts→PPMI→TruncatedSvd и повторного product patch. Исторический baseline был source-source, candidate — directed; A/B gold не берётся из текущего reader. Оба режима именуются явно и не выбираются по gold.

На пару 4factor fits (before/after каждого arm),2reference Bigram fits только A и reutilized reference receipts в B;20factor queries+10reference queries. Сохранить factors, PPMI, count tables, predictions и costs. Compare на готовых массивах: source factorsA=B,2wait correct, directed0→3/3, after5/5; Frobenius ошибки и tolerance относительная к ||M||F/машинной точности, не строковое совпадение округлённого числа. Матрицы для дополнительной произвольной задачи не обучать.

Поиск в существующих Tests не дал отдельного E10 test, проверяющего направление: это пробел покрытия, не подтверждённый красный тест. Новые assertions не заменяют старые range checks ExperimentsTests. Если return shape приватного TruncatedSvd меняется, обновить только native Run/caller и новый fixed-panel seam; не делать blanket replace dot во всём repository. Symmetric similarity сохраняется как compatibility contract на уже вычисленных SourceFactors.

## Граница E2E и порядок

Оба исправления приоритетны из-за объяснимого потерянного учебного сигнала. Но их семантическая модель событий не равна предметному selector из [старого 20](../fixes-based-on-experiments/20-prompt-domain-e2e.md). Не создавать mapping «cluster0=перевод» или «SVD component=документация». Пока нет подтверждённого продуктового consumer, эти IDs чинят existing34 и его тестовое покрытие. Они могут отдельно проверять downstream план действий **после** выбранного домена; соединять их с outer dispatch как новый алгоритм этот пакет не предлагает.
