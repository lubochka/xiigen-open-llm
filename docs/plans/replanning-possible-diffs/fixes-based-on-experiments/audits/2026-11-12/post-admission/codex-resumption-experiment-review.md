# Независимая сверка экспериментального пакета после восстановления связи

Дата: 12 ноября 2026. Проверяющий: `/root/experiment_package_review`.
Область: опубликованный пакет core `9dc7f71c775506ec7eb0d5186afec82ddada0444`, каталог `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission`.
Это проверка точности итогового ответа по уже опубликованному аудиту. Изменения последующих F03-коммитов рассматривает другой проверяющий.

**Три шага завершены:** прочитан пакет; независимо пересчитаны шесть исходных JSON; проверены границы итоговых утверждений. Новых C# запусков, обучения, Save/Load модели и изменений продуктового кода не было. Результат текущего независимого пересчёта сохранён в [experiment-recount.json](codex-resumption-experiment-recount.json).

## Вывод

Содержательные экспериментальные выводы опубликованного пакета согласуются с проверенными исходными строками. В итоговом ответе особенно важно сохранить две оговорки:

1. Семь оставшихся строк P58 имеют другие **буквальные Source**. Их независимость по aliases, model-visible key и source-family не доказана; называть их без оговорки «невиданными» или «нетронутыми» нельзя.
2. Числа 261/104 и 36/28 — условная цена исходного выбранного класса при одном полном успешном выполнении каждого Fact. Это не наблюдённый исторический расход, не native discovery и не число уникальных обучающих пар.

## Что выполнено заново

Через GitHub прочитаны шесть raw-файлов на закреплённом `f089a7d55d359ff103d0cdd9ddfb26eee346a16b`:

- P4801 attempt-0003 `arms.json`.
- P4901 attempt-0003 `arms.json`.
- P5201 attempt-0002 `before.json` и `arms.json`.
- P5801 attempt-0002 `before.json` и `arms.json`.

Для каждого независимо вычислены Git blob SHA-1, SHA-256 и число UTF-8 bytes; все совпадают с manifest. Затем выполнен самостоятельный JS-пересчёт строк. Сохранённый Python-скрипт не был запущен повторно. Дополнительно самостоятельно пересчитан double-precision контрпример loss/gradient.

На первом локальном черновом вычислении агрегата P49 была допущена ошибка проверяющего: Gold искался в observation, хотя находится в definition-панели. До сохранения результата расчёт исправлен соединением по RowId, согласно фактической структуре raw. Неправильные нулевые значения не опубликованы как результат аудита.

## Проверенные результаты

| Область | Что получилось | Правильный смысл |
|---|---|---|
| P48 | 12/16 у обоих плеч; 6 изменений; 3 gains и 3 losses | Равенство агрегата не означает отсутствие изменений поведения |
| P49, EXAM cold | 2 exact-pair seen, 1 input-seen/target-new, 1 input-unseen | Новая цель при знакомом входе не делает вход невиданным |
| P49, EXAM warm+new | Все 4 exact-pair seen | Общую панель нельзя выдавать за независимый перенос |
| P49, проект B | 6/8 → 7/8; единственный выигрыш c-f3-p2, input 3 → target 0, уже был в new TRAIN | Полезный измеренный выигрыш сохранён; unseen-transfer этим не доказан |
| P52 | Cold 0/4, независимый constant ceiling 2/4; оба обученных плеча answer 4/4; tense 1/4 → 3/4 | Cold, constant baseline и адрес времени — разные показатели |
| P58 | В каждом из трёх плеч corrected Source 1/1, другие literal Source 0/7 | Локальное усвоение предъявленной коррекции, без доказанного переноса |

P48 gains: `exam-t1`, `train-f1-00`, `train-f1-02`.
P48 losses: `exam-t3`, `train-f3-06`, `train-f3-08`.

В P58 исправляемая строка — `e1: Anna opened the window`; до коррекции ответ «Анна открыла дверь», gold «Анна открыла окно». После коррекции единственный правильный ответ во всех трёх плечах имеет adapter `executor-en_ru-exact-source`. У остальных literal Source — ноль правильных из семи. Общие 1/8 остаются правильной арифметикой; меняется допустимая интерпретация.

Отдельный результат P58 о пригодности фактически выданного неверного ответа 0→1 документирован в опубликованном пакете. В этой короткой независимой сверке он не пересчитывался, поэтому относится к проверенному содержанию пакета, а не к новому исполнению.

## Цена test selection: подтверждена постановка и сохранённый расчёт

| Выбор | Unit TrainOnExample | Прямые Tokenize→Execute reads |
|---|---:|---:|
| P58 main Fact | 105 | 56 |
| P58 весь класс, 16 объявлений Fact | 261 | 104 |
| P52 main Fact | 12 | 12 |
| P52 весь класс, 2 объявления Fact | 36 | 28 |

Прочитан алгоритм расчёта в `codex-post-admission-experiments.py`. Он проверяет hashes, исключает строки/комментарии из подсчёта call sites, выводит множитель три Unit examples на пару из закреплённого builder и отдельно учитывает повторные контрольные Facts.

Manifest имеет 32 уникальных входа: 25 JSON + 7 C#, включая три producer helper, добавленных после peer review. v1 с 29 входами сохранён отдельно. В текущей сверке **шесть** raw bytes независимо проверены целиком; остальные 26 не выдаются за заново материализованные и исполненные. Полный 32-file replay принадлежит сохранённой проверке исходного аудита.

## P50 и loss: точный статус доказательств

P50 source-контрпример в `codex-cross-check.py/json` использует явно искусственный вход: 9 основных lessons при назначенных 8 и положительные метрики. Переведённые в Python ветви возвращают одновременно:

- `COMPLETED_WITH_GUARD_PROBLEMS`;
- `VALID_FOR_NAMED_UNIT`;
- `SUPPORTED_FOR_NAMED_METRIC`;
- return 0.

Прочитаны скрипт и его JSON. Это доказательство независимости решения от guard в закреплённой ветви; оно **не означает**, что исторический P50 реально выполнил девятый урок. Raw P50 входят в manifest как сохранённые данные; новый численный verdict P50 по ним не заявляется.

Loss повторно рассчитан здесь самостоятельно в JS double:

| Для logits [0,-20], target=1 | Значение |
|---|---:|
| Clipped reported loss | 16.11809565095832 |
| Stable cross-entropy | 20.000000002061153 |
| Конечная разность clipped loss | 0 |
| Конечная разность stable CE | -0.9999999979370955 |
| Градиент p−y | -0.9999999979388464 |

Это проверка математических формул; native float network и все её блоки параметров не исполнялись. План R09 правильно сохраняет p−y и требует устойчивую CE из logits. В нём также есть важный контроль больших одинаковых logits [1e8,1e8] → ln2: порядок `(max-target)+log(sum(exp(logits-max)))` избегает потери малой добавки во float. Ни этот план, ни совпадение double-формул не закрывают native finite-difference приёмку.

## Что означают «шесть программ» и «12 отрицательных проверок»

`codex-final-verification.json` содержит:

- 6 сохранённых positive replay receipts: product, experiments-v2, attempt, cross, late-counterexamples, late-evidence.
- 12 отрицательных controls: существующий output и изменённый pinned input для каждой программы.
- 9 записей проверки сохранности прежних document bytes.
- Native tests = 0, Train calls = 0, production changes = 0.

В существующем receipt все шесть occupied-output checks сохраняют старые bytes. Изменённые pinned inputs дают отказ и `no_result_accepted=true`. У двух программ при таком отказе остаётся пустой файл (0 bytes); поэтому формулировка «все всегда отказывают до создания любого output» была бы неверной. Правильная формулировка: ни один повреждённый вход не принят как результат, старые данные не перезаписаны.

В этой сверке прочитаны receipts и проверена их структура, но все шесть программ и все 12 controls **заново не выполнялись**. Их нельзя прибавлять к сегодняшним native тестам.

## Материалы для итогового ответа

- [Сводный аудит](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-audit.md).
- [Экспериментальная проверка](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-post-admission-experiment-review.md).
- [Пересчёт v2](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-post-admission-recomputed-v2.json).
- [Финальная проверка пакета](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-final-verification.json).
- [Адресный план](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-repair-plan.md).

Прочитаны experiment-design, test-integrity и score-interpretation из docs/skills на закреплённом commit. Применены: отдельные TRAIN recall и transfer, сохранение малых полезных выигрышей и регрессий, запрет превращать численный pass/helper в доказательство native пути, явное происхождение и границы показателей. Экспериментальный scope не требует host UI проверок из общих разделов этих навыков.

