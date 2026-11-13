---
title: "322. Static control: размер search grid, failure rates и смешение K⋆ с retained K"
source_pdf_pages: [746, 748]
book_pages: [710, 712]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 322. Static control: размер search grid, failure rates и смешение K⋆ с retained K

## Вопрос

Что численно подтверждает Table 18.7 и можно ли вывести failure probability только из Phase-1 head-count bound?

**Таблица показывает различие трёх исследованных режимов, но не устанавливает causal law failure rate = function of $K^\star$.** Величины $K^\star$, выбранный $K$ и memory horizon меняются вместе; seed budget также неодинаков. [Static setup](../source/0746-0748-ch18-static-control-failure-and-runtime-growth.md#section-18-3-2), [Table 18.7](../source/0746-0748-ch18-static-control-failure-and-runtime-growth.md#table-18-7).

## Буквальный search interval

Источник задаёт все целые $K\in\{\lceil K^\star/2\rceil,\ldots,K^\star\}$. Число кандидатов равно $K^\star-\lceil K^\star/2\rceil+1$:

| $K^\star$ | Напечатанный диапазон | Число кандидатов | Retained $K$ |
|---:|---|---:|---:|
| 8 | 4–8 включительно | 5 | 4 |
| 14 | 7–14 включительно | 8 | 7 |
| 11 | 6–11 включительно | 6 | 8 |

Поэтому формулировка “four–seven training runs” не следует из этого интервала. Возможно, некоторые endpoint runs уже были доступны или использован иной grid, но это не указано. Число candidates не равно полному training budget: каждый вариант может требовать нескольких seeds, validation и отдельного Phase-1 estimate.

Retained value ниже upper bound само по себе допустимо. Но верхняя достаточная оценка $K^\star=11$ не доказывает, что выбранных восьми heads достаточно; это отдельный empirical/representational вопрос. Выбор лучшего $K$ по search также требует независимого evaluation от данных выбора, если результат используется как обобщающая оценка качества.

## Non-monotonicity и разные счётчики

Источник приводит $K^\star=\{8,14,8,9,11\}$ при $\tau_z=\{1,2,3,4,5\}$ s. Последовательность немонотонна. Поздняя фраза “the longer the memory horizon, the larger the head count” не описывает $K^\star$; в трёх строках таблицы растёт только **retained** $K=4,7,8$.

Для attribution нужны сравнения при фиксированном $K$ и разных $\tau_z$, а также при фиксированном $\tau_z$ и разных $K$, с одинаковым budget. По трём выбранным конфигурациям нельзя отделить сложность памяти, capacity, optimisation и selection effects. [Объяснение failure mode](../source/0746-0748-ch18-static-control-failure-and-runtime-growth.md#pdf-0748).

## Что говорят небольшие failure counts

Table 18.7 содержит $0/10$, **$0/5$**, $4/10$ failures. На длинном горизонте source разделяет четыре случая на два divergence и два payload-invariant collapses; сравнение с Transformer baseline — $4/10$ против $1/10$. Это наблюдаемые исходы, а не точные вероятности.

Для иллюстрации при независимых binomial runs и заранее заданном сравнении Fisher exact test для $4/10$ против $1/10$ даёт one-sided $p\approx0.1517$, two-sided $p\approx0.3034$. Это **другой тест**, чем reported Mann–Whitney $p_U$ по tracking error; результат не опровергает приведённые $p_U$ и не воспроизводит их без raw RMSE. Он показывает, что сами failure counts не дают сильного статистического отделения при обычном двухстороннем критерии.

Нулевой count также не означает нулевого риска: при $0/10$ exact one-sided 95% upper bound для общей failure probability равен $1-0.05^{1/10}\approx0.2589$. При иных зависимостях или парном seed design нужен соответствующий анализ.

Механизм silenced heads согласуется с авторской схемой и сообщённой payload invariance, но граница variation $\le0.05$ rad сама по себе не устанавливает причину: нужны per-head gradients, activity trajectories и matched interventions. Полезное практическое различение здесь — **выделенная representational capacity не гарантирует, что optimizer её использует**. Оно не требует объявлять доказанным точный causal relation failure–$K^\star$.
