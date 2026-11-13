"""Small safe renderer for the Markdown features used by the local report.

Standard library only. Formula and Mermaid source stays copyable; no external
renderer or network dependency. The complete original Markdown is also embedded.
"""
import html
import re


def render_markdown(markdown, prefix="report"):
    refs = {}
    lines = []
    for line in markdown.splitlines():
        m = re.match(r"^\[([^]]+)\]:\s*(https?://\S+)\s*$", line)
        if m:
            refs[m.group(1)] = m.group(2)
        else:
            lines.append(line)

    def inline(value):
        pattern = r"\[([^]]+)\]\[([^]]+)\]|\[([^]]+)\]\((https?://[^\s)]+)\)|\*\*(.+?)\*\*|`([^`]+)`"
        parts, pos = [], 0
        for match in re.finditer(pattern, value):
            parts.append(html.escape(value[pos:match.start()]))
            if match.group(1) is not None:
                label, address = match.group(1), refs.get(match.group(2))
                parts.append('<a target="_blank" rel="noopener noreferrer" href="' + html.escape(address, quote=True) + '">' + html.escape(label) + '</a>' if address else html.escape(match.group(0)))
            elif match.group(3) is not None:
                parts.append('<a target="_blank" rel="noopener noreferrer" href="' + html.escape(match.group(4), quote=True) + '">' + html.escape(match.group(3)) + '</a>')
            elif match.group(5) is not None:
                parts.append('<strong>' + inline(match.group(5)) + '</strong>')
            else:
                parts.append('<code>' + html.escape(match.group(6)) + '</code>')
            pos = match.end()
        parts.append(html.escape(value[pos:]))
        return ''.join(parts)

    def table_cells(line):
        return [x.strip() for x in line.strip().strip('|').split('|')]

    out, toc, i, heading_number = [], [], 0, 0
    while i < len(lines):
        line = lines[i]
        if not line.strip():
            i += 1
            continue
        heading = re.match(r"^(#{1,6})\s+(.+)$", line)
        if heading:
            heading_number += 1
            level = min(len(heading.group(1)) + 1, 6)
            anchor = prefix + '-h-' + str(heading_number)
            out.append(f'<h{level} id="{anchor}">' + inline(heading.group(2)) + f'</h{level}>')
            if len(heading.group(1)) == 2:
                toc.append('<a href="#' + anchor + '">' + html.escape(heading.group(2)) + '</a>')
            i += 1
            continue
        if line.startswith('```'):
            language = line[3:].strip()
            i += 1
            code = []
            while i < len(lines) and not lines[i].startswith('```'):
                code.append(lines[i]); i += 1
            i += 1
            label = 'Mermaid — исходник схемы' if language == 'mermaid' else 'Исходный текст'
            out.append('<section class="narrative-code"><p class="smallhint">' + label + '</p><button type="button" class="copy-narrative-code">Скопировать</button><pre><code>' + html.escape('\n'.join(code)) + '</code></pre></section>')
            continue
        if line.strip() == r'\[':
            i += 1
            formula = []
            while i < len(lines) and lines[i].strip() != r'\]':
                formula.append(lines[i]); i += 1
            i += 1
            out.append('<pre class="narrative-math" aria-label="Точная формула в записи LaTeX">' + html.escape('\n'.join(formula)) + '</pre>')
            continue
        if line.startswith('|') and i + 1 < len(lines) and re.match(r'^\|[\s:|\-]+\|\s*$', lines[i + 1]):
            headers = table_cells(line)
            i += 2
            table = ['<div class="narrative-table"><table><thead><tr>' + ''.join('<th>' + inline(x) + '</th>' for x in headers) + '</tr></thead><tbody>']
            while i < len(lines) and lines[i].startswith('|'):
                table.append('<tr>' + ''.join('<td>' + inline(x) + '</td>' for x in table_cells(lines[i])) + '</tr>')
                i += 1
            table.append('</tbody></table></div>')
            out.extend(table)
            continue
        bullet = re.match(r'^(?:[-*]|\d+\.)\s+(.+)$', line)
        if bullet:
            tag = 'ol' if re.match(r'^\d+\.', line) else 'ul'
            items = []
            while i < len(lines):
                item = re.match(r'^(?:[-*]|\d+\.)\s+(.+)$', lines[i])
                if not item: break
                items.append('<li>' + inline(item.group(1)) + '</li>'); i += 1
            out.append('<' + tag + '>' + ''.join(items) + '</' + tag + '>')
            continue
        paragraph = [line]
        i += 1
        while i < len(lines) and lines[i].strip() and not re.match(r'^(?:#{1,6}\s|```|\| |[-*] |\d+\. |\\\[)', lines[i]):
            paragraph.append(lines[i]); i += 1
        out.append('<p>' + inline(' '.join(paragraph)) + '</p>')
    return '<nav class="narrative-toc" aria-label="Разделы текста">' + ''.join(toc) + '</nav>' + ''.join(out)
