#!/bin/bash
###########

sh -c "FileWatcher --path /etc/nginx/conf.d -t 10 -c nginx -a -s reload &"

exec "$@"