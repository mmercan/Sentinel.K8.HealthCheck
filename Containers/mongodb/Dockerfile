FROM mongo:latest
VOLUME /data/db
# CMD ["mongod", "--smallfiles", "--logpath=/dev/null"]
# CMD ["mongod"]

COPY dump /home/dump
# COPY mongo.sh /home/mongo.sh
# RUN chmod 777 /home/mongo.sh
# CMD /home/mongo.sh

COPY mysetup.js /docker-entrypoint-initdb.d/
COPY mongo_import.sh /docker-entrypoint-initdb.d/


CMD ["mongod"]
EXPOSE 27017
EXPOSE 28017