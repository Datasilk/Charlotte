
(function () {
    window.scrolltoinfo = [];
    //scroll to bottom of page to make sure all lazy loaded images are loaded
    function scrollToBottom(bottom, y) {
        window.scrollTo(0, y);
        window.dispatchEvent(new CustomEvent('scroll'));
        window.scrolltoinfo.push(y);
        if (y >= bottom) { return; }
        var y2 = y + 100;
        if (y2 > bottom) { y2 = bottom; }
        if (y2 == y) { return; }
        setTimeout(() => { scrollToBottom(bottom, y2); }, 10);
    }
    scrollToBottom(document.body.scrollHeight, 0);
})();