---
title: "93. Ранг пакетного градиента и измеримые характеристики моторов"
source_pdf_pages: [334, 336]
book_pages: [298, 300]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 93. Ранг пакетного градиента и измеримые характеристики моторов

[Индекс](../README.md) · [Начало Theorem 9.23](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#theorem-9-23) · [Продолжение и доказательство](../source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#theorem-9-23-continued) · [Карточка 92](92-sa-population-maps-and-local-gradient-rank.md) · [Далее: анизотропия](94-value-path-anisotropy-and-normalisation.md)

## Точный результат для decoder contribution

Для одного masked position вклад градиента линейного декодера — внешнее произведение $A_m^\top\delta_m$, где $\delta_m=z_m-t_m$. После объединения $B$ таких позиций:

$$
G_D=A^\top\Delta,\qquad
A\in\mathbb R^{B\times d_{\mathrm{model}}},\quad
\Delta\in\mathbb R^{B\times D}.
$$

Поэтому

$$
\operatorname{rank}G_D
\le\min(\operatorname{rank}A,\operatorname{rank}\Delta)
\le\min(B,d_{\mathrm{model}},D).
$$

Это точная алгебраическая граница из **Theorem 9.23**, а не утверждение, что градиент обязательно имеет малый ранг относительно размеров задачи. При большом числе различных позиций или накопленных вкладов ранг может быть большим. Усреднение loss вместо суммы изменяет масштаб, но не ранг ненулевого градиента.

Редакторское уточнение для нормированных target distributions: $\Delta\mathbf1_D=0$, поэтому $\operatorname{rank}\Delta\le D-1$. Если веса декодера связаны с embedding-таблицей, $A^\top\Delta$ описывает именно decoder contribution; дополнительный путь через embedding тоже должен попасть в полный градиент общих параметров.

## Эффективный ранг требует определения и контроля обусловленности

На PDF 335 автор дополняет точный результат фразой об effective rank и сам оговаривает conditioning effects. Отдельно фиксируйте, что означает «эффективный»: порог сингулярных чисел, stable rank, entropy rank или другая метрика.

Редакторский пример:

$$
\Delta=\begin{pmatrix}1&0\\0&\epsilon\end{pmatrix},
\qquad
A^\top=\begin{pmatrix}1&0\\0&\epsilon^{-1}\end{pmatrix},
\qquad
A^\top\Delta=I_2.
$$

При малом $\epsilon>0$ относительный спектр $\Delta$ почти одномерен, а спектр произведения совершенно равномерен. Точный ранг обоих равен 2. Для stable rank значения равны $1+\epsilon^2$ и 2 соответственно. Это пример общей матричной операции; ограничение zero-sum строк ошибки можно сохранить, заменив $\Delta$ на

$$
\begin{pmatrix}1&-1&0&0\\0&0&\epsilon&-\epsilon\end{pmatrix},
$$

после чего два сингулярных числа произведения также становятся равными.

Корректная односторонняя оценка:

$$
\sigma_{r+1}(A^\top\Delta)
\le\lVert A\rVert_{\mathrm{op}}\sigma_{r+1}(\Delta).
$$

Она получается из rank-$r$ приближения $\Delta_r$ и не обещает сохранения относительного effective rank. [Продолжение Theorem 9.23 и Remark 9.24](../source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#theorem-9-23-continued).

## Что измерять

[§9.20](../source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#section-9-20) предлагает нормы моторов, точный и эффективный ранги, Q/K/V energy по головам, softmax tangent energy, долю активных ReLU-каналов, норму $d\Sigma$ и её согласование с наблюдаемым смещением сигнатуры.

Для сравнимого отчёта нужны определение каждой нормы, способ агрегации, loss reduction, размер batch, число masked positions и порог effective rank. Косинус с observed signature displacement не определён при нулевой норме; смена активной грани требует отдельного режима интерпретации.

## Статус чисел и иллюстраций

[Table 9.2](../source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#table-9-2) приводит синтетические значения: exact rank 30, effective rank 19.2, active ReLU fraction 0.56, 12 голов и mean $\lVert J_{\mathrm{softmax}}\mathbf1\rVert_2=2.8\cdot10^{-17}$. Это данные рукописи; здесь такие эксперименты не запускались.

В [Figure 9.13](../source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#figure-9-13) оранжевая кривая при 30 слагаемых заканчивается примерно около 22, что не совпадает с табличными 19.2. Оба представления сохранены. Отсутствие определения effective rank и исходных массивов не позволяет устранить это расхождение по книге.

**Remark 9.24 прямо ограничивает связь с LoRA:** локальная структура внешних произведений объясняет возможный мотив для параметризации обновлений, но не доказывает эффективность конкретного метода, сохранение качества или ускорение обучения Xiigen.
