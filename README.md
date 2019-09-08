# SerialPortMonitoringAndWorkerClassLibrary
For now, library contains: - Monitoring class => in loop, checking messages received via SerialPort and sending to "Worker Class", which should get it.     - (Worker) Locking class => After got message "AUTHY: Permission Denied!", workstation is locking. Workstation isn't going to lock, when message is "AUTHY: Permission Granted!".
