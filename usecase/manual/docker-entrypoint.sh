#!/bin/bash
###########

sh -c "filewatcher &"

exec "$@"
