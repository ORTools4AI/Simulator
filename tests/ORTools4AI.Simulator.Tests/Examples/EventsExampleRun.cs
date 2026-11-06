// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTke.Desmoj.Test
{
    public class EventsExampleRun
    {
        [Fact(Timeout = 120_000)]
        public void EventOrientedContainerTerminalSimulation_Runs()
        {
            Assert.NotEqual(3, EventsExample.Run());
        }
    }


    /**
     * This is the model class. It is the main class of a simple event-oriented
     * model of the loading zone of a container terminal. Trucks arrive at the
     * terminal to load containers. They wait in line until a van carrier is
     * available to fetch a certain container and load it onto the truck. After
     * loading is completed, the truck leaves the terminal while the van carrier
     * serves the next truck.
     * @author Olaf Neidhardt, Ruth Meyer
     */
    public class EventsExample : Model
    {

        /**
         * model parameter: the number of van carriers
         */
        protected static int NUM_VC = 2;
        /**
         * Random number stream used to draw an arrival time for the next truck.
         * See init() method for stream parameters.
         */
        private ContDistExponential truckArrivalTime;
        /**
         * Random number stream used to draw a service time for a truck.
         * Describes the time needed by the VC to fetch and load the container
         * onto the truck.
         * See init() method for stream parameters.
         */
        private ContDistUniform serviceTime;
        /**
         * A waiting queue object is used to represent the parking area for
         * the trucks.
         * Every time a truck arrives it is inserted into this queue (it parks)
         * and will be removed by the VC for service.
         *
         * This way all necessary basic statistics are monitored by the queue.
         */
        public System.Collections.Generic.Queue<Truck> truckQueue;
        /**
         * A waiting queue object is used to represent the parking spot for
         * the VC.
         * If there is no truck waiting for service the VC will return here
         * and wait for the next truck to come.
         *
         * This way all idle time statistics of the VC are monitored by the queue.
         */
        public System.Collections.Generic.Queue<VanCarrier> idleVCQueue;
        /// <summary>
        /// EventsExample constructor. Creates a new EventsExample model via calling the constructor of the
        /// superclass.
        /// </summary>
        /// <param name="owner">
        /// the model this model is part of (set to <c>null</c> when there is no such model)
        /// </param>
        /// <param name="modelName">this model's name</param>
        /// <param name="showInReport">
        /// flag to indicate if this model shall produce output to the report file
        /// </param>
        /// <param name="showInTrace">
        /// flag to indicate if this model shall produce output to the trace file
        /// </param>
        public EventsExample(Model owner, string modelName, bool showInReport, bool showInTrace)
                : base(owner, modelName, showInReport, showInTrace)
        {
            //super(owner, modelName, showInReport, showInTrace);
        }
        /// <summary>Returns a description of the model to be used in the report.</summary>
        /// <returns>model description as a string</returns>
        public override string description()
        {
            return "This model describes a queueing system located at a " +
                        "container terminal. Trucks will arrive and " +
                        "require the loading of a container. A van carrier (VC) is " +
                        "on duty and will head off to find the required container " +
                        "in the storage. It will then load the container onto the " +
                        "truck. Afterwards, the truck leaves the terminal. " +
                        "In case the VC is busy, the truck waits " +
                        "for its turn on the parking-lot. " +
                        "If the VC is idle, it waits on its own parking spot for the " +
                        "truck to come.";
        }
        /**
         * Activates dynamic model components (events).
         *
         * This method is used to place all events or processes on the
         * internal event list of the simulator which are necessary to start
         * the simulation.
         *
         * In this case, the truck generator event will have to be
         * created and scheduled for the start time of the simulation.
         */
        public override void doInitialSchedules()
        {

            // create the TruckGeneratorEvent
            TruckGeneratorEvent truckGenerator =
                    new TruckGeneratorEvent(this, "TruckGenerator", true);

            // schedule for start of simulation
            truckGenerator.schedule(new TimeSpan(0));
        }
        /**
         * Initialises static model components like distributions and queues.
         */
        public override void init()
        {

            // initialise the serviceTimeStream
            // Parameters:
            // this                = belongs to this model
            // "ServiceTimeStream" = the name of the stream
            // 3.0                 = minimum time in minutes to deliver a container
            // 7.0                 = maximum time in minutes to deliver a container
            // true                = show in report?
            // false               = show in trace?
            serviceTime = new ContDistUniform(this, "ServiceTimeStream", 3.0, 7.0, true, false);

            // initalise the truckArrivalTimeStream
            // Parameters:
            // this                     = belongs to this model
            // "TruckArrivalTimeStream" = the name of the stream
            // 3.0                      = mean time in minutes between arrival of trucks
            // true                     = show in report?
            // false                    = show in trace?
            truckArrivalTime = new ContDistExponential(this, "TruckArrivalTimeStream", 3.0, true, false);

            // necessary because an inter-arrival time can not be negative, but
            // a sample of an exponential distribution can...
            truckArrivalTime.setNonNegative(true);

            // initalise the truckQueue
            // Parameters:
            // this          = belongs to this model
            // "Truck Queue" = the name of the Queue
            // true          = show in report?
            // true          = show in trace?
            truckQueue = new System.Collections.Generic.Queue<Truck>();
              

            // initalise the idleVCQueue
            // Parameters:
            // this            = belongs to this model
            // "idle VC Queue" = the name of the Queue
            // true            = show in report?
            // true            = show in trace?
            idleVCQueue = new System.Collections.Generic.Queue<VanCarrier>();

            // place the van carriers into the idle queue
            // We don't do this in the doInitialSchedules() method because
            // we aren't placing anything on the event list here.
            //VanCarrier VC;
            for (int i = 0; i < NUM_VC; i++)
            {
                // create a new VanCarrier
               var VC = new VanCarrier(this, "VanCarrier", true);
                // put it on his parking spot
                idleVCQueue.Enqueue(VC);
            }
        }
        /// <summary>
        /// Returns a sample of the random stream used to determine the time needed to fetch the container
        /// for a truck from the storage area and the time the VC needs to load it onto the truck.
        /// </summary>
        /// <returns>a serviceTime sample</returns>
        public double getServiceTime()
        {
            return serviceTime.sample();
        }
        /// <summary>
        /// Returns a sample of the random stream used to determine the next truck arrival time.
        /// </summary>
        /// <returns>a truckArrivalTime sample</returns>
        public double getTruckArrivalTime()
        {
            return truckArrivalTime.sample();
        }
        /// <summary>
        /// Runs the model. In DESMO-J used to - instantiate the experiment - instantiate the model - connect
        /// the model to the experiment - steer length of simulation and outputs - set the ending criterion
        /// (normally the time) - start the simulation - initiate reporting - clean up the experiment
        /// </summary>
        public static int Run()
        {

            // create model and experiment
            EventsExample model = new EventsExample(null, "EventsExample", true, true);
            // null as first parameter because it is the main model and has no mastermodel

            Experiment exp = new Experiment("EventExampleExperiment", TimeUnit.SECONDS, TimeUnit.MINUTES, null);
            // ATTENTION, since the name of the experiment is used in the names of the
            // output files, you have to specify a string that's compatible with the
            // filename constraints of your computer's operating system. The remaing three
            // parameters specify the granularity of simulation time, default unit to
            // display time and the time formatter to use (null yields a default formatter).
            // connect both
            model.connectToExperiment(exp);
         

            // set experiment parameters
            exp.setShowProgressBar(true);  // display a progress bar (or not)
            exp.stop(new TimeInstant(1500, TimeUnit.MINUTES));   // set end of simulation at 1500 minutes
            exp.tracePeriod(new TimeInstant(0), new TimeInstant(100, TimeUnit.MINUTES));  // set the period of the trace
            exp.debugPeriod(new TimeInstant(0), new TimeInstant(50, TimeUnit.MINUTES));   // and debug output
                                                                                          // ATTENTION!
                                                                                          // Don't use too long periods. Otherwise a huge HTML page will
                                                                                          // be created which crashes Netscape :-)
                                                                

            // start the experiment at simulation time 0.0
            exp.start();
            int status = exp.getStatus();

            // --> now the simulation is running until it reaches its end criterion
            // ...
            // ...
            // <-- afterwards, the main thread returns here

            // generate the report (and other output files)
            exp.report();

            // stop all threads still alive and close all output files
            exp.finish();
            return status;
        }
    } /* end of model class */

    //======================================================
    public class Truck : Entity
    {
        /// <summary>Constructor of the truck entity.</summary>
        /// <param name="owner">the model this entity belongs to</param>
        /// <param name="name">this truck's name</param>
        /// <param name="showInTrace">
        /// flag to indicate if this entity shall produce output for the trace
        /// </param>
        public Truck(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
        {
           
        }
    }
    //=================================================
    public class VanCarrier : Entity
    {

        /// <summary>Constructor of the van carrier entity.</summary>
        /// <param name="owner">the model this entity belongs to</param>
        /// <param name="name">this VC's name</param>
        /// <param name="showInTrace">
        /// flag to indicate if this entity shall produce output for the trace
        /// </param>
        public VanCarrier(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
        {

        }
    }

    //=============================================================
    public class ServiceEndEvent : EventOf2Entities
    {

        /**
         * A reference to the model this event is a part of.
         * Useful shortcut to access the model's static components
         */
        private EventsExample myModel;

        /// <summary>Constructor of the service end event Used to create a new service end event</summary>
        /// <param name="owner">the model this event belongs to</param>
        /// <param name="name">this event's name</param>
        /// <param name="showInTrace">
        /// flag to indicate if this event shall produce output for the trace
        /// </param>
        public ServiceEndEvent(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
        {

            // store a reference to the model this event is associated with
            myModel = (EventsExample)owner;
        }

        /**
         * This eventRoutine() describes what happens when a van carrier finishes
         * loading a truck.
         *
         * The truck leaves the system.
         * The van carrier will then check if there is another truck
         * waiting for service.
         * If there is another truck waiting it will service it.
         * If not it will wait on its parking spot for the next
         * customer to arrive.
         */
        public override void eventRoutine(Entity entity1, Entity entity2)
        {
            var vanCarrier = (VanCarrier)entity1;
            var truck = (Truck)entity2;

            // pass the departure of the truck to the trace
            sendTraceNote(truck + " leaves the terminal");

            // check if there are other trucks waiting
            if (myModel.truckQueue.Count > 0)
            {
                // YES, there is at least one other truck waiting

                // remove the first waiting truck from the queue
                Truck nextTruck = myModel.truckQueue.Dequeue();

                // create a new service end event
                ServiceEndEvent evt = new ServiceEndEvent(myModel, "ServiceEndEvent", true);
                // and schedule it for the van carrier at the appropriate time
                evt.schedule(vanCarrier, nextTruck, new TimeSpan(myModel.getServiceTime(), TimeUnit.MINUTES));
            }
            else
            {
                // NO, there are no trucks waiting

                // --> the van carrier is placed on its parking spot
                myModel.idleVCQueue.Enqueue(vanCarrier);

                // the VC is now waiting for a new customer to arrive
            }

        }
    }
    //===================================================================
    /**
     * This class represents the truck arrival event
     * in the EventsExample model.
     * It occurs when a truck arrives at the terminal
     * to request loading of a container.
     * @author Olaf Neidhardt, Ruth Meyer
     */
    public class TruckArrivalEvent : Event
    {

        /** a reference to the model this event is a part of.
	     * Useful shortcut to access the model's static components
	     */
        private EventsExample myModel;

        /// <summary>Constructor of the truck arrival event Used to create a new truck arrival event</summary>
        /// <param name="owner">the model this event belongs to</param>
        /// <param name="name">this event's name</param>
        /// <param name="showInTrace">
        /// flag to indicate if this event shall produce output for the trace
        /// </param>
        public TruckArrivalEvent(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
        {
            //super(owner, name, showInTrace);
            // store a reference to the model this event is associated with
            myModel = (EventsExample)owner;
            debugOn();
        }

        /**
         * This eventRoutine() describes what happens when a truck
         * enters the terminal.
         *
         * On arrival, the truck will enter the queue (parking lot). It will then
         * check if the van carrier is available.
         * If this is the case, it will occupy the van carrier and schedule a
         * service end event.
         * Otherwise the truck just waits (does nothing).
         */
        public override void eventRoutine(Entity truck)
        {

            var truckObj = (Truck)truck;
            // truck enters parking-lot
            myModel.truckQueue.Enqueue(truckObj);
            sendTraceNote("TruckQueueLength: " + myModel.truckQueue.Count);

            // check if a VC is available
            if (myModel.idleVCQueue.Count > 0)
            {
                // yes, it is

                // get a reference to the first VC from the idle VC queue
                VanCarrier vanCarrier = myModel.idleVCQueue.Dequeue();
                // remove it from the queue
                //myModel.idleVCQueue.remove(vanCarrier);

                // remove the truck from the queue
                //myModel.truckQueue..remove(truck);

                // create a service end event
                ServiceEndEvent serviceEnd = new ServiceEndEvent(myModel, "ServiceEndEvent", true);

                // and place it on the event list
                serviceEnd.schedule(vanCarrier, truckObj, new TimeSpan(myModel.getServiceTime(), TimeUnit.MINUTES));

            }
        }
    }
    //================================

    /**
     * This class represents an entity (and event) source, which continually generates
     * trucks (and their arrival events) in order to keep the simulation running.
     *
     * It will create a new truck, schedule its arrival at the terminal (i.e. create
     * and schedule an arrival event) and then schedule itself for the point in
     * time when the next truck arrival is due.
     * @author Olaf Neidhardt, Ruth Meyer
     */
    public class TruckGeneratorEvent : ExternalEvent
    {

        /// <summary>Constructs a new TruckGeneratorEvent.</summary>
        /// <param name="owner">the model this event belongs to</param>
        /// <param name="name">this event's name</param>
        /// <param name="showInTrace">
        /// flag to indicate if this event shall produce output for the trace
        /// </param>
        public TruckGeneratorEvent(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
        {

        }
        /**
         * The eventRoutine() describes the generating of a new truck.
         *
         * It creates a new truck, a new TruckArrivalEvent
         * and schedules itself again for the next new truck generation.
         */
        public override void eventRoutine()
        {

            // get a reference to the model
            EventsExample model = (EventsExample)getModel();

            // create a new truck
            Truck truck = new Truck(model, "Truck", true);
            // create a new truck arrival event
            TruckArrivalEvent truckArrival = new TruckArrivalEvent(model, "TruckArrivalEvent", true);
            // and schedule it for the current point in time
            truckArrival.schedule(truck, new TimeSpan(0.0));

            // schedule this truck generator again for the next truck arrival time
            schedule(new TimeSpan(model.getTruckArrivalTime(), TimeUnit.MINUTES));
            // from inside to outside...
            // draw a new inter-arrival time value
            // wrap it in a TimeSpan object
            // and schedule this event for the current point in time + the inter-arrival time

        }
    }
}

