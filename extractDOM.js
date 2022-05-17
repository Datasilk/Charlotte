﻿(function () {
    var knownAttrs = [];
    var knownTags = ['a', 'p', 'div', 'span', '#text'];//prepopulate with most used tag names first
    var knownErrors = [];


    function getDOM(node) {
        var dom = walk(node);
        return { t: knownTags, a: knownAttrs, dom: dom, err: knownErrors };
    }

    function walk(node) {
        switch (node.nodeType) {
            case 1: case 3: //element node or text node
                //get computed style
                var style;
                var name = null;

                if (node.nodeType == 1) {
                    style = window.getComputedStyle(node);
                    name = node.tagName.toLowerCase();
                } else {
                    style = node.parentNode ? window.getComputedStyle(node.parentNode) : null;
                    name = "#text";
                }
                
                //convert style.display to int
                var display = 1;
                switch (style.display) {
                    case 'none':
                        //switch (name) {
                        //    //hidden elements that are required as part of the DOM
                        //    case "head": case "title": case "meta": case "#text":
                        //        display = 0;
                        //        break;
                        //    default: return null;
                        //}
                        display = 0; break;
                    case 'inline':
                        display = 2; break;
                    case 'inline-block':
                        display = 3; break;
                }

                //generate parent node object
                var tagIndex = knownTags.indexOf(name);
                if (tagIndex < 0) {
                    tagIndex = knownTags.length;
                    knownTags.push(name);
                }
                var parent = {
                    t: tagIndex,
                    s: [display,                                    //[0] display
                        parseInt(style.fontSize.replace('px', '')), //[1] font-size
                        parseInt(style.fontWeight) >= 700 ? 2 : 1,  //[2] font-weight
                        style.fontStyle == 'italic' ? 1 : 0         //[3] italic
                    ]
                };

                if (node.nodeType == 1) {
                    parent.a = {};
                    parent.c = [];

                    //check for invalid tags
                    switch (knownTags[parent.t]) {
                        case "style": case "script": case "link": case "svg": case "canvas": case "object":
                        case "embed": case "input": case "select": case "button": case "audio":
                        case "textarea": case "iframe": case "area": case "map": case "noscript":
                            return null;
                    }

                    //check for large meta tags
                    var attrs = [...node.attributes];
                    if (knownTags[parent.t] == "meta" && attrs.length > 0) {
                        var metacontent = attrs.filter(a => a.name == 'content');
                        if (metacontent.length > 0 && metacontent[0].value.length > 64) {
                            return null;
                        }
                    }

                    //get attributes for node
                    for (var x = 0; x < attrs.length; x++) {
                        switch (attrs[x].name) {
                            case "style": case "id": case "tabindex": case "index": case "role": case "onclick":
                            case "onchange": case "oninput": case "onsubmit": case "rel": case "loading":
                            case "width": case "height": case "media": case "itemscope":
                                //ignore unwanted attributes
                                break;
                            default:
                                if (attrs[x].anem == "itemprop" && attrs[x].value == 'image') { break }
                                if (attrs[x].name.indexOf('data-') == 0) { break; }
                                if (attrs[x].name.indexOf('aria-') == 0) { break; }
                                if (attrs[x].name.indexOf('xmlns') == 0) { break; }
                                if (knownAttrs.indexOf(attrs[x].name) < 0) {
                                    //add name to known attributes list
                                    knownAttrs.push(attrs[x].name);
                                }
                                if (attrs[x].value.indexOf('data:image') >= 0) {
                                    //embedded images (base64) should be ignored
                                    return null;
                                }

                                var attr;
                                if (attrs[x].name == 'src') {
                                    attr = node.src; //get absolute src
                                }
                                else if (attrs[x].name == 'srcset') {
                                    attr = node.srcset; //get absolute srcset
                                }
                                else if (attrs[x].name == 'href') {
                                    attr = node.href; //get absolute href
                                }
                                else {
                                    attr = attrs[x].value;
                                }
                                parent.a[knownAttrs.indexOf(attrs[x].name)] = clean(attr);
                                break;
                        }

                    }

                    //generate all child nodes
                    var children = node.childNodes;
                    for (var i = 0; i < children.length; i++) {
                        var child = walk(children[i]);
                        if (child != null) {
                            parent.c.push(child);
                        }
                    }

                    return parent;

                } else if (node.nodeType == 3) {
                    //#text node
                    var val = node.nodeValue.trim();
                    var nobreaks = false;

                    //check for pre tag to negate removing line-breaks from #text node
                    if (node.parentNode) {
                        if (node.parentNode.tagName.toLowerCase() == 'pre') {
                            nobreaks = true;
                        } else if (node.parentNode.tagName.toLowerCase() == 'code') {
                            if (node.parentNode.parentNode.tagName.toLowerCase() == 'pre') {
                                nobreaks = true;
                            }
                        }
                    }
                    val = val.replace(/\r\n/g, '\n').replace(/\r/g, '\n').trim();
                    if (nobreaks == false) {
                        val = val.replace(/\n/g, ' ').replace(/\r/g, '');
                    }
                    if (val != '') {
                        //replace unknown characters in text
                        parent.v = clean(val);
                        return parent;
                    }
                }
                break;
        }
        return null;
    }

    function clean(str) {
        return unescape(str
        .replace(/\u00a0/g, " ") //replace &nbsp; with a space
        .replace(/“/g, '"') //replace open quotes
        .replace(/”/g, '"') //replace close quotes
        .replace(/"/g, '"') //escape quotes (for C#)
        .replace(/[\u{0080}-\u{FFFF}]/gu, '')); //remove unknown characters
    }

    window["__getDOM"] = getDOM;
})();
__getDOM(document.body.parentNode);