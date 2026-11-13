# Аудит после опыта88: исполнение переносов и точность результатов

<!-- POST89-EXECUTION-AUDIT-2026-11-01:START -->
## Актуальная проверка после P89

[Новый аудит и границы публикации](2026-11-01-POST89-EXECUTION-BOUNDARY-AUDIT.md) / [core116](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/116-post89-execution-boundary-audit-2026-11-01.md) уточняют прежнюю сводку. Исправлены 45 строк исполнения и три входа продолжения; актуально 92 recorded/34 design из 126 Q на проверенной опубликованной ветке. Дополнительно усилены 20 карточек; исходные 53 относятся к прошлому пакету. Данные опыта и состояние продукта не менялись; протоколы новых маленьких audit probes указаны отдельно. Четыре локальных заметки теперь явно ограничены своими source snapshots и не подтверждают отсутствующие remoteQ127–Q130.
<!-- POST89-EXECUTION-AUDIT-2026-11-01:END -->


1 ноября2026. Проверены core `dbf4c99c519b76485f618f4a2043d7df024db88e` и planning `ce88e4fd86ab66644a0caf3558cd98ab81d9635b`. Это актуализация утреннего аудита, не повтор его113 Python checks и не новая scientific очередь.

## Поздняя сверка публикации

Основание публикации core продвинулось до b4b09bb5. P83 уже имеет source repair2df183ae; полные native/ordinary проверки не закрыты. P89 имеет3/3 updates и неизменные2/4 answers, но фактический Signal13/12 отличается от назначенного2, а fresh optimizer0 — от заданного restored400. Исходная серияP82–P88 остаётся отдельным доказательством; новый вариантP89 не перезапускается автоматически. [Позднее дополнение](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-01-post88-transfer-audit/publication-late-addendum.md).

## Результат

Шесть новых правок переводчика присутствуют в исходниках: целое слово P56, source guard P62, переход из непригодного tier P64, подтверждённый контраст P65, композиция фраз P54, chosen provenance P52. Расширены imports всех project paths, проверки HNSW и диагностика двух convergence callers. У этих ремонтов остаются точные границы: effective ProjectReference properties, cross-node topology, eligibility до cap, обычный consumer и полный native gate.

Свежий аудит выполнил0 обучения,7 Python oracle runs, native source-linked27 checks0fail и отдельно воспроизвёл2 topology defects. Настоящий PowerShell SourceClosure пропускает Conditional.cs, нужный реальной сборке при AdditionalProperties. Полное bound execution на Linux не аттестовано: parser imports там fail-closed. Прежний legacy completed bypass подтверждён на fixture без child launch.

Независимо пересчитаны P82–P88:214 pinned files/173 blob IDs. Новое: P85 Train5 при записанном1; P82 не сохранил EXAM vectors; P88 не сохранил tokenizer identity. P87 human report уже исправлен до4 sensitive families, machine result ещё5. Все6 preflight original hashes P88 уже опубликованы — прежний dirty-source blocker снят. Raw не переписаны; correction receipts привязаны к hashes.

## Канонические документы

- [113 — доказательства и текущая оценка переносов](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/113-post88-transfer-and-experiment-audit-2026-11-01.md).
- [114 — адресный план RF01–RF11 и новых кандидатов](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/114-post88-addressed-repair-plan-2026-11-01.md).
- [115 —53 адресно усиленные карточки](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/115-post88-card-hardening-and-coverage-2026-11-01.md).
- [Воспроизводимые данные аудита и corrections](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/experiments-v2/_audits/2026-11-01-post88-transfer-audit).

## Что меняет выводы planning

| Семейство | Уточнение |
|---|---|
| Split/exposure/codec | P82 A42/54,B38/54; целые файлы не улучшили этот набор. Missing EXAM vectors не разрешают объявить0 overlap. P83 все4 EXAM совпадают с TRAIN по признакам. |
| Method features | P83 expression body перенесён2df183ae после основной сверки; native ordinary consumer и target-side panel ещё не приняты. P84 lambda normalization/arity остаются кандидатами. |
| Composite obligations | P85 наборы2/4→4/4, но actual k5 label и obligations расходятся4/4. До переноса нужны согласованная policy и consumer. |
| DPO objective | P86 подтверждает текущую производную12/12, full answers0/4. Zero-mask defect нельзя приписать guarded exact entry по private-helper probe. |
| Atomicity | P87 подтверждает early profile rejection,4 чувствительных семейства; поздний rollback не проверен. RF01 full translation transaction остаётся. |
| Context consumption | P88 prototype3/6→6/6 и EXAM1/2→2/2; интеграция ordinary T34 не исполнена. Имя BpeTokenizer не является state identity. |
| Routing | ADR принят: owned predicate dictionary/unknown fraction и явный cold fallback. Необходимый измерительный паспорт/продуктовое подключение ещё не исполнены. |

53 карточки core получили индивидуальные дополнения:9 непосредственно связанных +P82–P88 +P89–P125. Все37 в последнем диапазоне охвачены, P125 остаётся исполненным alias. Старые PRIMARY/NEXT/budgets сохранены; число271 каталога не изменено. Это уточнение существующих заданий, не53 новых запуска или заявка на полный аудит561 entries planning.

Первыми ремонтировать фактический binding/одно исполнение и machine accounting, затем topology и full batch transaction. Остальные RF сохраняются с владельцами и приёмкой в114. Новые native suites и продуктовые изменения этим документным пакетом не объявляются выполненными.

## Дополнение01.11: что ещё не было найдено в этом аудите

Новый адресный разбор — [116, доказательства и план ремонта](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/116-post88-deep-transfer-audit-and-repair-plan-2026-11-01.md), отдельный [review RF02/RF09](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/reviews/2026-11-01-post88-rf02-rf09-deep-review.md). Проверенная опубликованная вершина — `76c1276584ef7e34412497e5433e8571a0ec61d5`; локальный HEAD `2004e81030c` отставал. Translation owners и RF source/test файлы отдельно сверены с опубликованным содержимым. Предыдущие runtime-свидетельства выше не являются новыми запусками этого дополнения.

| Новый пробел | Почему прежнее положительное свидетельство не закрывает его | Адрес ремонта |
|---|---|---|
| Проверка P52 идёт до lossy normalization | `TENSE.PRESENT` проходит как другая семья и затем становится tense tag; `!!!` молча исчезает | 105/116-D1: одна канонизация до проверки и сериализации |
| Подтверждение P65 не связано с новой annotation | Digest четырёх полей не меняется при изменении VerifiedRuleTags, которые P52 использует в учебном адресе | 111/116-D2: versioned frozen DTO v2; явная legacy policy |
| P52 исправляет полную signature, но не согласует reader | Обученный Tokenize оставляет сырые RuleTags, строит prefix/structure без tags; Pattern accuracy это не различает | 105/116-D3: один learned representation, checkpoint и обычный consumer |
| RF09 принимает missing/нулевой construction beam | Следующий Add получает пустой набор соседей и разъединяет новое и старое знание | RF09/RFD-01: validation до instance, continued-Add oracle |
| Постоянный RF09 test перестраивает сохранённый граф | Случайные8 Add и assert Count/одного ответа не проверяют исходную двухуровневую навигацию | RF09/RFD-02: frozen topology без rebuild |
| RF02 записывает дополнительные входы без повторной сверки | `extra_sources`/`optional_inputs` отсутствуют в prelaunch loops | RF02/RFD-03: проверка status+sha объявленного material |
| Checker RF02 не связывает claim values | Наличие полей claim не доказывает согласие с reservation/state/terminal | RF02/RFD-04: value comparisons через существующую identity |

Отдельно открыт **RFD-05**: target-added Compile и реально использованный assets state могут не входить в pre-build evaluation closure. Конкретный двухсборочный рецепт записан, но здесь не выполнен; это не восьмой подтверждённый дефект. Известные AdditionalProperties, две topology ошибки, legacy terminal bypass, P82/P85/P87/P88 accounting не пересчитываются как новые находки.

Ограничения важны: ordinary KNN restore-мост через HnswIndex.Load в адресном поиске не найден; runner, фактически вызывающий ExtraSourcePaths, не найден; дефект checker не равен обходу launch admission. Размер потери обучения и цена будущего ремонта не измерены. D1–D3 относятся к опубликованному training material/обученному reader, но положительные2/2 tags или3/3 exact answers прошлых опытов не являются новыми native приёмками.

Усилены действующие планы03/105/111/114 и **семь карточек P18/P19/P21/P51/P52/P65/P88**, с конкретными контрпримерами, владельцами и будущими oracle. Указатель115 дополнен точным охватом. Семь карточек пересекаются с прежним пакетом53: это не60 карточек и не семь прогонов. Старые PRIMARY/NEXT, эксперименты и результаты сохранены.

Итог дополнения: **7 конкретных новых пробелов исходника/приёмки +1 открытый build lead;0 запусков тестов, сборки, inference или обучения**. Изменены только Markdown. Исследовательский worker `/root/late_positive_evidence_review` вернул отдельный RF review и сверил D1/D2, локальные поля D3; родитель проверил цепочку ordinary adapter и выполнил адресные документные правки. Это дополненный аудит и план, не выполненный ремонт продукта и не аудит всего каталога.
