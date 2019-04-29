#!/bin/sh
set -e
curl -LO https://download.rethinkdb.com/windows/rethinkdb-2.3.6.zip
yes | unzip rethinkdb-2.3.6.zip
rm rethinkdb-2.3.6.zip

echo -e "\n\e[92mInstallation succeed, don't forget to compile Uberback using Visual Studio\nDefault output directory is at Uberback/bin/Debug."
echo -e "\nBefore launching Uberback, you must start ReThinkdb.\e[0m"