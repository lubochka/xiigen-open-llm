---
title: "166. Ранг суммы выходов голов и пределы вывода о ширине FFN"
source_pdf_pages: [489, 490]
book_pages: [453, 454]
batch: "010"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 166. Ранг суммы выходов голов и пределы вывода о ширине FFN

[Индекс](../README.md)

## Сумма подпространств даёт верхнюю границу, а не равенство

Обозначим $Z_h=Y_hW_{O,h}$ и $R_h=\operatorname{rowsp}Z_h$. В [Theorem 13.32, (13.62)](../source/0485-0490-ch13-dictionary-thresholds-and-rank-contraction.md#theorem-13-32) напечатано равенство ранга $\sum_hZ_h$ размерности $\sum_hR_h$. Общий корректный факт слабее:

$$
\operatorname{rowsp}\!\left(\sum_hZ_h\right)\subseteq\sum_hR_h,\qquad
\operatorname{rank}\!\left(\sum_hZ_h\right)
\le\dim\!\left(\sum_hR_h\right)
\le\sum_h\operatorname{rank}Z_h.
$$

При $Z_1=Z\ne0$ и $Z_2=-Z$ левая сторона равна нулю, а сумма row spaces имеет положительную размерность. Это реализуемая компенсация выходных проекций двух attention heads. Даже без точной компенсации равенство требует условий; например, число строк также ограничивает ранг суммы сверху.

Если все $R_h$ лежат в одном $V$, верхняя граница $\operatorname{rank}\operatorname{MHA}\le\dim V$ верна. Условие full column rank $Y_h$ и full row rank $W_{O,h}$ действительно даёт ранг произведения $d_k$; выражение источника «injective on its codomain» требует согласования направления линейного отображения.

## Одна skew-eigenplane не ограничивает ранг выхода числом 2

Матрица scores, row-softmax и value/output path — разные отображения. Даже pure-skew score ранга 2 после softmax может иметь больший ранг.

Простой пример: $X=I_3$, $M=M_a=J_{12}$, где ненулевые элементы $M_{12}=1$, $M_{21}=-1$, и $W_V=W_O=I_3$. У $M_a$ единственная eigenplane, но

$$
S=\operatorname{softmax}_{\rm row}(M),\qquad
\det S=\frac{e+e^{-1}-2}{3(2+e)(2+e^{-1})}>0.
$$

Поэтому $SXW_VW_O$ имеет ранг 3. Масштабирование logits на положительный $1/\sqrt{d_k}$ сохраняет этот контрпример.

Проверка совместимости с полным рангом после affine-LayerNorm даёт и четырёхмерный пример. Пусть

$$
P=I_4-\frac14\mathbf1\mathbf1^\top,\quad
X=\frac4{\sqrt3}P+\mathbf1\mathbf1^\top,\quad
q=\frac{(1,-1,0,0)^\top}{\sqrt2},\quad
v=\frac{(0,0,1,-1)^\top}{\sqrt2},
$$

$$
M_a=qv^\top-vq^\top,\quad
A=\operatorname{softmax}_{\rm row}(XM_aX^\top/2),\quad W_V=W_O=I_4.
$$

Центрированные строки $X$ имеют дисперсию 1; affine shift $\beta=\mathbf1$ делает $X$ полноранговой. Ранг $M_a$ равен 2, а сингулярные значения $A$ приблизительно $1,0.582783,0.582783,0.339636$, поэтому $\operatorname{rank}(AX)=4$. Этот пример проверяет именно неверный алгебраический переход «одна eigenplane ⇒ двумерный выход»; он не является экспериментом обучения или утверждением о распределении специализаций большого слоя.

## Какой dimension count возможен для FFN

[Corollary 13.33, (13.64)](../source/0485-0490-ch13-dictionary-thresholds-and-rank-contraction.md#corollary-13-33) заявляет $d_{\mathrm{ff}}^*\ge2d_{\mathrm{model}}-r$. Приведённый подсчёт этого не доказывает.

Для остаточного шага $Z\mapsto Z+F(Z)$ без output bias, где $F(Z)=\operatorname{GeLU}(ZW_1+b_1)W_2$, имеем $\operatorname{rank}F(Z)\le d_{\mathrm{ff}}$. Следовательно, необходимое условие полного матричного ранга:

$$
d_{\mathrm{model}}\le\operatorname{rank}(Z+F(Z))
\le r+d_{\mathrm{ff}},
\qquad n\ge d_{\mathrm{model}}.
$$

Отсюда следует лишь $d_{\mathrm{ff}}\ge d_{\mathrm{model}}-r$, а не $2d_{\mathrm{model}}-r$. При уже полноранговом $Z$ малый generic FFN сохраняет полный ранг по открытости множества обратимых матриц; ширина $d_{\mathrm{model}}$ для этого сама по себе не необходима. Без residual path требование другое: весь выход $F(Z)$ имеет ранг не выше ширины. Output bias также нужно учитывать отдельно.

Наконец, увеличение ранга матрицы на конечном наборе токенов не восстанавливает потерянную информацию об исходном входе, если отображение до FFN уже склеило разные входы. Нелинейное расширение представления и обратимость отображения не эквивалентны. Даже корректная необходимая нижняя граница ширины не доказывает достаточность конкретной ширины или избыточность стандартной архитектуры.
