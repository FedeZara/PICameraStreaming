let SSL_KEY = __dirname + '/certs/key.pem';
let SSL_CERT = __dirname + '/certs/certificate.pem';
let MONGOURL = 'mongodb://admin:admin123@tpsitiot-shard-00-00-v09b3.mongodb.net:27017,tpsitiot-shard-00-01-v09b3.mongodb.net:27017,tpsitiot-shard-00-02-v09b3.mongodb.net:27017/test?ssl=true&replicaSet=tpsitiot-shard-0&authSource=admin&retryWrites=true';

module.exports = {
    id: 'broker',
    stats: false,
    port: 8443,
    logger: {
        name: 'tpsitiot',
        level: 'debug'
    },
    secure: {
        keyPath: SSL_KEY,
        certPath: SSL_CERT,
    },
    backend: {
        type: 'mongodb',
        url: MONGOURL
    }
};
