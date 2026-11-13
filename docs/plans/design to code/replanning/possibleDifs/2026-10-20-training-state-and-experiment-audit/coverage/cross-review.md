# Независимая проверка WIKI-TAXONOMY-01 и WIKI-MARGIN-01

Дата:20.10.2026. Проверены README, EXECUTOR-PROMPT, fixture, source-factor.diff, оба Program.cs, native contracts, compiler projects и compare.py в новом пакете. Нативное обучение, C# сборка, запуск compare на вымышленных результатах и CI **не проводились**. Статус обеих поставок остаётся `NOT_BUILT / NOT_RUN`; статический review не заменяет compile proof.

## Результат

Критического противоречия правилам2/3/4/6/7 после указанных ниже правок не найдено. Обе карточки пригодны для передачи как **конкретный source scaffold с назначенным запуском**, а не как уже доказанное улучшение. До native исполнения неизвестны качество, ресурсная дельта и компилируемость подготовленной проекции. Они не объявляют эти данные измеренными.

| Проверка | TAXONOMY | MARGIN |
|---|---|---|
| Настоящий учебный фактор | Пространство output classes15→17 согласовано у Labels, LabelIndex и decoder; старые15индексов сохранены | Margin criterion0.10 и лучший competitor≠expected заменяют skip-correct; inference reader/gates прежние |
| Native learning trace | `Train(rows)` → feature weights/bias → `RouteForEvaluation` | Тот же native writer/reader; single-class исходный случай может не обновлять нулевые веса |
| Реальный consumer | Тот же `RouteForEvaluation`, что вызывает native TrainingRunner43–53; production static router не присвоен этому результату | То же; learned output target training не запускается |
| Before/after | Native `Train([])` создаёт zero initial state; native `Train(TRAIN)` — independent after из того же zero init | То же; это fresh training, не continuation существующего объекта |
| Сопоставимость начала | Общие15labels/biases равны нулю; два новых Bвыхода тоже нули. Полный hashA≠B допустим из-за размерности | Полные initial state hashes равны; labels15 одинаковы |
| Точный бюджет A+B |4APITrain:2empty init+2nonempty;4TRAIN наarm×18epochs×2=144presentations;6EXAM×2states×2arms=24reader calls |4APITrain:2empty init+2nonempty;2TRAIN×18×2=72presentations;4EXAM×2×2=16reader calls |
| Gold и utility | EXAMlabel не попадает в reader, толькоPrompt. Успех selected требует status+route+target, deferred NotProjectWiki не засчитывается | То же; OODпотери явно показываются отдельно от router_training gain |
| Размещение/запуск |4явныхCompile, literal paths/IDs/fixture; .NET9 BCL, без product ProjectReference и внешних package sources | То же; отдельный csproj и фиксированный result subtree |
| Сохранение старого | Host/comparator отказываются переписывать существующие outputs; другой ID не выбирается автоматически | То же |
| Полный перенос | Только после результата и отдельного решения пользователя; generator/restore/orchestration не добавляются вторым фактором | Только после решения пользователя; смешанный corpus и OOD проверяются будущим отдельным опытом |

## Найденные и устранённые в этой сессии отчётные остатки

**1. Resource-only успех был недостаточно явным в машинном сравнении.** Первоначальный compare.py выводил пары costsA/costsB, а явные metric rows и directional flags имел только для качества/отказов. Это не мешало человеку прочитать ресурсы, но плохо поддерживало требование2 «успех не требует accuracy». Автор добавил `resourceDeltas`: wall, CPU, current-thread allocations, whole-process peak, snapshot bytes и coefficient count. Теперь явно записаны A/B, B−A, полезное/неполезное направление и ограничение single-run/JIT/timer noise. Общий PASS не введён; цена увеличившейся модели видна рядом с качеством.

**2. Один decrease отказов мог получить автоматическую положительную пометку.** При этом README честно предупреждал, что замена отказов ошибочными решениями не является пользой качества. В последней версии compare `deferredRate` имеет `diagnosticsOnly=true`, `candidateMetricImproved=null`, отдельное raw direction и `conditionalBenefit`. Разрыв устранён: успешность выхода следует читать вместе с correct/wrong selected и группой домена.

Исправленные участки обоих compare.py повторно прочитаны. Автор обновил checksums. Это правки отчётной логики до любого исполнения, они не подменяют результат обучения и не меняют experimental factor.

## Что review не утверждает

- Не доказано, что Bулучшит ответы: числоupdates>0, hash-change и17labels сами по себе не успех качества.
- Не доказана устойчивость ресурсного выигрыша по одному короткому процессу. Peak working set относится ко всему процессу до наблюдения, а не отдельному Train.
- Не доказана integration готовность learned router в production orchestrator: карточки явно сохраняют эту границу.
- Не проверена snapshot migration15→17; fresh-state эксперимент не выдаётся за cold restore.
- Никакие новые факторы из последних protected N61карточек не совпадают с этими двумя вмешательствами: N61проMode1retention/identity и frozen-reference reuse. Поисковая новизна ограничена изученными pinnedкорпусами, не «всеми документами когда-либо».

Новый опыт должен исполняться только своим назначенным source-copy host с actual native данными. Любое измеренное улучшение полезного показателя — успех по этому показателю и домену; рост точности не обязателен; все ухудшения рядом. Итоговая роль этих карточек — дать пользователю сравнение и исходные свидетельства для выбора последующего переноса.
