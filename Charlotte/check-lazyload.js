var elems = [...document.querySelectorAll('*')];
for (var x = 0; x < elems.length; x++) {
    var el = elems[x];
    var c = el.getAttribute('class') || '';
    if (c.indexOf('lazy') >= 0) { return true; }
    if (el.tagName == 'img' &&
        (el.hasAttribute('src') == false || el.getAttribute('src') == '') &&
        (el.hasAttribute('srcset') == false || el.getAttribute('srcset') == '')
    ) {
        return true;
    }
}
return false;