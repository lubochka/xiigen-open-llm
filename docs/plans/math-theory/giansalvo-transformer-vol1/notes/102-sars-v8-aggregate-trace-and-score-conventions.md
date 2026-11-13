---
title: "102. SARS v8: агрегированный trace и соглашения score balance"
source_pdf_pages: [354, 362]
book_pages: [318, 326]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 102. SARS v8: агрегированный trace и соглашения score balance

[Индекс библиотеки](../README.md) · [Предыдущая карточка](101-joint-diagnostic-and-weak-signature-velocity.md) · [Следующая карточка](103-headwise-polarisation-and-attention-asymmetry.md)

## Статус данных и модель

Глава 10 переходит от template maps главы 9 к **авторскому сообщению об empirical SARS v8 run**. Описана компактная BERT-like DNA модель: 6 слоёв, 8 голов, hidden dimension 256, sequence length 512, 10 эпох; последний показанный step — 22730. Это заявленная конфигурация конкретного запуска, а не автоматически BERT-base или результат Xiigen. Мы проверили перенос таблиц и рисунков из PDF; обучение и исходный notebook здесь не воспроизводились. [PDF 355, Figure 10.7](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#figure-10-7); [PDF 361, §10.15](../source/0361-0364-ch10-empirical-status-and-synthesis.md#pdf-0361).

## Эмпирическая convention

Для fixed checkpoint, input и head §10.11 определяет

$$
\Gamma_s=\lVert XM_sX^\top\rVert_F,\quad
\Gamma_a=\lVert XM_aX^\top\rVert_F,\quad
\alpha_{\mathrm{score}}=\frac{\Gamma_a}{\Gamma_s+\Gamma_a+\varepsilon}.
$$

В source формула снова называется $\alpha_{\mathrm{SA}}$; заголовок самой Figure 10.7 использует `alpha_score`. Это нормы **без квадрата**, в отличие от squared energies главы 9. Они уже включают действие $X$; изменение representations — часть этого observable. [PDF 354, §10.11](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#section-10-11); [различия нормировок](97-canonical-qk-sa-geometry-and-balance-definitions.md).

Точное равенство ортогональных компонент имеет вид $\lVert L\rVert_F^2=\Gamma_s^2+\Gamma_a^2$. Поэтому словосочетание energy budget в окружающем тексте не превращает сумму unsquared norms или её среднее в аддитивную квадратичную энергию. Пороговые режимы $\tau_s,\tau_a$ названы автором presentation choices; их численные значения в этих страницах не установлены. [PDF 355, coarse regimes](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#display-10-coarse-regimes).

## Что сообщает Table 10.1

| Observable | Step 0 | Step 22730 | Смысл изменения в этом trace |
|---|---:|---:|---|
| Mean $\alpha$ | 0.453 | 0.245 | Средняя доля skew score norm уменьшилась |
| Standard deviation $\alpha$ | 0.047 | 0.176 | Различия между головами выросли |
| Range $\alpha$ | [0.329, 0.567] | [0.005, 0.659] | Появились более крайние значения |
| Mean $\Gamma_s$ | 0.442 | 6.277 | Symmetric score scale сильно вырос |
| Mean $\Gamma_a$ | 0.360 | 1.325 | Skew scale также вырос |
| Mean entropy | 3.118 | 3.076 | Небольшое итоговое снижение |
| Attention asymmetry | 0.108 | 0.287 | Другой observable вырос |

Все числа — из [Table 10.1](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#table-10-1). Промежуточный step 5000 имеет mean $\alpha=0.464$, выше initial 0.453; trace не является монотонным падением на каждом шаге. Аналогично $\Gamma_a$ после показанного максимума на step 10000 уменьшается. Конечная тенденция и монотонность — разные утверждения. [Полные шесть строк таблицы](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#table-10-1).

Среднее отношений не равно отношению средних. Из финальных means получается $1.325/(6.277+1.325)\approx0.1743$, а reported mean headwise balance равен 0.245. Это само по себе **не противоречие**: усреднение $\alpha_h$ и последующее вычисление ratio двух средних — разные операции. Сравнивать их можно только при дополнительном контроле распределения знаменателей. [Table 10.1 и эмпирическое определение](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#table-10-1).

## Что необходимо сохранить для воспроизведения

Помимо чисел нужны checkpoint IDs, actual probe inputs, маска/длина активной области, softmax scale, stabiliser, правило усреднения и формулы entropy/asymmetry/drift. На этих страницах названия последних summaries приведены, но их точные нормировки не восстановлены одной подписью оси. Source говорит об архиве `sources/` и нескольких таблицах; это заявление автора об его пакете, не подтверждение наличия архива в данной Markdown-библиотеке. [PDF 361–362, экспорт и archive statement](../source/0361-0364-ch10-empirical-status-and-synthesis.md#pdf-0361).

Figures 10.9 и 10.11 — две публикации одного и того же embedded raster исходного PDF. Разные подписи не делают их двумя независимыми экспериментами. Их повтор сохранён как особенность источника. [Figure 10.9](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#figure-10-9); [Figure 10.11](../source/0361-0364-ch10-empirical-status-and-synthesis.md#figure-10-11).
