// node sender.js daenetrelay.servicebus.windows.net hyco sample gIE8bsseCJxRkSiOCoNIkiiMBJXW0CygCKXnEvggP9c=

if (process.argv.length < 6) {
    console.log("listener.js [namespace] [path] [key-rule] [key]");
} else {

    var ns = process.argv[2];
    var path = process.argv[3];
    var keyrule = process.argv[4];
    var key = process.argv[5]; 

    var hycoWs = require('hyco-ws')

    run();

function run(){

    hycoWs.relayedConnect(
        hycoWs.createRelaySendUri(ns, path),
        hycoWs.createRelayToken('http://'+ns, keyrule, key),
        function (wss) {
            var id = setInterval(function () {
                
                try{
                    wss.send(JSON.stringify(process.memoryUsage()), function (err) { 
                    if(err != null){
                        console.log(err);  
                        clearInterval(id);
                        wss.close();
                        console.log("Restarting sender.");
                        run();
                    }
                    else
                        console.log(".");                 
                });
                }
                catch(e)
                {
                    console.log(e);
                    if(err.message == "not opened"){
                            console.log(":)"); 
                    } 
                }
            }, 1000);

            console.log('Started client interval. Press any key to stop.');
          
            wss.on('close', function () {
                console.log('stoppListener closed connection');
               //clearInterval(id);
               //process.exit();
            });

            process.stdin.setRawMode(true);
            process.stdin.resume();
            process.stdin.on('data', function () {
                console.log("Exit on demand.");
                clearInterval(id);
                wss.close();
                process.exit();
            });
        }
    );
    }
}