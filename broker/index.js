let SSL_KEY = __dirname + '/certs/key.pem';
let SSL_CERT = __dirname + '/certs/certificate.pem';

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
    }
};
