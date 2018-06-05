# Tool created for automatically creating backups of running docker containers

## backup-runner

.net core project that iterates over the running docker containers, figures out which ones requires a backup, and starts a docker-mount-backup-tool docker container for actually creating the backup.

## mount-backup

Simple docker image that expects a folder mounted at `/data` and a bunch of environment variables. The container will create an tar.gz archive from the `/data` directroy, and upload it to the (now hardcoded) s3 bucket. 


