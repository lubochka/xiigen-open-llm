# Финальная policy delta: core 256f603e

**Cutoff: 256f603e70032c11b1f4ffabbdd1fefdab642ea5**, 21 октября 2026. Проверены только пять файлов: индекс, контракт 30, N63I, N63R и N63C. Runtime, protected, original host/SourceBridge и финальный пакет не изменялись. Новая probe проверяется отдельно. Полного корпуса и новых HEAD здесь не проверяли.

## 1. Порядок сохранён; счётчики уточнены неравномерно

**Все 47 из 47 ordered tuples `(номер, ID, относительная карточка)` полностью совпали с d962f958.** NEXT остаётся **№10 PARENT-ADOPTION-01 → №11 N63-INGRESS-LENGTH-LEARNED-01 → №12 N63C**. Последние core ID: №43 SC01 → №44 IDX → №45 SEXP → №46 IDENTITY → №47 KN. [Индекс 7–15][index-top], [строки 79–83][index-tail].

Полная машинная сверка: [core-final-policy-id-parity.json](core-final-policy-id-parity.json). SHA256 канонического UTF-8 списка `NN<TAB>ID<TAB>card<LF>`: `c727ae8230ba89ea5ca04eb72b9297c0b8897af0ef648a7d8b988cc13c4d863f`. Совпадение списка не означает совпадение текстов статусов.

**49-entry dispatcher d962 сохраняет правильную последовательность** с SILU/HNSW после SEXP и перед consumers. Пересоздавать список не требуется; источник d962 сохранить, приложив эту сверку с 256. Внесённое состояние N63C `partial A / objective OPEN / preserve claim / no blind rerun` теперь подтверждено самой строкой №12. [Индекс 48][index-n63c].

Свежий индекс содержит внутренний конфликт:

- L7 сохраняет старые **9 saved / 38 unused**.
- L11 и L15 уже указывают **10 сохранённых/частичных попыток и 37 невыданных проектов**, включая partial N63C в числе 10.
- L48 делает N63C consumed/open; переход к №13 разрешён только по прямому NEXT или допустимому независимому переходу.

Foreground может цитировать уточнение L15 с оговоркой: это сведения индекса, не новый полный аудит всех runtime claims. Вместе с двумя предложениями остаётся 49 ID. По этой разметке 39 позиций ещё не имеют известной выданной попытки; 40 открытых документных позиций дополнительно включают partial N63C. Нельзя называть все 40 неиспользованными attempts.

## 2. Два локальных useful-gain пропуска закрыты

У **N63I и N63R теперь есть локальный обязательный блок**, распространяющийся на их промпты. L12 содержит полезный gain как успех своей метрики/домена, необязательность роста accuracy, показ регрессий, default model, русский отчёт, отсутствие runtime authorization и немедленный STOP/NEXT. Новое условие — «в валидном учебном сравнении». Это уже не отсутствие локального правила. [N63I 3–15][n63i-rule], [N63R 3–15][n63r-rule].

**Историческое 169/171 не переписывать.** Свежая адресная проверка установила исправление ровно двух прежних пропусков; вывод 171/171 для всего текущего корпуса не делался.

Условие валидности нельзя использовать, чтобы стереть фактически наблюдаемые полезные NLL/rank/margin изменения настоящего обучения. Их можно сохранить как узкий результат соответствующей оси с регрессиями и пределами вывода. При этом 0 правильных ответов, отсутствие B или непоказанный before не закрывают acquisition/retention и не доказывают A/B эффект. Контракт 30 сам разделяет execution, learning_validity и hypothesis_status. [Контракт 44–56][validity].

**Старый конфликт NEXT N63R остаётся:** L17 направляет к N63C №12, пропуская PA01/N63IL. Действующая глобальная очередь имеет приоритет. [N63R 17][n63r-route].

## 3. N63C: статус поправлен, таблица по-прежнему неполна

Карточка теперь обозначает **ATTEMPT_CONSUMED / PARTIAL_A / LEARNING_OBJECTIVE_OPEN / CORRECTED_DESIGN_REQUIRED**. Исторические команды больше не являются pending entry. Достраивание B, results-02, warmup/probe/retry запрещены; текущая работа документная. [N63C 3–15][n63c-status].

Документная правка описывает отдельный RNG путь `sft-state/BuildFileName(snapshot.RandomStateStep)` и независимые issued/returned counters. Она не меняет сохранённые raw/host и не разрешает запуск. [N63C 130–132][n63c-repair].

**Конфликт таблицы остаётся:** L171 требует C/T/E/N/M, а L189–193 показывают только margin/chosen NLL. Additive таблицу из нашего пакета оставить. [Требование 171][n63c-fields], [шаблон 189–193][n63c-table].

L210–216 требуют для retention полного ответа S1 с 23/23 правильными targets и E=true; положительный margin или отдельные правильные токены недостаточны. Совместимый qualified snapshot с receipts пока не предъявлен; исправленный дизайн явно CORRECTED_LEARNING_DESIGN_NOT_CLOSED. Это не поручение executor искать модель или автоматически назначать 60/600 steps. [N63C 210–216][n63c-qualified].

## 4. Recovery 30 не выдаёт разрешения на runtime

**Прямого конфликта с нашим documentation-only/no-rerun нет.** L3 прямо запрещает понимать контракт как разрешение запуска, повтора, обучения, изменения продукта или чужого процесса. После двух пустых наблюдений назначается ограниченная диагностика уже выданной операции. Claim, partial или неизвестный расход сохраняют identity попытки. [Контракт 3–20][recovery].

L20 допускает исправление точной entry-ошибки только при доказанном отсутствии issued attempt и любых model/load/Train/Forward **в уже разрешённой исполнительской фазе**. У нашей документной сессии такой фазы нет. При issued attempt новый рецепт сам по себе не разрешает повтор. L54 запрещает выводить retry из невалидности; L62 отдаёт приоритет live STOP/NEXT даже при открытой цели. [Контракт 54–64][recovery-end].

Repo-правила не расширяют пользовательскую авторизацию. Допустимы additive документы в planning; protected core остаётся read-only. Указания индекса L24 и N63C L17 писать в protected required-changes не отменяют прямой запрет пользователя. Нового approval-вопроса, runtime gate или поручения повторять исходы из этой сверки не возникает.

Для публикации достаточно cutoff-поправки, актуального статуса N63C и адресного уточнения, что два локальных rule-пропуска исправлены на 256. Старые 169/171, d962 dispatcher/source SHA и pinned reviews сохраняются как исторические доказательства.

[index-top]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L7-L15
[index-tail]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L79-L83
[index-n63c]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L48
[n63i-rule]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-ingress-length.md#L3-L15
[n63r-rule]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L3-L15
[n63r-route]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L17
[n63c-status]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L3-L15
[n63c-repair]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L130-L132
[n63c-fields]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L171
[n63c-table]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L189-L193
[n63c-qualified]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L210-L216
[validity]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md#L44-L56
[recovery]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md#L3-L20
[recovery-end]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md#L54-L64

