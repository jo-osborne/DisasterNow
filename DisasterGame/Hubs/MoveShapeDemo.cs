using System;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace MoveShapeDemo
{
    public class Broadcaster
    {
        private readonly static Lazy<Broadcaster> _instance =
            new Lazy<Broadcaster>(() => new Broadcaster());
        // We're going to broadcast to all clients a maximum of 25 times per second
        private readonly TimeSpan BroadcastInterval =
            TimeSpan.FromMilliseconds(40);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;
        private Timer _personBroadcastLoop;
        private Timer _timeBroadcastLoop;
        private ShapeModel _model;
        private bool _modelUpdated;

        private PersonModel _personModel;
        private bool _personModelUpdated;

        private static object shapeLocker = new object();
        private static object peopleLocker = new object();
        public static List<ShapeModel> Shapes = new List<ShapeModel>();
        public static List<PersonModel> People = new List<PersonModel>();

        private const int GameLength = 60;

        private DateTime started;

        static Broadcaster()
        {
            
            makeMeSomePeople(375, 443, 5);
            makeMeSomePeople(380, 578, 5);
            makeMeSomePeople(502, 612, 6); //People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = 502, Top = 612, Visible = true });
            makeMeSomePeople(920, 548, 5);
            makeMeSomePeople(608, 476, 5);
            makeMeSomePeople(705, 316, 5);
            //People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = 510, Top = 635, Visible = true });
            //People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = 520, Top = 624, Visible = true });
            //People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = 495, Top = 595, Visible = true });
            //People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = 482, Top = 587, Visible = true });
            //People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = 480, Top = 605, Visible = true });

        }

        private static void makeMeSomePeople(int seedLeft, int seedTop, int people)
        {

            People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = seedLeft, Top = seedTop, Visible = true });
            People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = seedLeft - 8, Top = seedTop - 23, Visible = true });
            if (people > 2)
            {
                People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = seedLeft - 18, Top = seedTop - 12, Visible = true });
                People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = seedLeft + 7, Top = seedTop - 17, Visible = true });
                People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = seedLeft + 20, Top = seedTop - 25, Visible = true });
            }
            if (people > 5)
            {
                People.Add(new PersonModel { Id = Guid.NewGuid().ToString(), Left = seedLeft + 22, Top = seedTop + 7, Visible = true });

            }

        }
        
        public Broadcaster()
        {
            started = DateTime.Now;
            // Save our hub context so we can easily use it 
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>();
            _model = new ShapeModel();
            _modelUpdated = false;
            // Start the broadcast loop
            _broadcastLoop = new Timer(
                BroadcastShape,
                null,
                BroadcastInterval,
                BroadcastInterval);
            _personBroadcastLoop = new Timer(
                BroadcastPerson,
                null,
                BroadcastInterval,
                BroadcastInterval);
            _timeBroadcastLoop = new Timer(
                BroadcastTime,
                null,
                BroadcastInterval,
                BroadcastInterval);
        }
        public void BroadcastShape(object state)
        {
            // No need to send anything if our model hasn't changed
            if (_modelUpdated)
            {
                // This is how we can access the Clients property 
                // in a static hub method or outside of the hub entirely
                _hubContext.Clients.AllExcept(_model.LastUpdatedBy).updateShape(_model);
                _modelUpdated = false;
            }
        }
        public void BroadcastTime(object state)
        {
            var timeLeft = GameLength - (int)Math.Floor((DateTime.Now - started).TotalSeconds);
            if (timeLeft >= 0)
            {
                _hubContext.Clients.All.updateTimer(new TimeModel { TimeLeft = timeLeft });
            }
            else
            {
                _timeBroadcastLoop.Dispose();
                var survivers = People.Count(x => !x.Visible);
                var victims = People.Count(x => x.Visible);
                _hubContext.Clients.All.gameOver(new GameOverModel { Survivers = survivers, Victims = victims });
            }
        }

        public void BroadcastPerson(object state)
        {
            // No need to send anything if our model hasn't changed
            if (_personModelUpdated)
            {
                // This is how we can access the Clients property 
                // in a static hub method or outside of the hub entirely
                _hubContext.Clients.AllExcept(_model.LastUpdatedBy).updatePerson(_personModel);
                _personModelUpdated = false;
            }
        }
        public void UpdateShape(ShapeModel clientModel)
        {
            _model = clientModel;
            _modelUpdated = true;
            lock (shapeLocker)
            {
                if (!Shapes.Any(x => x.Id == clientModel.Id))
                {
                    Shapes.Add(clientModel);
                }
            }
        }

        public void UpdatePerson(PersonModel clientModel)
        {
            _personModel = clientModel;
            _personModelUpdated = true;
            lock (peopleLocker)
            {
                var person = People.FirstOrDefault(x => x.Id == clientModel.Id);
                if (person != null)
                {
                    person.Visible = clientModel.Visible;
                }
            }
        }

        public static Broadcaster Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }

    public class MoveShapeHub : Hub
    {
        // Is set via the constructor on each creation
        private Broadcaster _broadcaster;
        public MoveShapeHub()
            : this(Broadcaster.Instance)
        {
        }
        public MoveShapeHub(Broadcaster broadcaster)
        {
            _broadcaster = broadcaster;
        }
        public void UpdateModel(ShapeModel clientModel)
        {
            clientModel.LastUpdatedBy = Context.ConnectionId;
            // Update the shape model within our broadcaster
            _broadcaster.UpdateShape(clientModel);
        }

        public void UpdatePerson(PersonModel clientModel)
        {
            clientModel.LastUpdatedBy = Context.ConnectionId;
            // Update the shape model within our broadcaster
            _broadcaster.UpdatePerson(clientModel);
        }
    }
    public class ShapeModel
    {
        // We declare Left and Top as lowercase with 
        // JsonProperty to sync the client and server models
        [JsonProperty("left")]
        public double Left { get; set; }
        [JsonProperty("top")]
        public double Top { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        // We don't want the client to get the "LastUpdatedBy" property
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
        [JsonProperty("full")]
        public bool Full { get; set; }
    }

    public class PersonModel
    {
        // We declare Left and Top as lowercase with 
        // JsonProperty to sync the client and server models
        [JsonProperty("left")]
        public double Left { get; set; }
        [JsonProperty("top")]
        public double Top { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("visible")]
        public bool Visible { get; set; }
        // We don't want the client to get the "LastUpdatedBy" property
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
    }

    public class TimeModel
    {
        [JsonProperty("timeleft")]
        public int TimeLeft { get; set; }
    }

    public class GameOverModel
    {
        [JsonProperty("survivers")]
        public int Survivers { get; set; }

        [JsonProperty("victims")]
        public int Victims { get; set; }
    }
}
