# Источники, границы проверки и независимый review

**Дата:** 20.10.2026. Вид работы: исследование исходников и документов с публикацией дополнений только в `xiigen-planning/llm_plan3/llm mvp/design to code session/replanning/possibleDifs`.

**Последняя сверка:** [a822/bcf880 и параллельные planning-публикации](05-CONCURRENT-PUBLICATION-AND-EXPERIMENT-COPIES.md). Reference cache больше не предъявляется как отдельное новое открытие относительно актуального planning.

## 1. Зафиксированные ревизии

| Источник | Revision | Роль |
|---|---|---|
| `xiigen-planning`, `llm_plan3` | `1ad622fe1f6d1e8b0860265d8c208aeada3fefb0` | Исходный corpus possibleDifs, RP01 pack и parent G/F RAG. |
| `llm_mvp_core`, `codex/dpo-implementation` | `bf9b543e149ef197da85a767821d98c9b67916d2` | Current code, защищённые эксперименты и сохранённые результаты18 октября. |
| Тот же core, документная дельта перед публикацией | `f5dd532045716495a2c7668573250e5d7cc46810` | 3 последующих commits,152 изменённых файла только в protected. Код обучения не менялся. Новая политика и адресные изменения экспериментов учтены отдельно. |
| Тот же core, финальная документная дельта | `a8222d1b15745fe6207f36cd294fd3fb61c01d3b` | 144 документа; все125 card blobs объяснены, native код не менялся. Эксперименты на точных копиях, отдельное решение о product переносе. |
| Финальное уточнение документного маршрута | `bcf880f232d090e3cfcb459c108d3945ce6dbc15` | 145 документов; все125P blobs подтверждены заменой только общего header. N-KU — новый отдельный файл существующего N60-рецепта. Native код не менялся. |
| Planning, конкурентные публикации | `426385f91e7cb454e7e6827d9102d1e0d1284534` и `fb9aadf69e0669d36b820d61f76fe3ecc8697a7d`; финально `c38a2a84516c698056b6912d2933b22cd69281d4` | Пакет61 и current-learning-audit сохранены, пересечения и авторство учтены; master содержит их прежнюю навигацию. |
| `llm_mvp_core`, `external_domains` | `3036a01acec938f0b83fa4ffbda26202cc35d639` | Первичные E09/E14/E26/E27/E34 и code/commit lineage. |

Соседние branches также обнаружены и зафиксированы при разведке: `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548`, `DesignToCode-clean@e346b88a3cc1fcce48aefa626bfc7b00509eefd6`, `PrepareToGPL@31346d8928be4469b01aaf313780e8f950ff91e2`, `orchestrator-extensions@b293045bd89b1eb96d3e32fedd341f1a4170adbc`, `local_wiki_planning@219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`, `claude/sonnet-local-wiki@93a46381758257b72ee158159caf26b7fc4390b0`. **Обнаружение branch не заявляется полным аудитом её кода или восстановлением модели.** Её уже опубликованные findings сохранены в общем corpus и таблице покрытия.

## 2. Что прочитано и что означает полнота

- **555 planning Markdown:** полный текстовый corpus possibleDifs на pin для поиска совпадений и приоритета старых находок. Связанные с выбранными seams документы открывались адресно. Полное последовательное ручное чтение всех 555 не заявлено.
- **125 P-карточек / 470 подслучаев:** содержательная проверка механизмов, declared learning paths, domains, readiness, rule-of-success и active successors. Проверка содержания карточки не равна новой проверке каждого метода из неё. [Реестр](card-audit-125.json).
- **164 основных protected sources:** 125 карточек,24 active small-probe документа,7 общих experiment документов,8 root документов. Все164 побайтно сверены с Git blob SHA. [Проверка](verified_sources.json).
- **6 дополнительных protected result/review files:** исторические опыты P125, loader ingress, P25 и их независимые reviews. Основные выводы сверены по исходным отчётам. Их прежние reviews не объявлены сегодняшним независимым воспроизведением. Вместе170 Markdown участвуют в поиске покрытия.
- **Current neural sources:** exact DPO material, recipient/LoRA, trainer, convergence, native/simulation callers, SFT factory и связанные TensorCore paths. [Manifest](core-source-manifest.json).
- **64 исторических/дополнительных источника:** pinned raw results, code, commit patches, parent RAG и текущие kNN/translation seams. [Manifest](historical-evidence-manifest.json). Наборы источников пересекаются; числа не суммируются в число уникальных вручную прочитанных файлов.

Source proofs в отчётах — immutable GitHub links с code spans. `C:/...` внутри старых карточек и локальные paths в manifests — provenance прошлого окружения/сохранённого снимка, не обещание доступного сейчас runtime.

Основной 125-card registry и `audit-counts.json` сохраняют исходный pin и историю. Они не возвращают отменённый общий `NOT_EXECUTABLE_AS_IS` запрет. Актуальная семантика готовности — [PREPARABLE/NEEDS_INPUT/NOT_RUN из сверки f5dd](04-LATEST-POLICY-RECONCILIATION.md). [Полная граница изменённых файлов](core-head-delta-manifest.json) и [10 адресно перепроверенных latest документов](TARGETED-DELTA-EVIDENCE.json) позволяют отличить обновлённую политику от неизменённого native кода.

Дополнительно прочитаны и сопоставлены **полные latest тексты всех 125 карточек**: compare отдавал patches только для11, поэтому отсутствие patch не принято за отсутствие изменения. Новые125 карточек и3 вспомогательных источника совпали с Git blob SHA. Сохранены470 подслучаев,125 body rules,133 prompt rules и125 successors/anchors; изменились общая policy и32 адресные формулировки её применения. [Подробный receipt](LATEST-CARD-DELTA-RECEIPT.md), [per-card hashes и сравнения](LATEST-CARD-DELTA-RECEIPT.json). Это delta-review, не новая полная проверка native owners всех 125 тем.

## 3. Навыки и действующие инструкции

Применены содержательные инструкции предоставленного пользователем skill pack: `architect-behavior-classifier`, `architectural-decision-capture`, `plan-review`, `flow-plan-review`, `code-examination`, `promptops-cycle`, `human-language-gate`, `experiment-data-integrity`, `improvement-measurement`, `dpo-triples-to-experiment-pairs`. Дополнительно прочитаны актуальные `human-readable-architecture-audit-SKILL.md`, `prompt-patch-authoring-SKILL.md` и `human-communication-GLOSSARY.md` из core/docs/skills. Это роли проверки и способ оформления доказательств, а не выполненные native tests.

В результате архитектурная проверка требует реального writer/consumer, плановая — одного реализуемого фактора, кодовая — достижимости текущего caller, prompt-проверка — полного задания без обхода существующего learning owner, языковая — различения факта/гипотезы/исторического результата. Старые универсальные пороги улучшения и обязательные большие матрицы **не применяются вместо текущего пользовательского правила**. Прямое пользовательское назначение `possibleDifs` и запрет менять protected имеют приоритет над старыми путями публикации.

Актуальные [session AGENTS](https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/AGENTS.md) и [replanning AGENTS](https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/replanning/AGENTS.md) разрешают чтение и назначенную planning-публикацию с сохранением конкурентных изменений. Они не превращают этот документальный аудит в разрешение незаметно менять core, веса или запускать teacher. Новая команда исполнения не выдумана.

## 4. Независимая работа и проверка новизны

Три отдельных исполнителя работали параллельно: `experiment_audit` проверял защищённый каталог и выполненные результаты, `historical_knn` — историю/kNN/parent RAG, `neural_learning` — текущий neural code и новую гипотезу. Root интегрировал выводы, перечитал ключевые отчёты и source spans, сохранил приоритеты прежних авторов.

Reference-score cache проверен поиском по 555 planning и 170 protected Markdown. Exact matches —0; proximity matches —14 строк planning и4 строки protected. Близкие совпадения разобраны: KV cache generation, BM25, checkpoint caches и соседство разных тем. [Журнал поиска, все совпадения и SHA256](NOVELTY-EVIDENCE-2026-10-20.json). Это **отсутствие совпадений в исходном corpus**, не претензия на мировой приоритет DPO caching. До нашей публикации concurrent LC-REF-CACHE уже описал тот же механизм; E-REF-CACHE теперь отмечена как независимая проверка общей семьи, не дополнительное новое открытие. Отсутствие слова не доказывает отсутствие идеи: семантический разбор близких мест и независимая сверка P09/P105/P110/P119 включены в вывод.

Новый адресный E-OPT-STEP найден после обнаружения дублирующей cache-публикации. Два исполнителя отдельно проверили portable export, actual callback и rollback. Установлено условие: removal materialization должно сохранять все исходные metadata/recipient validations и порядок отказов; простой getter этому не соответствует. Конечный дизайн ограничен одним pre-RNG read внутри ProjectLoRA update,4actual calls/0Generate; внешние exports остаются общими. Source/novelty receipt — [OPTIMIZER-STEP-READ-EVIDENCE-2026-10-20.json](OPTIMIZER-STEP-READ-EVIDENCE-2026-10-20.json). Приоритет новизны ограничен baseline и адресной concurrent-дельтой, результат ещё не измерен.

Pre-update metrics отнесены к prior54; convergence — к prior35/38/P78; AST/generic kNN/POS/phrase mapping — к существующим N/P owners. Positive-verifier кандидат оставлен условным, потому что действительный producer не установлен. Это предотвращает искусственное увеличение числа «новых багов».

## 5. Финальная проверка публикационного кандидата

Выполнен отдельный review агентом `experiment_audit` двух neural документов по pinned code/protected источникам. Подтверждены reference→objective→backward→optimizer, weighted vector formula, фаза returned metrics и реальные caller-оговорки convergence. Замечание: основной cache опыт был перегружен обязательными промежуточными panels/cold/no-hit. Исправлено: основной бюджет4 updates; прочие вопросы имеют отдельные ограниченные профили. Повторная адресная проверка подтвердила это сокращение и нашла точную reservation-границу парного server опыта; она добавлена в обе соответствующие карточки.

Агент `historical_knn` отдельно прочитал README и карту приоритетов, заново сверил ingress/P25 с первичными отчётами. Подтверждены числа, доменные границы, отсутствие ложного заявления о125 готовых runs и статус unresolved matrix→kNN. Единственная поправка: четырёхточечный training A/B не должен навязываться restore-проверке. В §4 карты прямо добавлено исключение для доказанно обученного checkpoint без новых updates.

После появления f5dd агент `neural_learning` адресно перепроверил10 latest документов. Root прочитал новую execution policy и compare:152 изменённых файла относятся только к protected docs. В пакет перенесены отмена повторного личного approval gate, разрешённые dependencies и ресурсный update без Generate; readonly scope текущего документального задания сохранён. Основной cache опыт сокращён до 4 updates/0Generate, accuracy честно NOT_MEASURED.

Последний узкий review E-REF-CACHE подтвердил бюджет и согласованность статусов; замечание — явно повторять **обе** пары в каждом из двух StepVector. Эта строка добавлена, чтобы знакомый последовательный batch1 T1→T2 не подменил repeated workload без cache hits. Также уточнён независимый интервал измерения peak RAM; накопленный lifetime peak A→B не выдаётся за peakB. Изменения адресные, дополнительный запуск не назначен.

После a822 отдельно проверены все125 card Git blobs:124 отличаются только общим header, P86 — также пятью точными approval-phrase заменами. Необъяснённых отличий нет. Все19 patches изменённых документов вне cards прочитаны. После параллельных planning commits сохранены чужие Mode1/router находки, снят дубль новизны reference cache, обновлён маршрут новых карточек и prompts на точные экспериментальные копии.

Последняя bcf880-дельта сверена адресно:125P blobs снова подтверждены одним header-only преобразованием, без нормализации текста. Учтены точный host, запрет дополнительных warmup/повторов, сохранение существующих artifacts и отсутствие бесконечного перечитывания из-за документного HEAD. N-KU прочитан по scope/budget/metrics; его runner не компилировался и полностью не сертифицировался.

Для E-OPT-STEP [независимый review neural_learning](OPTIMIZER-STEP-READ-INDEPENDENT-REVIEW-2026-10-20.json) подтвердил достижимость callback даже при dropout0 у simulation caller, сохранение metadata guards без копий m/v и цену этой проверки. Внесены замечания: не утяжелять A общим refactor; optional rollback injection только после настоящего ApplyOptimizer и в пределах четырёх calls; подтверждённые guard/state условия не выдают ещё не написанный host за протестированный. Root отдельно прочитал исходные ExportPortableState/portable DTO/CaptureState/RequireAllowedState и actual caller. После c38a2a8 полностью прочитаны новые R1/R2: bitwise base comparison и backward scratch lifetime отличаются от E-OPT-STEP. Их авторство сохранено;32 новых файла конкурентного пакета не перезаписываются. Финальные правки размещения и будущего prompt выполнены отдельно, без заявления нового native review.

Все reviews статические и имеют указанные границы. Они не удостоверяют numeric reproducibility, доступность actual runtime artifacts или измеренный выигрыш. Предыдущие и последующие изменения кандидата не выдаются за один неизменный проверенный SHA.

Проверка самого документационного набора: JSON читается, IDs P01–P125 уникальны,470 подслучаев сохранены, локальные Markdown-ссылки разрешаются, правило успеха присутствует в новых карточках и их промптах. Публикационный список ограничивается новым пакетом62 и master navigation; ни одного core path в write set нет. Финальные машинные результаты проверки сохраняются в `DOCUMENT-VALIDATION.json` перед записью.

## 6. Что не измерялось

Ни одна модель не обучалась, inference/teacher/native build или benchmark не запускались. Не заявлены новое ускорение, исправление product core, качество после нового optimizer update либо воспроизведение matrix→kNN истории. Указанные численные улучшения имеют ссылки на сохранённые первичные отчёты с их границами.

Сохранность protected обеспечивается отсутствием любых write operations в core: планируемый commit состоит только из нового пакета62 и короткого навигационного дополнения к `00-MASTER-SYNTHESIS-AND-CONFLICTS.md`. Реальный publication commit и результат remote readback сообщаются после записи.
