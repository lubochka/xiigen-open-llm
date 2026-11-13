# Согласование параллельной публикации9d311 перед нашим commit

При финальной проверке HEAD planning изменился с6bc4b6f на **9d311be43f4e108f6fa1cda1774549d54941c3c5**. Новый [пакет63](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/README.md) состоит из40 добавленных файлов и добавочной секции прежнего README; удалений нет. Его20 Markdown прочитаны до нашей публикации. Core остался883496, protected tree41587e4a86b38928e4f4d642a8f1a0f2d35bfcf1.

## Что реально изменило наш вывод

1. **SKNN больше не новый.** Canonical [AST-SIB-CACHE-01](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md) и наш SKNN устраняют один и тот же повторный обход siblings перед Structural kNN Train. Исходный StructuralKnn blob совпадает. Различие facade/TrainFromSource, fixtures и budgets не создаёт второй новый фактор. AST сохранён в очереди, SKNN остаётся reference design без отдельного запуска. [Независимое сравнение](reviews/concurrent-SKNN-reconciliation.md).
2. **Сохранены ROPE и NG01.** Они не вытесняются нашим пакетом. Приоритет пользователя относит reader16 IDENTITY после direct learners; latest63 уже исправил это, и новый единый реестр сохраняет перенос.
3. **Единственный новый отдельный фактор — BPE-REALIZED.** Overlap counting уже известен и явно охраняется weighted-chunks как прежняя семантика. Новое — отдельный малый опыт альтернативной цели обучения, со всеми проигрывающими профилями рядом. Поиск20 новых Markdown63 по CountPairs/realized/non-overlap/перекрывающимся не нашёл такого отдельного опыта. Это ограниченная новизна на767 planning и334 protected Markdown; неизвестные refs не исключены.
4. **Выходные пути не задвоены.** Сохранены28 актуальных R/entry из63/14 и roots ROPE/AST/NG01; добавлен только BPE root. Оговорка host-owned claim/blueprint предотвращает преждевременное создание results. В5 skill addenda внесены те же roots. Старые protected документы не меняются.

## Единственная будущая программа

05–15 → 17–31 → S01 ROPE → S02 AST → S03 NG01 → S04 BPE-REALIZED → исходный16 IDENTITY → исходный32 KN-ROUTER → END.

36distinct rows,4completed,32pending,NEXT05 N62-R2. SKNN не создаёт37-й row. Исходные номера16/32 сохраняют source identity, не обозначают положение раньше direct learners. Прежние locally drafted32a/32b/33 не являются второй действующей очередью. Для одиночного ID END после назначенной фазы; новый реестр не запускается сейчас.

Числовые результаты01–04 не переснимаются; новыйTRAIN protocol не приписывается старым raw. Первичный RP01 readback, текущий DPO→Units, доказательства334/334 blob SHA и новая BPE-карточка добавляют проверяемые сведения к уже опубликованному63. Совпадающие сведения помечены как подтверждение, а не новый алгоритмический дефект.

## Границы независимого чтения

У пакета63 есть собственные утверждения о прочитанных raw. Наш аудит для N-KU/frozen/BPE сохраняет более узкое фактическое происхождение: первичные report прочитаны, полученные нами comparison/часть arm Git blobs —LFS pointers. Мы не присваиваем себе чужое independent raw parsing. N62A/B, исторические E09/E22/E25/E43 JSON и первичные summaryRP01 прочитаны нами с обозначенными границами. Это разные уровни доказательств, не повод повторять обучение.

Публикация создаётся поверх свежегоHEAD с expected_sha lease и без force; все изменения ограничены новым дополнением и добавочной секцией possibleDifs README. Существующие63 и прежние experiments не переписываются/не удаляются. Никаких production/weights/CI/protected изменений или новых learner calls.
