//dashboard 
var dashboardHub = null;

function showConsole() {
    var consl = document.getElementsByClassName('console')[0];
    if (consl.className.indexOf('show') >= 0) {
        //hide console
        consl.classList.remove('show');
        consl.classList.add('hide');
        //dashboardHub.stop();
    } else {
        //show console and load SignalR hub
        consl.classList.remove('hide');
        consl.classList.add('show');
        if (dashboardHub == null) {
            dashboardHub = new signalR.HubConnectionBuilder().withUrl('/dashboardhub').build();
            dashboardHub.on('update', logEvent);
            dashboardHub.on('response', toggleCharlotteResult);
            dashboardHub.start().catch(hubError);
            setTimeout(() => { dashboardHub.invoke('UpdateConsole'); }, 500);
        }
    }
}

function toggleCheckUrl() {
    var modal = document.getElementsByClassName('check-url')[0];
    var btn = document.getElementsByClassName('btn-checkurl')[0];
    if (modal.className.indexOf('hide') >= 0) {
        modal.classList.remove('hide');
        btn.classList.add('hide');
    } else {
        modal.classList.add('hide');
        btn.classList.remove('hide');
    }
}

function submitUrl() {
    dashboardHub.invoke('CheckUrl', charlotte_url.value, charlotte_session.checked, charlotte_macros.value);
}

function toggleCharlotteResult(result) {
    var modal = document.getElementsByClassName('charlotte-result')[0];
    if (modal.className.indexOf('hide') >= 0) {
        modal.classList.remove('hide');
        charlotte_result.value = result;
        document.querySelectorAll('.charlotte-result h6')[0].innerHTML = 'Results for ' + charlotte_url.value;
        toggleCheckUrl();
    } else {
        modal.classList.add('hide');
    }
}

function hubError(e) {
    console.log(e);
}

function logEvent(msg) {
    var div = document.createElement("div");
    div.innerHTML = msg;
    document.querySelectorAll('.console .scrollable')[0].appendChild(div);
}