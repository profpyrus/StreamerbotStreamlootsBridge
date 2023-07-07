// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;
using Websocket.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using StreamerbotStreamlootsBridge;

Settings settings = SettingsSaveLoader.ReadSettings();

HttpClient req = new HttpClient();
Uri sluri = new Uri("https://api.streamloots.com");
req.DefaultRequestHeaders.Add("Authorization", "Bearer " + settings.authToken);

var exitEvent = new ManualResetEvent(false);
Uri uri = new Uri("ws://127.0.0.1:8080/");

WebsocketClient botclient = new WebsocketClient(uri);

botclient.ReconnectTimeout = null;
botclient.ReconnectionHappened.Subscribe(info =>
{
	Console.WriteLine(info.Type + " connection established");
});

botclient.MessageReceived.Subscribe(async msg =>
{
	JObject? obj = (JObject?)JsonConvert.DeserializeObject(msg.Text);
	var eventVal = obj.Value<JToken>("event");

	if (obj != null && eventVal != null && eventVal.Value<string>("type") == "Custom")
	{
		JToken token = obj.Value<JToken>("data");
		RequestStructure request = JsonConvert.DeserializeObject<RequestStructure>(token.Value<string>("data"));
		Console.WriteLine("Gifting pack to " + request.user + "...");
		string slrequest = "{\"items\": [ { \"item\": { \"setId\": \"" + request.packId + "\", \"cardAmount\": " + request.cardAmount + " }, \"quantity\": " + request.packAmount + " } ], \"gifteeUsername\": \"" + request.user + "\", \"type\": \"FREE_GIFT\"}";
		var data = new StringContent(slrequest, Encoding.UTF8, "application/json");
		HttpResponseMessage res = await req.PostAsync("https://api.streamloots.com/loot-orders", data);
		Console.WriteLine(res.IsSuccessStatusCode ? "Pack gifted succesfully!" : "Something went wrong...");
	}
});

botclient.Start();
Task.Run(() => botclient.Send("{ \"request\": \"Subscribe\", \"id\": \"<id>\", \"events\": { \"General\": [ \"Custom\", ] }, }"));
exitEvent.WaitOne();