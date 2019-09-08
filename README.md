# SerialPortMonitoringAndWorkerClassLibrary
In this repo, You can find library, arduino code and usage example (named SPL).

For now, library contains: 

    - Log to file and console (console output is "switchable")
    
    - Monitoring class => in loop, checking messages received via SerialPort and sending to "Worker Class", which should get it.
    
    - (Worker) Locking class => After got message "AUTHY: Permission Denied!", workstation is locking. Workstation isn't going to lock, when message is "AUTHY: Permission Granted!".
