# Содержательная сверка 116 источников, не найденных по буквальному имени

> **Исторический срез883/53cb.** На окончательном827 №01–06 завершены, следующий LF-03; P54 blanket запрет снят, NG01 имеет protected карточку и новый host, R1 report уже исправлен. Актуальный статус и оставшиеся gaps — [final827 update](../08-final-core827.md), действующие outputs — [root05](../05-executor-contract.md). Старый R2 wrapper/claims не запускать; этот receipt не назначает повтор.


**Результат:** у списка `not_named` выявлены два конкретных подслучая, для которых адресная сверка не нашла точного protected-рецепта: **NG01** и **E-METRIC-PHASE**. NG01 уже подробно оформлен в planning. Отдельно сохранены альтернативный контракт N60-E2, условная positive-correction гипотеза без установленного caller и вопрос учёта настоящего update без изменения битов LoRA. Остальные результаты классификации ниже; полный список из 116 строк находится в [машиночитаемой сверке](evidence/unmentioned-source-crosswalk-116.json).

Это ограниченное дополнение к основному аудиту, без обучения, сборки, изменения production/защищённых документов или повторения прежних опытов. Проверенные срезы: planning `6bc4b6fba8f14870f5dc4966cf5a344754962d66`, core `883496bca18113bf37e0afe683c75b05a0075048`.

## 1. Глубина проверки и счётчики

Исходная выборка — 116 relative paths из `current-meta/planning-name-reference-coverage.json:not_named`. Для всех 116 прочитаны заголовки и выбранные содержательные абзацы; для конкретных факторов дополнительно сопоставлены соответствующие protected owners/подслучаи и указанные далее current code paths. Git blob SHA каждого planning-файла перепроверен по pinned manifest: 116/116 совпали. JSON содержит отдельные поля `review_scope`, `confidence`, `source_evidence`, `protected_or_current_code_evidence` и `all_embedded_claims_certified=false`.

**Уверенность относится к указанному выводу о документе или факторе.** Для обзора 125 карточек высокий уровень уверенности в том, что это обзор, не означает проверки всех его 125 научных тезисов. В этой работе не подтверждается полное семантическое покрытие всех 747 Markdown, всех 470 P-подслучаев или всех runtime hosts.

| Классификация документов | Число | Что означает |
| --- | ---: | --- |
| Навигаторы, policy, provenance, review и исполнительские промпты | 79 | Самостоятельного нового опыта для самого метадокумента не требуется. Уточнения сохраняются, а named mechanisms сопоставляются со своими owners |
| Исторические результаты, lineage и реестры кандидатов | 13 | Сохранить результаты/регрессии/доменные границы; отсутствие имени отчёта не назначает повтор обучения |
| Широкие карты нескольких механизмов | 10 | Полная построчная семантическая сверка **unresolved** в данном ограниченном проходе |
| Документы с конкретными факторами, уже имеющими current recipe | 8 | Совпадение названного фактора подтверждено; старые readiness, budgets и статусы не переносятся автоматически |
| Известная семья, точный подслучай не найден в protected | 2 | NG01 и E-METRIC-PHASE; подробно ниже |
| Описанные подслучаи с конфликтом допуска | 1 | P54.1–.7 уже есть, blanket запрет противоречит разрешению learned-state reader |
| Условная гипотеза без доказанного caller | 1 | Positive verifier correction: оставить вне активной очереди |
| Учёт результата настоящего update | 1 | No-delta и invalid readback необходимо различать; отдельный run ради статуса не нужен |
| Альтернативный дизайн той же учебной гипотезы | 1 | OrderedPhraseSupport/v1 и текущий SourceSpanFrames имеют разные представления и границы reader |
| **Всего** | **116** | Это счёт документов, не число независимых гипотез или запланированных запусков |

Десять широких карт с неполной семантической аттестацией: **U006, U012, U017, U024, U028, U037, U042, U046, U059, U089**. Их полные пути и проверенные места находятся в JSON. Для них не выставлен общий `covered` или общий `missing`.

## 2. Какие прежние заявления об отсутствии карточек уже устарели

В поздних review часто описан старый снимок, на котором специальной карточки ещё не было. Следующие конкретные факторы теперь присутствуют в core 883; отдельный review-файл не должен создавать ещё один опыт той же причины.

| Механизм из 116 источников | Актуальное точное покрытие | Граница сопоставления |
| --- | --- | --- |
| LC-REF-CACHE, U022/U082 и concurrent reviews | [N61:96–110][ref-cache] | Cache только frozen-reference scores, instance-local identity/material key; не policy cache |
| R1 frozen-base comparison; R2 softmax scratch; LF03 initial gradient copy | [N62:R1/R2:265–297][n62-r12], [LF03:327–349][n62-lf3] | Три раздельных фактора. Совпавший источник не складывает их бюджеты |
| Mode1 retention/identity из U007/U112/U114 | [N61 retention:166–201][m1-ret], [identity:242–280][m1-id] | Сохранение InputFeatureWeights и фильтр по FlowRef — разные опыты; теперь оба названы точно |
| RoutingProfile C#/.NET, U074/U076 | [KN-ROUTER:34–60][kn] | Тот же writer/reader ключа; generic P34/P36 другого router не используется как замена |
| Frozen transpose и BPE no-match, U039 | [Transpose:47–53][transpose], [BPE:37–49][bpe-copy] | Настоящее SFT/LoRA и обучение tokenizer соответственно; поздние retained results не принадлежат старому source-аудиту как новому запуску |
| RMS coordinate allocation, U050/U053 | [RMS:52–76][rms] | Тот же broadcast index без промежуточного массива; не новая математика RMS |
| Wiki taxonomy/margin, U099/U098 | [Taxonomy:31–43][taxonomy], [Margin:37–43][margin] | Output contract 15→17 отдельно от margin update при correct argmax |
| BPE weighted chunks, tokenizer snapshot, Wiki feature cache/keys, U003/U106 | [BPE weighted:52–58][bpe-weight], [snapshot:53–61][snapshot], [Wiki cache:28–44][wiki-cache], [keys:40–61][wiki-keys] | Четыре named factors имеют свои current cards; это не утверждение, что все hosts уже собраны |
| Ingress length и correction collision, U085 | [N63 ingress:33–39][ingress], [collision:40–61][collision] | Доставка допустимых коротких rows и выбор исправленного SFT target при сохранённом DPO→SFT — разные контрасты |
| AUT-WEIGHT/AUT-REPLAY и interdomain capacity, U086 | [Protected15:214–228][aut], [N63 capacity:62–79][capacity] | Качество урока, отказ кандидату и вытеснение другого домена не объединяются в один механизм |
| N60-E1 marker material, U065/U115 | [N60-E1:52–101][e1] | Нынешний спокойно/громко материал достигает writer; нежно/горестно старого review — альтернативная fixture того же фактора, не обязательный второй опыт |

Эта таблица подтверждает **названные механизмы**, а не каждую вспомогательную фразу каждого review. Полные ссылки source→current owner сохранены в строках JSON.

## 3. Два точных подслучая, которые нельзя скрыть общим P-номером

### U083 — NG01: числовой pattern ошибочно считается null evidence

[Planning NG01:25–47][ng01] меняет только direct relational pattern над параметром явно типа `int`, сохраняя nullable-int/type/null controls. Метки поступают в настоящий `FileActionKnnPredictor.Train` и затем читаются `Predict`. Protected [C2/N48-01:116–126][ng-prior] задаёт другой контраст: nested guard, receiver/call/order и заранее исправленные supervised labels. [Общий prior IsPatternExpression][ng-general] подтверждает происхождение идеи, но не содержит exact NG01 factor с его controls.

**Действие:** зарегистрировать существующие planning card/prompt, без второго NG01. Это узкий подслучай известной причины, не новая семья и не доказанное объяснение RP01. Полный passport уже включает literal TRAIN/EXAM, короткий host, source-copy patch, команды, модель по умолчанию, русский отчёт, полезные метрики/регрессии и STOP/NEXT. Runtime/build success в настоящем аудите не проверялся. Подробная сверка — [документ03](03-p54-ng01-semantic-verdict.md).

### U023 — E-METRIC-PHASE: какую фазу состояния читает convergence monitor

[Источник:32–70,94–103][phase-source] предлагает при одинаковой weighted objective сравнить monitor, читающий loss **до** update, с monitor, читающим ту же функцию **после** update. Оба плеча выполняют одинаковый post-probe, чтобы его цена не стала скрытым вторым фактором. Конкретная учебная связь подтверждена актуальным кодом: [StepVector вычисляет loss до ApplyOptimizer:891–940][phase-loss], а [LogicalDpoTrainingUnit:1371–1389][phase-caller] передаёт `lastMetrics.Loss` в `ShouldStop`, определяя дальнейшее число updates.

Ближайшие protected подслучаи этого фактора не изолируют:

- [P78.1:142–144][p78] меняет convergence window 1→5 при max 5.
- [P89.2:149–153][p89] меняет weighted loss на normalized unscaled loss; это изменение нормировки, а не временной фазы при одной функции.

Адресный поиск `E-METRIC-PHASE`/phase формулировок и содержательное сравнение P78/P89/P119 не нашли exact phase-monitor recipe. Формулировка «покрыто P78/P89» была бы лишь семейным, частичным покрытием.

**Действие:** сохранить отдельное exact уточнение в additive crosswalk. Сам источник честно не закрепляет все actual model/tokenizer/lesson bytes и готовый paired host; поэтому он не объявляется READY. До назначения runtime автор должен подготовить один конкретный паспорт и существующие host/команду, сохранив настоящий `LogicalDpoTrainingUnit`/capability path. Direct RP01 simulation не подставляется вместо этого owner. Успех может состоять в уменьшении полезно затраченного времени/updates либо другом полезном показателе; ухудшение качества/retention показывается рядом. Не проводить отдельный benchmark переименования `LastLoss`.

Это уже опубликованная гипотеза другого автора, выявленная crosswalk как точный пробел protected-покрытия. Новизна открытия себе не приписывается.

## 4. Три важных уточнения, для которых нельзя автоматически добавлять новый запуск

**U065, альтернативный N60-E2.** [OrderedPhraseSupport/v1:69–79][e2-alt] записывает learned phrase→slot IDs и наблюдённые порядки; общий reader расположен **до exact/phrase/word/kNN**. Нынешний [protected 23:353–367][e2-current] выбирает другой helper: `SourceSpanFrames{spanTokenLengths,gapTokens,supportExampleIds}`, проверяемый только перед успешным phrase return. Это частичное семейное покрытие, не тождественный контракт. Источник сам на строке 65 называет его параллельной конкретизацией одной гипотезы, без обязательного второго прогона. Нужно явно выбрать writer/state/reader-вариант и сохранить его границу fallback. Нельзя приписывать phrase-only guard защиту всех последующих routes или смешивать обе схемы в незаявленное двухфакторное исправление.

**U013, positive verifier correction.** [Источник:7–33][positive] фиксирует локальный no-op: `chosen.Pass=true` не отзывает старые required terms. Близкие [P56.2][p56] и [N59-L02][feedback] не устанавливают настоящий producer/admission такого положительного урока. В source-документе это прямо `CALLER_NOT_ESTABLISHED`. Условный вопрос сохраняется вне активной очереди. Ручной JSON в публичный метод не доказывает утрату существовавшего учебного сценария.

**U064, native update без bitwise LoRA delta.** [Текущий owner:193–221][zero-source] относит отсутствие изменённых adapter hashes к тому же failure condition, что нарушение frozen/readback/optimizer целостности. [Источник:17–42][zero-doc] предлагает различать completion, parameter effect и integrity, но не обещает, что ветвь встретится на выбранных уроках; старые C50/C100 меняли LoRA. Это наблюдение участия в настоящем update/readback, а не отдельная задача «улучшить статус». Точный protected overlay адресным поиском не найден. Дополнение уместно в уже назначенном bounded update, без искусственного снижения LR, нового fake Train или повторного опыта ради срабатывания условия. Admission, readback и исторические continuation guards сохраняются.

## 5. Применение к очереди и ограничения результата

P54-addendum U026 корректирует конфликт допуска уже существующих семи reader-подслучаев. Он не создаёт семь новых причин и не даёт готовый learned checkpoint. Подробное доказательство по текущим исходникам сохранено в [документе03](03-p54-ng01-semantic-verdict.md).

Исторические порядки и next-ID в метадокументах не используются как команда запуска. Например, U027 называет следующим N62-R1, тогда как [нынешняя N62:3–7][next] уже сохраняет завершённый R1 и задаёт R2. Запасной BPE `bp02` в U103 сам ограничен отдельным назначением; он не разрешает повтор основного завершённого BPE01. Успешные результаты и старые карточки сохраняются.

Ни один source review, name match, confidence в JSON или список из 116 строк не доказывает build success, фактическое before/after измерение или готовность всех доменных профилей. Установленный здесь полезный результат — более точное покрытие документов: известные факторы не дублируются, реальные exact отличия и неразрешённые вопросы не скрываются общей темой.

[ref-cache]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L96-L110
[n62-r12]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L265-L297
[n62-lf3]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L327-L349
[m1-ret]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L166-L201
[m1-id]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L242-L280
[kn]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md#L34-L60
[transpose]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md#L47-L53
[bpe-copy]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/BPE-NO-MATCH-COPY-01.md#L37-L49
[rms]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/RMS-COORD-ALLOC-01.md#L52-L76
[taxonomy]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-TAXONOMY-01.md#L31-L43
[margin]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-MARGIN-01.md#L37-L43
[bpe-weight]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/BPE-WEIGHTED-CHUNKS-01.md#L52-L58
[snapshot]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/DPO-TOKENIZER-SNAPSHOT-01.md#L53-L61
[wiki-cache]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURES-ONCE-01.md#L28-L44
[wiki-keys]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURE-KEYS-01.md#L40-L61
[ingress]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N63-ingress-length.md#L33-L39
[collision]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L40-L61
[aut]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/15-additions-designtocode-learning-semantics.md#L214-L228
[capacity]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L62-L79
[e1]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md#L52-L101
[ng01]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-results-and-new-gaps/cards/NG01-null-guard-label.md#L25-L47
[ng-prior]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/09-additions-44-48.md#L116-L126
[ng-general]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md#L45
[phase-source]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/62-LEARNING-METRICS-AND-REFERENCE-COST-AUDIT-2026-10-20/TRAINING-METRIC-PHASE-REPAIR-2026-10-20.md
[phase-loss]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L891-L940
[phase-caller]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1371-L1389
[p78]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/P78-loss-plateau.md#L142-L144
[p89]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/P89-density-stop.md#L149-L153
[e2-alt]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/10-CURRENT-EXPERIMENT-OVERLAYS.md#L63-L79
[e2-current]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L353-L367
[positive]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/62-LEARNING-METRICS-AND-REFERENCE-COST-AUDIT-2026-10-20/CANDIDATE-NOT-PROMOTED-VERIFIER-POSITIVE-FEEDBACK.md#L7-L33
[p56]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/P56-verifier-context.md#L147-L149
[feedback]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/22-source-join-and-learning-scope.md#L275-L279
[zero-source]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L193-L221
[zero-doc]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/04-NATIVE-UPDATE-ZERO-DELTA-RESULT.md#L17-L42
[next]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L3-L7
