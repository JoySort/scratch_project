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
function send_bulk(start_triggerid,trigger_id_count_per_batch,arg_batch_count,endpoint_host,endpoint_port,remote_uuid,callback){
    host= endpoint_host;
    port = endpoint_port;
    global_counter = start_triggerid ;
    count_per_batch=trigger_id_count_per_batch;
    batch_count=arg_batch_count
    uuid=remote_uuid
    finish_callback=callback
    console.log({  host: host,
        port : port,
        global_counter : global_counter ,
        count_per_batch:count_per_batch,
        batch_count:batch_count,
        uuid:uuid,
        finish_callback:callback})
    send_consolidate_bulk()
}

function send_consolidate_bulk(){

    var data = generate_rec_obj(global_counter,count_per_batch);
    var timestamp = new Date().getTime();
    //console.log("start sending with starting trigger id"+data[0]);
    axios.post('http://'+host+':'+port+'/sort/consolidate_batch', data, config)
    .then(function(response) {
        
        //console.log(global_counter*1000/(batch_count*count_per_batch));
        printProgress("Current progress: ",(Math.round(global_counter*1000/(batch_count*count_per_batch))/10)+"%"+" "+(new Date().getTime()-timestamp) ,"ms");
        //global_counter++;
        //console.log("sending data, took "+(new Date().getTime()-timestamp)+" ms");
        //console.log("finished. batch "+ (Math.floor(global_counter/batch_count)+1)+" server response:"+JSON.stringify(response.data))
        //console.log(global_counter,batch_count,count_per_batch)
        if(global_counter< (batch_count*count_per_batch)){
            //console.log("keep sending ")
            send_consolidate_bulk();
        }else{
            console.log("End sending")
            finish_callback(uuid)
        }
    })
    .catch(function(error) {
        console.log("error catched sending data: "+error.message)
        
    })
    
}

function generate_rec_obj(start,count){
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
    global_counter=start+count;
    //console.log(JSON.stringify(results));
    //console.log("finish prepare data ready to send request, took "+(new Date().getTime()-timestamp)+" ms");
    //console.log(`trigger: `,global_counter);
    //console.log(results.length);
    //console.log("expected results from server"+results.length/5)
    
    return results
}

function colorize(color, output) {
    return ['\033[', color, 'm', output, '\033[0m'].join('');
}
function printProgress(text1,progress,text2){
    process.stdout.clearLine();
    process.stdout.cursorTo(0);
    process.stdout.write(text1+colorize(32,progress)+text2);
}
module.exports.send_consolidate_bulk=send_bulk