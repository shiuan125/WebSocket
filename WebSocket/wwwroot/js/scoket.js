var socket;
var l = document.location;
var scheme = l.protocol === 'https:' ? 'wss' : 'ws';
var port = l.port ? (':' + l.port) : '';
var wsUrl = scheme + '://' + l.hostname + port + '/ws';
function logWebSocketStatus(event) {
    if (!socket) return;
    var status = 'Unknown';
    switch (socket.readyState) {
        case WebSocket.CLOSED:
            status = 'Closed / Code = ' + event.code + ', Reason = ' + event.reason;
            break;
        case WebSocket.CLOSING:
            status = 'Closing';
            break;
        case WebSocket.OPEN:
            status = 'Open';
            break;
        case WebSocket.CONNECTING:
            status = 'Connecting';
            break;
    }
}
function connect() {
    socket = new WebSocket(wsUrl);
    socket.onopen = function () {
        logWebSocketStatus();
        userName.onchange();
    };
    socket.onclose = logWebSocketStatus;
    socket.onerror = logWebSocketStatus;
    socket.onmessage = function (e) {
        processMessage(e.data);
    }
}
var list = document.getElementById('list');
function processMessage(data) {
    let li = document.createElement('li');
    li.innerHTML = "<span class=t></span><span class=u></span><span class=m></span>";
    let p = data.split('\t');
    li.querySelector('.t').innerText = p[0];
    li.querySelector('.u').innerText = p[1];
    li.querySelector('.m').innerText = p[2] || '';
    list.appendChild(li);
}
function sendMessage(msg) {
    if (socket && socket.readyState == WebSocket.OPEN)
        socket.send(msg);
}
connect();
var userName = document.getElementById('userName');
userName.value = 'User' + (new Date().getTime() % 10000);
userName.onchange = function () {
    sendMessage('/USER ' + userName.value);
};
var message = document.getElementById('message');
var sedn = document.getElementById('send');
message.addEventListener('keydown', function (e) {
    if (e.keyCode === 13) send.click();
});
send.addEventListener('click', function () {
    sendMessage(message.value);
});