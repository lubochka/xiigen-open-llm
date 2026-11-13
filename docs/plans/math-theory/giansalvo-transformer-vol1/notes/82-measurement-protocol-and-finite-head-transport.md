---
title: "82. Протокол измерений и проверенный транспорт четырёх голов"
source_pdf_pages: [305, 308]
book_pages: [269, 272]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 82. Протокол измерений и проверенный транспорт четырёх голов

[Индекс библиотеки](../README.md) · [§8.14](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-14) · [§§8.15–8.16](../source/0306-0311-ch08-experiments-protocol-and-synthesis.md#section-8-15) · [Предыдущая карточка](81-signature-bridges-and-hellinger-transport-bounds.md) · [Следующая карточка](83-signature-crowding-and-chapter-eight-boundaries.md)

## Что является синтетическим примером

Experiments A–D иллюстрируют re-extraction geography, entropy envelope, оптимальное сопоставление и несколько transition observables. Автор пишет, что scripts/data входят в сопровождающий package. В предоставленном материале этой карточки доступны страницы PDF и рисунки; само заявление не означает, что эти scripts/data получены или выполнены нами. [§§8.14–8.14.6](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-14).

Table 8.1 содержит **иллюстративные** значения: 12 слоёв, средняя inter-layer velocity 0.269, максимальный переход с индексом 6, средний residual 0.180, падение residual между первым и последним слоем 0.160. Это не результаты нового BERT/Xiigen training run. Их также нельзя смешивать с отдельным четырёхголовым примером §8.16. [Table 8.1](../source/0306-0311-ch08-experiments-protocol-and-synthesis.md#table-8-1).

## Что фиксировать для реальной модели

Источник разделяет три уровня измерений. Для каждой головы нужны $\Lambda$, $\rho$, entropy $H$, средний margin $\bar\Delta$ и activation $\Gamma$; cone index относится к общему представлению слоя. По слоям строятся empirical measures и $W_2$; по переходам — signed drifts и spread change. [§§8.15.1–8.15.3](../source/0306-0311-ch08-experiments-protocol-and-synthesis.md#section-8-15-1).

В отчёте указываются ground metric и $\alpha$, dictionary version, sequence length/mask, input/task distribution, transport solver и интервалы неопределённости по входам или seeds. Автор предлагает median и interquartile range для распределения layer velocities. Это summary statistics, а не автоматически заданный тест «значимого» transition layer. [§8.15.4](../source/0306-0311-ch08-experiments-protocol-and-synthesis.md#section-8-15-4).

**Редакторские уточнения:** averaging measures по batch и averaging отдельных per-input distances дают разные quantities. Протокол должен назвать выбранную агрегацию, tie-breaking и точность transport solver. Signed drifts и разность заранее определённых spreads можно вычислить из marginals без поиска coupling; coupling нужен для transport cost и соответствующих unsigned pairwise quantities. [Карточки 67](67-reproducible-signature-measurements-and-model-comparison.md) и [76](76-transport-diagnostics-and-metric-dependence.md).

## Четыре головы: исходные числа

В §8.16 сигнатура упрощена до $z=(\lambda_1,\lambda_2,\rho)$. Первая координата вместе со второй образует probability vector; residual — отдельная величина. Автор здесь **явно выбирает евклидову стоимость**, поэтому этот пример нельзя молча пересчитывать как прежнюю Hellinger–residual metric. [Обе исходные таблицы](../source/0306-0311-ch08-experiments-protocol-and-synthesis.md#section-8-16).

| Голова первого слоя | $z_h$ | Сопоставленная голова второго слоя | $z'_{\sigma(h)}$ | Квадрат евклидова расстояния |
|---|---|---|---|---:|
| 1 | $(0.80,0.20,0.12)$ | 1 | $(0.76,0.24,0.10)$ | 0.0036 |
| 2 | $(0.70,0.30,0.15)$ | 4 | $(0.66,0.34,0.14)$ | 0.0033 |
| 3 | $(0.25,0.75,0.20)$ | 2 | $(0.30,0.70,0.16)$ | 0.0066 |
| 4 | $(0.20,0.80,0.18)$ | 3 | $(0.18,0.82,0.17)$ | 0.0009 |

Последний столбец — редакторский расчёт по числам источника. Перебор всех $4!=24$ assignments подтвердил единственный минимум для $\sigma=(1,4,2,3)$. При одинаковых массах $1/4$ и $W_2$ с евклидовой ground metric:

$$
W_2^2=\frac{0.0036+0.0033+0.0066+0.0009}{4}=0.0036,
\qquad W_2=0.06.
$$

Следующее по величине значение среднего squared cost равно 0.0095. Перестановка автора также минимизирует сумму обычных евклидовых расстояний; численные значения этих двух objectives различаются. Дополнительные вычисления проверяют напечатанный пример и не являются результатом обучения модели.

## Дрейфы и потеря информации при hard labels

Средние сигнатуры равны

$$
\bar z=(0.4875,0.5125,0.1625),
\qquad
\bar z'=(0.4750,0.5250,0.1425).
$$

Поэтому

$$
D^\Lambda=(-0.0125,\;0.0125),\qquad D^\rho=-0.0200.
$$

Эти числа — редакторское раскрытие формул §8.16.1. Знак residual drift подтверждает авторское «mildly canonicalising» относительно данного словаря. Он не измеряет улучшения loss или accuracy. [§8.16.1](../source/0306-0311-ch08-experiments-protocol-and-synthesis.md#section-8-16-1).

При данном matching каждая голова сохраняет свою доминирующую координату. Hard transition matrix отметила бы только два перехода «1→1» и два «2→2», пропустив ненулевые движения смесей и уменьшение residual. Но даже transport plan не устанавливает, что одна голова буквально стала другой: он сравнивает две популяции при выбранной метрике. [§8.16.2](../source/0306-0311-ch08-experiments-protocol-and-synthesis.md#section-8-16-2).
