# Источники, границы новизны и независимая проверка

## 1. Закреплённые версии

| Назначение | Repository / branch | Immutable commit |
|---|---|---|
| Эталон, отдельно названный пользователем | llm_mvp_core / DesignToCode-clean | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` |
| Проверенная нынешняя реализация | llm_mvp_core / codex/dpo-implementation | `358af351e22a92a3f7938d1f9221d5aeb7ddb053` |
| Исходный planning/RAG и prior corpus | xiigen-planning / llm_plan3 | `b0308fbaa783758104987e6544d057062ccf6fc3` |
| Свежие параллельные planning-пакеты | xiigen-planning / llm_plan3 | `82c37d186ef8a95616d68b995207d5c1803f5bcb` |
| Следующая прочитанная редакция planning master | xiigen-planning / llm_plan3 | `b95278b25a6697b42ef7025e6965a9f2837a34b7` |
| Дополнительный пакет, появившийся во время публикации | xiigen-planning / llm_plan3 | `6b97c01deecc0382e1a5b5c68b10d520a483c1f2` |
| Последний прочитанный пакет восстановления и общий README | xiigen-planning / llm_plan3 | `33bf0f449d0a440a6ef6f0a32799ce5e1b4af1de` |
| Соседняя wiki-линия, отдельное происхождение результатов | llm_mvp_core / local_wiki_planning | `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` |
| Исторические соседи, использованные для lineage/prior сопоставления | DesignToCode / PrepareToGPL / external_domains / language_translate / orchestrator-extensions | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` / `31346d8928be4469b01aaf313780e8f950ff91e2` / `3036a01acec938f0b83fa4ffbda26202cc35d639` / `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` / `b293045bd89b1eb96d3e32fedd341f1a4170adbc` |

Наличие branch/ref в таблице не означает полный аудит каждого её исходника. Главные direct comparisons — clean/current; соседние receipts и предыдущие исследования сохраняют собственный evidence class. Полные тексты `CodeTrace064`, `QueryPlanning134`, `ContextRouting075` в этом пакете заново не пересчитывались: их предыдущие цифры отдельно обозначены PRIOR/SIBLING_ONLY.

[Source manifest](evidence/source-manifest.json) содержит 43 критические записи immutable path/blob identity из direct сравнений и clean archive. Это выборка ключевых первичных файлов, а не список каждого просмотренного файла. Дополнительные точные source anchors даны непосредственно в главах00–03; [independent tree proof](evidence/knn-independent-tree-proof.json) подтверждает две translation blob identities.

## 2. Как пройдены RAG и ссылки в родительский каталог

Planning читался как источник требований и навигации. Исторические статусы PASS/FAIL в нём не заменяли чтение функции и raw результата. Там, где новые current sections отменяли старое описание режима, сохранена граница даты и назначения.

| Источник на исходном planning snapshot | Что из него применено | Чем проверялось исполнение |
|---|---|---|
| [RP01 README](../../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md) и [recovery/code audit16.10](../../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | Learning-first цель; различие weight/margin и обычного ответа; текущие узкие recovery направления | Current DpoTrainer/materializer/StudentGeneration, domain consumer и архивные receipts |
| [Minimal verification policy](../../LEARNING-FIRST-MINIMAL-VERIFICATION-POLICY-2026-10-14.md) | Исследовать один bottleneck и предметный эффект; не подменять его широким CI | Две source-пробы, одна схема P/C0/T и конкретные доменные панели |
| [original task](../../../original%20task.md), [task prompt](../../../task%20prompt.md), [алгоритм DPO](../../../%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC%20%D1%81%D0%BE%D0%B7%D0%B4%D0%B0%D0%BD%D0%B8%D1%8F%20%D0%94%D0%9F%D0%9E.md) | Many-to-many typed outputs, реальная оценка/render, сохранение результата обучения, teacher/quality цикл | Builder→typed Unit→state→runtime; cardinality и evidence authority рассматриваются отдельно |
| [RAG02 design-to-code](../../../09.09.26WorkLeft-RAG-02-design-to-code.md) и его ndjson | Навигация по design lifecycle и прежним исследованиям | Прямое чтение Machine/TrainedUnits/TrainingPipeline/RoutingFlow |
| [RAG01 dpo-legal-build](../../../09.09.26WorkLeft-RAG-01-dpo-legal-build.md), [RESEARCH03 conformance](../../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md) | Exact pair, generator/judge separation, once-only, rollback и independent source families | Поле→получатель→loss/state→обычный ответ; будущие teacher/model calls явно NOT_RUN |
| [Родительский план07 DPO builder](../../../../dpo%20legal%20build/active-codex-execution-plan-07-dpo-builder.md), особенно §3.2a | Canonical serialization, policy/reference reward, winner-vs-each-loser; judge score не является DPO reward | Current materializer/DpoTrainer и domain projections; нет заявления о полном conformance всего builder |
| [Родительский план10 lifecycle](../../../../dpo%20legal%20build/active-codex-execution-plan-10-training-lifecycle.md), CANON и LC1…12 | Import/load, state identity, continue, rollback, cold process и ordinary consumer | CodeTrace LFS граница и DC-01b import/load отказ; native восстановления не запускались |

Крупные parent-планы читались адресно по относящимся разделам; полный аудит всех содержащихся в них исторических задач не заявляется. Оглавление/индекс не считались доказательством исполнимого алгоритма. Домен означает предмет задачи; «KNN», «LoRA» и «Unit» в документах названы механизмом или получателем, а не новым предметным доменом.

## 3. Дедупликация и её реальный охват

Исходный снимок: **415/415 Markdown possibleDifs + 92/274 Markdown protected core**, всего 507 полных текстов. В обоих каталогах получены полные деревья. [Prior corpus manifest](evidence/prior-corpus-manifest.json) фиксирует path и Git blob SHA; все 507 кэшированных текстов byte-exact. Это full-text поиск и адресное смысловое чтение, **не 507 глубоких независимых кодовых аудитов**.

До публикации обнаружены 33 новых/изменённых planning Markdown относительно начального снимка: **31 новый путь и 2 изменённых**, удалений нет. Они прочитаны отдельно; следующая редакция master также сверена. Это расширяет уникальную planning-выборку до 446 путей, а вместе с 92 protected — до **538 уникальных Markdown-путей**. Версии отдельных README/master читались повторно. Затем HEAD изменился при записи: дополнительно прочитаны **15 Markdown версий (14 новых путей + README)** пакета `2026-10-19-designtocode-clean-causal-audit` на `6b97c01d`. На этом этапе охват составил **460 planning + 92 protected = 552 уникальных Markdown-пути**. [Квитанция15/15 и смысловая сверка](evidence/publish-head-delta-receipt.json) подтверждает, что четыре bounded семьи сохраняются; E27 roles/span, E13 While/Do, vector/mean-state находки относятся к другим механизмам. Их материалы сохранены при перебазировании публикации. На следующем HEAD `33bf0f449d0a440a6ef6f0a32799ce5e1b4af1de` получены ещё **12 Markdown-версий (11 новых путей + общий README)**. Финальный охват — **471 planning + 92 protected = 563 уникальных Markdown-пути**. Адресное смысловое чтение [CLEAN-CP01](../54-DESIGNTOCODE-CLEAN-RECOVERY-2026-10-19/02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md) обнаружило уже описанное следствие `whitelist-invalid → StageLearnedWeights → отказ целого checkpoint`; оно больше не заявляется новым в DC-01b. Специфические phantom/canonical generic и alias migration остаются ограниченным уточнением, KU-01…03 не совпали. Четыре семьи сохраняются. [Подробная квитанция позднего refresh](evidence/planning-late-dedup-receipt.json) и [34/34 Git blob integrity](evidence/planning-late-final-integrity.json) включены в evidence; после последнего snapshot глобальная неизменность ветки не предполагается.

[Novelty receipt](evidence/novelty-receipt.json) содержит точные поисковые группы и hits начального набора. Единственный exact hit `BaseConceptsRu` относится к прежнему N23 о previous/future context, а не suffix→dictionary mismatch. Отсутствие имени метода само по себе не доказывает новизну: поздние DC-02/03 совпали **по содержанию**, поэтому их статус изменён.

| Наш ID / вопрос | Предшествующая работа и свежая сверка | Окончательный статус |
|---|---|---|
| **DC-01a: absent scalar→phantom generic** | В initial507 нет exact методов/IDs. Поздний N54-00B трактует отсутствие как пустой набор; source/probe показывают generic. Общая проблема feature signal уже обсуждалась | **BOUNDED_NEW_REFINEMENT** с исправлением описания missing fields |
| **DC-01b: canonical generic→accepted clean→whole import rejected current** | Review54 описал whitelist delta; поздний [CLEAN-CP01](../54-DESIGNTOCODE-CLEAN-RECOVERY-2026-10-19/02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md) на `33bf0f44` уже проследил `StageLearnedWeights` и целостный отказ для retired hash-diversity/pair-family keys. Наше отличие — canonical semantic generic/underscore, phantom producer и collision двух alias при migration | **BOUNDED_NEW_GENERIC_ALIAS_REFINEMENT**; общее следствие отказа всего импорта — **KNOWN_LATE_CORROBORATED** |
| **DC-02: all-negative fallback** | Поздний GDC-A2 содержит нулевые/отрицательные weights и возврат двух rules | **KNOWN_LATE_REFINED**; новый контрпример/ограничение ущерба, не новый механизм |
| **DC-03: positive top-1 вместо all-rules** | Поздний N54-01 содержит ту же ветвь и тот же blob | **KNOWN_LATE_REVALIDATED**; независимое подтверждение с совместимостью CSS/control |
| **KU-01: suffix/POS уничтожает concept до RU→EN KNN gate** | Предыдущие BaseConceptsRu/E26 и noun augmentation отличают другой этап; exact новая producer→gate цепь не найдена | **BOUNDED_NEW** |
| **KU-02: PAST-on-any-VERB** | Exact и содержательное совпадение в прочитанных prior/late docs не найдено | **BOUNDED_NEW** |
| **KU-03: raw punctuation в gender при cleaned POS** | Прежние punctuation/phrase ordinal и previous-concept boundary отличаются числом/индексацией; здесь тот же токен меняет morphology address | **BOUNDED_NEW** |
| Neural DPO/LoRA/optimizer, E25/26/09/43, old checkpoint4→6, KNN namespaces, admission, source routing | Уже опубликованные главы02–53 и protected index | **KNOWN / RETAIN / REVALIDATED**, без присвоения новых ID |

Итого **четыре новые относительно проверенного объёма семьи (DC-01 и KU-01…03)** и два независимо уточнённых известных механизма. DC-01a/b — связанные части одной normalization/state семьи; они не превращены в два независимых доказанных root cause. Успех/провал конкретного RP01 run этим перечнем не объяснён причинно без его input/state/consumer trace.

### Почему часть protected-корпуса не дочитана

Автоматическая проверка отклонила расширение **read-only скачивания оставшихся файлов core/docs/plans/replanning-possible-diffs в локальный research-кэш**. Указанная причина: фраза пользователя «this one you can't touch» была истолкована как запрет также на чтение. Повторное представление read-only scope не сняло отказ. После него новых чтений защищённой части через другой способ не выполнялось. Анализ продолжен по уже полученным 92 текстам, полному индексу и доступным planning-материалам.

Остаются 182 непрочитанных protected Markdown; глобальная уникальность находок среди них не утверждается. Для расширенной проверки нужно отдельное явное разрешение **только на чтение** этой части; разрешение на её редактирование не запрашивается и публикация туда не выполняется. Ограничение не мешает публиковать доказанные source-контрпримеры и честный bounded novelty результат в разрешённом possibleDifs.

## 4. Применённые проектные skills

Skills прочитаны из указанной пользователем ветки `docs/skills`, закреплённой за current SHA. Использованы относящиеся к исследованию требования, а не старые числа фаз/проходов или заявления PLAN_READY без фактического evidence.

| Навык | Применение в этом пакете |
|---|---|
| [architect-behavior-classifier](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/architect-behavior-classifier-SKILL.md) | Детерминированное правило, learned memory и neural authority разделены; каждый claim привязан к реальному consumer |
| [intent-to-plan](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/intent-to-plan-SKILL.md), [phase-planning](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/phase-planning-SKILL.md) | Сначала восстановление конкретной способности, затем ограниченный causal contrast; user scope — документы/эксперименты |
| [plan-review](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/plan-review-SKILL.md), [plan-self-validation](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/plan-self-validation-SKILL.md) | Независимые отрицательные контроли, evidence boundaries, верные роли P/C0/T, отсутствие ложного PASS. plan-self-validation прочитан целиком; plan-review — адресно, весь файл4274строки не заявлен прочитанным |
| [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/generated-code-review-SKILL.md) и применимые положения [code-examination](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/code-examination-SKILL.md) | Независимое чтение методов и caller, source vs native разделены; blanket inheritance исправлен по whitelist/import |
| [prompt-patch-authoring](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/prompt-patch-authoring-SKILL.md) | Одно вмешательство, конкретный target/контрпример, frozen mappings и готовое ограниченное поручение06 |
| [human-communication glossary](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/human-communication-GLOSSARY.md), [output-readability](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/output-readability-gate-SKILL.md) | Предмет, получатель, механизм, измерение и неизвестное изложены отдельно; важная восстановительная глава вынесена в начало |

Это применение проектных навыков исследовательскими агентами текущей сессии. Отдельный вызов CODEX с Astra 6 ultracode для quality approval не заявляется: эта роль обязательна в спецификации будущих DPO/learning опытов, а не выдумана как уже выполненный запуск.

## 5. Независимая проверка и внесённые исправления

| Замечание review | Что изменено до публикации |
|---|---|
| Первоначальное «всё унаследовано» было слишком широким | Прочитаны оба IsAllowedLearnedWeightFeatureKey и Import/Load. DC-01b выделен как current admission/restore delta; ошибочный blanket claim удалён |
| Skip-empty оставлял повреждённый canonical ID | Добавлены явный generic, контроль всех scalar полей и 2×2 skip/preserve. Для migration добавлен collision case |
| Старый generic мог приниматься clean, а current отвергать весь state | Добавлена exact-blob baseline проверка, separate cross-branch independent receipt и zero-update restore опыт; native отказ на реальном artifact не заявлен |
| Две «новые» design-гипотезы опубликованы параллельно | DC-02/03 переведены в KNOWN_LATE_REFINED/REVALIDATED, даны ссылки на автора54 |
| На последнем HEAD уже описан whole-checkpoint отказ | CLEAN-CP01 получает приоритет общего StageLearnedWeights/import consequence. DC-01b сохранён только как generic-specific refinement, включая отличие semantic alias от retired provenance keys |
| Изначальный KU negative control «файл» оказался неверен | Сохранён original failed-control JSON. Слово стало явным anomaly row; неизменный control заменён на «метод» |
| Смена encoder меняет и WordMappings | Frozen chosen/WordMappings/PhraseMappings явно добавлены в02/03/04/06; word-map mediator отделён от KNN-address эффекта |
| CodeTrace49/50 неверно было бы переносить в free generation | Определение margin проверено по evaluator и всем100 diagnostics; sibling-only archives и LFS pointers отделены |
| C0 сначала был назван стадией исправления input | Во всех опытах C0 означает no-update load/eval control; стадии R0 именуются отдельно |
| Неполные source ranges и SourceLaw attribution | Исправлены Adapter124–139, backward до1121, raw JSON до18; добавлены current/clean SourceLaw и NormalizeMapKey ссылки |

[Финальная независимая source/plan записка](evidence/final-package-review-neural.md) и [поздняя дедупликация](evidence/final-planning-dedup-review.md) сохранены. Первоначальные current-only DC axes независимо повторены; новая clean/current admission граница отдельно проверена по финальной версии скрипта без ненужного повторения остальных axes. KU probe повторён другим агентом, JSON совпал. Два reviewer-а отдельно прочитали существенные source paths; final boundary review выявил и исправил ошибку исходного обзора, а не просто подтвердил его.

## 6. Что сделано, что не сделано

**Сделано:** чтение primary sources и истории; сопоставление immutable blobs; пересчёт raw CodeTrace019 и E25/26/09/43/E07; full-text prior/late dedup; две воспроизводимые source-equivalent пробы; независимый review механизмов/планов/ссылок; документы и конкретные domain experiments.

**Не сделано:** native C#/.NET execution; actual DPO training; новые teacher/judge/model calls; свободная генерация Student; actual LFS payload load/cold restore; render и измерение visual/semantic quality gain. Числа контрпримеров не являются model accuracy. Файлы core, его protected каталог, исходные checkpoints и чужие planning-документы не изменяются; для навигации в общем README добавляется собственная первая глава с сохранением прежнего содержимого.

Публикационный readback и проверка scope фиксируются отдельной технической квитанцией. Финальная GitHub ссылка указывает на конкретный commit; имена веток используются только для навигации и получения нового актуального HEAD.
