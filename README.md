# Uberback
[![Build status](https://ci.appveyor.com/api/projects/status/gflus8beksm2x8ns/branch/master?svg=true)](https://ci.appveyor.com/project/Xwilarg/uberback/branch/master)<br/>
Überback for Überschutz

By default, the backend is launched on port 5412.<br/>
There are 2 endpoints:
 - (POST) /data: Send datas to the backend
    - type: image or text
    - userId: ID of the user that is reported
    - flags: Flags that were triggered separated with a pipe |, there must always be at least one flag, please put SAFE if user message is safe
    - token: Authentification token

 - (POST) /collect: Get datas stored in the database
    - token: Authentification token
    - type (optional): text or image. If none then both are returned
    
### How to use it
To install Uberback, download this project, run install.sh and compile the project with Visual Studio<br/>
Once it's done, launch ReThinkdb and the project executable (in this order)
