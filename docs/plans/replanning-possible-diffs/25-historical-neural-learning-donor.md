# Исторический neural donor: ProjectWiki DPO и границы найденных свидетельств

Дата: 2026-10-16. Фаза READ_ONLY_RESEARCH. Исходники читаются на `259c61769c3c66c9353f70d3dd569da19682067b` и в адресно выбранных старых commits. Здесь не запускались обучение, inference, тесты, сборки или probes; меняется только этот Markdown. v309, E13/E18 и translation memory повторно не исследуются.

## НД-01. Найдено реальное изменение нейронного состояния ProjectWiki, но генеративный успех этой цепочкой не установлен

Первичный [отчёт продвижения query-planning, 19 мая](../../../43.LearningLocalWikiData/artifacts/phase-metrics/2026-05-19-project-wiki-query-planning-promotion-025/PHASE-RESOURCE-REPORT.md) сообщает: training 50, heldout 50, final accuracy 0,9600000024, heldout accuracy 0,9800000191, heldout reward margin 0,0023556729. Сохранена ссылка на `batch-025-run-output.json`, pair diagnostics и generated manifest. Отчёт также перечисляет предыдущие batch-019/021/022/023/024, но это ещё не сравнение одного неизменного контрольного набора: в описании меняется shape rejected.

Итог адресной проверки: **есть реальные различающиеся neural checkpoints и сохранённые положительные preference-метрики**. Это отдельное свидетельство, которое нельзя обесценивать выводом о v309. Но 49/50 — reference-relative preference, а опубликованный предметный план строил другой, детерминированный consumer. Полная связка «изменились нейронные параметры → улучшился собственный предметный ответ» здесь пока не найдена.

Исходный адрес из отчёта: `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator extension/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/`. Репозиторная копия находится в `43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/`. Ниже разобрана именно эта цепочка, без полного каталога остальных экспериментов.

### Источники и привязка к истории

Для краткости далее **R** — репозиторная папка серии, указанная выше; **D** — `R/dpo-runs/project-wiki-query-planning-flow/`. Это адреса, а не новые созданные каталоги.

Raw `R/batch-025-run-output.json` добавлен commit `dd22eb10bb4148e3124ffcbb97cf11a64dd3d5bc` 24 мая 2026, вместе с переносом LearningLocalWikiData. Снимок кода этого commit прочитан через `git show`, без checkout. `195840c0c4f01e685aeb5606d252c5df48fdc340` от 20 мая ввёл ProjectWiki orchestration units и уже содержит `logits.Backward`; это отдельная линия истории, не только июльский ремонт DPO. `git branch --all --contains 195840c…` показывает несколько локальных веток, включая DesignToCode, DpoImplementation и claude/sonnet-local-wiki. Не производились fetch и полный обход этих веток.

Дата старых результатов — 19 мая; импорт и прочитанный snapshot — 24 мая. В JSON нет SHA исполнявшейся сборки. Поэтому кодовая связь подтверждена сохранённым снимком рядом с артефактами и согласуется с их форматом, но не является доказанной аттестацией того бинарника. Исходный внешний `batch-025-run-output.json` по пути из отчёта при проверке не найден; читается репозиторная копия, а не объявляются две независимые копии доказательством.

Основные raw receipts:

- [batch-019-run-output.json](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/batch-019-run-output.json) — heldout 44/50, reference-relative margin 0,00185214996.
- [batch-022-run-output.json](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/batch-022-run-output.json), [batch-023-run-output.json](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/batch-023-run-output.json), [batch-025-run-output.json](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/batch-025-run-output.json) — каждый 49/50; содержат training/heldout pairs и отдельно `trainingRun`/`dpoMetrics`.
- [DPO summary 025](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259/project-wiki-dpo-metric-summary.json:8): строки 8–14 — epochs=1, learningRate≈5e−5, начальная policy из 023, исходный reference из 023, initial policy/reference maxAbsDelta=0,05606907606; 15–22 — размеры и метрики.
- [pair diagnostics 025](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259/dpo-pair-diagnostics.jsonl) — per-pair Split/PairId/Accuracy/RewardMargin, teacher-forced token counts, а не свободный текст ответа.

### Что обучалось и чем это отличается от RP01

Исторический `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs@dd22eb10:30–43,94–138` задаёт по умолчанию hidden=64, layers=1, FFN=128, vocab=2048, context=192, heads=4/KV=1. Новая policy/reference создаются через `new LlmModel`; при продолжении загружаются сохранённые файлы. Без начальной policy reference копируется из policy (`:105–134`). В прочитанном `R/batch-002-run-output.json` непосредственно записано `fresh_random_policy`, `copied_from_fresh_policy`, initial delta=0. В 025 source — checkpoint 023. Доказательства отдельного языкового pretraining этой серии не найдено; наличие более позднего состояния не следует называть pretrained language base без его родословной.

Actual training chain: `ProjectWikiDpoMetricRunner:144–176` → `DpoTrainer.StepMany` → `BackwardLogProbs` → `logits.Backward(logits.Grad)` (`11.DPO/DpoTrainer.cs@dd22eb10:308`) → AdamW по `_policy.Parameters()` (`:109`). Отдельного LoRA allowlist здесь нет. Вход — hashed word IDs, не BPE: `ProjectWikiDpoMetricRunner:309–347` формирует общий `prompt + Answer`, prompt budget 48, response mask и hash в 1…2047; `TakeAnswerTokens:350–362` при превышении бюджета оставляет начало и конец ответа. Это представление пригодно для оценки заданных последовательностей, но сохранённого обратимого словаря/decoder для свободного языка в этом runner не видно.

**Фактически менялась более узкая часть модели, чем перечисляет optimizer.** На 023→025 ниже меняется только output head. Исторический `03.TransformerModel/LlmModel.cs@dd22eb10:27–38,42–48` регистрирует token embedding, block, final norm, затем `lm_head` и вызывает их в этом порядке. `00.TensorCore/Layers/Module.cs:22–29` рекурсивно перечисляет параметры; `Linear.cs:19–27` задаёт weight `[out,in]`. Последний тензор `[2048,64]` соответствует `lm_head.weight`.

Первичный source объясняет ограничение: `RmsNorm.cs@dd22eb10:37–60` в `MultiplyBroadcast` и `MultiplyGamma` возвращает `new Tensor(data, x.Shape)` без autograd-связи; `Tensor.cs:26–41` по умолчанию `requiresGrad=false`. Финальный norm перед head обрывает градиент к предыдущим слоям. Это согласуется с binary delta. В `8a8cad4a23026b7f007f99f5a979ce4bfe21d547` от 28 июля helpers заменены на дифференцируемый `TensorOps.MultiplyBroadcast`. Объявлять старый обрыв желательной архитектурой или откатывать этот ремонт оснований нет.

Нынешний RP01 обучает q/v-LoRA и сохраняет output head frozen: [DpoTrainer.cs:190–254](../../../11.DPO/DpoTrainer.cs:190), [CodeDocumentationSimulationDpoExperiment.cs:130–160](../../../14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:130), сводка измеренного состояния в [12, С-01](12-student-update-to-generation-path.md). **Это реальное отличие места адаптации: прежний head против нынешних q/v.** Причинный вклад в качество не измерен; различаются также размер модели, данные, tokenizer, objective и consumer.

### Сохранённое изменение весов: прочитаны bytes, а не только summary

Все четыре указанных `policy_final.bin` имеют длину 1 188 744 bytes. Reference каждого из 019/022/023/025 имеет SHA-256 `7C3BFDACB17667843C7B9BC65FEB380489106059A3E5EB1D7A89FE4B55B5D77C`. Это лишь identity данного reference, не повторное исследование другой серии.

| Batch / run-папка внутри D | SHA-256 policy_final.bin |
|---|---|
| `project-wiki-query-planning-status-contract-lowlr-019-project-wiki-query-planning-flow-20260519172921` | `9341C477B6BD680E57ADE1F8816EC1075CA86416550DC937E20A7B7080AFE8A3` |
| `project-wiki-query-planning-validation-surface-lowlr-022-project-wiki-query-planning-flow-20260519174823` | `EF230B53D3508EAF247001E851E6F1325E2E4C52D6C1C998AD53B7D99F91DE16` |
| `project-wiki-query-planning-mixed-confirmation-023-project-wiki-query-planning-flow-20260519174947` | `B7E979EB556FE3F3DEC5D772DF6A8E9E75A23C3B7AEDAC8E8B6A5ED4BF8DB56D` |
| `project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259` | `779A6813875980736FCD0CA8185E18453F58229E526C656C1C3F9BBFFB6D167F` |

Путь каждого binary — `D/<run-папка>/checkpoints/policy_final.bin`. Анализ 023→025 прочитал сохранённый формат из `LlmModel.cs@dd22eb10:70–107`: int parameter-count, rank/dimensions, float values. В обоих файлах 12 тензоров и 297 152 чисел; читатель дошёл точно до 1 188 744/1 188 744 bytes. Индексы 0…10 равны по всем float values. В индексе 11 `[2048,64]` различаются 131 072/131 072 значений; max absolute delta = **0,0005016028881073**. Это чтение артефактов в памяти, не загрузка модели, inference или новый probe. Отдельный скрипт/выходной JSON не создавался.

Для 025 дополнительно сопоставлен HEAD LFS pointer: `oid sha256:779a6813…167f`, `size 1188744`; он совпадает с прочитанным локальным binary. Pointer не использовался вместо содержимого весов. Различие чисел доказывает изменение состояния; оно не доказывает, что все изменившиеся элементы получили полезный semantic gradient — AdamW включает и weight decay.

### Что значит 49/50 и что показывает одинаковая контрольная панель

`DpoTrainer.EvaluateMany@dd22eb10:140–165` считает:

`margin = Beta × [(policyChosen − referenceChosen) − (policyRejected − referenceRejected)]`,

`accuracy = fraction(margin > 0)`.

`LogProbs:227–260` усредняет masked response log-probabilities по числу ответных токенов. Это не точность написания плана, не raw `policyChosen > policyRejected` и не нынешняя summed DPO цель RP01. Historical 49/50 сохраняется как положительный результат **этой** метрики. Переводить его в 98% собственных правильных ответов нельзя.

Прочитанные raw JSON/JSONL позволяют выполнить более узкую сверку **без исполнения модели**. Для каждого heldout примера сопоставлены точные `(a, chosen, rejected)` с учётом регистра и переводов строк; PairId используется для соединения с saved diagnostics своего run. Никакая токенизация заново не выполнялась.

| Сохранённый переход | Точно одинаковые raw triples | Сумма saved Accuracy до → после на этих triples | Средний saved margin до → после |
|---|---:|---:|---:|
| 019→022 | 45/50 | 44 → 44 | 0,002165071765 → 0,002152091210 |
| 022→023 | 50/50 | 49 → 49 | 0,002359769905 → 0,002350332304 |
| 023→025 | 50/50 | 49 → 49 | 0,002350332304 → 0,002355672860 |

В 019→022 изменены ровно пять triples типа `no_tests_or_validation_surface`. Именно поэтому общий рост 44/50→49/50 нельзя приписать лучшему ответу модели на неизменный экзамен. На 023→025 есть реальный neural-state delta и небольшой положительный сдвиг среднего preference margin (~0,00000534056), при прежних 49/50. Это узкое положительное свидетельство изменения вероятностного поведения на сохранённой панели. Новых правильных свободных ответов оно не устанавливает; причинность полезного обучения против decay/численного эффекта здесь отдельно не выделена.

Контраргумент к отрицательному общему выводу: неизменная accuracy на почти насыщенной панели не означает, что обучение бесполезно. Могли укрепляться уже верные предпочтения. Но после просмотра этой серии нельзя придумывать более сильный measured effect или выдавать changed rejected за одинаковый вопрос.

## НД-02. Полезный предметный planner и neural DPO здесь расходятся по получателю

**Статус:** подтверждённое разделение двух путей. Это объясняет границы найденного positive receipt; не отменяет полезность правил или отдельных нейронных изменений.

В `ProjectWikiQueryPlanningTrainingPipeline.cs@dd22eb10:51–61` сначала запускается domain runner, затем отдельно DPO metric runner. Условия публикации проверяют оба (`:67–79`), но в publisher передаётся `run.Value`, а не DPO policy (`:82`).

`ProjectWikiQueryPlanningTrainingRunner.cs@dd22eb10:43–68` для heldout вызывает `_planner.PlanAsync(question)`. В `ProjectWikiQueryPlanningFlow.cs:130–169` intent определяется через `ContainsAny` и фиксированные ветви; `:30–56` собирает steps/checks/answer shape. Прочитанный runner не вызывает обновлённый `LlmModel` и не учит planner на `trainingRows` до evaluation. Saved `trainingRun.queryPlanAccuracy` у 019/022/023/025 равен 1, отдельно от `dpoMetrics.heldOutAccuracy`.

В `ProjectWikiQueryPlanningManifestPublisher.cs@dd22eb10:51–65` файл с именем `policy.bin` создаётся через `File.WriteAllTextAsync` и содержит JSON `kind=query_planning_policy_state`, `training_result=...`; tokenizer — JSON с перечнем fields. Это **другой файл**, чем настоящий binary `D/.../checkpoints/policy_final.bin`. Прочитан и опубликованный артефакт `R/project-wiki-query-planning-flow/project-wiki-query-planning-20260519175400/policy.bin`: 19 636 bytes, начинается с указанного JSON. Его нельзя считать экспортом обученных transformer weights.

Current diff от `dd22eb10` до `259c617…` для [training runner](../../../42.ProjectWikiDomain/ProjectWikiQueryPlanningTrainingRunner.cs) и [manifest publisher](../../../42.ProjectWikiDomain/ProjectWikiQueryPlanningManifestPublisher.cs) показывает только пять строк license header. Поэтому этот конкретный разрыв получателей сохранился в текущих версиях этих двух файлов. Более широкий нынешний serving/host всех wiki units этим ограниченным чтением не исследован.

**Человеческий смысл:** система могла правильно строить планы благодаря предметным правилам и одновременно реально менять neural preference weights. Из общего отчёта нельзя заключать, что первый успех создан вторым процессом. Для RP01 это полезное архитектурное отличие: предметный deterministic consumer существовал рядом с neural learner, а free-generation экзамен RP01 требует ответа именно Student.

**Самая дешёвая следующая исследовательская проверка, без запуска:** найти сохранённый receipt, где exact `policy_final.bin` 023/025 загружен в answer consumer и на одинаковом input записан его собственный output до/после, вместе с decoder/tokenizer. В прочитанных источниках серии такой receipt не найден. Подходящие первичные места — соседние файлы этого же run и `42.ProjectWikiDomain` caller, который принимает `ProjectWikiDpoMetricRunResult.PolicyCheckpointPath`, а не одноимённый JSON `policy.bin`. Пока этот link не найден, donor остаётся подтверждённым для mutable neural state/preference, но открытым для собственного ответа.

## Переносимый вопрос для RP01 — только будущий дизайн

Найденное различие, которое имеет смысл проверять отдельно: **место адаптации — output head или q/v-LoRA**. Старый receipt показывает изменение head; нынешний RP01 оставляет его frozen. На будущей отдельно разрешённой проверке следует удержать один B0, reference, tokenizer, corpus, objective, optimizer schedule и decoder; менять только разрешённый recipient. Число trainable parameters следует уравнять либо явно считать частью вмешательства: full-head против q/v-LoRA без такого контроля не отделяет расположение адаптации от её ёмкости. Сначала read-only подбор уже сохранённой пары runs с таким различием, если она существует. Если её нет, эксперимент — отдельное поручение; этот документ его не начинает.

Измерения должны разделять изменение разрешённых tensors, preference на одинаковых raw/token inputs и собственный предметный ответ. Улучшение только margin — только preference-эффект. Отрицательный короткий результат на случайной B0 не опровергнет полезность head adaptation на обученной language base. Не предлагается переносить старый отсоединённый RmsNorm, hash-tokenizer или исторический gradient-код ради воспроизведения положительного числа.

## Границы поиска и следующий первичный след

Углублённо разобрана **одна сильная серия**, а не каталог всего проекта: ProjectWiki query-planning 019→022→023→025, с двумя более ранними initialization receipts 002/013. Внутри неё отдельно проверены neural state и domain-quality claim. Поиск до выбора включал имена артефактов, короткий git log по обучению и адресные исходники; это не полный аудит всех refs.

В RAG `09.09.26WorkLeft-RAG-01-dpo-legal-build.ndjson` найдены адреса lifecycle/measurement guides. Адресно прочитанное первичное `../dpo legal build/active-codex-execution-plan-10-training-lifecycle.md:5,22–24` содержит исторический `dpo_status: NOT_STARTED` и требования lifecycle; это план, не нужный positive receipt. Helper `11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md` использован только как указатель; его общие заявления не приняты вместо bytes и actual consumer. Новый отрицательный вывод обо всех прежних экспериментах из этих файлов не делается.

Кратко просмотренные E28V2/E31V2/E44V2/E45V2 May1 JSON не дали положительного neural donor для углубления; их механизмы и поздние successors в этом пакете не расследованы, старый FAIL не считается последним словом. Commit `f70f306b3e` с названием «train result-gap adaptive model» проверен по checkpoint: там `missingPatternProfiles`/signal IDs, а не neural weights; этот полезный memory-сигнал не подменяет искомый neural donor.

Открытый точный след для следующего поиска: `43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15-code-trace-batch-001-after-backward-fix/` и дальнейшие code-trace remediation runs. В одном summary уже прочитаны train/heldout 50/50, heldout accuracy≈0,62 и margin≈0,00194637; byte-delta, одинаковая экзаменационная панель и consumer этой **другой** серии ещё не проверены. Это адрес дальнейшей археологии, не предложение запуска и не объявленный второй успех.

Файл прочитан обратно после записи. Соседние 00/04/06/07 не изменялись. Этот bounded пакет не завершает общий поиск исторически полезного обучения.
