#!/bin/bash
mitmweb --mode upstream:http://localhost:5185 --listen-port 8082 --web-port 8083 &
./UpperRunner/bin/Debug/net6.0/UpperRunner --config_folder=../../../../runner_config/upper_config
