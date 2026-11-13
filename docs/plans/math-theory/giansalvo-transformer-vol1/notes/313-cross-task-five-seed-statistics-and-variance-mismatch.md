---
title: "313. Cross-task table: пять seeds, баланс pruning и неверное обобщение variance"
source_pdf_pages: [731, 733]
book_pages: [695, 697]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 313. Cross-task table: пять seeds, баланс pruning и неверное обобщение variance

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Какие выводы о thresholds и BD/PCA поддерживает Table 18.4, а какой вывод противоречит её числам?

## Состав агрегирования и корректные равенства

Восемь конфигураций объединяют task SST-2/QNLI, gate BD/PCA и high/low threshold; по пять seeds на каждую дают $8\cdot5=40$ reported runs. Везде $K_{\rm init}=4$ и $n_{\rm growth}=0$. Для каждой строки mean values удовлетворяют

$$
\overline K_{\rm obs}+\overline n_{\rm pruning}=4.
$$

Например, $2.2+1.8=4$ и $1.2+2.8=4$. Здесь дробные $K_{\rm obs}$ — средние целых counts, не дробные головы в отдельной модели. [Постановка агрегирования](../source/0729-0731-ch18-signed-pruning-and-initial-head-count-sweep.md#section-18-1-4), [Table 18.4](../source/0732-0734-ch18-cross-task-validation-and-proteome-setup.md#table-18-4).

Predictive reference меняется с 5.73 до 230.26, то есть примерно в **40.185 раза**, тогда как среднее наблюдаемое число голов остаётся между 1.2 и 2.6. Это поддерживает качественное наблюдение об отсутствии сопоставимого расширения архитектуры в данной кампании. Однако «без какого-либо изменения поведения» нельзя читать как точное равенство всех trajectories: у PCA mean $K_{\rm obs}$ меняется с 2.2 до 2.4 на SST-2 и с 2.4 до 2.6 на QNLI.

Численное $\theta_{\rm low}$ в этих страницах не задано. Поэтому таблица позволяет сравнить опубликованные outputs, но не воспроизвести low-threshold условие из одних её значений. Идентификаторы пяти seeds и исходные per-run показатели также отсутствуют.

## Конкретное расхождение о variance

Проза утверждает, что PCA получает более высокую accuracy на обеих задачах ценой большей variance $K_{\rm obs}$. Для SST-2 это противоречит указанным standard deviations:

| Task | BD std $K_{\rm obs}$ | PCA high std | PCA low std | Фактическое направление |
|---|---:|---:|---:|---|
| SST-2 | 1.41 | 1.30 | 1.14 | variance уменьшается |
| QNLI | .45 | 1.34 | 1.52 | variance увеличивается |

Квадраты SST-2 standard deviations равны 1.9881, 1.6900, 1.2996; QNLI — .2025, 1.7956, 2.3104. Таким образом, оговорка касается именно обобщения на обе задачи. Числа таблицы сохранены, текст источника молча не исправлялся. [Авторская интерпретация после таблицы](../source/0732-0734-ch18-cross-task-validation-and-proteome-setup.md#pdf-0732).

## Среднее, std и доказательство различия

Напечатанное «mean ± std» обозначает разброс seeds. Оно не является confidence interval среднего. При независимых одинаково распределённых runs standard error равен $s/\sqrt5$, а при normal approximation с оценённой variance 95%-интервал среднего использует $t_4\approx2.776$. Например, для SST-2 BD $.532\pm.032$ такой условный interval равен примерно $[.4923,.5717]$.

Эта иллюстрация не является новой оценкой из raw runs и не доказывает качество относительно реального task baseline. Для сравнения BD/PCA важно, использованы ли paired seeds: variance разности зависит от их covariance. Например, unpaired standard error разности SST-2 high means равен $\sqrt{(.045^2+.032^2)/5}\approx.02469$, при разности means .025. По одним агрегатам нельзя приписывать этой разнице установленную paired significance.

Ноль событий в сорока runs — наблюдение на выбранном protocol. Он не задаёт нулевую вероятность роста на новых данных, другом optimiser, более длинном horizon или иной calibration. С другой стороны, нельзя требовать competitive NLP quality от этой таблицы: автор прямо ограничивает её роль диагностикой малого encoder. [Оговорка о цели таблицы](../source/0732-0734-ch18-cross-task-validation-and-proteome-setup.md#pdf-0732).

Для проекта полезны per-seed logs, отдельные счётчики heads/events, точные thresholds, paired comparisons и заранее выбранная task metric. Все выводы здесь основаны на напечатанных aggregates и их пересчёте; новых seeds не запускалось.
