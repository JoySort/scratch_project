var axios = require("axios");
const fs = require('fs')

const config = { headers: { 'Content-Type': 'application/json' } };


var host;
var port;
var uuid;
var start_finish_call_back;

var project_start_parameter;
function entrance(end_point_host,end_point_port,callback,remote_uuid){
    start_finish_call_back=callback;
    host=end_point_host;
    port=end_point_port;
    uuid=remote_uuid;

   
    fs.readFile('config/project_apple_rec_start.json', 'utf8' , (err, data) => {
        if (err) {
            console.error(err)
            return
        }
    
        project_start_parameter = JSON.parse(data);
        //console.log(project_start_parameter)
        try{
            start_project();
        }catch(error){
            console.log(error)
        }
        
        
    })
}

function start_project(){
    console.log("start project")
    axios.post('http://'+host+':'+port+'/apis/project_start', project_start_parameter, config)
        .then(function(response) {
            console.log("Project_START: "+JSON.stringify(response.data))
            start_finish_call_back(uuid);
        })
        .catch(function(error) {
            console.log("Project_START_catched_error: "+error)
            //start_finish_call_back(uuid);
            if((error+"").indexOf("ECONNREFUSED")){
                console.log("server not responding, try again in 5 sec")
                setTimeout(() => {
                    start_project();
                }, 5000);
            }
        })
}


module.exports.start=entrance
//entrance();