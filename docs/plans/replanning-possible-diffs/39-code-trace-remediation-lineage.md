# CodeTraceBuilder 19 мая: baseline и ближайший кандидат contract-markers

2026-10-16. Автор проверки: `/root/rp01_code_trace_astra`. Фаза **READ_ONLY_RESEARCH**: чтение source/history, сохранённых JSON/JSONL и bytes; единственное изменение этого поручения — данный Markdown. Нейросеть не пересчитывалась, tests/builds/inference/training/proxy не запускались. Общие индекс и лог не изменялись.

**Ответ на вопрос пакета:** в этих двух архивах не найден сопоставимый, доказанно улучшенный learned результат. Baseline сохраняет положительное DPO-предпочтение на 38/50 heldout; соседний contract-markers — на 32/50. Однако это разные экзамены: изменены все chosen и 20/50 heldout rejected. Уменьшение 0,76→0,64 нельзя приписать изменению весов. Найден полезный детерминированный trace builder, раздельная DPO-диагностика и явно оформленные контракты трассы; доказательство улучшения собственного neural ответа отсутствует.

## 1. Граница архива и связь с продолжением

Исходный lead из [отчёта 28](28-code-trace-learning-donor.md) проверен по содержимому. Ниже A/B — только локальные обозначения двух сохранённых условий:

| Обозначение | Каталог в `43.LearningLocalWikiData/artifacts/runner-metrics/` | DPO run suffix | Published report suffix |
|---|---|---|---|
| A | `2026-05-19-code-trace-builder-baseline-before-remediation` | `20260519092415` | `20260519102112` |
| B | `2026-05-19-code-trace-builder-batch-002-contract-markers` | `20260519104205` | `20260519104723` |

В каждом каталоге 57 файлов: один neural `policy_final.bin`, 50 JSON step summaries, общий DPO summary, raw pairs, pair diagnostics, published manifest, published JSON `policy.bin` и `tokenizer.json`. **Initial policy, reference checkpoint, run command и свободные сгенерированные ответы среди этих файлов отсутствуют.** Название baseline не восполняет эти пробелы.

B — ближайший по имени/времени кандидат следующего remediation-условия. Сходство подтверждено одинаковыми 100 prompt/id и изменением contract-текста. Но **явной ссылки A→B как checkpoint continuation не найдено**: оба manifest имеют `parentUnitId`, `parentPolicyCheckpointPath`, `parentTokenizerPath = null` ([A manifest, 43–45][a-manifest], [B manifest, 43–45][b-manifest]). Позднейший registry также хранит их раздельно, `experimental_no_authority`, `checkpoint_allowed_as_continuation=false` ([B, 8936–8971][registry-b], [A, 15593–15628][registry-a]). Это метаданные нынешней классификации, не доказательство исторического запрета или фактического старта с нуля.

Поиск точных имён A/B в tracked Markdown и в `43.LearningLocalWikiData` вне runner-metrics нашёл registry и ранее записанный lead 28; прямую историческую запись «загрузить A и получить B» не обнаружил. Это предел указанного поиска, а не утверждение об отсутствии записей во всех личных журналах/внешних каталогах. Другие code-trace run не открывались. B анализируется как кандидат связанного изменения данных, **не как аттестованное дообучение A**.

HEAD при чтении: `d5c2409b24292d361593b8262b0c54997b15f539`. `git diff HEAD --stat` для обоих архивов и registry пуст. Оба архива внесены commit **dd22eb10bb4148e3124ffcbb97cf11a64dd3d5bc**, 2026-05-24. Адресные historical source-окна прочитаны на **89dc7a6adc2d6faebc8fd4476bb9409a61aea624**; source CodeTraceBuilder впервые виден в истории на **195840c0c4f01e685aeb5606d252c5df48fdc340**, 2026-05-20. Эти source/import SHA позже timestamps run 19 мая и **не являются runtime-binary attestation**.

## 2. Corpus: та же программа, изменённые предпочтения и экзамен

По всем 100 строкам двух `dpo-pairs.jsonl` сопоставлены `id`, `prompt`, `chosen`, `rejected`. Split восстановлен по сохранённым pair diagnostics, а не угадан по имени каталога ([A pairs][a-pairs], [B pairs][b-pairs], [A diagnostics][a-diag], [B diagnostics][b-diag]).

| Сравнение A→B | Train, 50 строк | Heldout, 50 строк |
|---|---:|---:|
| Одинаковые id / prompt | 50 / 50 | 50 / 50 |
| Полностью одинаковые chosen | 0 | 0 |
| Полностью одинаковые rejected | 30 | 30 |
| Chosen B содержит весь chosen A как suffix | 30 | 30 |
| Одинаковые поля chosen `methods`, `call_edges`, `value_flows` | 50 / 50 / 50 | 50 / 50 / 50 |

B добавляет `trace_verdict`, method/caller/callee/value-flow/terminal-return contracts и явную фразу о возврате результата через Persist к Save. У `invented_callee` и `incomplete_trace` меняются также прежние contract-маркеры; rejected меняется только для этих двух типов. Например, JSONL-строка 4 заменяет source-evidence маркер на развёрнутые контракты фиктивной зависимости; строка 5 меняет неверный final transit с `side_effect` на `unpropagated` и дописывает текст отсутствующего пути возврата. Это не просто дополнительная эпоха на старых парах.

Ядро source-grounded trace в chosen осталось одинаковым, что полезно для локализации вмешательства. Но DPO сравнивает весь упакованный answer, включая добавленные маркеры, поэтому одинаковые trace-поля не делают heldout неизменным. В этих 50 heldout нет ни одной полностью совпадающей тройки prompt/chosen/rejected для строгого cross-run сравнения.

Train содержит десять доменных имён Campaign/Order/Client/DietPlan/MediaAsset/Task/Game/Article/Booking/Survey; heldout — Invoice/Profile/Message/Shipment/Ticket/Payment/Subscription/Report/Alert/Session. При простой текстовой нормализации доменного имени из `*Controller` и числовых suffix все train prompts сводятся к одному шаблону, и 50/50 heldout совпадают с ним. Это read-only сравнение строк, не новый эксперимент. Успех на таком heldout может показывать перенос на другие имена в том же графе; он не устанавливает перенос на новые формы вызовов или иной кодовый проект.

Оба manifest содержат одинаковый `sourceDatasetSha256=1BC58B776651A6DD941377825DB78A442287E03B6898971D6DDF2BEAB61DA9F8`. Source объясняет, что hash вычислен по `trainingResult.Cases`, а не по raw preference corpus ([publisher, 125–129][publisher-hash]); case arrays A/B действительно совпадают. Поэтому этот hash не опровергает обнаруженную смену chosen/rejected.

| Raw artifact | SHA256 |
|---|---|
| A `dpo-pairs.jsonl`, 219 493 bytes | `2c580e3060668c2e4a499bcf6d27fb273d85ed4eb0335df9e591c9ee389c42ad` |
| B `dpo-pairs.jsonl`, 290 293 bytes | `d4652b0a43ab50d2ecc2cb034014f93186140b93a2f42d0ce576199d58a94c46` |

## 3. Сохранённые neural metrics: полезное различение, но улучшение не показано

| Поле summary | A baseline | B contract-markers |
|---|---:|---:|
| Train / heldout rows | 50 / 50 | 50 / 50 |
| Число batch summaries | 50 | 50 |
| Размер каждого batch / сумма предъявлений | 5 / 250 | 5 / 250 |
| `finalLoss` | 0,686132019 | 0,692440752 |
| `finalAccuracy` | 0,680000016 | 0,460000011 |
| `finalRewardMargin` | 0,014275080 | 0,001597624 |
| `heldOutLoss` | 0,685803592 | 0,689037383 |
| `heldOutAccuracy` | 0,759999990 | 0,639999986 |
| `heldOutRewardMargin` | 0,014893983 | 0,008432736 |

Источники: summary A/B, строки 8–15 и массив `batchSummaries` ([A][a-summary], [B][b-summary]). Сумма предъявлений пересчитана только из сохранённых чисел. `final*` в соответствующем historical runner — среднее по всем train step summaries, а не отдельный последний post-training train exam ([185–188][metric-averages]). Например, у B конечные pair diagnostics дают 22/50 train positives, тогда как среднее `finalAccuracy` соответствует 23/50. Это разные измерения, не противоречие данных.

По пяти последовательным окнам по десять batch summaries training accuracy остаётся A=0,68 и B=0,46. Средний margin снижается: A `0,015182858 → 0,013372913`, B `0,001694108 → 0,001513660`; loss растёт. Это не pre/post оценка одного checkpoint на frozen exam: каждое окно содержит обучающие шаги, измеренные до соответствующего update. Но сохранённый журнал сам по себе также не показывает растущий training score.

| Heldout rejected type, по 10 пар | A positives | B positives | A mean margin | B mean margin |
|---|---:|---:|---:|---:|
| missing_callee | 9 | 9 | 0,028616896 | 0,027199269 |
| missing_value_flow | 9 | 7 | 0,023265295 | 0,019542780 |
| wrong_caller | 9 | 6 | 0,023128539 | 0,011106882 |
| invented_callee | 7 | 6 | 0,010434179 | 0,001142865 |
| incomplete_trace | 4 | 4 | −0,010974989 | −0,016828118 |

Эта детализация сохраняет положительный результат A: preference metric различает выбранные пары в трёх типах 9/10 и в целом 38/50. Она также показывает конкретный слабый terminal-return тип, который общий 0,76 скрывал. У B ни один тип не имеет больше positives в своём новом heldout, но это не причинная оценка вреда contract-маркеров: изменились сами targets, reference/initial state не известны.

Historical DPO определяет успех как `margin > 0`, где margin = `0,1 × ((policyChosen − referenceChosen) − (policyRejected − referenceRejected))` ([DpoTrainer, 138–165][dpo-metric]). Это относительное предпочтение против reference; оно не равно доле самостоятельно сгенерированных правильных трасс и даже не требует абсолютного `policyChosen > policyRejected`. Initial/reference bytes в A/B отсутствуют, поэтому нельзя считать исходный margin нулём или выводить выигрыш обучения из одного положительного конечного margin.

## 4. Checkpoint/parameter lineage: head отличается, прямое наследование не установлено

Оба neural `checkpoints/policy_final.bin` имеют 1 188 744 bytes. Формат полностью разобран до EOF как 12 тензоров: число параметров, rank, dimensions и float32, согласованно с historical `SaveWeights` ([LlmModel, 70–87][model-save]). Это чтение сохранённых bytes без загрузки модели для вычисления.

| Тензоры | Shape / количество float | A против B |
|---|---|---|
| 0 | `[2048,64]`, 131 072 | Побайтно одинаков |
| 1–10 | `[64]`, `[64,64]`, `[16,64]`, `[16,64]`, `[64,64]`, `[64]`, `[128,64]`, `[128,64]`, `[64,128]`, `[64]`; суммарно 35 008 | Все побайтно одинаковы |
| 11 | `[2048,64]`, 131 072 | Различаются все 131 072 float; max abs delta `0,0497647225856781`, mean abs delta `0,013181218740470166` |

Итого первые 166 080 float совпадают. Последний тензор соответствует `lm_head` в historical module registration order; head расположен после final norm ([LlmModel, 23–48][model-order]). Это узкое позитивное свидетельство наличия двух разных численных policy states с одинаковым backbone и разной головой. Оно **не доказывает A→B training**, отсутствие различий в не сохранённом optimizer/reference или прирост навыка. Такую же картину могут дать два независимых запуска с одинаковой исходной частью модели.

| Neural artifact | SHA256 |
|---|---|
| A `policy_final.bin` | `665d097123c992f746a170b4bcc9f92e2c7cd6c656413d3b6c7f8ff77b47eb0b` |
| B `policy_final.bin` | `692fc29e8983778415ff6733aeff1ce269f9bcd3d1588512ba32f3b69be6e9c5` |

Historical runner задаёт neural config vocabulary=2048, hidden=64, один transformer block, heads=4/KV=1, FFN=128 ([85–94][metric-config]); это согласуется с binary shapes. Manifest при этом заявляет vocabulary=4096, hidden=96, layers=2, q/v-LoRA rank2, но указывает на **другой JSON policy artifact**, не neural binary. Эти manifest-параметры нельзя переносить на фактический получатель DPO-градиента. В историческом metric caller используется `DpoTrainer(policy, reference, …)`; наличие q/v-LoRA в JSON manifest не является доказательством LoRA training этих binaries.

В snapshot 89dc runner уже умеет загружать initial/reference, копировать fresh policy в reference и сохранять `reference_base.bin` ([96–129][metric-init], [171–183][metric-checkpoint]). В A/B такого файла и соответствующих initialization/learning-rate полей summary нет. Поэтому defaults snapshot — пять epochs, learning rate `5e-4` — не аттестуют точную конфигурацию run 19 мая; сохранённые 50×5 rows подтверждают только число предъявлений и steps. Snapshot содержит и более позднюю реализацию генератора, не совпадающую дословно с A/B contract text.

Ранее разобранный в 28 исторический RmsNorm создаёт новые tensors без связи backward до головы ([37–60][rmsnorm]). Это source-supported возможное объяснение одинакового backbone, а не новое runtime доказательство ошибки и не основание заново объявлять весь run бесполезным. Здесь не воспроизводится autograd-разбор из 25/28.

## 5. Какой результат реально потребляется

В historical `CodeTraceBuilderDpoGenerator` expected methods/edges/value-flows создаёт `_builder.Analyze(seed.SourceText)` ([22–37][generator-gold]). `CodeTraceBuilderTrainingRunner` вызывает тот же extractor на heldout и считает recall по этим expected множествам ([44–67][builder-eval]); обучение параметров extractor в этой функции отсутствует. `Analyze` извлекает методы/переходы и задаёт confidence правилом: 0,94 при наличии методов и call edges ([25–38][builder-analyze]).

В A и B published `policy.bin` — читаемый JSON `kind=code_trace_builder_policy_state`, с training report и 50 одинаковыми case rows: method/call-edge/value-flow recall=1, confidence≈0,94, `CODE_TRACE_BUILDER_GATE_PASSED` ([A policy, 1–22][a-policy], [B policy, 1–22][b-policy]). Это реальный положительный результат структурного извлечения на используемом шаблоне. Но gold и consumer разделяют extractor; он не является независимым экзаменом neural генерации.

Historical pipeline сначала запускает builder gate, затем DPO metrics и после успешного возврата metric runner публикует **builder training result**. В этом snapshot между DPO и публикацией нет численного DPO quality threshold ([46–84][pipeline]). Поэтому `Trusted`/публикация B при heldout 0,64 не доказывает neural quality pass. Текущий pipeline позднее добавил DPO thresholds; применять их задним числом к запуску 19 мая нельзя.

Publisher сериализует builder report в `policy.bin`, пишет в `tokenizer.json` только четыре имена полей и направляет manifest на эти файлы ([54–65][publisher-state]). В проверенных A/B нет доказанного decode consumer, который загрузил neural `policy_final.bin` и выдал собственную трассу. `ProjectWikiTrainingOrchestrator.FromCodeTrace` возвращает builder verdict/минимум трёх recalls и отдельно DPO fields ([172–202][orchestrator-result]). Это consumer результатов обучения/отчётности, не neural answer generator.

Отдельная граница tokenizer: historical DPO runner разбивает текст на lowercase слова и хеширует их в `1…2047`, применяя answer mask и ограничение контекста ([314–387][metric-tokenizer]). Такой словарь не обратим в исходный код/трассу; четырёхполевой published tokenizer также не является BPE decoder. Поля diagnostics `ChosenTokenCount`/`RejectedTokenCount` в этом source — **число разных слов** через HashSet, не число реально поданных sequence positions ([248–263][diagnostic-tokens]). Из роста такого count после contracts нельзя вычислять реальную длину masked обучения без точной исторической tokenization/packing версии.

## 6. Полезное обучение для RP01 и непроверенный малый опыт

Сохраняются три полезных механизма, каждый со своей границей доказательства:

1. **Структурные факты из source.** Builder выдаёт конкретные methods/caller-callee/return-use факты; их можно использовать как проверяемое evidence для документации. Сохранённый успех относится к шаблонному extractor path и не требует приписывать его нейросети.
2. **Разбиение ошибок по семантическому типу.** Диагностика выявляет устойчивую слабость `incomplete_trace`, несмотря на общий score A выше 0,75. Это полезнее одного aggregate для выбора следующего исследования; такой вывод получен из сохранённых данных.
3. **Явный контракт возврата.** B делает terminal-return и invented-dependency требования видимыми в учебном тексте, не меняя правильные trace-поля. Это полезное представление цели и проверяемая гипотеза для обучения. Данный архив не подтверждает, что такая запись улучшила веса или собственные ответы.

Разница с текущим RP01: здесь обнаружен иной parameter recipient, hash tokenizer и отдельный deterministic consumer. Нельзя объявлять эти 38/50 готовым донором связной free generation в B0 random/q-v LoRA/frozen lm_head, либо переносить manifest config как описание обучавшейся сети.

**Будущий опыт — только текст, НЕ ИСПОЛНЕН.** Для отдельного разрешения зафиксировать один initial policy, один reference, tokenizer/packing/version и независимый маленький exam с новым call-graph shape. Два разрешённых training набора должны различаться ровно contract-текстом; source и правильные trace-поля одинаковы, rejected-ошибки одинаковы. Оба состояния проверить на одном unchanged preference exam и на обычном own-answer consumer. Для hash-only runner сначала честно ограничить вывод preference-метрикой: отсутствие reversible decode нельзя закрывать подстановкой deterministic builder answer. Записать pre/post parameter recipients, margin по типам, exact evidence/неподтверждённые символы в собственном ответе и отдельно builder-only контроль. Никаких новых DPO pairs, кода или запусков этот документ не создаёт и не разрешает.

## 7. Открытые источники и actual coverage

**Дополнение родителя после возврата пакета.** Указанный ниже PHASE-RESOURCE-REPORT.json теперь прочитан как сохранённый документ: [источник](../../../43.LearningLocalWikiData/artifacts/phase-metrics/2026-05-19-dpo-runner-correction/PHASE-RESOURCE-REPORT.json):13–20. Он сам перечисляет отсутствие baseline_id/comparison_run_id и сообщает о reference-copy исправлении, недействительности старых random-reference метрик и статусе DATA_NEEDED для NegativeDpoGenerator/CodeTraceBuilder. Поле stale_metrics_invalidated прямо называет negative-dpo batch009 и более ранние DPO metric runs; A/B этого отчёта поимённо в нём не связаны с initialization. Поэтому это важное историческое предупреждение, но не восстановленная binary/init-lineage A/B. Самоподпись false_positive_risk=none не является независимой проверкой.

В документе девять run summaries, из них четыре code-trace remediation с указанными heldout0,42/0,52/0,52/0,52. Их сырые корпуса/checkpoints этим дополнением не проверялись, поэтому числа не объявляются подтверждёнными независимыми запусками или сопоставимой кривой. Документ внесён тем же import commit dd22eb10bb от24 мая. Его next_action прочитан как исторический текст, а не разрешение сегодняшнего исполнения. Конкретная command/build/init-связь A→B остаётся открытой; названный файл больше не считается непрочитанным lead.

Следующий первичный пробел — **исторический command/run record 19 мая**, который мог бы подтвердить загрузку A перед B и точную compiled version. Показанные manifest/registry такой связи не дают. Если такой документ появится, следующими точными артефактами для сопоставления будут initial policy/reference, на которые он ссылается, а не произвольно выбранный более поздний успешный run. В пределах текущего пакета два archive roots, их сырые пары, metrics и binaries уже проверены; следующая серия не объявляется исследованной.

При возврате пакета worker оставил непрочитанным `43.LearningLocalWikiData/artifacts/phase-metrics/2026-05-19-dpo-runner-correction/PHASE-RESOURCE-REPORT.json`. Родитель затем прочитал его; результат указан в начале этого раздела. Непрочитанным lead этот файл больше не является, но точная связь runner/init/reference не восстановлена. Новый gradient-chain аудит в этот пакет не входит.

Полностью прочитаны два raw pair файла по 100 строк, два diagnostics по 100 строк, оба summary и их 50 batch rows, два neural binaries, два published JSON policy, manifest/tokenizer. Проверены адресные registry entries. 50 отдельных `dpo_step_*.json` каждого run перечислены; их содержимое поштучно не перечитывалось. Historical/current source читался адресными окнами, а не полными проектами. Источники других run, E09 и RuleTags здесь не повторялись.

Применимые AGENTS/CLAUDE и семь исследовательских skills использованы в границах ранее выполненной целевой загрузки; полного чтения огромных файлов и новой полной reload не заявляется. Byte/JSON анализ — read-only обработка уже сохранённых артефактов. Это один bounded исследовательский пакет, не новые training cycles, не независимые N passes и не завершение общего исследования.

[a-summary]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-baseline-before-remediation/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260519092415/project-wiki-dpo-metric-summary.json:8> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L8-L22; локальный файл может отличаться"
[b-summary]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-batch-002-contract-markers/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260519104205/project-wiki-dpo-metric-summary.json:8> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L8-L22; локальный файл может отличаться"
[a-pairs]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-baseline-before-remediation/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260519092415/dpo-pairs.jsonl:1> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L1-L100; локальный файл может отличаться"
[b-pairs]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-batch-002-contract-markers/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260519104205/dpo-pairs.jsonl:1> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L1-L100; локальный файл может отличаться"
[a-diag]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-baseline-before-remediation/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260519092415/dpo-pair-diagnostics.jsonl:1> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L1-L100; локальный файл может отличаться"
[b-diag]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-batch-002-contract-markers/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260519104205/dpo-pair-diagnostics.jsonl:1> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L1-L100; локальный файл может отличаться"
[a-manifest]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-baseline-before-remediation/code-trace-builder-unit/code-trace-builder-20260519102112/generated_capability_manifest.json:25> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L25-L53; локальный файл может отличаться"
[b-manifest]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-batch-002-contract-markers/code-trace-builder-unit/code-trace-builder-20260519104723/generated_capability_manifest.json:25> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L25-L53; локальный файл может отличаться"
[a-policy]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-baseline-before-remediation/code-trace-builder-unit/code-trace-builder-20260519102112/policy.bin:1> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L1-L22; локальный файл может отличаться"
[b-policy]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-batch-002-contract-markers/code-trace-builder-unit/code-trace-builder-20260519104723/policy.bin:1> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L1-L22; локальный файл может отличаться"
[registry-a]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json:15593> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L15593-L15628; локальный файл может отличаться"
[registry-b]: <C:/Projects/llm_mvp_core/43.LearningLocalWikiData/registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json:8936> "Исторический ref d5c2409b24292d361593b8262b0c54997b15f539, L8936-L8971; локальный файл может отличаться"
[publisher-hash]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/CodeTraceBuilderManifestPublisher.cs:125> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L125-L129; локальный файл может отличаться"
[metric-averages]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs:185> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L185-L188; локальный файл может отличаться"
[dpo-metric]: <C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:138> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L138-L165; локальный файл может отличаться"
[model-save]: <C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:70> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L70-L87; локальный файл может отличаться"
[model-order]: <C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:23> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L23-L48; локальный файл может отличаться"
[metric-config]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs:85> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L85-L94; локальный файл может отличаться"
[metric-init]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs:96> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L96-L129; локальный файл может отличаться"
[metric-checkpoint]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs:171> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L171-L183; локальный файл может отличаться"
[rmsnorm]: <C:/Projects/llm_mvp_core/03.TransformerModel/Layers/RmsNorm.cs:37> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L37-L60; локальный файл может отличаться"
[generator-gold]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/CodeTraceBuilderDpoGenerator.cs:22> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L22-L37; локальный файл может отличаться"
[builder-eval]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/CodeTraceBuilderTrainingRunner.cs:44> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L44-L67; локальный файл может отличаться"
[builder-analyze]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/CodeTraceBuilderUnit.cs:25> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L25-L38; локальный файл может отличаться"
[pipeline]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs:46> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L46-L84; локальный файл может отличаться"
[publisher-state]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/CodeTraceBuilderManifestPublisher.cs:54> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L54-L65; локальный файл может отличаться"
[orchestrator-result]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs:172> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L172-L202; локальный файл может отличаться"
[metric-tokenizer]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs:314> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L314-L387; локальный файл может отличаться"
[diagnostic-tokens]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs:248> "Исторический ref 89dc7a6adc2d6faebc8fd4476bb9409a61aea624, L248-L263; локальный файл может отличаться"
