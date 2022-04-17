const udp_service = require("./lib/udp_discover")
const chalk = require("chalk");

const project_start_service = require("./lib/start_project");
const project_stop_service = require("./lib/stop_project");
const send_data_service = require("./lib/send_data");

var start_with_stop_flag=false;
if(process.argv[2]!=null){
    if(process.argv[2]=="stop"){
        start_with_stop_flag=true
    }
}

udp_service.start(on_discover_server,13567);




var services={};

function on_discover_server(address,rpc_port,uuid){
    
    services[uuid]={host:address,port:rpc_port};
    //console.log(services)
    if(start_with_stop_flag){
        project_stop_service.stop_project(
            services[uuid].host,
            services[uuid].port,
            on_project_stop,uuid)
        return;
    }
    project_start_service.start(
        services[uuid].host,
        services[uuid].port,
        on_project_start_complete,
        uuid);
}

  function on_project_start_complete(remote_uuid){
    console.log(chalk.blue("[index]"),"project",chalk.green(remote_uuid),chalk.bgBlue("started"))
    services[remote_uuid].start_time=new Date().getTime();
    //send_data_service.send_consolidate_bulk(start_triggerid ,trigger_id_count_per_batch ,batch_count  ,endpoint_host                      ,endpoint_port                      ,remote_uuid ,callback)
      send_data_service.send_consolidate_bulk(0               ,14                         ,60*10        ,services[remote_uuid].host        ,services[remote_uuid].port         ,remote_uuid ,on_send_data_complete)
  }

  function on_send_data_complete(uuid){
    time_took=(new Date().getTime()-services[uuid].start_time);
    console.log("[index]uuid %s took %f",uuid,time_took/1000);
    project_stop_service.stop_project(services[uuid].host,services[uuid].port,on_project_stop,uuid)
  }

  function on_project_stop(uuid){
      console.log(chalk.blue("[index]"), "project ",chalk.green(uuid),chalk.bgRedBright("stoped"))
  }