# Источники, новизна, независимая проверка и предел полноты

**Результат этого прохода — документация. Обучение, сборки, product tests и перенос исправлений не выполнялись.** Проверены исходники, формулировки рецептов и уже сохранённые raw. Арифметический пересчёт существующих ответов/метрик не выдаётся за новое исполнение.

## 1. Закреплённые версии

| Источник | Версия | Для чего использован |
|---|---|---|
| xiigen-planning, llm_plan3 | `53cbcabd3926cda714bab428d294eb666301e9e4` | Все 799 Markdown possibleDifs для новизны, выбранные RP01/RAG и родительские требования |
| llm_mvp_core, codex/dpo-implementation — основной аудит | `66947785d1318f7cbbc88118f3156cdc75001e16` |154 карточки, 133 P-промпта, source learning paths, первые 5 completed и исторические receipt |
| llm_mvp_core — первая актуализация | `827a1acdbef95d220886e54f9d36bce0b405c8cd` |157 карточек, 36 queueID, E-OPT-STEPcompleted, изменённые пути и остаточные конфликты протокола |
| llm_mvp_core — последний readback | `6e3e8864933e35fccdb4f21369c9912fa195aaf6` |LF-03 completed, EOPT erratum, 27 card deltas, 29 unchanged future roots |
| xiigen-planning — параллельная публикация | `eaed843ad2b063fbb003f073deb69da19f0cb135` |33 новых Markdown; GRAD/FROZEN canonical cards и взаимная новизна |
| Исторический RP01 | `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` |Два scientific-result и source Main500/continuation |
| external_domains | `3036a01acec938f0b83fa4ffbda26202cc35d639` |E07/E09/E25/E26/E43 source/результаты |
| language_translate | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` |Сохранённые StructuralKnn/ContextualDictionary и сравнение с current |
| Дополнительно просмотренные деревья | local_wiki_planning `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`; DesignToCode `8f20ceb8a9bfbadab2ad309226c47dd76e714548` |Навигация и проверка наличия материалов; не полный аудит этих веток |

Название папки пакета сохраняет основной 669 срез. Это не утверждение, что следующий 827 коммит отсутствует; он специально разобран в [09](09-core-827-delta.md). Production subtrees девяти проектов, охватывающих 36 RMS и 7 VT native files, имеют одинаковые Git tree SHA на обоих срезах. Поэтому закреплённые версии исходников новых карточек корректны и после обновления. [Точная квитанция](evidence/latest-core-learning-source-delta.json).

Последний 6e и соседний planning eaed разобраны в [10](10-final-reconciliation.md). На 6e все 45 production project trees совпадают с 827. [Квитанция](evidence/core6e3-production-tree-delta.json). Предыдущие review receipts сохраняют явную область своего среза; их старые counts не являются текущим NEXT.

## 2. Что можно проверить по приложенным квитанциям

| Проверяемое утверждение | Файл |
|---|---|
| Все 799 исходных planning Markdown получены с правильными bytes | [planning-corpus-manifest.json](evidence/planning-corpus-manifest.json): path, Git blob SHA1, SHA256, размер каждого |
| Откуда взяты приложения и выбранные первичные RAG/RP01 | [input-and-scope.json](evidence/input-and-scope.json): SHA256 четырёх приложений, source refs/paths/hashes и scope |
| Какая из 154 карточек чему соответствует, какие 470 подслучаев reference-only | [полный указатель](evidence/all-154-card-readiness.md) и [JSON](evidence/all-154-card-readiness.json) |
| В каких baseline карточках/промптах найдены обязательные правила | [card-rule-presence](evidence/card-rule-presence.json), [p-prompt-rule-presence](evidence/p-prompt-rule-presence.json) |
| Пересчёт current R1/R2/BPE значений | [current-raw-deltas](evidence/current-raw-deltas.json), [16 primarysourcefiles](evidence/primary-results-source-manifest.json) |
| EOPT, свежий census и неизменённые протоколы | [core827delta](evidence/core-827-delta.json), [49 sourcefiles](evidence/core-827-source-manifest.json), [inventory158](evidence/core827-cards-inventory.json) |
| Сопоставление 144 P125 ответов, 24 ingress пар и 24 cold пар | [historical-primary-readback](evidence/historical-primary-readback.json), [primarymanifest](evidence/historical-primary-source-manifest.json), [blobvalidation](evidence/historical-primary-snapshot-validation.json) |
| Primary источники соседних веток и совпадение тела двух старых/current классов | [historical-branch-source-manifest](evidence/historical-branch-source-manifest.json) |
| Новизна VT/RMS в объявленном корпусе | [VTsearch](evidence/verifier-apostrophe-novelty.json), [RMSsearch](evidence/rms-row-reduce-novelty.json), [проверка 3 добавленныхкарточек](evidence/latest-core-three-card-overlap.json) |
| Native closure нового RMS и независимое совпадение 36 hashes | [source manifest](evidence/rms-row-reduce-source-manifest.json), [independent hashes](evidence/review-rms-source-hashes.json) |
| Дополнительные 33 planning MD и взаимная новизна | [readback](evidence/concurrent-planning-readback.json), [сравнение факторов](evidence/concurrent-eaed-learning-merge.md) |
| LF-03, последние 27 card changes и неизменность roots | [delta](evidence/core6e-delta.json), [source manifest](evidence/core6e-source-manifest.json), [paths](evidence/core6e-output-map.json) |
| Однозначные 40 ID и 33 future roots/entry | [execution-registry](execution-registry.json), [output-path-map](output-path-map.json) |

Git blob hash проверяет идентичность прочитанного текстового файла, не правильность его научного вывода. Для JSON с BOM использовано UTF-8-sig при чтении; исходные bytes и CRLF не переписаны в primary snapshots. В файлах квитанций встречаются рабочие локальные snapshot paths: они обозначают происхождение прочитанного материала, а не обещание наличия этих временных путей у будущего исполнителя. Авторитетные адреса задаются repository/commit/path и ссылками соответствующего документа.

Механическая таблица card-rule-presence сохранена как результат поиска фраз. Отдельные false означают несовпадение точного шаблона, а не окончательный verdict о нарушении: синонимы запрета удаления и успеха вручную проверены. Подробное объяснение — [03](03-experiment-coverage.md); наличие формулировки не превращается в доказательство правильного выполнения всех подслучаев.

## 3. Независимая проверка новых постановок

Автор VT проверил RMS, автор neural-аудита проверил VT; отдельный проверяющий сопоставил registry, outputs и measured summaries. Во всех проверках использовалась модель текущей сессии по умолчанию, без смены модели исполнителя или внешних judges.

| Материал | Что проверено | Итог |
|---|---|---|
| [VT review](evidence/review-verifier-card.md) |7 source hashes, producer→native Train→state→Verify, раздельные маркеры, gold и trade-off, бюджет/пути/guards | **DESIGN_VALID / RUNTIME_UNVERIFIED** |
| [RMS review](evidence/review-rms-card.md) |36 source hashes, last-axis адресация/порядок float sums, 8 updates/16diagnostic forwards, таймеры/receipts | **DESIGN_VALID / RUNTIME_UNVERIFIED** |
| [Learning summary review](evidence/review-learning-summary.md) |Точность LoRA/autograd/mask, parser внутри Train, бюджет, lazyLINQ в R1 |Адресные уточнения внесены; утверждения об исходнике и измерениях ограничены |
| [Master review](evidence/review-master-docs.md) |Проверенный прежний срез 827: 38 ID/6completed/32future, NEXTLF-03; последующий 6e delta вынесен отдельно |Документная проверка; готовность к запуску не заявлена |
| [Последний review6e/eaed](evidence/review-final-reconciliation.md) |40 ID/7 completed/33 future; NEXT08 N63;12 inherited; LF-03 не повторяется;33 команды и roots |Документная согласованность; runtime не проверен |

В результате исправлены конкретные неоднозначности: дообучение не подменяется parser-only benchmark; контрольный маркер не маскирует target через OR; malformed строка parser сохраняет старый fallback; hash gold до Train не раскрывает labels; неизвестные counters не заменяются нулём; RMS receipt не утверждает BPE/LogicalUnit, которых нет в этом host; source guard проверяет original bytes до patch. Общий audit также уточнил, что frozen embedding штатно не имеет backward, R1 использует ленивое LINQ сравнение, а newRMS бюджет —4 на arm/8 всего.

Проверка ссылок и взаимной согласованности документов выполняется отдельно от обучения. Ни один design review не утверждает, что новый host уже построен, что WindowsSDK доступен или что будущая гипотеза дала измеренное улучшение.

## 4. Что остаётся неизвестным

- Не установлена единственная причинная ошибка, объясняющая все нулевые свободные ответы RP01. Известны изменение веса/предпочтения и конкретные факторы исходника, но не их изолированный вклад в исходный результат.
- Не восстановлен exact matched происхождение донорской функции того самого matrix→kNN переноса, на который указала Люба. Исторические положительные структурной и контекстной памяти результаты сохранены; ограниченное чтение не опровергает её свидетельство.
- Не сделан построчный смысловой аудит всех репозиториев, веток, 832 документов и 470 подслучаев. Полным был download/search корпус и baselinecard/promptcensus, а causalreview — адресным.
- Для R2, EOPT и LF-03 не полностью подтверждено происхождение исторического B-бинарника. В EOPT остались подписи raw от R1; поздний erratum уже раскрывает эту ошибку. В LF-03 также расходятся manifest и нынешний bridge hash. LFS payload ingress comparison недоступен; основные сопоставимые числа восстановлены из доступных raw. Эти ограничения не скрыты и не «исправлены» новым прогоном.
- P54.1–P54.7 требуют конкретных learnedartifacts/contract/host. Новые VT/RMS ещё не исполнены. Успехи текущего набора не доказывают перенос на все языки, модели, размеры и предметные темы.
- Scope этой публикации — добавление документов в possibleDifs и входной ссылки его README. Защищённые core эксперименты, productcode, веса и старые raw этой работой не меняются; старые исследования не удаляются.

**Правило результата сохраняется:** полезное измеренное улучшение — успех своей оси и темы, accuracy не обязана расти. Пользователь решает, какие кандидаты переносить в основной код. Доказательство уже сохранённого успеха не служит предлогом повторять эксперимент, а STOP/NEXT немедленно прекращает дооформление прежней задачи.
