var config = require('./config.js');
var mqtt = require('mqtt');
var Raspistill = require('node-raspistill').Raspistill;
var jpeg = require('jpeg-js');
var fs = require('fs');

var raspistill = new Raspistill({
    noFileSave: true,
    encoding: 'jpg',
    width: 320,
    height: 240
});


var client = mqtt.connect({
    port: config.mqtt.port,
    protocol: 'mqtt',
    host: config.mqtt.host,
    clientId: config.mqtt.clientId,
    reconnectPeriod: 1000,
    username: config.mqtt.clientId,
    password: config.mqtt.clientId,
    keepalive: 300,
    rejectUnauthorized: false
});

client.on('connect', function() {
    client.subscribe('rpi');
});

var noImage = fs.readFileSync(__dirname + "/images/no-image.jpg");

var handshake1Arrived = false, handshake3Arrived = false;
var clientConnected = false;
	
client.on('message', function(topic, message) {
    message = message.toString();
    console.log(topic + ": " + message); 
    if (topic === 'rpi') {
        if(message === 'handshake1'){
			handshake1Arrived = true;
			client.publish('client-app', 'handshake2');
		}
		else if(message === 'handshake3' && handshake1Arrived){
			handshake3Arrived = true;
			startStreaming();
			handshake1Arrived = false;
			handshake3Arrived = false;
		}
		else if(message === 'rhandshake'){	
			stopStreaming();
		}
    } else {
        console.log('Unknown topic', topic);
    }
});

function startStreaming() {
	clientConnected = true;
    raspistill
        .timelapse(200, 0, async function(image) { // every 200ms ~~FOREVER~~
			if(!clientConnected){
				raspistill.stop();
			}
			
			if(image.byteLength === 0){
				image = noImage;
			}
            var data2Send = {
				image: image,
				time: (new Date()).getTime()
            };
			// console.log(image);
            client.publish('image', JSON.stringify(data2Send));
            // console.log(JSON.stringify(data2Send), 'published');
        })
        .then(function() {
            console.log('Timelapse Ended');
        })
        .catch(function(err) {
            console.log('Error', err);
        });
    
}

function stopStreaming(){
	clientConnected = false;
}


