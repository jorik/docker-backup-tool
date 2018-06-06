FROM jorik/aws-cli

COPY ./docker-entrypoint.sh /

# The volume that holds the data which we should backup
VOLUME /data

CMD /docker-entrypoint.sh ${BACKUP_PROJECT} ${BACKUP_ROLE} ${BACKUP_CONTAINER_PATH} ${AWS_SECRET_ACCESS_KEY} ${AWS_ACCESS_KEY_ID} /data

