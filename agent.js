var events = require('events');

var Agent = function () {
    this.lastId = 0;

    // TODO: Is it effetive way to manage mutable dictionary in V8.
    this.clients = {};

    var agent = this;

    this.onConnected = function(client) {
	var clientId = this.lastId += 1;
	this.clients[clientId] = client;
	client.id = clientId;

	console.log('client', clientId, 'connected');

	client.on('end', function() {
	    delete agent.clients[clientId];
	    console.log('client', clientId, 'disconnected');
	});

	// TODO: Package completeness check.
	client.on('data', function(data) {
	    try {
		var position = JSON.parse(data);
		position.clientId = clientId;
	    } catch (err) {
		console.log(err);
		return;
	    }

	    agent.broadcast(position);
	});
    };

    this.broadcast = function(message) {
	Object.keys(this.clients).forEach(function(id) {
	    var c = agent.clients[id];
	    c.write(JSON.stringify(message));
	});
    };
};

Agent.prototype.__proto__ = events.EventEmitter.prototype;

exports.Agent = Agent;
