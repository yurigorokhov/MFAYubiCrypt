#!/usr/bin/env bash

apt-get update && apt-get install -y docker.io

# Setup docker
ln -s /vagrant /opt/docker
cd /opt/docker

