---
title: "292. Lanczos при кратности и kernel, контракт QR и точная граница PDF 700"
source_pdf_pages: [700, 700]
book_pages: [664, 664]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 292. Lanczos при кратности и kernel, контракт QR и точная граница PDF 700

## Вопрос

Какие обещания applicability Lanczos требуют дополнительных условий и какая часть incremental QR уже прочитана?

**На PDF 700 завершены сравнения spectral methods и лишь начат §17.4.4 об incremental QR.** Сам алгоритмический display (17.34), его шаги и последующие доказательства находятся вне batch 014. До этой границы уже можно проверить две существенные ошибки: один scalar-Krylov seed не восстанавливает весь кратный eigenspace, а нулевое Ritz value не удостоверяет kernel. [Applicability и начало QR](../source/0699-0700-ch17-lanczos-two-step-and-orthogonalisation.md#pdf-0700).

## Кратное пространство нельзя получить одним seed

Пусть $V$ — eigenspace eigenvalue $\lambda$ кратности $m>1$, $P_V$ — его проектор. Для любого полинома $p$

$$
P_Vp(A)u_0=p(\lambda)P_Vu_0.
$$

Следовательно, проекция всего $\mathcal K_\tau(A,u_0)$ в $V$ имеет размерность не более единицы независимо от depth. Перезапуски, использующие только полученный Ritz-вектор, не добавляют независимого направления в этот exact eigenspace. Поэтому авторское “the projector is recovered exactly” для Type A не следует из двух Ritz-векторов одного scalar pass. Нужны независимые seeds, block method или отдельная процедура дополнения basis и проверки rank. Для индивидуального направления зависимость от initialisation естественна; это не равнозначно восстановлению rank-$m$ projector.

## Точный контрпример к kernel detection по Ritz value

Рассмотрим обратимую signed матрицу и единичный seed

$$
A=\operatorname{diag}(-1,1,2),\qquad
u_0=(1,\sqrt{11},\sqrt{11})/\sqrt{23}.
$$

Настоящий Lanczos $m=2$ даёт $T_2=U^\top AU$ с eigenvalues $0$ и $7/4$. Для проверки его moments равны

$$
u_0^\top Au_0=\frac{32}{23},\quad
u_0^\top A^2u_0=\frac{56}{23},\quad
u_0^\top A^3u_0=\frac{98}{23};
$$

их соотношение $32\cdot98=56^2$ обеспечивает $\det T_2=0$. Соответствующий нулевой Ritz-вектор можно записать точно:

$$
\widehat v=\frac{(-11,-3\sqrt{11},\sqrt{11})}{\sqrt{231}},
\qquad
\widehat v^\top A\widehat v=0,\qquad
\|A\widehat v\|^2=\frac87.
$$

То есть residual имеет норму около $1.069$, хотя Ritz value равно нулю, а настоящего kernel вообще нет. Добавление этого вектора в «нулевой» deflation basis удалит значимую геометрию. Перед rank truncation нужны eigenpair residual $\|A\widehat v-\widehat\lambda\widehat v\|$, spectral separation и согласованный tolerance. Этот пример относится именно к двухвекторному Lanczos, не только к произвольному Rayleigh quotient.

## Reorthogonalisation и размер basis

При $\beta_0$ близком к нулю вычитание $Au_0-\alpha_0u_0$ и последующая нормализация могут усиливать roundoff. Поэтому отсутствие длинной цепочки Lanczos не означает отсутствия любых orthogonality errors. Reorthogonalisation при $m=2$ действительно стоит $O(d)$, но для полного pass depth $\tau$ последовательная полная reorthogonalisation требует суммарно $O(\tau^2d)$ операций: на очередной vector — $O(\tau d)$ в худшем случае. Авторское $O(\tau d)$ “per pass” требует уточнения единицы работы.

Для QR следует различать head count $K$, block ranks и число всех stored columns $r_Q$, включая kernel directions. Тогда $Q\in\mathbb R^{d\times r_Q}$, а активная размерность — $d-r_Q$. Ортонормированность обеспечивает projector identity в точной арифметике; floating-point multiplication всё равно даёт roundoff. Ошибка residual в зависимости от $\|Q^\top Q-I\|$ разобрана в [карточке 284](284-orthogonality-defect-and-unproved-dwell-time-speedup.md).

## Точная граница чтения

Последний абзац PDF 700 озаглавлен **Incremental QR via Givens rotations** и заканчивается словами:

> “… a sequence of Givens rotations that reorthogonalise it against the existing columns of $Q$:”

Это введение, **не полный algorithm**. Preview PDF 701 начинается фразой “Given $Q^{(K)}=\widetilde Q^{(K)}R^{(K)}$ in QR form, append column $u_{K+1}$:” внутри display **(17.34)**. Ни его шаги, ни последующие доказательства, ни Table 17.2 в этот пакет не включены. [Начало §17.4.4](../source/0699-0700-ch17-lanczos-two-step-and-orthogonalisation.md#section-17-4-4), [незавершённое предписание](../source/0699-0700-ch17-lanczos-two-step-and-orthogonalisation.md#prescription-0700-incremental-qr-introduction).

Таким образом, §17.4.4 и глава 17 остаются открытыми; следующий диапазон — **PDF 701–750 / печатные страницы 665–714**. Никакие свойства ещё не прочитанной QR-процедуры здесь не объявляются установленными.

## Продолжение опубликовано в пакете 015

Описанная выше граница относится к пакету 014. Теперь §17.4.4 полностью перенесён на PDF 700–702: [display (17.34), Table 17.2 и продолжение](../source/0701-0702-ch17-incremental-qr-and-spectral-method-comparison.md#pdf-0701). Согласованность QR-обозначений, предпосылки и пределы гарантий разобраны в [карточке 293](293-incremental-qr-symbols-reprojection-and-stability.md). Исторический текст карточки сохранён.
