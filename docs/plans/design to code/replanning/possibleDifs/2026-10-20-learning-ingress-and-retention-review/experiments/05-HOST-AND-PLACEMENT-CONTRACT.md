# Исполнимый проект: что существует и что ещё только описано

Назначенный каталог не означает существующий `.csproj` или подтверждённую сборку. На первичном срезе ce890a оба каталога RP01-MICRO отсутствовали. На позднем срезе 67ab появился N-KU. Поэтому вывод об отсутствии проекта относится только к первому срезу.

| Контракт | Состояние на 67ab | Пригодность |
|---|---|---|
| Реальный `experiments-v2/RP01-MICRO/N-KU-01/runner/NKu01Runner.csproj` | Есть код и raw прошлой сессии; ProjectReference подключает весь ExperimentsV2 | Сохранить результат; не использовать как шаблон нового минимального проекта |
| Будущий отдельный N-KU runner в карточке | Явные Compile Include для Program, SourceA и SourceB; Roslyn 4.12 | PREPARABLE; новая версия карточки не разрешает повтор |
| N61 Mode1 | Описаны точный набор копий, BCL/net9 и команды | PREPARABLE; материализация Program и сборка здесь не проверены |
| N61 reference cache | Есть точные вызовы и контракт кэша; полного набора зависимостей не хватало | PREPARABLE, но ещё не готовая команда исполнения |
| N62 neural | 101 копия исходников, три собственные сборки и bridge, внешние фиксированные DLL; LF-03 добавляет TensorCore | PREPARABLE; совместимость DLL, исходные веса и сборка не проверены |
| T-TRAIN-CONTEXT-01 | Полные csproj, Program, Prepare и Run в Markdown | PREPARABLE; файлы ещё не созданы и не собраны |

Исторические P125, embedding и DPO live harness используют локальные DLL по абсолютным Windows-путям. Их код доказывает вызов реального Train в сохранённом опыте, но не задаёт готовую текущую композицию копий исходников с типами capabilities. Нельзя сравнить изменённый исходник с прежней DLL и назвать это matched A/B. Нельзя выдавать Runner имя friend assembly ради обхода границ доступа. Новая N62 описывает композицию через копии исходных библиотек; её следует материализовать по полной карточке, не угадывая граф зависимостей во время запуска.

## Что автор карточки задаёт до передачи исполнителю

- Буквальные пути, выбранный ID и отдельный RunId.
- Manifest копий с исходным commit и SHA; явные Compile Include и идентичности сборок.
- Точный entrypoint, аргументы и команды сборки и запуска.
- Фиксированные TRAIN, EXAM и отдельный gold/scorer.
- Исходное состояние, reference, optimizer и RNG либо обоснованные N/A.
- Одно изменение; предельное число updates и reads.
- Новый output вне защищённой папки; отказ при совпадении со старым результатом.
- Реальные счётчики и запись ошибки без превращения исключения в нулевую точность.

Автор карточки заполняет эти поля. Исполнитель материализует назначенный рецепт. В текущем окружении нет dotnet, поэтому ни одна будущая команда не выдана за выполненную.

## Прямые источники

- [Реальный csproj N-KU, строки 3–15](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/experiments-v2/RP01-MICRO/N-KU-01/runner/NKu01Runner.csproj#L3-L15).
- [Mode1: набор копий и команды, строки 260–395](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L260-L395).
- [N61 reference: незаполненный набор зависимостей, строки 66–69](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L66-L69).
- [N62 neural: композиция и команды, строки 76–187](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L76-L187).
- [T-TRAIN-CONTEXT-01: тексты runner и скриптов, строки 67–303](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-training-context.md#L67-L303).
