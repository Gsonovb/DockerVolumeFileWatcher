
FROM debian:latest

# Setup cron.
RUN apt-get update && apt-get install cron apt-utils  inotify-tools -y && apt-get clean


CMD inotifywait 
