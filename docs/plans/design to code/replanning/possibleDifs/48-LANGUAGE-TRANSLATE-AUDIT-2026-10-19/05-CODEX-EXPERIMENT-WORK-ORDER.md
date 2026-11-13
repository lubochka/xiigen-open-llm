# 05. Задание CODEX: один опыт восстановления language_translate

19 октября 2026. Готовый текст передачи исполнителю. **Это проект следующей работы; описанные вызовы и обучение в данном аудите не выполнялись.** Главная цель — быстро проверить конкретный механизм и его предметный эффект.

## 1. Первый рабочий заход

Начни с R25-MEMORY в [03-SMALL-DOMAIN-EXPERIMENTS.md](03-SMALL-DOMAIN-EXPERIMENTS.md), затем при положительном локальном результате перейди к отдельно измеряемому R25-CONSUMER. Зафиксируй source commit language_translate f6032aff81f1f83de7c09cf9b9efe6b058dd12b9 и current a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6. Если реализация выполняется на другом commit, покажи относящийся к опыту diff до интерпретации результата.

Задача первого захода: установить, улучшает ли **обученная память POS-биграмм** предсказание структуры на новых семействах предложений, а затем использует ли actual current KNN этот сигнал при выборе ответа. Исторические 34,38%→87,5% — ориентир существования полезного механизма, не обязательная цель новой панели. [Donor classifier][pc12], [архив][e25], [current consumer][fusion].

Для нового R25-MEMORY вызывай существующие PatternClassifier.Train/Predict из небольшого harness. dispatcher.RunE25(tier: 1) допустим только как отдельная необязательная проверка исторической synthetic panel: он сам создаёт examples/split и не принимает новые TRAIN/EXAM или shuffled-label arm C. Проект 35.ExperimentsV2 — library; не выдумывай CLI --exp. Малый harness вызывает существующую функцию; новую копию classifier formula не выдавай за проверку продукта. [Dispatcher][dispatcher], [project][experimentproject].

## 2. Контракт данных и ролей

Для POS memory опыта можно использовать новые размеченные POS-примеры без создания DPO. Заморозь исходные предложения, POS, source-family split и gold до получения A/B outputs. Если понадобятся новые preference pairs для neural или Unit опыта:

1. **Внешняя открытая китайская модель создаёт chosen и rejected для одного prompt.**
2. **CODEX с astra 6 ultracode выполняет человеческий отбор**: выбранная сторона правильна, отвергнутая нарушает конкретный предметный закон.
3. **Отдельный независимый CODEX review** проверяет качество точных bytes пары и применимость закона. Не заменяй его заявлением того же генератора, что он прав.
4. Фактические model/provider/run identifiers записываются после реальных вызовов. Если заданная конфигурация недоступна, результат нельзя маркировать как выполненный этим teacher/reviewer.

Пример смыслового контраста для перевода, **иллюстрация требования, не готовая teacher-generated строка**:

- Source: “Do not delete the file.”
- Правильный вариант: «Не удаляй файл».
- Отвергаемый вариант: «Удаляй файл».
- Различающий закон: сохранение отрицания; направление и остальное действие одинаковы.

Плохая пара для этого закона: два правильных синонимичных отрицательных перевода, разница лишь в стиле. Другой плохой случай — выбранный текст сам меняет агента действия. Для кода подтверждай закон через семантику исходника и подходящий предметный oracle, а не через наличие слова return. Правильность полной пары ещё не доказывает правильные mappings/targets всех под-Unit. [DPO quality][prevquality], [subunit role/alignment][prevroles].

Не помещай EXAM answers, выбранные indices, reviewer verdict или скрытый expected label в learner input. Не переатрибутируй старые simulation материалы внешней китайской модели.

## 3. Инструкция на реализацию опыта

Сделай один небольшой disposable harness в отведённой для реализации рабочей области. Не редактируй core/docs/plans/replanning-possible-diffs. При чистой проверке существующего механизма продуктовые исходники остаются прежними; если требуется экспериментальное вмешательство, его единственный фактор виден отдельно в diff.

A — свежий classifier без памяти; B — такой же classifier после Train; C — те же примеры с заранее переставленными labels. Размер стартовой панели — примерно 24 TRAIN / 16 EXAM, при необходимости меньше, если уже получается различающий случай. Это не квота покрытия. Используй разные sentence/template families; одинаковые POS-шаблоны в разных переводах не являются независимыми семействами.

На одной EXAM сними predictions, neighbors, class state/count, confusion по вопросам/императивам/порядку ролей, время и память. Ответ C нужен, чтобы отличить вклад labels от случайной формы корпуса. Не «исправляй» gold после просмотра outputs.

Если B не отличается от A/C, сначала проверь actual Train/consumer/input; не запускай Main500 и полный набор экспериментов. Если B лучше, следующим отдельным шагом подключи его к **существующему** structural scoring slot. Общий text→POS ingress, corpus, кандидаты, vector/lexical weights и thresholds одинаковы в обеих integration arms. Запиши ID фактически вызванного Unit и raw output, который он возвращает.

Если actual output — только общий capability/subphase текст, отчёт о качестве касается выбора подфазы. Полный перевод или объяснение C# оценивается только у реально производящего его consumer. Подстановка oracle ответа внутри helper не является улучшением обученной системы.

## 4. Когда выбирать другие карточки

| Наблюдение | Следующий конкретный опыт |
|---|---|
| POS memory полезна локально, но current consumer использует rules | R25-CONSUMER |
| Неоднозначные слова теряют смысл при исчезновении соседнего контекста | R26-CONTEXT, с отдельным empty/partial context контролем и учётом [соседней E26 карточки][latee26] |
| Нужный AST signal есть, но disabled/unused | R09-CONSUMER |
| Поздние ветви скрываются в склеенном тексте | R-GRANULARITY, уникальные допустимые IDs, веса прежние |
| Старые примеры не попали в serving memory | R-CORPUS; затем отдельно R43-RETENTION |
| Полезное live state не воспроизводится после загрузки | R-RESTORE с actual owner и следующим update |
| Указанный C# label зависит от невызванной lambda | N48-01: сначала 6 fixtures, затем label-only A/B |
| Реальный optimizer state имеет Inf-v/аномальный finite gradient | N48-02: scalar step→restore→steps, затем только релевантный learning contrast |
| Один text triplet получил разные active counts current/replay | N48-03: actual mixed gradients/AdamW, same material version; generic output отдельно |

Не выполнять всю таблицу автоматически. Переход обусловлен наблюдением из законченного опыта. Старый положительный механизм восстанавливается раньше общего расширения KNN/LoRA.

## 5. Что вернуть по завершении одного опыта

Нужны компактный отчёт, воспроизводимый вызов существующего кода и raw результаты:

- Source commit, actual entry point, изменение единственного фактора.
- Состав TRAIN/EXAM и разделение семей; реальные hashes материала и исходного состояния.
- Какой learned state изменился и какой consumer его прочитал.
- Числа правильных/всех до, A, B; конкретные gains/losses; отдельно wrong accepted и retention.
- Два-три показательных исходных запроса с полными raw outputs, без выбора только удачных.
- Фактические время/память и граница их измерения.
- Вывод: поддержан ли механизм, улучшилось ли качество на этой панели, что осталось неизвестно, какое одно действие следует дальше.

Сохранённый JSON или отчёт о сборке не заменяет subject-quality result. Изменение LoRA/hash не заменяет свободный ответ, а 24/24 совпадений cold outputs не означает 24/24 правильных ответов. Уже исправленный ingress и P125/P25 используются как положительные controls со своими границами. [Ingress][ingress], [P125][memory125], [P25][cold25].

Остановись после достаточного различающего результата или конкретного опровержения. Дополнительные тесты нужны для найденного изменения/риска, а не для произвольного числа PASS. Этот заказ не требует Windows CI-марафона.

[pc12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/PatternClassifier.cs#L29-L86
[e25]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[fusion]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L205-L300
[dispatcher]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/ExperimentsV2Dispatcher.cs
[experimentproject]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/ExperimentsV2.csproj
[prevquality]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/14-data-quality-and-evaluation-alignment.md
[prevroles]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/44-structured-lessons-roles-keys-and-word-alignment.md
[latee26]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-E26-UNKNOWN-CONTEXT-AND-ORDER-2026-10-19.md
[ingress]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[memory125]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[cold25]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
