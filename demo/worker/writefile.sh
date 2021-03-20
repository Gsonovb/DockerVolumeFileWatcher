#!/usr/bin/env bash


echo "server {"
echo "  listen 80; "
echo "  location / {"
echo "  default_type  'text/html; charset=utf-8';"
echo '    return 200 "'$(date +%Y-%m-%d_%H:%M:%S)'";'
echo "  }"
echo "}"
