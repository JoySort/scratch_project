#!/bin/bash
#mitmweb --mode upstream:http://localhost:5113 --listen-port 9082 --web-port 9083 &
./RecognizeRunner/bin/Debug/net6.0/RecognizeRunner --config_folder=../../../../runner_config/recognizer_config_simulation
