var axios = require("axios");
const fs = require('fs')

const config = { headers: { 'Content-Type': 'application/json' } };


var global_counter = 0;
var count_per_batch = 0;
var batch_count=0;

var host;
var port;

var finish_callback;
var uuid;

var services={};

var interval_id

function send_bulk(start_triggerid,trigger_id_count_per_batch,arg_batch_count,endpoint_host,endpoint_port,remote_uuid,callback){
    services[remote_uuid]={};
    services[remote_uuid].host= endpoint_host;
    services[remote_uuid].port = endpoint_port;
    services[remote_uuid].global_counter = start_triggerid ;
    services[remote_uuid].count_per_batch=trigger_id_count_per_batch;
    services[remote_uuid].batch_count=arg_batch_count
    services[remote_uuid].uuid=remote_uuid

    finish_callback=callback
    console.log(services[remote_uuid])
    send_consolidate_bulk(remote_uuid)
    services[remote_uuid].interval_id=setInterval(function(){
        processWrite(remote_uuid);
    },100)
}

function send_consolidate_bulk(remote_uuid){

    var data = generate_rec_obj(services[remote_uuid].global_counter,services[remote_uuid].count_per_batch,remote_uuid);
    var timestamp = new Date().getTime();
    //console.log("start sending with starting trigger id"+data[0]);
    axios.post('http://'+services[remote_uuid].host+':'+services[remote_uuid].port+'/sort/consolidate_batch', data, config)
    .then(function(response) {
        
        //console.log(global_counter*1000/(batch_count*count_per_batch));
        var progress=(Math.round(services[remote_uuid].global_counter*1000/(services[remote_uuid].batch_count * services[remote_uuid].count_per_batch))/10);
        services[remote_uuid].progress=progress;
        printProgress(services[remote_uuid].host,services[remote_uuid].port,"Current progress: ",progress.toFixed(1)+"%"+" "+(new Date().getTime()-timestamp) ,"ms");
        //global_counter++;
        //console.log("sending data, took "+(new Date().getTime()-timestamp)+" ms");
        //console.log("finished. batch "+ (Math.floor(global_counter/batch_count)+1)+" server response:"+JSON.stringify(response.data))
        //console.log(global_counter,batch_count,count_per_batch)
        if(services[remote_uuid].global_counter < (services[remote_uuid].batch_count * services[remote_uuid].count_per_batch)){
            //console.log("keep sending ")
            send_consolidate_bulk(remote_uuid);
        }else{
            console.log("End sending")
            finish_callback(remote_uuid)
            
        }
    })
    .catch(function(error) {
        console.log("error catched sending data: "+error.message)
        console.log(error)
        
    })
    
}

function generate_rec_obj(start,count,remote_uuid){
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

    
    var timestamp = new Date().getTime();

    var results = [];
    for(var i=start;i<start+count;i++){
        for(var col=0;col<24;col++){
            for(var row=0 ; row <4; row++){
            
                cloned = JSON.parse(JSON.stringify(template1));

                cloned.coordinate.rowOffset=row;
                cloned.coordinate.triggerId=i;
                cloned.coordinate.column=col;
                cloned.coordinate.section=Math.floor(col/6);
                cloned.features[0].value=40;
                results.push(cloned);
                if(row == 3){
                    cloned2 =  JSON.parse(JSON.stringify(template2));
                    cloned2.coordinate.triggerId=i;
                    cloned2.coordinate.column=col;
                    cloned2.coordinate.section=Math.floor(col/6);
                    cloned2.features[0].value=(col%2==0)?4:6;
                    results.push(cloned2);
                }
                
            }
            

        }

    }
    services[remote_uuid].global_counter=start+count;
    //console.log(JSON.stringify(results));
    //console.log("finish prepare data ready to send request, took "+(new Date().getTime()-timestamp)+" ms");
    //console.log(`trigger: `,global_counter);
    //console.log(results.length);
    //console.log("expected results from server"+results.length/5)
    
    return results
}

var progressObj={};

function colorize(color, output) {
    return ['\033[', color, 'm', output, '\033[0m'].join('');
}
function printProgress(host,port,text1,progress,text2){
    progressObj[host+":"+port]={};
    key = host+":"+port;
    progressObj[key].target=host+":"+port;
    progressObj[key].text1=text1;
    progressObj[key].progress=progress;
    progressObj[key].text2=text2;

}

function processWrite(remote_uuid){

    process.stdout.clearLine();
    process.stdout.cursorTo(0);
    var all_complete = true;
    for (const [key, value] of Object.entries(progressObj)) {
        process.stdout.write("  ")
        process.stdout.write(progressObj[key].target+" "+colorize(32,progressObj[key].progress)+progressObj[key].text2);

    }
   
    if(services[remote_uuid].progress==1){
        clearInterval(services[remote_uuid].interval_id);
    }
    
      
  }

module.exports.send_consolidate_bulk=send_bulk