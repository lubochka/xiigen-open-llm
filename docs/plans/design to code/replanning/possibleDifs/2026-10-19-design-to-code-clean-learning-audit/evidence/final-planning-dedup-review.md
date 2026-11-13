# Финальная сверка исторических метрик и новых публикаций possibleDifs

Дата: 19 октября 2026. Reviewer: recovery_catalog. GitHub mutations: 0. Новых чтений защищённого core каталога через GitHub: 0.

## Свежий planning snapshot

- Исходный llm_plan3: `b0308fbaa783758104987e6544d057062ccf6fc3`; possibleDifs tree: `a0a728e11b3e2276e696fe0e8811a95d88beffbf`.
- Повторно проверенный HEAD: `b95278b25a6697b42ef7025e6965a9f2837a34b7`; root tree: `2b083b28dcf605d0f7655a1a21fc9bd84a1feb9a`.
- possibleDifs tree: `1f9118b728806f3478ca491827a718f3008d1033`, 720 entries, non-truncated.
- От первоначального snapshot: 71 новый/изменённый blob path; 33 Markdown path, из них 31 добавлен и 2 изменены (README и 00-MASTER). Удалённых путей нет.
- Все 33 версии Markdown прочитаны для полнотекстового поиска и адресного semantic dedup. После первого refresh `82c37d...` изменился только 00-MASTER; его новый текст также прочитан. Итого 34 поздние версии, Git blob SHA совпал у 34/34.
- Каталог на последнем snapshot: 446 Markdown path. С учётом первоначальных 415 planning текстов, 34 поздних версий и 92 ранее кешированных protected текстов исследован полнотекстовым поиском корпус 541 версии. Это не утверждение о ручном независимом построчном аудите всех документов. Остальные 182 protected документа не дочитаны.

Полный список 71 delta path и конкретные ссылки для решений: [planning-late-dedup-receipt.json](planning-late-dedup-receipt.json). Квитанция точности: [planning-late-final-integrity.json](planning-late-final-integrity.json). Исходный README в локальном кеше и на GitHub этой проверкой не изменялся.

## Поправки к новизне

| Находка нашего пакета | Решение после позднего чтения | Основание |
|---|---|---|
| DC-01a — phantom generic из отсутствующих полей | BOUNDED_NEW_REFINEMENT, точная корректировка опубликованного утверждения | Peer N54-00B ошибочно пишет, что parser даёт пустой список. Точная цепь ReadCorrectionRuleIds → NormalizeCorrectionRuleId(empty) отсутствует в проверенных текстах |
| DC-01b — canonical generic/whitelist/import | BOUNDED_NEW_REFINEMENT | Peer уже заметил permissive → strict whitelist и потерю pair_family/hash_diversity. Конкретные generic underscore, accepted clean → rejected current, целостный отказ StageLearnedWeights и collision при alias migration там не описаны |
| DC-02 — all-negative preference → fallback all | KNOWN_LATE_REFINED | GDC-A2 уже содержит arm D с нулевыми/отрицательными весами и прогнозом выбора двух правил. Интерпретация как игнорирования отвергнутого действия полезна, но не новый механизм |
| DC-03 — learned top-1 cliff | KNOWN_LATE_REVALIDATED | N54-01 и GDC-A2 уже описывают тот же исходник, тот же contrast/overflow counterexample и inherited статус |
| KU-01 — уничтоженный известный stem/concept | BOUNDED_NEW | В поздних текстах есть corrected-key/RuleTags mismatch, но не TrimVerbSuffix → BaseConceptsRu key loss |
| KU-02 — VERB всегда PAST | BOUNDED_NEW | Точный механизм в проверенном корпусе не найден |
| KU-03 — raw Word gender против cleaned POS | BOUNDED_NEW | Поздний known Unit crosswalk описывает иной compacted-index и иной DeriveGenderAlternateForm noun alias |

Сохраняются **четыре семьи с ограниченной новизной**, а не шесть новых механизмов. DC-01a и KU унаследованы; DC-01b содержит реальную разницу в правилах совместимости между clean/current. Реальный checkpoint, получивший такой import failure, пока не предъявлен.

## Проверка главы 00, README и 04

Метрики и P/C0/T согласованы. CodeTrace019 49/50 явно ограничено reference-relative pair ranking, не free generation. Batch019 продолжает018; исторические epochs/LR/runtime не названы текущим измерением. LFS pointers не выданы за загруженные веса. Соседние064/134/075 не приписаны clean. E25/E26/E09/E43 разделены по механизму/знаменателю, E26 обозначен преднаполненным lookup.

После замечаний исправлены две формулировки: единственный отрицательный margin ограничен 50 heldout строками; необоснованное pretrained-compatible заменено восстановленным совместимым обученным parent. В README проверена итоговая формулировка о четырёх новых семьях и двух поздних известных уточнениях.

P = замороженный parent; C0 = тот же parent после того же load/eval без update; T = одна интервенция. R0-SNAPSHOT/R0-INPUT/R0-LEARN — стадии, а не переопределение control arms.

Хеши трёх проверенных документов и границы review: [final-metric-plan-review-receipt.json](final-metric-plan-review-receipt.json). Native training, C# compilation, consumer quality gain и cold restore в этом review не запускались.
