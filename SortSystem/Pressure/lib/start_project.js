var axios = require("axios");
const fs = require('fs')
const chalk = require("chalk");
const config = { headers: { 'Content-Type': 'application/json' } };


var host;
var port;
var uuid;
var start_finish_call_back;

var services={};

var project_start_parameter;
function entrance(end_point_host,end_point_port,callback,remote_uuid){
    services[remote_uuid]={};
    start_finish_call_back=callback;
    services[remote_uuid].host=end_point_host;
    services[remote_uuid].port=end_point_port;
    services[remote_uuid].uuid=remote_uuid;
    //console.log(services[remote_uuid]);
   
    fs.readFile('config/project_apple_rec_start.json', 'utf8' , (err, data) => {
        if (err) {
            console.error(err)
            return
        }
    
        project_start_parameter = JSON.parse(data);
        //console.log(project_start_parameter)
        try{
            getModuleConfig(remote_uuid);
        }catch(error){
            console.log(error)
        }
        
        
    })
}

function getModuleConfig(remote_uuid){
    axios.get('http://'+services[remote_uuid].host+':'+services[remote_uuid].port+'/config/module', config)
        .then(function(response) {
           // console.log(response.data)
            if(response.data.module==3){
                console.log("upper found issue start ",services[remote_uuid].host+':'+services[remote_uuid].port)
                start_project(remote_uuid);
            }
        })
        .catch(function(error) {
            console.log(chalk.grey("getmodule error: "),chalk.green(remote_uuid==null?"remoteid:null":remote_uuid),chalk.red(error))
            //start_finish_call_back(uuid);
            //console.log(error);
            if((error+"").indexOf("ECONNREFUSED")>0){
                console.log(chalk.yellow("server not responding, try again in 5 sec"))
                setTimeout(() => {
                    getModuleConfig(remote_uuid);
                }, 5000);
            }
        })
}

function start_project(remote_uuid){
    console.log("start project "+remote_uuid)
    //console.log(services[remote_uuid]);
    axios.post('http://'+services[remote_uuid].host+':'+services[remote_uuid].port+'/apis/project_start', project_start_parameter, config)
        .then(function(response) {
            //console.log("Project_START: "+JSON.stringify(response.data))
            start_finish_call_back(remote_uuid);
        })
        .catch(function(error) {
            console.log(chalk.grey("Project_START_catched_error: "),chalk.green(remote_uuid==null?"remoteid:null":remote_uuid),chalk.red(error))
            //start_finish_call_back(uuid);
            //console.log(error);
            if((error+"").indexOf("ECONNREFUSED")>0){
                console.log(chalk.yellow("server not responding, try again in 5 sec"))
                setTimeout(() => {
                    start_project(remote_uuid);
                }, 5000);
            }
        })
}


module.exports.start=entrance
//entrance();