// node listener.js daenetrelay.servicebus.windows.net hyco sample gIE8bsseCJxRkSiOCoNIkiiMBJXW0CygCKXnEvggP9c=

if ( process.argv.length < 6) {  

    console.log("listener.js [namespace] [path] [key-rule] [key]");
}
 else {

    var ns = process.argv[2];
    var path = process.argv[3];
    var keyrule = process.argv[4];
    var key = process.argv[5]; 
  
 //   var token = "Endpoint=sb://relaytunnel.servicebus.windows.net/;SharedAccessKeyName=Listen;SharedAccessKey=KSAgZI+TXSZswGGw975PVq8nbaW6GlcI30lBvMsv6qE=;EntityPath=Relay1;
    
    var hycoWs = require('hyco-ws')

    var wss = hycoWs.createRelayedServer(
        {
            server : hycoWs.createRelayListenUri(ns, path),
            token: hycoWs.createRelayToken('http://'+ns, keyrule, key)
        }, 
        function (ws) {
            console.log('connection accepted');
            ws.onmessage = function (event) {
                console.log(JSON.parse(event.data));
            };
            ws.on('close', function () {
                console.log('connection closed');
            });       
    });

    wss.on('error', function(err) {
        console.log('error' + err);
    });

     console.log(process.pid  + " Listener started...");
}