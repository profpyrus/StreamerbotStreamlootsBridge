﻿// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using StreamerbotStreamlootsBridge;
using Websocket.Client;

await Start();

static async Task Start()
{
	Console.WriteLine("Welcome to the Streamer.bot Streamloots Bridge Setup. What do you want to do?");
	Console.WriteLine("\t\"auth\": Setup your authorization token to connect to Streamloots.");
	Console.WriteLine("\t\"action\": Create the string for a new auto-gift action (NEEDS AUTH SETUP)");
	Console.WriteLine("\t\"exit\": Exit the program.");
	Console.WriteLine("");
	string? input = Console.ReadLine();
	switch (input){
		case "auth":
			AuthSetup();
			await Start();
			break;
		case "action":
			await ActionSetupAsync();
			await Start();
			break;
		case "exit":
			break;
		default:
			Console.WriteLine("I didn't quite get that. Try again! (check spelling)");
			await Start();
			break;
	}
}

static void AuthSetup()
{
	Settings settings = SettingsSaveLoader.ReadSettings();

	Console.WriteLine("Go to the streamloots homepage, make sure you are logged in, then Open the console (google how to do that) and paste the following command in there and press enter.");
	Console.WriteLine("");
	Console.WriteLine("decodeURIComponent(document.cookie).split('; ').forEach(val => { if (val.indexOf(\"AUTH_TOKEN=\") === 0) console.log(val.substring(\"AUTH_TOKEN=\".length));});");
	Console.WriteLine("");
	Console.WriteLine("Paste the result here:");

	string? token = Console.ReadLine();
	if(token != null)
		settings.authToken = token;

	SettingsSaveLoader.SaveSettings(settings);
}

static async Task ActionSetupAsync()
{
	Settings settings = SettingsSaveLoader.ReadSettings();
	RequestStructure request = new RequestStructure();

	HttpClient req = new HttpClient();
	Uri sluri = new Uri("https://api.streamloots.com");
	req.DefaultRequestHeaders.Add("Authorization", "Bearer " + settings.authToken);

	Console.WriteLine("What Channel do you want to gift packs from? Enter it's name:");
	string? name = Console.ReadLine();

	var exitEvent = new ManualResetEvent(false);
	Uri uri = new Uri("ws://127.0.0.1:8080/");

	WebsocketClient botclient = new WebsocketClient(uri);

	HttpResponseMessage reply = await req.GetAsync(new Uri("https://api.streamloots.com/sets?slug=" + name));
	string replyString = await reply.Content.ReadAsStringAsync();
	StreamlootsReply? packsObj = JsonConvert.DeserializeObject<StreamlootsReply>(replyString);

	int cnt = 0;
	if(packsObj != null)
	{
		foreach (CardPack pack in packsObj.data)
		{
			cnt++;
			Console.WriteLine(cnt + ": " + pack.name);
		}

		Console.WriteLine("\nPlease enter the index of the pack you want to be gifted: ");
		int ind = Convert.ToInt32(Console.ReadLine());

		request.packId = packsObj.data[ind-1]._id;
	}

	Console.WriteLine("How many packs should be gifted?");
	request.packAmount = Convert.ToInt32(Console.ReadLine());

	Console.WriteLine("How many cards should be in a pack? (1, 2 or 3)");
	request.cardAmount = Convert.ToInt32(Console.ReadLine());


	Console.WriteLine("Add a Core > C# > Execute C# Code subaction to the gifting action, edit the code and paste the following into a new line above the line saying 'return true;':\n");
	Console.WriteLine("CPH.WebsocketBroadcastString(\"{ packId: \\\"" + request.packId + "\\\", user: \\\"\" + args[\"userName\"].ToString() + \"\\\", cardAmount: " + request.cardAmount + ", packAmount: " + request.packAmount + " }\");\n\n");
}