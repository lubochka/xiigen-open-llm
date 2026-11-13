# Точечная сверка P54 / P52 / P106 на core6e3e886

Дата: 20.10.2026. Последний прочитанный core — `6e3e8864933e35fccdb4f21369c9912fa195aaf6`; сравнение с исходным snapshot `66947785d1318f7cbbc88118f3156cdc75001e16`. Прочитаны только три изменённые карточки и их diff. Runtime/build/tests не выполнялись; вывод о готовности остальных470подслучаев не делается.

## P54: семь NEEDS_INPUT остаются актуальными

**Новый diff P54 не закрыл ни одного из семи недостающих рецептов.** Единственное изменение относительно669 — строка114: локальный указатель `№06 E-OPT-STEP` заменён на `№07 LF-03`. Это не добавление checkpoint, host, fixture или command. [Текущая строка114][p54-next].

Актуальная карточка прямо сохраняет `P54.1–P54.7 ... NEEDS_INPUT_FOR_RECIPE`, `Host, project и команды ... ещё НЕ назначены` в [строке21][p54-state]. При этом она уже корректно **допускает** reader опыт на подтверждённом learned checkpoint без лишних Train [19–25][p54-state]. Поэтому нельзя писать ни «P54 исключены из обучения», ни «P54 уже готовы к запуску».

| Подслучай | Что ещё требуется по latest6e | Строка |
|---|---|---|
| P54.1 сегментация | Реально обученный checkpoint с пересекающимися phrases, контрольными word maps и no-exact запросами | [45][p54-1] |
| P54.2 original index | Learned phrase/fallback maps и реальные token sequences, где фильтрация создаёт сдвиг; нет exact перехвата | [55][p54-2] |
| P54.3 обязательность | Partial learned phrase coverage и независимый разрешённый supplier mandatory mask | [65][p54-3] |
| P54.4 boundary | Learned multiword phrase, actual tokenizer output разделителя и документированный смысл boundary | [75][p54-4] |
| P54.5 word-map omission | Подтверждённые word entries, отсутствие exact/phrase перехвата, mask и downstream fallback contract | [85][p54-5] |
| P54.6 semantic rerank | Один общий fixed composer/candidate list, learned checkpoint и разрешённый action-object supplier | [95][p54-6] |
| P54.7 word→kNN | Checkpoint/query с обоими достижимыми путями, source meaning checker и kNN eligibility trace | [105][p54-7] |

P54.6 зависит от одинакового composer вA/B и не разрешает объединить генерациюP54.1 с rerank одним фактором [89–95][p54-6]. P54.7 сохраняет `UNKNOWN/COMPATIBLE` на прежнем return, только `CONTRADICTION` допускает старыйkNN [99–105][p54-7]. Эти ограничения не изменились.

**Для общей очереди:** строка114 P54 — локальный текст карточки, не независимое подтверждение текущего NEXT. Если latest execution index/primary receipt уже завершили LF-03, этот локальный указатель следует считать отстающим. Не назначать по нему повтор. Индекс и LF-03 receipt проверяет отдельный coverage worker.

## P52: уточнение получателя уже внесено

Новые [строки3–5][p52] различают native neural bridge и structured-memory builder. Зафиксировано, что RULE/PHRASE/AGREEMENT из rejected могут быть законным supervision для positive unit JSON; три component examples одной пары коррелированы. Указано уже существующее очищение WRONG и PHRASE/AGREEMENT в нейронном baseline P52.5; blanket-delete и утверждение «WRONG всегда в model bytes» исключены.

Следовательно, отчёт должен отметить **APPLIED_SOURCE_CLARIFICATION**, а не требовать заново внести это разграничение. Это source-level исправление постановки, не выполненный эксперимент. Для подслучая без конкретного producer/annotation contract/writer/query consumer та же новая строка3 сохраняет `NEEDS_INPUT_FOR_RECIPE`.

## P106: producer gap отделён от способности Unit

Новые [строки3–5][p106] уже объясняют: текущий builder передаёт Pattern/signature собственного rule tokenizer, тогда как Tokenizer Unit способен принять другой chosen JSON. Отсутствие независимой исправленной цели у producer **не доказывает необучаемость Unit**. Это разграничение уже внесено и не является pending document fix.

Literal corrected JSON, origin/supplier, consumer и fixed before/after panel пока автором конкретного опыта должны быть назначены; без них карточка прямо остаётся `NEEDS_INPUT_FOR_RECIPE`. Никакой новыйID/run или подтверждённый эффект этой правкой не создан.

## Прочитанные blobs

Все три snapshot проверены вычислением Git blob SHA по фактическим bytes; diff выполнен после удаления только транспортом добавленного завершающего newline.

- P54: `e3b8ef4a05b23547d381d2df3179cda59624dbc2`.
- P52: `4368523cfcf114280d38b17bca162d6591276d18`.
- P106: `7b412f455cc6ba2f1af823948a28d789f8653ea5`.

Изменение этих Markdown не меняет ранее зафиксированные условия опытов и не требует их повторения. Утверждение «все470рецептов готовы» из этой адресной сверки не следует.

[p54-state]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L19-L25
[p54-next]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L114
[p54-1]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L37-L45
[p54-2]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L47-L55
[p54-3]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L57-L65
[p54-4]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L67-L75
[p54-5]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L77-L85
[p54-6]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L87-L95
[p54-7]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L97-L105
[p52]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P52-semantic-annotation.md#L3-L5
[p106]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P106-corrected-semantic-target.md#L3-L5
