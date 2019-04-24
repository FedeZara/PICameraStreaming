let SSL_KEY = __dirname + '/certs/key.pem';
let SSL_CERT = __dirname + '/certs/certificate.pem';

module.exports = {
    id: 'broker',
    stats: false,
    port: 8883,
    logger: {
        name: 'tpsitiot',
        level: 'debug'
    }
};
