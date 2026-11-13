# Добавочные исправления очереди и интерпретации N63I

Статус: документное дополнение к snapshot `dd150aa5216e42b8a8bba14955337398ec5f8397`. Защищённые карточки, отчёты, raw и журналы не меняются. Эта страница не назначает повторный запуск.

## C1. №08 уже выполнен

В [начале индекса](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L5-L17) и [карточке№08](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/N63-ingress-length.md#L23-L25) записано, что runner ещё не создан. Фактически [status.json](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/results/status.json#L1-L17) и [arms.json](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/results/arms.json#L1-L61) фиксируют две попытки,20 scoring forwards и два полезных обновления вB. [required-changes](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/required-changes.md#L129-L143) уже содержит результат.

**Действующее разрешение противоречия:** №08 = `MEASURED_UNBLOCK_SUCCESS / LEARNING_PANEL_INVALID`; старый NOT_RUN/host-absent является историческим описанием. №01–08 не запускать и не дополнять новой диагностикой ради отчёта. При общей программе следующий№09 N63-AR-CAP; одиночное поручение заканчивается END. В pinned Git tree отсутствует `experiments-v2/RP01-MICRO/N63R`; наличие локальных неопубликованных файлов на компьютере пользователя здесь не проверялось. №09 остаётся подготовкой строго своего рецепта, без самовольного поиска замены.

## C2. Что полезного доказал№08

| Ось | A: все3 строки | B: короткие2, длинная deferred | Вывод |
|---|---:|---:|---|
|Настоящие optimizer updates|0|2|Полезное разблокирование обучения; кандидат сохраняется.|
|Учебных target positions|0|70|Работа B подтверждена; A отказался до обучения.|
|Состояние|hash не изменился|hash изменился|Достижимость записи, не доказательство навыка.|
|Правильные response tokens|0 до/после|0 до/после|Учебный вердикт панели недействителен по правилу пользователя.|
|Охват файлов|0/3|2/3|Long.cs221tokens при limit65 остаётся необученным.|

Код будущего production кандидата уже назван в [required-changes](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/required-changes.md#L129-L143): план admission до кандидатного хэша/мутации в `ContinuousLearningPipeline` и `IncrementalTrainer`; длину не ослаблять, текст не обрезать, provenance сохранить. Это кандидат по доступности, перенос не выполнялся.

## C3. Уточнение кривой0/2/10/50/200/1000

В [отчёте](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I-LEARNABILITY-PROBE/report.md#L17-L46) есть слишком сильная формулировка «первый токен только на1000» и «внутри2steps валидный результат невозможен». Правильная интерпретация: **первая положительная измеренная точка —1000**. Между200 и1000 точек нет. Поэтому точный минимальный шаг не установлен; можно сказать, что наблюдался переход между измерениями200 и1000, не приписывая монотонность каждому промежуточному шагу. Вдобавок база пробы seeded, база№08 unseeded; [код](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I-LEARNABILITY-PROBE/Program.cs#L67-L71) прямо отказывается от побайтной идентичности. Это поддерживает гипотезу недостаточного бюджета данного режима, но не доказывает универсальную невозможность улучшения за2шага.

В raw точки2 TRAIN response NLL = **6.684999711464082**, не6.684100 из таблицы report. Источник: [raw-кривая](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I-LEARNABILITY-PROBE/probe-result.json). На1000 только3/40 правильных TRAIN response tokens,0/2 целых ответа и0 правильных EXAM response tokens. Слова «почти обученная» или «восстановили полный навык» не подтверждены.

## C4. Числовые и смысловые противоречия report№08

В [таблице](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/results/report.md#L31-L45) NLL сдвиги подписаны «усвоение»/«локальный успех», хотя выше и ниже автор правильно признаёт панель недействительной. Сохранять числа как **наблюдаемые NLL-сдвиги без валидного вердикта освоения**. Ненулевая полезная ось этого опыта — admission0→2updates.

Текст [о времени](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/results/report.md#L51-L59) говорит «уA меньше время», но raw дают A=279.9091ms и B=111.3761ms. Исправление: **в данном наблюдении B имеет меньше wall time; причинное ускорение не установлено, а сравниваются0 и2updates**. CPU A62.5→B93.75ms и allocations81992→3330408bytes показываются рядом. Ни одна сторона не называется более быстрой обучающей только из этих двух значений.

## C5. Точный addendum к будущему автору карточек

При документном обновлении отражать actual outcome по source результату, а не возвращать DONE в NOT_RUN из-за неудачной панели. Применять source precedence: pinned raw/receipts → явное erratum → индекс → старый дизайн. Новый warm-start вариант — отдельный будущий ID/версия только по решению пользователя, без перезаписи первого опыта и без обхода execution.claim. Общий warm donor уже имеется; описание в `warm-state-candidate.md`. Все budgets включают подготовку, update и readouts; EXAM не участвует в выборе состояния. Последнее STOP/NEXT прекращает дополнительный отчёт/проверку/повтор сразу.
