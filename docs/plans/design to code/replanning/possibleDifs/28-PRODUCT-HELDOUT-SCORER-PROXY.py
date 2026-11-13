#!/usr/bin/env python3
"""PROXY ONLY: audit demonstrator of TranslationProductHeldOutRunner scoring predicates.
Not compiled C#, not a ProductHost call, not proof of training gain.
"""
import json, re, unicodedata, collections

def norm(s):
    return ' '.join(re.sub(r'\s+', ' ', s).strip().strip('.!?').lower().split())

def contains(text, marker):
    return (not marker.strip()) or marker.lower() in text.lower()

def is_script(text, lang):
    has_latin = any('a' <= c.lower() <= 'z' for c in text)
    has_ru = any('\u0400' <= c <= '\u04ff' for c in text)
    has_he = any('\u0590' <= c <= '\u05ff' for c in text)
    return ((has_latin and not has_ru and not has_he) if lang == 'en' else
            (has_ru and not has_he) if lang == 'ru' else
            (has_he and not has_ru) if lang == 'he' else True)

def copied(output, source, lang):
    if norm(source) and norm(source) in norm(output): return True
    return (any('\u0400' <= c <= '\u04ff' or '\u0590' <= c <= '\u05ff' for c in output) if lang=='en' else
            not any('\u0400' <= c <= '\u04ff' for c in output) or any('\u0590' <= c <= '\u05ff' for c in output) if lang=='ru' else
            not any('\u0590' <= c <= '\u05ff' for c in output) or any('\u0400' <= c <= '\u04ff' for c in output) if lang=='he' else False)

def tokens(s): return re.findall(r"[\w']+", s.lower(), flags=re.UNICODE)
def bleu1(actual,expected):
    a,e=tokens(actual),tokens(expected)
    if not a or not e:return 0
    ec=collections.Counter(e);n=0
    for t in a:
        if ec[t]>0: ec[t]-=1; n+=1
    return n/len(a)

def chrf1(actual, expected):
    a=collections.Counter(c for c in actual.lower() if not (c.isspace() or unicodedata.category(c)[0] in 'PS'))
    e=collections.Counter(c for c in expected.lower() if not (c.isspace() or unicodedata.category(c)[0] in 'PS'))
    overlap=sum(min(n,e[c]) for c,n in a.items())
    p=overlap/sum(a.values()) if a else 0
    r=overlap/sum(e.values()) if e else 0
    return (2*p*r/(p+r)) if p+r else 0

def score(name,source,expected,actual,required=None,forbidden=None,emoji=None,entities=None,language='en'):
    required=required or [];forbidden=forbidden or [];emoji=emoji or [];entities=entities or []
    missing=[v for v in required if not contains(actual,v)]
    found=[v for v in forbidden if contains(actual,v)]
    script=is_script(actual,language)
    copy=copied(actual,source,language)
    emoji_preserved=all(v in actual for v in emoji)
    entities_preserved=all(contains(actual,v) for v in entities)
    product_match=('BOOTSTRAP]' not in actual.upper() and not missing and not found and script and not copy and emoji_preserved and entities_preserved)
    return dict(case=name,source=source,expected=expected,actual=actual,
                product_match=bool(product_match),exact=(norm(actual)==norm(expected)),
                bleu1=round(bleu1(actual,expected),4),chrf1=round(chrf1(actual,expected),4),
                emoji_preserved=emoji_preserved,missing_required=missing)

result={
  'status':'PYTHON_LOGIC_PROXY_ONLY','model_training':'NOT_RUN','native_dotnet':'NOT_RUN',
  'source_sha':'d5c2409b24292d361593b8262b0c54997b15f539',
  'cases':[
    score('no_markers_unrelated','Привет мир','Hello world','banana'),
    score('negation_lost_with_markers','Не удаляй файл','Do not delete the file','Delete the file',required=['delete','file']),
    score('reversed_roles','Собака кусает человека','dog bites man','man bites dog',required=['dog','man']),
    score('emoji_missing_without_declaration','Привет 🙂','Hello 🙂','Hello')
  ],
  'ceiling_delta_example': {'unit_accuracy_before':.99,'unit_accuracy_after':1.0,
                            'required_delta':.02,'meets_improvement_clause':1.0 >= .99+.02}
}
print(json.dumps(result,ensure_ascii=False,indent=2))
