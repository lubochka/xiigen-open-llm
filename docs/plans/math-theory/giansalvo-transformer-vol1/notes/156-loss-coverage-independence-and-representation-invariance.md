---
title: "156. Почему loss, coverage и directional energy не заменяют друг друга"
source_pdf_pages: [469, 471]
book_pages: [433, 435]
batch: "010"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 156. Почему loss, coverage и directional energy не заменяют друг друга

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Основная идея и её корректное ядро

Theorem 13.14 утверждает возможность $\mathcal V_{\mathrm{Cov}}=\mathcal V_\Gamma=0$ при $\mathcal V_L>0$. Идея верна: геометрия span не определяет точное значение суммируемого выхода. Особенно просто взять ненулевую симметричную голову, цель $u^*=\sum_j u_j$ и квадратичный loss $L(u)=\frac12\|u-u^*\|_F^2$. Удаление головы меняет loss на $\frac12\|u_h\|_F^2>0$, хотя её skew activation нулевая. Совпадение направлений с другими головами также не компенсирует удалённый вектор автоматически. [Theorem 13.14](../source/0468-0471-ch13-marginal-signs-and-independence.md#theorem-13-14).

## Ограничения напечатанного построения

Источник смешивает coverage через $\operatorname{range}\mathcal A_h$ и span фактических выходов. Первое — skew-оператор на feature space, второе зависит также от softmax, values и output projection. Их равенство не установлено.

Из $3d_k>d_{\mathrm{model}}$ не следует, что две оставшиеся головы заполняют всё пространство: их суммарная размерность не превосходит $2d_k$. В числовом примере $H=3,d_k=4,d_{\mathrm{model}}=16$ имеем $Hd_k=12<16$, хотя ранее требовался saturated regime. Кроме того, $n=8<d_{\mathrm{model}}=16$ делает standing assumption H-rank ($\operatorname{rank}X=d_{\mathrm{model}}$) невыполнимой для этого примера. Одного seed и четырёх размеров недостаточно для точного воспроизведения заявленного $15.7$: нужны распределения, нормировки, сами projections и процедура построения. [PDF 469–470](../source/0468-0471-ch13-marginal-signs-and-independence.md#pdf-0469).

При напечатанной цели $u^*=u_{\mathrm{full}}+\epsilon$ правильная **разность** квадратичных потерь равна

$$
\tfrac12\|u_h+\epsilon\|_F^2-\tfrac12\|\epsilon\|_F^2
=\tfrac12\|u_h\|_F^2+\langle u_h,\epsilon\rangle,
$$

а не $\frac12\|u_h-\epsilon\|_F^2$. Ненулевой Gaussian noise также не даёт точной стационарности и безусловного положительного знака. Для строгого существования достаточно взять $\epsilon=0$.

## Что не доказывает H-rep

Theorem 13.15 вводит инвариантность loss по row space, хотя в тексте она названа column-space invariance. Переход (13.23) не следует из $\mathcal V_{\mathrm{Cov}}=0$: даже неизменность **суммы пространств** выходов не означает неизменность пространства **суммы матриц**. Например, $u_1=u$, $u_2=-u$ дают нулевую сумму; после удаления одной головы остаётся ненулевой span. [H-rep и доказательство](../source/0468-0471-ch13-marginal-signs-and-independence.md#hypothesis-h-rep).

Full-column-rank линейный classifier не обеспечивает H-rep: уже в одномерном случае $u=1$ и $u'=2$ имеют одинаковый span, но fixed-target squared loss после classifier $W=1$ различается.

Более того, если $L:\mathbb R^{n\times d}\to\mathbb R$ всюду конечна, выпукла и глобально зависит только от row space, она постоянна. Для любого $t>0$ пространства $u$ и $tu$ совпадают, поэтому $L(u)=L(tu)$; непрерывность конечной выпуклой функции и $t\downarrow0$ дают $L(u)=L(0)$. При этих буквальных глобальных условиях условная теорема может быть пустой по содержанию. Для нетривиальной subspace loss нужно уточнять область, нормировку, исключение нуля или отказ от глобальной выпуклости.

Conjecture 13.16 сохраняет статус гипотезы. Без независимой верхней границы на $\mathcal R^{(\ell)}(h)$ формула с остатком не ограничивает marginal loss: в остаток можно включить всю необъяснённую величину. [Гипотеза](../source/0468-0471-ch13-marginal-signs-and-independence.md#conjecture-13-16).
