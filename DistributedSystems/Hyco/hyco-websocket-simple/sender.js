// node sender.js relaytunnel.servicebus.windows.net Relay1 Send Y8DUJ0V9yf/p5vgqsQE5E2OM3k+KWblXsbtnY4mI75g=
// node sender.js sagerelay.servicebus.windows.net hybridrelay send_listen 1Pm7JWL94IXhVxxB9/HfoHbBWp0VvIk3Htv1r3bhatc=
if (process.argv.length < 6) {
    console.log("listener.js [namespace] [path] [key-rule] [key]");
} else {

    var ns = process.argv[2];
    var path = process.argv[3];
    var keyrule = process.argv[4];
    var key = process.argv[5]; 

    var WebSocket = require('hyco-websocket')    
    var WebSocketClient = WebSocket.client
    
    var address =  WebSocket.createRelaySendUri(ns, path);    
    var token = WebSocket.createRelayToken(address, keyrule, key);

    var client = new WebSocketClient({tlsOptions: { rejectUnauthorized: false }});
    client.connect(address, null, null, { 'ServiceBusAuthorization' : token});
    
    client.on('connect', function(connection){
        var id = setInterval(function () {
            connection.send(JSON.stringify(process.memoryUsage()), 
            function (err)
             {
                  /* ignore errors */
            
            });
        }, 500);

        console.log('Started client interval. Press any key to stop.');
        connection.on('close', function () {
            console.log('stopping client interval');
            clearInterval(id);
            process.exit();
        });

        //process.stdin.setRawMode(true);
        process.stdin.resume();
        process.stdin.on('data', function() {
            connection.close();
        });
    });
}