# Независимый адресный review пакета learning-evidence-827a1acd

Рецензент: experiment_audit, текущая модель сессии. Фаза: статическое чтение документов и JSON; новых build/runtime/Train/Generate нет. Проверка относится к собранным 01–06,08,CANDIDATES,registry/output maps, двум новым карточкам и адресным source/receipts. README/07 ещё дополнялись во время первоначального просмотра. Не заявляется повторный полный семантический review всех157 core cards/133embedded prompts.

## Существенные замечания

### R-AUD-01 — служебные подписанные URL в candidate publication

В новом evidence/protected827-prompts-metadata.json найдены два HTTPS download URL с token query. Это локальный retrieval manifest, не публикуемое доказательство. Содержимое токенов не выводилось и не включено в этот review.

**Адресное исправление:** исключить этот новый служебный файл из пакета либо оставить только path и sha. Старые эксперименты при этом не затрагиваются. В остальных JSON пакета при адресном сканировании подписанных token URL не обнаружено. Root уведомлён немедленно; перед публикацией нужен один адресный readback отсутствия чувствительных query-полей.

### R-AUD-02 — KN-ROUTER overlay должен быть буквальным

03-experiment-validity-audit описывает правильное окно, но первоначально не задаёт готовой C# замены. Это оставляет исполнителю проектирование observer, хотя пользователь требует определённый рецепт.

**Готовое исправление:** work/KN-ROUTER-reader-observer-overlay.md. Добавить его в пакет как09-KN-ROUTER-reader-observer.md, сослаться из04/05 и соответствующей строки output map. Там приведены полная замена local Read, два callsite и raw declaration на основе current827 L145–177; оба arms получают одинаковый observer. Измеряются прежние четыре RouteAsync+row capture; process-wide approximate allocations корректно подписаны для async. Остальные Train timers сохраняются. Бюджет остаётся2Train/16Route, дополнительных readouts/Score/Train не возникает.

Это документный ремонт будущего рецепта, не новый ID и не разрешение повторить старую работу.

### R-AUD-03 — равенство optimizer artifacts не выдавать за повторно прочитанный binary payload

01/08 корректно ограничивают исходный E-OPT claim малой сохранённой панелью. Дополнительное уточнение желательное: A/B optimizer checkpoint tree refs совпали на обоих шагах, но соответствующие blobs размером131bytes — LFS pointers; внутренние binary payloads этим аудитом не десериализовались. В отличие от двух comparison.json pointers, эти binary pointers вообще не входили в materialized414 count.

**Адресное уточнение:** «Совпадают записанные состояния/метрики и ссылки на optimizer artifacts; полный binary payload chain заново не реконструировался». Это не отменяет recorded allocation gain и не назначает новое обучение. В пакете уже есть общая LFS/binary custody оговорка; существенного числового противоречия не обнаружено.

## Что подтверждено

| Проверка | Результат |
|---|---|
| Registry |38уникальных ID, позиции1–38, корректная NEXT-цепочка доEND |
| Завершённые |Первые6COMPLETED_SAVED_NO_RERUN; E-OPT не повторяется |
| NEXT |LF-03, позиция07 |
| Сохранение актуальной очереди |Первые34core827ID в прежнем порядке; новыеATTN/JSON35/36; readersIDENTITY/KN37/38 |
| Alias |SKNN-SIBLING-PREP не добавлен вторымAST |
| Output map |32futureIDs ровно соответствуют registry future; все30унаследованных R совпадают latest827map |
| Новые текущие пути |AST01 иBR01 сохраняют внутренние Run-Assigned; нет возврата к старым planning/C:/xg roots |
| Исторические результаты |N-KUregression,transpose retained+CPU,BPECPU,R1/R2GC/peak,E-OPTGC1/2+peak показаны |
| E-OPT source descriptor |Ошибка R1labels отделена от фактического sourcefactor; raw не переписан |
| Исторические гипотезы |Малый margin gain признан полезным, нулевая generationaccuracy не названа отсутствием обучения |
| Новые факторы |ATTN/JSON явноNOT_RUN; нет вымышленных измеренных успехов |
| P54 |Семь missingrecipe inputs названы адресно,не объявлены готовыми |
| Protected scope |Текущий контракт запрещает core/protected/product/test изменения; candidateoutputs внеprotected |
| STOP/NEXT |Явные немедленные приказы вышеreport/review/hash/retry; старыеpartial сохраняются |

Машинный receipt этих structural checks: work/package-review-static-checks.json. Это проверка JSON/Markdown структуры, не host compilation или runtime test.

### Числа, проверенные независимо

E-OPT allocations3764994488→3764526544, delta−467944 (−0.0124288097%); wall50068.549→40665.43ms; CPU37687.5→35828.125ms; GC[193,19,13]→[189,14,10]; peak840916992→1045463040 (+204546048,+24.32416635%). В01/08/CANDIDATES эти значения и ограничения представлены согласованно. Fixed-order time/CPU считаются observed полезными изменениями, а не универсальным причинным ускорением.

N-KU actual arm JSON подтверждаютA0/B0null, A1ADD_LOOP4/4, B1ADD_METHOD4/4, снижение лишнегоloop3→0 и потерю необходимогоloop1→0. Gold rubric не превращён в доказательство полного semantic качества. BPE counts172→87 и192→87,64merges,roundtrip4/4 и samefinalstate — отдельные выводы от ценыallocations; downstreamaccuracy не придумана.

### Границы окончательного readback

Ремонт JSON runner/receipt обвязки и уточнение05timerboundary уже выполняются авторами по отдельным review. Они не дублируются здесь как новые находки. После интеграции буквального KN overlay, очистки подписанных URL и завершения README/07 достаточно одного адресного readback затронутых документов. Полная production цепочка, дополнительный native эксперимент, повтор научной панели или новый поиск модели не нужны.

