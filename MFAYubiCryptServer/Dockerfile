# Pull base image.
FROM ubuntu:14.04

# Install apache
RUN sudo apt-get update 
RUN sudo apt-get install -y mono-complete

# Define working directory.
WORKDIR /data

EXPOSE 8888

# Add files
ADD . /data

# Build
RUN xbuild

# Define default command.
CMD ["./docker/run_api.sh"]
