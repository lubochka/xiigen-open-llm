---
title: "310. Начало experimental validation: warm-start, signed spectrum и структурная гипотеза"
source_pdf_pages: [726, 728]
book_pages: [690, 692]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 310. Начало experimental validation: warm-start, signed spectrum и структурная гипотеза

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Что именно показывают четыре калибровочных спектра и достаточно ли их для вывода о структурной инволюции?

## Что сообщает источник

Глава 18 начинается на PDF 726 с кампании signed stabilisation. Используется single-layer encoder с $d=256$, длиной последовательности $n=128$, начальным числом голов от 1 до 8 и одним MLM epoch перед включением controller в warm-start варианте. BD и PCA различаются отрицательным probe; перечислены SST-2 BD MLM, MNLI BD MLM, MNLI PCA MLM и MNLI BD no MLM. [Начало главы](../source/0726-0728-ch18-warm-start-protocol-and-signed-protection.md#chapter-18), [setup](../source/0726-0728-ch18-warm-start-protocol-and-signed-protection.md#section-18-1-1).

Этот перечень не является полным $2\times2$ crossing «gate × warm-start» на одной задаче: есть смена задачи и нет, например, MNLI PCA no MLM. Совпадение отдельных диагностик нельзя трактовать как изолированный causal effect каждого фактора. Для BD/MNLI есть пара с warm-start и без него, но причинное сравнение всё равно требует одинаковых прочих settings и исходных run logs.

В начале нового chapter напечатано «preceding campaigns of this chapter», хотя перед §18.1 в этом расположении текста других campaigns нет. Ссылки Theorem ?? и Definition ?? сохраняются как source defects. Figure 18.1 — схема режимов, включая **мотивированную** architecture initialisation; она не служит отдельным экспериментом, подтверждающим все три режима. [Figure 18.1](../source/0726-0728-ch18-warm-start-protocol-and-signed-protection.md#figure-18-1).

## Числа калибровки

Table 18.1 согласуется с правилом $\theta_w=.40\Gamma_0$ до напечатанного округления: $2084.92\cdot.4=833.968$, $1788.60\cdot.4=715.44$, $2876.17\cdot.4=1150.468$. Напечатаны 833.97, 715.44 и 1150.47. Это корректные округления. В тексте говорится о «four counts», но в таблице три строки counts: positive above $\theta_w$, negative below $-\theta_w$ и total magnitude above $\phi_g$. [Table 18.1](../source/0726-0728-ch18-warm-start-protocol-and-signed-protection.md#table-18-1).

Pairing errors Figure 18.2 равны .072, .175, .175 и .037. Их denominator — **средняя величина top-10 eigenvalues**, а не модуль каждого eigenvalue. Поэтому .175 не означает 17.5% relative error каждой малой пары, и близость кривых возле нуля не является гарантией точного numerical rank. Raw spectrum, uncertainty и повторения pairing statistic в этих страницах не приведены. [Figure 18.2 и описание метрики](../source/0726-0728-ch18-warm-start-protocol-and-signed-protection.md#figure-18-2).

## Traceless не означает парность спектра

Автор правильно отделяет эти утверждения. Для явного примера положим

$$
\Sigma=\operatorname{diag}(1,2,3),\qquad
K=\begin{pmatrix}0&-2&-1\\2&0&-2\\1&2&0\end{pmatrix}.
$$

Тогда

$$
\tfrac12[\Sigma,K]
=\begin{pmatrix}0&1&1\\1&0&1\\1&1&0\end{pmatrix}
$$

имеет eigenvalues $2,-1,-1$. Это настоящий commutator из symmetric PSD $\Sigma$ и skew $K$, симметричный и traceless, но без парности $\pm\lambda$.

Наблюдаемая approximate pairing поэтому содержательна как дополнительная эмпирическая характеристика. Однако она не доказывает конкретное общее преобразование $J$ с двумя отдельными условиями $J\Sigma J^\top=\Sigma$, $JKJ^\top=-K$. Для перехода к block-off-diagonal форме необходимо также определить, в какой метрике $J$ ортогонален и как действует на произведение операторов. Спектр одного $A$ не восстанавливает совместную структуру его множителей. Сам источник оставляет claim в статусе [P-COND], что сохранено в индексе.

Есть внутреннее расхождение прозы: на PDF 727 и 730 no-MLM spectrum описан через «single large negative» mode, но Table 18.1 содержит и одно положительное eigenvalue выше 1150.47, Figure 18.2 показывает обе крайние ветви, а PDF 732 прямо говорит о двух крупных modes противоположного знака. Эти формулировки нельзя молча свести к одностороннему спектру. [Позднейшая формулировка](../source/0732-0734-ch18-cross-task-validation-and-proteome-setup.md#section-18-1-5).

Для Xiigen данные полезны как шаблон диагностического отчёта: сохранять подписанный spectrum, нормировку pairing error, calibration batch и точные counts. Они не доказывают ускорения обучения, качества pruning или универсальности структурной гипотезы. Здесь перепроверены опубликованные числа и матричный пример; author-reported experiments не воспроизводились.
