var axios = require("axios");
const fs = require('fs')
const chalk = require("chalk");
const config = { headers: { 'Content-Type': 'application/json' } };

var host;
var port;
var uuid;
var stop_finish_call_back;
var services={};
function entrance(end_point_host,end_point_port,callback,remote_uuid){
    stop_finish_call_back=callback;

    services[remote_uuid]={};
    services[remote_uuid].host=end_point_host;
    services[remote_uuid].port=end_point_port;
    services[remote_uuid].uuid=remote_uuid;
    stop_project(remote_uuid)
}

function stop_project(remote_uuid){
    axios.get('http://'+services[remote_uuid].host+':'+services[remote_uuid].port+'/apis/project_stop', {}, config)
    .then(function(response) {
        console.log(chalk.grey("Project_Stop:"))
        //console.log(JSON.stringify(response.data));
        stop_finish_call_back(remote_uuid)
    })
    .catch(function(error) {
        console.log("Project_Stop_error: "+error)
    })
}
module.exports.stop_project=entrance;
