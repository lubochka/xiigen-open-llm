---
title: "72. Mean-field география, границы обобщений и итог XIA"
source_pdf_pages: [282, 284]
book_pages: [246, 248]
batch: "006"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 72. Mean-field география, границы обобщений и итог XIA

[Индекс библиотеки](../README.md) · [Исходник §§7.41–7.43](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-41) · [Hellinger-residual geometry](69-hellinger-wasserstein-layer-geometry.md)

## Что доказано при росте числа heads

Для фиксированных входа $X$ и слоя $\ell$ рассматривается

$$
\mu_\ell^{(H)}(X)=\frac1H
\sum_{h=1}^{H}\delta_{\Sigma_{\ell,h}(X)}.
$$

Если все сигнатуры лежат в одном фиксированном compact space

$$
\Sigma_R=\Delta_8\times[0,R]
$$

с Hellinger-residual metric и конечными фиксированными $R,\alpha$, Proposition 7.47 даёт precompactness семейства мер в $W_2$. У каждой последовательности $H_k\to\infty$ можно выделить подпоследовательность со сходящимися мерами. [Proposition 7.47](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#proposition-7-47).

Доказательство короткое и корректное: probability measures на компактном metric space образуют компактное пространство для weak convergence; из bounded ground metric следует совпадение weak и $W_2$ convergence.

Это математический результат о возможных **подпоследовательностях распределений**, а не о том, как обучаются всё более широкие сети. Требование общего пространства существенно: если меняются definition of signature, dictionary labels, residual scale или metric, нужна дополнительная идентификация пространств.

## Что не следует из compactness

Можно построить последовательность

$$
\mu^{(H)}=
\begin{cases}
\delta_{\Sigma_a}, & H\ \text{чётно},\\
\delta_{\Sigma_b}, & H\ \text{нечётно},
\end{cases}
\qquad \Sigma_a\ne\Sigma_b.
$$

Каждая мера может быть эмпирической мерой $H$ одинаковых signatures. У чётной и нечётной подпоследовательностей есть разные пределы, а вся последовательность не сходится. Это редакторский пример, показывающий точный предел Proposition 7.47; он не заявляет существование таких обученных моделей в конкретной архитектуре.

Compactness также не устанавливает:

- единственный предел;
- независимость предела от seed;
- одинаковый предел у всех способов обучения;
- uniform convergence по входам;
- task-canonical fingerprint;
- скорость сходимости при росте $H$.

## Mean-field limit выделен как conjecture

Conjecture 7.48 предполагает для фиксированных task, architecture family и подходящей parametrisation детерминированный предел

$$
\mu_\ell^{(H)}(X)\to\mu_\ell^{\infty,\mathcal T}(X)
$$

в $W_2$, uniformly по слоям и compact input families. Это существенно сильнее subsequential convergence и в главе не доказано. [Conjecture 7.48](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#conjecture-7-48).

Только **если** такой предел существует, автор предлагает

$$
\bar\mu_{\mathcal T}^{\infty}
=\mathbb E_{X\sim\mathcal T}
\frac1L\sum_{\ell=1}^{L}\mu_\ell^{\infty,\mathcal T}(X)
$$

как task-canonical fingerprint и обсуждает effective dimension его support как возможную сложность задачи.

Существование записи fingerprint само по себе не делает её intrinsic invariant задачи. Она зависит от представления данных, architecture family, parametrisation, словаря, метрики, выбранных training dynamics и averaging. Как и обычный fingerprint из Conjecture 7.33, эта смесь теряет порядок слоёв. Термины «сложность» и «каноничность» здесь обозначают исследовательскую программу, а не готовый измерительный стандарт.

## Почему автор отказался от sheaf-подхода

В §7.42 автор объясняет свой выбор языка:

1. Для token sequences в рассматриваемой постановке не задана каноническая topology на пространстве входов.
2. Trained model уже задаёт детерминированное отображение входа в географию.
3. Head permutations удобно учитывать эмпирическими мерами, а query-key gauge относится к параметризации операторов.

В [Remark 7.49](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#remark-7-49) остаётся отображение

$$
X\longmapsto\mu_\ell(X)
$$

в Wasserstein space, для которого можно исследовать variation и stability.

Это аргумент о **выборе формализма данной главы**, не общая теорема невозможности sheaf methods для последовательностей. Детерминированность отображения сама по себе не запрещает локально-глобальные модели при дополнительно заданных topology, restrictions и compatibility conditions. Автор не строит такую альтернативную модель и не доказывает её универсальную невозможность. В конце он отдельно допускает естественность sheaf methods для images, graphs и manifolds с подходящей структурой. [§7.42 и PDF 284](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-42).

## Что означает XIA в итогах главы

§7.43 расшифровывает XIA как **explainable internal attention geometry**. Предложен промежуточный уровень описания:

| Уровень | Объект |
|---|---|
| Одна голова на одном входе | Canonical signature |
| Слой | Распределение signatures |
| Модель по глубине | Последовательность layer geographies |
| Обучение | Траектория этих объектов |
| Проверка объяснения | Canonical replacement и измеренный task/output drift |

[Итоги §7.43](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-43) намечают дальнейшие главы: re-extraction, signature velocity, projected escape signatures, redundancy и architecture design. Формулировка об incremental Transformer как способе добавлять географию только при полезности — авторская архитектурная мотивация.

Она не доказывает, что все standard Transformers избыточны в одинаковом смысле, что каждый рост coverage улучшает task quality или что incremental architecture быстрее обучается. Измеренный выигрыш требует отдельно сопоставлять качество, данные, вычисления и область применимости.

## Граница этого фрагмента

На **PDF 284 / книге 248** глава 7 заканчивается полностью. Последнее предложение завершается словами “only when it increases coverage, reduces useful residual, or improves task function.” Следующая глава начинается на [PDF 285](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#pdf-0285). Эта карточка опирается только на прочитанные §§7.41–7.43 и явно указанные определения предыдущих карточек.

