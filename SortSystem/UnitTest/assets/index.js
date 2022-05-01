var axios = require("axios");
const fs = require('fs')

const config = { headers: { 'Content-Type': 'application/json' } };


fs.readFile('../fixtures/project_apple_rec_start.json', 'utf8' , (err, data) => {
    if (err) {
        console.error(err)
        return
    }
   
    var project_start_parameter = JSON.parse(data);
    console.log(project_start_parameter)
})

function start_project(parameter){
    
    
axios.post('http://localhost:5113/sort/consolidate_batch', parameter, config)
    .then(function(response) {
        console.log(response.data)
    })
    .catch(function(error) {
        console.log(error)
    })
}