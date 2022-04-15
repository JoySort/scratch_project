var axios = require("axios");
const fs = require('fs')

const config = { headers: { 'Content-Type': 'application/json' } };

var host;
var port;
var uuid;
var stop_finish_call_back;

function entrance(end_point_host,end_point_port,callback,remote_uuid){
    stop_finish_call_back=callback;
    host=end_point_host;
    port=end_point_port;
    uuid=remote_uuid;
    stop_project()
}

function stop_project(){
    axios.get('http://'+host+':'+port+'/apis/project_stop', {}, config)
    .then(function(response) {
        console.log("Project_Stop: "+JSON.stringify(response.data))
        stop_finish_call_back(uuid)
    })
    .catch(function(error) {
        console.log("Project_Stop_error: "+error)
    })
}
module.exports.stop_project=entrance;
