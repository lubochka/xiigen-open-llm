---
title: "74. Алгебра блока, границы ранга и симметрии Q/K"
source_pdf_pages: [289, 292]
book_pages: [253, 256]
batch: "006"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 74. Алгебра блока, границы ранга и симметрии Q/K

[Индекс библиотеки](../README.md) · [§8.3](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#section-8-3) · [Предыдущая карточка](73-reextraction-loop-and-head-recoverability.md) · [Следующая карточка](75-reextraction-cost-and-operator-populations.md)

## Что задаёт выход слоя

В §8.3 вводятся $Q=XW_Q$, $K=XW_K$, $V=XW_V$, затем

$$
S=\operatorname{softmax}_{\mathrm{row}}\!\left(\frac{QK^\top}{\sqrt{d_k}}\right),
\qquad Y^{(h)}=S^{(h)}V^{(h)},\qquad
Y=\sum_hY^{(h)}\widetilde W_{O,h}.
$$

Показанная формула полного блока имеет post-norm вид:

$$
X^+=\mathcal N_2\!\left(\mathcal N_1(X+Y)+
\operatorname{MLP}(\mathcal N_1(X+Y))\right).
$$

Утверждение о совместном переименовании голов остаётся верным и для обычных pre-norm вариантов, но сама эта запись не является буквальной формулой любого pre-norm блока. При переносе в код нужно сохранить фактическое расположение нормировок и residual-связей. [PDF 289](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#pdf-0289).

В §8.1 $L$ уже подаётся в softmax, а здесь $L=QK^\top$ делится на $\sqrt{d_k}$ отдельно. Это локальное соглашение источника: нельзя повторно делить уже масштабированные logits.

## Ранг MLP: какое условие скрыто в короткой записи

Источник даёт

$$
\operatorname{rank}\operatorname{MLP}(Z)
\le\min(n,d_{\mathrm{ff}},d_{\mathrm{model}}).
$$

Для выхода вида $H(Z)W_2$, где $H(Z)\in\mathbb R^{n\times d_{\mathrm{ff}}}$, эта оценка корректна. Она следует из последнего матричного произведения и не требует ложного утверждения, что поэлементная нелинейность всегда уменьшает ранг. [§8.3.3](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#section-8-3-3).

**Редакторское уточнение для bias.** Если выход имеет вид $H(Z)W_2+\mathbf1b_2^\top$, общая оценка становится

$$
\operatorname{rank}\operatorname{MLP}(Z)
\le\min(n,d_{\mathrm{model}},d_{\mathrm{ff}}+1).
$$

Например, при $d_{\mathrm{ff}}=1$ столбец $H=(0,1)^\top$, строка $W_2=(1,0)$ и bias $b_2=(0,1)^\top$ дают

$$
HW_2+\mathbf1b_2^\top=
\begin{pmatrix}0&1\\1&1\end{pmatrix},
$$

матрицу ранга 2. Это проверка границы применимости, а не новая авторская теорема. Для широкой MLP с $d_{\mathrm{ff}}\ge d_{\mathrm{model}}$ размерностная граница $\min(n,d_{\mathrm{model}})$ сохраняется.

Оценка относится к **выходу MLP**. Добавление $Z$ по residual-ветви может изменить ранг суммы, поэтому из неё нельзя заключить, что весь блок обязательно сжимает представление. См. также [карточку 21](21-residual-head-rank-and-mlp.md).

## Почему Q/K имеют избыточную параметризацию

Score-side матрица

$$
M^{(\ell,h)}=W_Q^{(\ell,h)}(W_K^{(\ell,h)})^\top
$$

не меняется при

$$
W_Q\mapsto W_QA,\qquad W_K\mapsto W_KA^{-\top},
\qquad A\in GL(d_k).
$$

Проверка состоит из равенства

$$
W_QA(W_KA^{-\top})^\top=W_QAA^{-1}W_K^\top=W_QW_K^\top.
$$

Значит, при фиксированном $X$ неизменен $L=XMX^\top$. [Proposition 8.5 и proof](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#proposition-8-5).

Инвариантный объект слоя — мультимножество $\{M^{(\ell,h)}\}_{h=1}^H$. «Мультимножество» сохраняет повторения; замена его обычным множеством потеряла бы кратность одинаковых голов.

## Группа в исходнике требует уточнения

Автор печатает $\mathfrak S_H\times GL(d_k)^H$. Независимые преобразования каждой головы и перестановки голов в естественном действии не коммутируют: перестановка переименовывает и соответствующие множители $GL(d_k)$. Поэтому естественная совокупная группа имеет вид

$$
GL(d_k)^H\rtimes\mathfrak S_H
$$

(порядок записи зависит от выбранного соглашения). Это редакторское уточнение структуры группы; отдельные равенства инвариантности и неизменность мультимножества $M$ остаются верными. Исходная формула сохранена в [PDF 292](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#display-8-head-gauge-group).

Не следует расширять Proposition 8.5 до утверждения, что равная $M$ всегда даёт единственную gauge-орбиту всех вырожденных факторизаций, или что $M$ определяет весь attention block. В этой формуле не представлены values, выходная проекция, biases и MLP.

## Иллюстрации

[Figure 8.3](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#figure-8-3) показывает совместную перестановку; [Figure 8.4](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#figure-8-4) — Q/K-факторизацию. Во второй схеме часть подписей накладывается друг на друга уже в PDF. Изображение сохранено непосредственно из источника, без дорисовывания.
