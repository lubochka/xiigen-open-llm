---
title: "103. Поляризация голов и различие score balance и attention asymmetry"
source_pdf_pages: [355, 363]
book_pages: [319, 327]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 103. Поляризация голов и различие score balance и attention asymmetry

[Индекс библиотеки](../README.md) · [Предыдущая карточка](102-sars-v8-aggregate-trace-and-score-conventions.md) · [Следующая карточка](104-sa-interventions-and-orientation-criterion.md)

## Среднее скрывает разные headwise trajectories

Table 10.2 показывает головы с противоположными изменениями. Номера layer/head сохранены как в книге; Figure 10.7 размечена начиная с 0. Название «strongest directional candidate» здесь относится к balance и выбранному анализу, а не к доказанной максимальной task importance. [Figure 10.7](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#figure-10-7); [Table 10.2](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#table-10-2).

| Layer / head | Final $\alpha$ | $\Delta\alpha$ | $\Gamma_s$ | $\Gamma_a$ | Авторское чтение |
|---|---:|---:|---:|---:|---|
| 2 / 7 | 0.659 | +0.283 | 1.224 | 2.366 | Strongest directional candidate |
| 2 / 0 | 0.523 | +0.073 | 2.924 | 3.208 | Mixed-directional |
| 1 / 7 | 0.488 | -0.014 | 1.586 | 1.511 | Mixed with high energy |
| 4 / 4 | 0.044 | -0.484 | 8.060 | 0.367 | Symmetric collapse |
| 5 / 5 | 0.010 | -0.457 | 25.769 | 0.255 | Symmetric collapse |

Это выбранные строки исходной таблицы; все десять перенесены в source. Малый balance при большой $\Gamma_s$ не означает неактивность: мала относительная skew доля, а не обе компоненты. И наоборот, большой balance при малых обеих нормах может сопровождаться слабым intervention effect. [PDF 358, интерпретация Table 10.2](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#pdf-0358); [PDF 360, три failure modes](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#pdf-0360).

## Почему attention asymmetry не определяется одним alpha

Reported mean balance уменьшается 0.453→0.245, а attention asymmetry растёт 0.108→0.287. Источник верно не объявляет это противоречием: raw scores и row-softmax outputs различаются. Но объяснение через «amplification oriented part» остаётся возможной интерпретацией, не установленной единственной причиной такого сочетания. [PDF 357](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#display-10-representation-attention-bridge).

Редакторский контрпример к обратному выводу «асимметрия attention обязательно означает ненулевой $M_a$»: пусть $X=I_2$, $M=M_s=\operatorname{diag}(3,1)$. Тогда scores симметричны, однако

$$
A_{12}=\frac1{e^3+1},\qquad A_{21}=\frac1{e+1},
$$

и $A_{12}\ne A_{21}$. Причина — разные row normalizers; skew score component равна нулю. Это не модель reported SARS run, а проверка различия диагностик. [Source distinction score/attention/signature](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#remark-10-17); [предыдущая геометрическая оговорка](97-canonical-qk-sa-geometry-and-balance-definitions.md).

Обратное направление также требует осторожности: score-side skew balance не равен magnitude полного output change. На output влияют values, $W_O$, оставшиеся головы и последующие блоки. Сам автор перечисляет эти возможные подавления в §10.13. [PDF 360](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#pdf-0360).

## Entropy и phase portrait

Figure 10.13 показывает пересекающиеся значения entropy и balance; её подпись прямо отвергает универсальный закон. Scatter plot подтверждает только наличие разных сочетаний в заявленном trace. Он не даёт функциональной зависимости $H=f(\alpha)$, направления причинности или статистической независимости. [PDF 363, Figure 10.13](../source/0361-0364-ch10-empirical-status-and-synthesis.md#figure-10-13).

Phase portraits Figures 10.9/10.11 содержат общие тренды и headwise различия, но геометрическая ортогональность S/A subspaces не означает динамической независимости scalar norms на плоскости графика. Обе компоненты связаны loss, representations, factorization и softmax. [PDF 357, объяснение phase portrait](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#pdf-0357).

## Что остаётся невыведенным

Фраза PDF 358 об «entropy and margin columns» шире реально показанных таблиц: Table 10.1 содержит entropy и asymmetry, Table 10.2 — balance и energies; margin column в них отсутствует. Конкретную margin trajectory по этим таблицам восстановить нельзя. Это текстовая неточность источника, а не основание добавить недостающие значения. [PDF 358](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#pdf-0358).

Для проекта здесь полезен именно совместный отчёт: абсолютные нормы, относительный balance, attention statistics и intervention deltas на одном probe set. Ни terminal heatmap, ни layer mean не заменяют исходные headwise trajectories и проверку downstream/task effects. [§10.12](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#section-10-12); [§10.15](../source/0361-0364-ch10-empirical-status-and-synthesis.md#pdf-0361).
