var net = require('net');
var agent_module = require('./agent.js');

var agent = new agent_module.Agent();

var server = net.createServer(function(c) { agent.onConnected(c); });

server.listen(8124, function() { //'listening' listener
  console.log('server bound');
});
