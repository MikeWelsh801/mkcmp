#!/bin/sh

proj_dir="$(dirname "${BASH_SOURCE[0]}")/src/mi/bin/Debug/net7.0"

dot -Tpng "$proj_dir/cfg.dot" -o "$proj_dir/graph.png"
start "$proj_dir/graph.png"
