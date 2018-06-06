#!/bin/sh
set -e

project=$1
role=$2
container_path=$3
aws_secret_access_key=$4
aws_access_key_id=$5
datadir=$6

date=`date +%Y-%m-%d`
tarfile="$project.$role.$container_path.$date.tar.gz"

echo "Starting backup of $datadir to file: $tarfile"

# create the actual tarfile
tar -zcf $tarfile $datadir 

echo "Logging in to AWS with key: $aws_secret_access_key and key_id: $aws_access_key_id"

# login to AWS
aws configure set aws_secret_access_key $aws_secret_access_key
aws configure set aws_access_key_id $aws_access_key_id
aws configure set default.region eu-central-1

# upload the tarfile to S3
aws s3 cp $tarfile s3://wubwubnl/docker-backups/$project/$tarfile
