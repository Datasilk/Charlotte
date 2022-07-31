
(function () {
    window.scrolltoinfo = [];
    //scroll to bottom of page to make sure all lazy loaded images are loaded
    function scrollToBottom(bottom, y, callback) {
        window.scrollTo(0, y);
        window.scrolltoinfo.push(y);
        if (y >= bottom) { callback(); return; }
        var y2 = y + 500;
        if (y2 > bottom) { y2 = bottom; }
        if (y2 == y) { callback(); return; }
        setTimeout(() => { scrollToBottom(bottom, y2, callback); }, 10);
    }
    scrollToBottom(document.body.scrollHeight, 0, () => { });
})();