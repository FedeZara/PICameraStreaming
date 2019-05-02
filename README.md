# PiCameraStreaming

The main purpose of this project, as the name suggests, is to provide a way to stream the video from the Raspberry Pi camera to a Windows application. 

## Structure

This project consists of three components:
- MQTT broker using Mosca.
- Raspberry Pi script using Node.js.
- Windows Client app using WPF.

### MQTT Broker

The MQTT server runs on the Raspberry Pi and listens to the default port 8883. 
The server is created using [Mosca](https://github.com/mcollina/mosca).
The configuration file is <i>index.js</i> inside the <i>broker</i> folder.

### Raspberry Pi

The client side running on the Raspberry Pi is a [Node.js](https://nodejs.org/it/) script. 
It's a never-ending program that, after initialization and connection to the broker, waits for communication from the Client App.
After connection is established (using three-way handshake method), the script takes a photo (320x240 pixels of resolution) from the Pi Camera every fifth of a second and sends it, together with the time it was taken, to the Client app through the broker.

The script makes use of two main npm packages:
- [mqtt](https://www.npmjs.com/package/mqtt)
- [node-raspistill](https://www.npmjs.com/package/node-raspistill)


### Windows Client App

The Windows Client App is a WPF Visual Studio project. Its main purpouse is to show to the user the photo stream coming from the Raspberry Pi. To connect to the Raspberry, its MAC address is required. Once it is provided, the user can start the stream.

The Client App has three different phases:
- Connection to the broker: in this phase the Client App tries to connect to the MQTT broker. A connection is attempted every second.
- Three-way handshake: once connected to the broker, the Client App tries to start a connection with the client script running on the Pi using three-way handshake. The Client App sends the first message and waits for a response: if it arrives, communication is considered established, otherwise after 1 second a new handshake phase begins.
- Photo streaming: the Client App displays the images coming from the Pi. If no message arrives after 10 seconds, the app suggests restarting the Pi.

## Photos

### Before connection
![alt text](https://github.com/FedeZara/PICameraStreaming/blob/master/documentation/images/image1.jpeg)

### Streaming
![alt text](https://github.com/FedeZara/PICameraStreaming/blob/master/documentation/images/image2.jpeg)
![alt text](https://github.com/FedeZara/PICameraStreaming/blob/master/documentation/images/image3.png)

## Authors

- <b> Federico Zarantonello </b> - <i> Team leader, project ideator, responsible for the communication between Raspberry Pi and Windows App </i> - [FedeZara](https://github.com/FedeZara)

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)

