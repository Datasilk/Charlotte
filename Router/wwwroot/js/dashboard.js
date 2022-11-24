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
            dashboardHub.start().catch(hubError);
            setTimeout(() => { dashboardHub.invoke('UpdateConsole'); }, 500);
        }
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