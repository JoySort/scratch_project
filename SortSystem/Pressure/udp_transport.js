const dgram = require('dgram');
const server = dgram.createSocket('udp4');
const { networkInterfaces } = require('os');

const nets = networkInterfaces();
const localAddr = []; // Or just '{}', an empty object
counter=0;
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
  
    var timetook=Date.now()-currentTime;
    console.log(`server got msg from ${rinfo.address}:${rinfo.port}`);
    var remote_stats = JSON.parse(msg);
    remote_stats.localTimetook=timetook
    console.log(remote_stats);
    if(counter++<100){
      sendMSG() ;
    }else{
      console.log("finished");
    }


});

server.on('listening', () => {
  const address = server.address();
  console.log(`server listening ${address.address}:${address.port}`);
});

server.bind(13567,function(){
  server.setBroadcast(true);
  sendMSG();
});

var currentTime
function sendMSG() {
  //counter++;
  currentTime= Date.now();
  var message = Buffer.from(JSON.stringify(generate_rec_obj(3)));
  console.log("start sending msg");
  server.send(message, 0, message.length, 5113, "10.10.40.218", function() {
      //console.log("Sent '" + message + "'");
  });
}


 
// function start_send(host){
//   setTimeout(()=>{
//     console.log("start project for host"+host)
//     require("./start.js").start(host,counter++,()=>{
//       start_send(host);
//     });
//   },3000);

// }
function generate_rec_obj(count){
  var template1={
      "coordinate": { "section": 0, "column": 0,"triggerId": 0,"rowOffset": 0},
      "expectedFeatureCount": 2,
      "features": [ { "criteriaIndex": 1, "value": 10 }]
    }

  var template2=  {
      "coordinate": { "section": 0, "column": 0,"triggerId": 0,"rowOffset": 14},
      "expectedFeatureCount": 2,
      "features": [ { "criteriaIndex": 12, "value": 4 }]
    }

  console.log("prepare data")
  var timestamp = new Date().getTime();

  var results = [];
  for(var i=0;i<count;i++){
      for(var col=0;col<24;col++){
          for(var row=0 ; row <4; row++){
          
              cloned = JSON.parse(JSON.stringify(template1));
              //cloned = Object.assign({}, template1);
              cloned.coordinate.rowOffset=row;
              cloned.coordinate.triggerId=i;
              cloned.coordinate.column=col;
              cloned.coordinate.section=Math.floor(col/6);
              cloned.features[0].value=40;
              results.push(cloned);
              if(row == 3){
                  cloned2 =  JSON.parse(JSON.stringify(template2));
                  //cloned2.coordinate.rowOffset=row;
                  cloned2.coordinate.triggerId=i;
                  cloned2.coordinate.column=col;
                  cloned2.coordinate.section=Math.floor(col/6);
                  cloned2.features[0].value=(col%2==0)?4:6;
                  results.push(cloned2);
              }
              
          }
          

      }

  }
  //console.log(JSON.stringify(results));
  //console.log("ready to send request, took "+(new Date().getTime()-timestamp)+" ms");
  //console.log(results.length);
  //console.log("expected results from server"+results.length/5)
  
  return results
}