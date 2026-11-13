---
title: "360. Appendix B: локальный смысл символов, размерности и завершение PDF 800"
source_pdf_pages: [797, 800]
book_pages: [761, 764]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 360. Appendix B: локальный смысл символов, размерности и завершение PDF 800

## Вопрос

Можно ли считать заключительный словарь единой схемой обозначений для всех глав?

**Сам Appendix B предупреждает, что он служит lookup table, а определения нужно брать там, где объект введён.** Это важно для RAG: одинаковый символ не означает одинаковый объект. [Начало Appendix B](../source/0797-0800-appendix-b-notation-summary.md#appendix-b).

## Как разбирать неоднозначные записи

| Запись Appendix B | Что проверить при использовании |
|---|---|
| $W_Q,W_K,W_V\in\mathbb R^{d\times d_k}$ | Общая запись верна для values при $d_v=d_k$; иначе $W_V$ имеет ширину $d_v$. Равенство для BERT-base указано отдельно, но не является универсальным определением. |
| $A_{\mathrm{res}}$ | Здесь residual attention matrix; не отождествлять без проверки с signed head operator другого размера. |
| $\Gamma,\Gamma_{\mathrm{res}}$ | Здесь scalar directional energies; конкретную сумму, norm и нормировку брать из соответствующей главы. |
| $\sigma^{(h,\ell)}$ | Здесь signature tuple; в других формулах $\sigma$ может быть activation, а $\sigma_i(M)$ — singular value. |
| $K$, $H$, $k$ | Controller count, число heads и число выбранных token partners могут быть разными величинами. |
| $\theta_w$ | Здесь назван warm-start threshold; §19.11 использует ту же запись для residual budget. Нельзя автоматически считать обе формулировки одной величиной. |
| $K^\star$ | Glossary называет его converged count; §19.11 отдельно уточняет, что spectral reference не гарантированно является attractor. |
| $\kappa$, $\kappa_T$ | Regime label и temperature constant — разные объекты, несмотря на общий буквенный корень. |

[Архитектурные размерности](../source/0797-0800-appendix-b-notation-summary.md#section-b-3), [motor и controller symbols](../source/0797-0800-appendix-b-notation-summary.md#pdf-0799), [ограничение $K^\star$](../source/0788-0791-ch19-residual-questions-spectral-design-and-conclusion.md#pdf-0790).

В §B.5 signature названа 8-tuple по dictionary из восьми regimes. Это утверждение именно данного словаря; при иной signature schema в конкретной главе её нельзя молча приводить к восьми координатам. Head/layer accents и номер страницы должны сопровождать extracted definition.

## Нумерация и незаполненные ссылки

Appendix B сообщает общий counter для theorem-class objects и отдельный для equations. Следовательно, “19.5” без типа неоднозначно: это может быть Lemma 19.5 или equation (19.5). Индекс должен хранить тип и отдельный anchor.

В citation example на PDF 800 буквально напечатаны “?” и “(?)”. Они сохранены как содержимое рукописи, а не заполнены предположением о missing author. Словарь также не заменяет bibliography, которая ещё находится за текущей границей.

## Точная граница пакета

PDF 800 / книга 764 завершает §B.8 и всё приложение B фразой:

> “The bibliography is unified at the end of the volume; there are no per-chapter reference lists.”

Продолжение начинается с **PDF 801 / книга 765 — Appendix C, Experimental Pipeline**. Незавершённого предложения на границе нет. PDF 801 просмотрена только для определения следующей точки; её содержание не входит в пакет 016.

