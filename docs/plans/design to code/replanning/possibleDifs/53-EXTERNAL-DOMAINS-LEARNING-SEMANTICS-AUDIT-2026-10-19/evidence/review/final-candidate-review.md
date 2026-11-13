# Итог проверки после исправлений

**ACCEPTED_AS_BOUNDED_RESEARCH_DOCUMENTATION.** REV-H01/H02/H03 закрыты; финальные SHA и проверка новых 05/06/README приведены в `final-candidate-review-addendum.md` / `.json`. Ниже сохранена исходная стадия review с первоначальными hashes и замечаниями, а не перечень открытых требований.

# Независимый content review: точный исследовательский кандидат

Дата: 2026-10-19. **Основные шесть механических цепочек подтверждены в заявленных source-bound границах.** К публикационному тексту есть два средних и одно небольшое уточнение ниже. Опровергающей первичной ошибки либо заявленного нового native прироста качества в шести разделах не найдено. Это независимый исследовательский review фактическим унаследованным runtime, без сертификации имени модели `Astra 6 ultracode` и без формального PLAN_READY/ProductReady.

## Проверенные точные файлы

| Файл | SHA-256 | Строк |
|---|---|---|
| `01-MECHANISM-CROSSWALK.md` | `84fed1dfd415944d3ec503426b4f858b2e813b8b5e02b10acbd8540a2b704f97` | 97 |
| `01B-KNOWN-UNIT-TRANSFORMATION-GAPS.md` | `083ef32d4104e6e7910b1b7116eb01d148fba0b2b0575436c77daf2511ad70c7` | 48 |
| `02A-NEW-SEGMENT-AGGREGATION-GAP.md` | `40dacd8336707d291bc7982594b5238a7ee7c0342323667bc340f97e1c6cb053` | 83 |
| `02B-NEW-CANDIDATE-ELIGIBILITY-GAP.md` | `239f42de666ff630070b12df28689669617b061279171df5d0acf7bbe32c4a49` | 92 |
| `03-NEW-PROMPT-PATTERN-GAPS.md` | `b198a9d2c80e5f4a231025e33d717e7bc4c6540aced0dd0332839c54f89c3e9e` | 98 |
| `04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md` | `836cd4b3dfef9d4f7da7671790050d488f10722d499d204441c2d637bb69b25f` | 143 |

Файлы кандидата не изменялись. SHA повторно сопоставлены в конце этой стадии и совпали. Глава 00 не включена: её отдельно проверяет родитель. Итоговые 05/06/README ещё ожидаются для дополнения этого review.

## Замечания и точные правки

### REV-H01 · MEDIUM · 02A-NEW-SEGMENT-AGGREGATION-GAP.md:51–59

B должен проверить, какой конкретный carrier доставляет оба source associations исполнителю. Outer EvidenceIds сам по себе этого не доказывает.

**Исправление:** В L1/L2 явно записать вход реального downstream executor: оба исходных текста, source IDs и связь с выбранным target. Если полный исходный request уже законно сохраняет обе задачи, зафиксировать этот путь. Если B сохраняет IDs лишь в отчёте/outer evidence, считать это промежуточным результатом, а не восстановленной доставкой двух задач.

**Первичная опора:**
- [21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:157–175](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L157-L175): Сегментные IDs лежат во внешнем LogicalUnitResult.
- [21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:1275–1291,1330–1332](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs#L1275-L1291): Caller сохраняет ownedDecisions и возвращает только их; outer carrier не является возвращаемым результатом RouteSubPhasesAsync.

**Граница:** Не новая гипотеза потери всех источников: полный request может сохранять обе задачи. Основной ID-collision подтверждён.

### REV-H02 · MEDIUM · 03-NEW-PROMPT-PATTERN-GAPS.md:37–41

Фраза «четыре запроса… намеренно конфликтующее решение» не определяет, откуда в памяти берётся конфликт. При одном исходном паттерне + его точном семантическом дубле смена запроса сама по себе не создаёт конфликтующий output.

**Исправление:** Указать отдельный frozen negative-control state: к тому же исходному P1 добавляется P3 с теми же LearnedSignals/Confidence, но действительно другим полным semantic payload. Для положительного повтора используется P2, для отрицательного P3. Сохранить ordered (caseId,stateHash,request) manifest; бюджет 16 Translate можно сохранить, если четыре case rows явно ссылаются на нужный state.

**Первичная опора:**
- [43.SmartSkillsDomain/PromptToTaskMachine.cs:263–300](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L263-L300): Continue добавляет переданный pattern в состояние.
- [43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs:175–208,233–249](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L175-L208): Конкурирующие outputs выбираются из patterns состояния, а не создаются запросом.

**Граница:** Не опровержение PT-01: source/formula и существующий Python conflict control корректны. Уточнение нужно будущему исполнителю native протокола.

### REV-H03 · LOW · 02A-NEW-SEGMENT-AGGREGATION-GAP.md:36,49,59

Различный confidence назван настоящим/реальным конфликтом. Это доказанный конфликт текущего carrier equality contract, но само различие уверенности для двух разных входов ещё не доказывает смыслового противоречия.

**Исправление:** Заменить формулировки на «конфликт по текущему контракту (различный confidence)» и сохранить его отрицательным контролем узкого B. Политику допустимости разных confidence не менять и не объявлять решённой этим опытом.

**Первичная опора:**
- [21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:738–752](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs#L738-L752): Confidence.Equals является отдельным обязательным условием equality.

**Граница:** Не требует нового алгоритма объединения и не меняет подтверждённую коллизию разных source IDs.

## Результат по разделам

| Раздел | Вердикт и основание |
|---|---|
| `01-MECHANISM-CROSSWALK.md` | **SUPPORTED_WITH_BOUNDED_SECONDARY_COVERAGE**. Каталог честно отделяет KNOWN от шести новых цепочек. Old/current Backward первично подтверждён. Все старые ссылки не являются новым полным аудитом их runtime. |
| `01B-KNOWN-UNIT-TRANSFORMATION-GAPS.md` | **SUPPORTED_AS_KNOWN_CONDITIONAL_RISKS**. Cap/fusion, generic stored output, corrected keys/RuleTags, compacted-token fallback и morphology helper соответствуют исходникам; текущая применимость и masking явно оговорены. |
| `02A-NEW-SEGMENT-AGGREGATION-GAP.md` | **CORE_MECHANISM_CONFIRMED_CLARIFY_PROTOCOL**. Producer ID и target-key equality несовместимы в указанном learned seam. Уточнить downstream source carrier и термин confidence conflict. |
| `02B-NEW-CANDIDATE-ELIGIBILITY-GAP.md` | **SUPPORTED**. Source ordering и bound .295 < .44 подтверждены. B сохраняет full-corpus raw scores, исключая IDF/fusion confound. Неподключённый learner не назван обучением. |
| `03-NEW-PROMPT-PATTERN-GAPS.md` | **CORE_MECHANISMS_CONFIRMED_CLARIFY_NEGATIVE_STATE**. Append→margin0 и substring gate→missing_gates подтверждены; правильные authority/fallback/semantic quality границы сохранены. Требуется явный negative-control state в native протоколе. |
| `04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md` | **SUPPORTED_AS_P2_CONDITIONAL**. Single Domain→cap1 и replay commit before candidate reject подтверждены. Унаследованность и отсутствие non-test composition отделены от RP01. Lesson validity не отождествлена с adapter promotion. |

## Сопоставимость, budget и consumer

- KU-02 корректно фиксирует full-corpus raw scores в A/B: перестановка eligibility и relative floor не смешана с пересчётом BM25 IDF, max normalization или corpus order. Аналитическая граница не названа native score.
- PT-01 сравнивает полный semantic payload, LearnedSignals, SplitId и Confidence; IDs и accepted provenance остаются отдельно. PT-02 учитывает authority extraction и evidence-only quarantine. ASCII proxy 2/6→6/6 не распространяется на Unicode или весь API.
- AUT-WEIGHT фиксирует одинаковые предъявления, state/optimizer/RNG и различие weighted-loss contribution. Нормированные веса и built-in Weight=1 сохранены как контрсвидетельства; веса 4 сами по себе не названы гарантией эффекта AdamW.
- AUT-REPLAY различает качество урока, technical training success и candidate quality. Малая ёмкость 5 — выбранный профиль, default 4096 указан отдельно. Во втором цикле A/B получают ровно 7 supervised updates. Общий бюджет с подготовительным первым циклом и экзаменом следует явно свести в 05; слово «7» не должно выглядеть полной стоимостью двухциклового опыта.
- Условные recipients названы: resolved learned multi-router, restricted edge provider, loaded выбранный PromptToTask экземпляр, живой автономный composition/buffer. Ни одна из этих новых цепочек автоматически не приписана isolated SimulationDpo RP01.

## Новизна и доказательства проверки

Самостоятельно объединены 389 planning + 272 protected core Markdown и 12 refresh файлов с заменой двух старых путей: **671 уникальный путь = 399 planning + 272 core**. По точным владельцам и формулам KU-01/KU-02/AUT совпадений нет. По PT найдены соседние N52 asset/readiness и Wiki margin/calibration; точного append→semantic duplicate→top2 margin0 либо generic gate→missing_gates механизма не найдено. Близкие общие top2/duplicate/replay/rare-class темы не объявлены вновь открытыми. Все совпавшие excerpts просмотрены; полный содержательный аудит 671 документа этим не заявляется.

Ledger: `final-review-novelty-search.json`. Независимо сопоставлены локальные байты 27 primary source copies с записанными Git blob SHA: **27/27 совпадений**, с явно записанной транспортной нормализацией там, где она требовалась (`final-review-source-byte-checks.json`). Это проверка byte identity, а не 27 native запусков. Адресные участки producer/consumer/формулы перечитаны; полнота чтения всего каждого файла не заявляется.

Сохранённые Python probe/receipts прочитаны как source/formula evidence. В этой стадии не запускались C#, обучение, модели, API потребителя или новые proxy-прогоны; результаты чужого probe не переименованы в наш native результат. Remote mutations и изменения candidate: 0.
