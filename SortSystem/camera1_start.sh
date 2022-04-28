#!/bin/bash
#mitmweb --mode upstream:http://localhost:5113 --listen-port 9082 --web-port 9083 &
./CameraRunner/bin/Debug/net6.0/CameraRunner --config_folder=../../../../runner_config/camera1_config
