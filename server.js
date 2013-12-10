var net = require('net');

var Agent = function () {
    this.lastId = 0;

    // TODO: Is it effe
    this.clients = {};

    this.onConnected = function(client) {
	var clientId = this.lastId += 1;
	this.clients[clientId] = client;

	console.log('client connected');

	var agent = this;
	client.on('end', function() {
	    delete agent.clients[clientId];
	    console.log('client disconnected');
	});

	client.on('data', function(data) {
	    var position = JSON.parse(data);
	    position.clientId = clientId;

	    Object.keys(agent.clients).forEach(function(id) {
		var c = agent.clients[id];
		if (c != client)
		    c.write(JSON.stringify(position));
	    });
	});
    };
};

var agent = new Agent();

var server = net.createServer(function(c) { agent.onConnected(c); });

server.listen(8124, function() { //'listening' listener
  console.log('server bound');
});
