# 76. Исторический WikiTrace: утраченный вход DPO и два разных «policy»

Граница независимого review: worker77 прислал предварительные сообщения, но завершился ошибкой доступа сети до сохранения итогового файла. Завершённая независимая приёмка76 не заявляется; проверенное родителем authority-уточнение ниже сохранено. Остальное продолжение указано в80.

16 октября 2026. READ_ONLY_RESEARCH. Исследование продолжает след из70: есть ли реальный run generated-capability с ранней остановкой. Вместо подтверждения этого события найден другой исторический путь с настоящими сохранёнными DPO-метриками и конкретным изменением учебного входа. Новых запусков нет. Core при чтении: `793d7b755b23aac90445da3ac6661b1bff7e6672`.

**Главное новое различие:** в мае DPO для WikiTrace получал конкретную трассу и исходные ответы. В июне к ним добавили подсказки правильности, а в июле конкретные вход и ответы исчезли из преобразования — остался шаблон по типу ошибки. Это точная история изменения алгоритма подготовки пары. Она не доказывает, что именно это вызвало RP01 free-generation0/3, поскольку RP01 использует другой вход. Но она отвечает на вопрос, где при переносе могла исчезнуть содержательная часть ранее работавшего пути.

## 1. Что действительно записано в историческом запуске

Архив: [flow result](C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-25-wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006/project-wiki-training-flow-result.json), [DPO summary](C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-25-wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006/dpo-runs/wiki-trace-aggregator-unit/wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006-wiki-trace-aggregator-unit-20260525031407/project-wiki-dpo-metric-summary.json), [100 сохранённых пар](C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-25-wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006/dpo-runs/wiki-trace-aggregator-unit/wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006-wiki-trace-aggregator-unit-20260525031407/dpo-pairs.jsonl), [100 диагностик пар](C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-25-wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006/dpo-runs/wiki-trace-aggregator-unit/wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006-wiki-trace-aggregator-unit-20260525031407/dpo-pair-diagnostics.jsonl). Полностью разобраны JSON-поля и все записи; вывод большого summary был сокращён, затем массив обработан целиком в памяти.

| Величина | Сохранённый факт | Как читать |
|---|---|---|
| TRAIN / heldout |50 /50 строк | Не100 независимых семейств. |
| Обучение |8 эпох,80 minibatches по5 |400 предъявлений50 пар; в manifest число50 означает строки. |
| Run accuracy |0.9875 |Средневзвешенная accuracy во время всех80 шагов, не только последнего checkpoint. |
| Последний шаг80 |loss0.683270335; accuracy1; margin0.019869825 |Отдельный [dpo_step_000080.json](C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-25-wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006/dpo-runs/wiki-trace-aggregator-unit/wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006-wiki-trace-aggregator-unit-20260525031407/checkpoints/dpo_step_000080.json). |
| Итоговый heldout |accuracy1; loss0.675145507; margin0.036361873 |Сохранённая preference-оценка, не свободная генерация. |
| Инициализация |fresh_random_policy; reference copied; начальная max-delta0 |Не доказательство предобученного языкового навыка. |
| Общая стоимость DPO runner |216295.6448ms; CPU183750ms; peak working set120.175781MiB |Не время одного optimizer и не вся память PC. |

Расчёт80×5=400 и weighted accuracy0.9875 выполнен по сохранённым batchSummaries, а не новым вызовом модели. [Section22 report](C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-25-wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006/runner-reports/wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006-wiki-trace-aggregator-unit-section22.json) честно оставляет accuracy/loss/margin delta как UNKNOWN и completion_claim_allowed=false. Его главный accuracy1 нельзя автоматически читать как доказанный рост0→1: внешний flow summary записал baseline0, но отдельный исходный предметный экзамен этим полем не подтверждён.

**Уточнение независимости:** каждый split содержит50 уникальных троек(prompt,chosen,rejected), но40 из50 heldout-троек дословно совпадают с TRAIN при исключении только поля id. Это сравнение точных строк без нормализации и без запуска tokenizer. Остальные10 текстово различаются; независимость их смысловых семейств не установлена. Не обнуляем сохранённый успех предпочтения, но100% здесь нельзя назвать100% обобщением на50 новых задач.

## 2. Точная история исчезновения конкретной трассы

Проверены Git snapshots и diff одного файла `42.ProjectWikiDomain/WikiTraceAggregatorTrainingPipeline.cs`:

| Снимок | Что передаёт ToDpoMetricPair |
|---|---|
| `d172291008ae739ef71bc3fb099530acc9d667d3`,25мая, строки104–115 |Instruction + `pair.A`; затем исходные `pair.Chosen` и `pair.Rejected`. |
| `7a59f5d7eb6d083cbd93cb57a819e69d5f51114d`,2июня,106–129 |Трасса и обе стороны сохранены, но добавлены PREFERRED/REJECTED, CHOSEN_IS_CORRECT и тип ошибки; одновременно меняется порядок через interleave. |
| `777ead8dfc3dfc05bae34d7369e088770355784f`,9июля,107–135; нынешние [112–140](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiTraceAggregatorTrainingPipeline.cs:112) |Нет `pair.A`, `pair.Chosen`, `pair.Rejected`; тексты зависят от `RejectedType` и фиксированных маркеров. PairId сохраняется как ID, но не заменяет содержательный prompt. |

Это не предположение по имени commit: соответствующие тела функций прочитаны. В5 группах майского TRAIN по10 разных строк с одинаковым типом ошибки. Нынешняя функция не может различать внутри такой группы реальные owners, traces и варианты ответа: она их не читает. Для будущего изучения обучения это существеннее простого числа500 строк — разные исходные ситуации могут стать одной модельной задачей ещё до токенизации.

Нельзя приписывать все отличия июльской правке: маркеры правильности и перестановка batch появились ранее. Майские сохранённые стороны тоже уже имеют GOOD/BAD и [WRONG:] — тот опыт не является чистым контролем без подсказок. Исторический metric tokenizer также ограничивал prompt первыми48 lexical tokens при context192 и использовал hash-IDs; наличие полного текста в JSON не доказывает видимость всей трассы модели. Exact token survival этим чтением не измерялся.

Архив внесён commit `34268629a2d8c13f42a70004c1189b2e9fd34f01`, который является предком текущего HEAD. Исходная функция совпадает с форматом сохранённых пар; это согласование source/artifact, не аттестация реально загруженного бинарника.

## 3. Почему manifest с LoRA не описывает найденный neural checkpoint

[Published manifest](C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-25-wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006/wiki-trace-aggregator-unit/wiki-trace-aggregator-20260525031744/generated_capability_manifest.json) указывает vocab4096/context128/hidden96/layers2/Q4/KV4/LoRA rank2 и `trainingStepCount=50`. Но [его policy.bin](C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-25-wiki-trace-aggregator-z-cross-unit-sequence-signal-correction-batch-006/wiki-trace-aggregator-unit/wiki-trace-aggregator-20260525031744/policy.bin) прочитан как JSON: kind=`wiki_trace_aggregator_policy_state`, training_result с50 cases, а не tensor checkpoint.

Это ровно то, что делает [Publisher59–70](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiTraceAggregatorManifestPublisher.cs:59): сериализует результат и описание полей tokenizer. [Строка103](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiTraceAggregatorManifestPublisher.cs:103) заполняет TrainingStepCount числом учебных строк. SourceDatasetSha256 здесь хеширует result.Cases, а не исходные пары ([130–134](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiTraceAggregatorManifestPublisher.cs:130)). Эти три класса кода — Publisher, TrainingRunner, Unit — от архивного342686 до HEAD изменились только license header.

Neural `policy_final.bin` и `reference_base.bin` находятся отдельно под DPO checkpoints; pipeline не передаёт их Publisher. Исторический ProjectWikiDpoMetricRunner@342686, строки30–45/94–106, создаёт по умолчанию vocab2048/context192/hidden64/layers1/Q4/KV1; pipeline задаёт только8 эпох и lr.001. Исторический DpoTrainer@342686 использует AdamW по `policy.Parameters()` (строки54/109/209), не установленный здесь LoRA-only набор. Это такой же тип различия, который подробно разобран для code-trace в [28](28-code-trace-learning-donor.md), но binary tensors именно этого WikiTrace запуска в76 не сравнивались.

**Следствие:** переносить manifest dimensions/LoRA rank/50 steps как настройки успешной нейросети нельзя. Изменение выходного слоя против q/v-LoRA остаётся обоснованной гипотезой из28, а не доказанным результатом tensor-аудита этого дополнительного run.

## 4. Что реально выдаёт предметный результат

[TrainingRunner49–66](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiTraceAggregatorTrainingRunner.cs:49) применяет `_aggregator.Aggregate` к heldout. TrainingRows проверяются на размер/контракт; эта функция не учит aggregator по ним. [Unit29–60](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiTraceAggregatorUnit.cs:29) собирает failed units и remediation из входных traces, задаёт stop по условиям и вычисляет confidence формулой. Это полезная детерминированная функция обработки следов; её успешность не является обновлением neural state.

[Pipeline54–97](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/WikiTraceAggregatorTrainingPipeline.cs:54) сначала получает этот результат, затем отдельно обучает DPO scorer и лишь после его gate публикует `run.Value` предметного runner. Поэтому надо разделить три факта: правильность агрегатора; изменение DPO предпочтения; использование neural весов обычным получателем. Первый и второй имеют свои артефакты, третий для найденного neural checkpoint здесь не установлен.

Текущая достижимость требует отдельной оговорки, найденной при независимой сверке77 и прочитанной родителем: [ProjectWikiDpoMetricRunner262–268](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs:262) отказывает при отсутствии legal teacher admission authority. Штатный CreateDpoMetricRunner в pipeline задаёт только epochs/lr. Поэтому описанный порядок «DPO → publish» подтверждён историческим run и нынешней условной веткой при допущенном injected runner; он не доказывает успешность default-вызова сегодня. Никакой обход допуска или запуск не выполнялся.

Это не подтверждает преждевременную остановку из70: WikiTrace имеет свой8-epoch ProjectWikiDpoMetricRunner, а не generated-capability max5/window1. В найденном архиве сохранены все80 шагов. Свидетельство фактического раннего stop по monitor остаётся ненайденным в ограниченном поиске.

## 5. Один будущий различающий опыт — не запускался

Для домена обработки traces зафиксировать маленький набор с одинаковыми типами дефекта, но разными правильными owners/порядками/stop-decisions. До обучения закрыть независимые группы и убрать exact-overlap TRAIN/EXAM. Сравнить две подготовки **одних и тех же пар**: сохранённые конкретные trace/ответы и нынешние шаблоны по RejectedType. Фиксировать tokenizer, исходное состояние, optimizer, число updates, порядок строк и вычислительный бюджет. Маркеры правильности должны быть одинаково контролированы, иначе это опыт сразу о двух изменениях.

Измерять отдельно preference на невиденных контрастах и ответ настоящего neural consumer, если он существует. Детерминированный Aggregate оставить неизменным и показывать его собственный результат отдельно. Изменение обычного ответа нельзя приписать DPO, если endpoint продолжает читать только JSON report или рассчитывать Aggregate из входов. Вначале нужна письменная проверка связи checkpoint→consumer; не новый обязательный широкий CI.

Этот опыт ещё не реализован и не выполнен. Для RP01 его результат был бы ограничен этим доменом; проверка q/v-LoRA против обучаемого output head — другое вмешательство, не часть данного сравнения.

## Границы поиска

Scoped поиск текстовых receipts по convergence полям в .kiro results/artifacts/docs/plans нашёл планы/source-readback и чужой one-step route, но не нужный max5/window1 run. Затем поиск имён без ignore обнаружил Wiki архив. Первый общий вывод dpo_step paths превысил buffer; неполный список не использован как полный inventory. Повторный узкий поиск только generated_capability_manifest в43/runner-metrics успешно дал167 файлов; их метаданные разобраны, но это не167 независимо исследованных опытов. Подробно в76 рассмотрен один указанный run. Бинари не загружались в модель, не исполнялись и не менялись; продуктовых изменений нет.

