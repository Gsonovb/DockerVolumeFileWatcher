FROM nginx:latest

#Remove the following line comments to install the curl and Bash tools you need
RUN apt-get update && apt-get install -y --no-install-recommends apt-transport-https ca-certificates bash libc6 libgcc1 libgssapi-krb5-2 libicu63 libssl1.1 libstdc++6 zlib1g && apt-get clean


COPY ./filewatcher /usr/bin/filewatcher
RUN chmod +x /usr/bin/filewatcher

COPY ./docker-entrypoint.sh /usr/bin/docker-entrypoint.sh
RUN chmod +x /usr/bin/docker-entrypoint.sh

ENV filewatcher_path='/etc/nginx/conf.d'
ENV filewatcher_recurse=false
ENV filewatcher_time=60
ENV filewatcher_command='nginx'
ENV filewatcher_arguments='-s,reload'


ENTRYPOINT [ "/usr/bin/docker-entrypoint.sh" ]

CMD ["nginx", "-g", "daemon off;"]
