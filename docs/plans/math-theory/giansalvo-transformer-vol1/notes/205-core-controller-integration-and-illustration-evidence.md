---
title: "205. Полный core controller: состав механизма и статус иллюстраций"
source_pdf_pages: [551, 567]
book_pages: [515, 531]
batch: "012"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 205. Полный core controller: состав механизма и статус иллюстраций

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Что необходимо перенести вместе

Algorithm 6 объединяет diagnostics, anchor protection, формирование кандидатов, safe initialisation, validation, выбор лучшего допустимого события, hysteresis и stopping. Изъятие любого из этих звеньев меняет механизм, описанный в главе. Особенно существенно сохранить сравнение после warm-up и защиту уже освоенных направлений. [Algorithm 6](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#algorithm-6).

| Часть механизма | На что отвечает | Чего сама не гарантирует |
|---|---|---|
| Coverage / candidate geometry | Где имеется диагностируемый пробел | Что он важен для loss |
| Unresolved gradient | Активен ли соответствующий спрос в обучении | Что конкретный шаблон реализует нужную функцию |
| Safe birth / split | Как ограничить ущерб от вмешательства | Что новый компонент будет обучаться |
| Validation / marginal value | Что изменилось после адаптации | Глобальную оптимальность архитектуры |
| Anchors / neutralisation | Что сохранять и как откладывать удаление | Сохранение доли и всех функций без дополнительных проверок |
| Hysteresis / StopGrowth | Когда прекращать текущие попытки | Универсальную сходимость обучения сети |

Эта таблица — редакторская карта требований, а не дополнительная теорема книги.

## Как согласовать отдельные algorithm boxes

Algorithm 2 вычисляет состояние; Algorithm 3 выбирает тип события; Algorithm 4 обеспечивает checkpoint и rollback; Algorithm 5 даёт компактный loop; Algorithm 6 собирает расширенную версию. Номера в книге глобальные **2–6**, а не 15.2–15.6. [Диагностика](../source/0551-0555-ch15-state-events-and-diagnostic-regularity.md#algorithm-2), [selector](../source/0551-0555-ch15-state-events-and-diagnostic-regularity.md#algorithm-3), [rollback](../source/0551-0555-ch15-state-events-and-diagnostic-regularity.md#algorithm-4), [compact loop](../source/0551-0555-ch15-state-events-and-diagnostic-regularity.md#algorithm-5).

Algorithm 6 не выписывает сам rollback внутри каждого candidate trial. Сравнение кандидатов должно иметь определённое общее исходное состояние; иначе второй кандидат оценивается после вмешательства первого. Словарь, optimizer state и параметры, изменённые во время адаптации, должны входить в практический контракт trial/reset. Это уточнение реализации; источник явно называет checkpoint слоя и словаря, но не перечисляет все mutable training objects.

Кроме того, перед candidate trials в Algorithm 6 нет отдельной ветки проверки stable window, которая явно стоит первой в Algorithm 3. Его применение в рамках H1 требует явно наследовать этот guard: нестабильные оценки должны приводить к обновлению или расширению окна до архитектурного вмешательства. Наличие диагностик в строке 2 само по себе не проверяет их устойчивость.

§15.20 отдельно показывает, что geometric criterion может выбрать $e_3$, а training-aware criterion — $e_4$. Marginal validation затем принимает решение. Это авторский пример полезности раздельных критериев, а не численный benchmark. [Пример](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#section-15-20).

## Иллюстрации нельзя превращать в отсутствующие результаты

На PDF 551 Figure 15.1 показывает asymmetry trajectory, threshold 0.9 и Level-3 saturation trigger; core state на той же странице фиксирует глубину, а depth growth относит к расширению. Рисунок служит общей иллюстрацией trigger, а не прямым экспериментом фиксированного core controller. [Figure 15.1](../source/0551-0555-ch15-state-events-and-diagnostic-regularity.md#figure-15-1).

Figure 15.2 фактически содержит графики commutator error для Standard/Centered Attention, тогда как подпись описывает коммутативность static rationalisation и incrementalisation. Оба элемента сохранены, но график не является сам по себе доказательством эквивалентности путей архитектурного преобразования. [Figure 15.2](../source/0551-0555-ch15-state-events-and-diagnostic-regularity.md#figure-15-2).

Figure 15.4 на PDF 567 — пустая рамка с напечатанным filename figures/fig_worked_dim4.pdf. Исходник не содержит готовой четырёхмерной иллюстрации на этой странице. В библиотеке сохранён прямой render placeholder и точная подпись. [Figure 15.4](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#figure-15-4).

## Проектное применение и роли

Для Xiigen этот материал задаёт ресурс для последующей проверки полноты алгоритма, а не автоматически принятый design change и не выполненный training experiment. Проектное правило о DPO сохраняется отдельно от содержания книги: **внешняя открытая китайская модель создаёт пары chosen/rejected; роль человека при выборе выполняет CODEX, и качество DPO также оценивает CODEX — в обоих случаях с моделью Astra 6 ultracode**. Из теорем главы не следует передача выбора или оценки качества генератору.

§15.21 прямо ограничивает утверждения сходимостью архитектурного контроллера при явных условиях. На этих страницах не установлены общее превосходство над фиксированной архитектурой, уменьшение sample complexity или сокращение времени обучения. [Границы заявления](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#section-15-21).
