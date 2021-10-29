# SmartHouseControl

## Description

This system is highly specialized, as it is designed for a specific laboratory bench that simulates the "Smart Building". The main idea of ​​the system is to receive data from the controller board (Arduino) of the Smart Building laboratory bench, which in turn receives data from sensors (temperature, lighting, humidity, etc.) located within the bench. Also, the developed system allows you to control the actuators of the laboratory bench to influence its environment (that is, increase / decrease the room temperature, open doors / windows, turn on lighting in certain rooms, etc.).

## Initialization

1. [Download this repository](https://github.com/AndreyBV/SmartHouseControl/archive/refs/heads/main.zip);
1. [Download and install **PostgreSQL**](https://www.enterprisedb.com/downloads/postgres-postgresql-downloads);
1. Run the console command: `psql -U postgres -p 5432 name_your_database < d:\smart_house.sql`;
1. Open the project file **"SmartHouseWPF.sln"**;
1. Install required packages project;
1. Run the project **"Ctrl+F5"**.

## Architecture

<img src="./_documentation/architecture.png?raw=true" width=600px  height="auto" />

#### UML Diagram

<img src="./_documentation/uml-tobe.png?raw=true" width=600px  height="auto" style="border: 1px solid gray"/>

## Base function

- Configuring the COM port to which the control controller (Arduino) is bound;
- Receiving raw commands from the controller and displaying them in the system console;
- Sending raw commands to the controller;
- Receiving data from sensors in five rooms, as well as from sensors located on the "Smart Building" stand;
- Used sensors: illumination, temperature, humidity, motion, vibration, door opening, current;
- Setting the frequency of polling sensors;
- Ability to write data from sensors to the PostgreSQL database;
- Ability to view database records (history of data received from sensors);
- Display of readings from sensors on a graph in real time and for a certain period;
- Display of notifications as a result of reaching certain values ​​by sensors;
- Control of executive devices to influence the environment of the stand;
- Used actuators: single color LEDs, rgb LEDs, heating elements, servo motors;
- Differentiation of rights (system administrator and other users);

## Demonstration of work

### Login

<img src="./_documentation/login.png?raw=true" width=250px height="auto" style="border: 1px solid gray"/>

### Settings system

<img src="./_documentation/settings.png?raw=true" width=500px height="auto" style="border: 1px solid gray"/>

### Help in using system

<img src="./_documentation/helps.png?raw=true" width=500px  height="auto" style="border: 1px solid gray"/>

### Main window

<img src="./_documentation/main.png?raw=true" width=500px  height="auto" style="border: 1px solid gray"/>

### System statistics in graphs view

<img src="./_documentation/statistic.png?raw=true" width=500px  height="auto" style="border: 1px solid gray"/>

### System statistics in table view

<img src="./_documentation/parameters_system.png?raw=true" width=500px  height="auto" style="border: 1px solid gray"/>

### List of system users

<img src="./_documentation/panel_users.png?raw=true" width=500px  height="auto" style="border: 1px solid gray"/>
