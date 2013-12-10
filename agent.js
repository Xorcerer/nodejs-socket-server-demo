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

	// TODO: Package completeness check.
	client.on('data', function(data) {
	    try {
		var position = JSON.parse(data);
		position.clientId = clientId;
	    } catch (err) {
		console.log(err);
		return;
	    }

	    Object.keys(agent.clients).forEach(function(id) {
		var c = agent.clients[id];
		if (c != client)
		    c.write(JSON.stringify(position));
	    });
	});
    };
};

exports.Agent = Agent;
