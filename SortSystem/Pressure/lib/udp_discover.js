const dgram = require('dgram');
const server = dgram.createSocket({type: 'udp4', reuseAddr: true});
const { networkInterfaces } = require('os');
const { v4: uuidv4 } = require('uuid');
const uuid = uuidv4();
const nets = networkInterfaces();
const localAddr = []; // Or just '{}', an empty object

var udpPort;

for (const name of Object.keys(nets)) {
    for (const net of nets[name]) {
        // Skip over non-IPv4 and internal (i.e. 127.0.0.1) addresses
        if (net.family === 'IPv4' && !net.internal) {
          localAddr.push(net.address);
        }
    }
}

server.on('error', (err) => {
  console.log(`server error:\n${err.stack}`);
  server.close();
});

server.on('message', (msg, rinfo) => {
  
  foundLocal=false;

  console.log(`server got: ${msg} from ${rinfo.address}:${rinfo.port}`);
  server_info=JSON.parse(msg);
  if(uuid==server_info.Uuid)foundLocal=true;
  if(!foundLocal){
    console.log("try to start invoke project start with uuid"+uuid)
    found_server_callback(rinfo.address,server_info.RpcPort,server_info.Uuid)
  }
});

server.on('listening', () => {
  const address = server.address();
  console.log(`server listening ${address.address}:${address.port}`);
});


counter=0
function broadcastNew() {
  counter++;
  var message = Buffer.from(JSON.stringify({"ListenPort":udpPort,"Uuid":uuid,"Count":counter,"Type":"BRD","RpcPort":5114}));
  server.send(message, 0, message.length, udpPort, "255.255.255.255", function() {
      console.log("Sent '" + message + "'");
  });
}

function sendAck(address,port) {
  counter++;
  var message = Buffer.from(JSON.stringify({"ListenPort":udpPort,"Uuid":uuid,"Count":counter,"Type":"ACK","RpcPort":5114}));
  server.send(message, 0, message.length, udpPort, "255.255.255.255", function() {
      console.log("Sent '" + message + "'");
  });
}

var found_server_callback;
function entrance(callback,port){
  found_server_callback=callback
  udpPort=port ==null?13567:port;
  console.log("Starting discovery on interfaces on port "+udpPort);
  
  console.log(localAddr);

  server.bind(udpPort,function(){
    server.setBroadcast(true);
    broadcastNew();
  });
}

module.exports.start=entrance