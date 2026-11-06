// Modified for ORTools4AI.Simulator.


using System.Collections;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;

using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test
{
    public class SimProcessExampleRun
    {
        [Fact(Timeout = 120_000)]
        public void ProcessOrientedContainerTerminalSimulation_Runs()
        {
            Assert.NotEqual(3, SimProcessExample.run());
        }
    }

    /**
* This is the model class. It is the main class of a simple process-oriented
* model of the loading zone of a container terminal. Trucks arrive at the
* terminal to load containers. They wait in line until a van carrier (VC) is
* available to fetch a certain container and load it onto the truck. After
* loading is completed, the truck leaves the terminal while the van carrier
* serves the next truck.
*
* http://desmoj.sourceforge.net/tutorial/processes/impl0.html
*
* Model is a subclasses of ModelComponent
*/
    
    public class SimProcessExample : Model
    {


        /**
         * Model parameter: the number of van carriers
         */
        protected static int NUM_VC = 2;

        /**
         * Random number stream used to draw an arrival time for the next truck.
         * See init() method for stream parameters.
         */
        private ContDistExponential? truckArrivalTime;

        /**
         * Random number stream used to draw a service time for a truck.
         * Describes the time needed by the VC to fetch and load the container
         * onto the truck.
         * See init() method for stream parameters.
         */
        private ContDistUniform? serviceTime;
        /**
         * A waiting queue object is used to represent the parking area for
         * the trucks.
         * Every time a truck arrives it is inserted into this queue (it parks)
         * and will be removed by the VC for service.
         *
         * This way all necessary basic statistics are monitored by the queue.
         */
        public System.Collections.Generic.Queue<SimProcessTruck>? truckQueue;
        /**
         * A waiting queue object is used to represent the parking spot for
         * the VC.
         * If there is no truck waiting for service the VC will return here
         * and wait for the next truck to come.
         *
         * This way all idle time statistics of the VC are monitored by the queue.
         */
        public System.Collections.Generic.Queue<SimProcessVanCarrier>? idleVCQueue;

        /// <summary>
        /// ProcessesExample constructor. Creates a new ProcessesExample model via calling the constructor of
        /// the superclass.
        /// </summary>
        /// <param name="owner">
        /// the model this model is part of (set to null when there is no such model)
        /// </param>
        /// <param name="modelName">this model's name</param>
        /// <param name="showInReport">
        /// flag to indicate if this model shall produce output to the report file
        /// </param>
        /// <param name="showInTrace">
        /// flag to indicate if this model shall produce output to the trace file
        /// </param>
        public SimProcessExample(Model owner, string modelName, bool showInReport,
                                bool showInTrace) : base(owner, modelName, showInReport, showInTrace)
        {
            // super(owner, modelName, showInReport, showInTrace);
        }



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
         * Activates dynamic model components (simulation processes).
         *
         * This method is used to place all events or processes on the
         * internal event list of the simulator which are necessary to start
         * the simulation.
         *
         * In this case, the truck generator and the van carrier(s) have to be
         * created and activated.
         */

        public override void doInitialSchedules()
        {

            // create and activate the van carrier(s)
            for (int i = 0; i < NUM_VC; i++)
            {
                var vanCarrier = new SimProcessVanCarrier(this, "Van Carrier", true);
                vanCarrier.activate(new TimeSpan(0.0));
                // Use TimeSpan to activate a process after a span of time relative to actual simulation time,
                // or use TimeInstant to activate the process at an absolute point in time.
            }

            // create and activate the truck generator process
            var generator = new SimProcessTruckGenerator(this, "TruckArrival", false);
            generator.activate(new TimeSpan(0.0));
        }


        public override void init()
        {

            // initialise the serviceTimeStream
            // Values produced by this distribution are uniformly distributed in the range specified as parameters of the constructor.
            // Parameters:
            // this                = belongs to this model
            // "ServiceTimeStream" = the name of the stream
            // 3.0                 = minimum time in minutes to deliver a container (lower bound)
            // 7.0                 = maximum time in minutes to deliver a container (upper bound)
            // true                = show in report?
            // false               = show in trace?
            serviceTime = new ContDistUniform(this, "ServiceTimeStream", 3.0, 7.0, true, false);

            // initialise the truckArrivalTimeStream
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

            // initialise the truckQueue
            // Parameters:
            // this          = belongs to this model
            // "Truck Queue" = the name of the Queue
            // true          = show in report?
            // true          = show in trace?
            truckQueue = new System.Collections.Generic.Queue<SimProcessTruck>();

            // initialise the idleVCQueue
            // Parameters:
            // this            = belongs to this model
            // "idle VC Queue" = the name of the Queue
            // true            = show in report?
            // true            = show in trace?
            idleVCQueue = new System.Collections.Generic.Queue<SimProcessVanCarrier>();

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

        /// <summary>Runs the model.</summary>
        public static int run()
        {

            // create model and experiment
            SimProcessExample model = new SimProcessExample(null,
                    "Simple Process-Oriented Van Carrier Model", true, true);
            // null as first parameter because it is the main model and has no mastermodel

            Experiment exp = new Experiment("SimProcessExampleExperiment", TimeUnit.SECONDS, TimeUnit.MINUTES, null);
            // ATTENTION, since the name of the experiment is used in the names of the
            // output files, you have to specify a string that's compatible with the
            // filename constraints of your computer's operating system.

            // connect both
            model.connectToExperiment(exp);

            // set experiment parameters
            exp.setShowProgressBar(true);  // display a progress bar (or not)
            exp.stop(new TimeInstant(1500, TimeUnit.MINUTES));   // set end of simulation at 1500 minutes
            exp.tracePeriod(new TimeInstant(0), new TimeInstant(100, TimeUnit.MINUTES));
            // set the period of the trace
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

    }

    /**
 * This class represents the truck in the ProcessesExample
 * model.
 * <p>
 * A truck arrives at the container terminal and requests
 * loading of a container. If possible, it is served
 * by the van carrier immediately. Otherwise it waits in the
 * parking area for its turn.
 * After service is completed, it leaves the system.
 */
    public class SimProcessTruck : SimProcess
    {

        /**
         * a reference to the model this process is a part of
         * useful shortcut to access the model's static components
         */
        private SimProcessExample myModel;

        /// <summary>
        /// Constructor of the truck process Used to create a new truck to be serviced by a van carrier.
        /// </summary>
        /// <param name="owner">the model this process belongs to</param>
        /// <param name="name">this truck's name</param>
        /// <param name="showInTrace">
        /// flag to indicate if this process shall produce output for the trace
        /// </param>
        public SimProcessTruck(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
        {

            //super(owner, name, showInTrace);
            // store a reference to the model this truck is associated with
            myModel = (SimProcessExample)owner;
            //debugOn();
        }

        /// <summary>
        /// Describes this truck's life cycle: On arrival, the truck will enter the queue (parking lot). It
        /// will then check if the van carrier is available. If this is the case, it will activate the van
        /// carrier to get serviced and transfer the control to the VC. Otherwise it just passivates (waits).
        /// After service it leaves the system.
        /// </summary>

        protected override IEnumerable Run()
        {

            // enter parking-lot
            // driving our truck onto the parking lot
            // Each queue is generating a report from the statistical data collected during the simulation run.
            myModel.truckQueue.Enqueue(this);
            sendTraceNote("TruckQueuelength: " + myModel.truckQueue.Count());

            // check if a VC is available
            if (myModel.idleVCQueue.Count > 0)
            {
                // yes, it is

                // get a reference to the first  VC from the idle VC queue
                var vanCarrier = myModel.idleVCQueue.Dequeue();
                // remove the van carrier from the queue
                //myModel.idleVCQueue.remove(vanCarrier);

                // place the VC on the eventlist right after me,
                // to ensure that I will be the next customer to get serviced
                vanCarrier.activateAfter(this);
            }

            // wait for service
            yield return Suspend();

            // When the scheduler returns control back to our truck object,
            // the truck will just send a message to the trace output before it ends its life cycle.
            // Ok, I am back online again, which means I was serviced
            // by the VC. I can leave the systems now.
            // Luckily I don't have to do anything more than sending
            // a message to the trace file, because the
            // Java VM garbage collector will get the job done.
            // Bye!
            sendTraceNote("Truck was serviced and leaves system.");


        }

       
    }

   

    
    /**
 * This class represents a process source, which continually generates
 * trucks in order to keep the simulation running.
 * <p>
 * It will create a new truck, activate it (so that it arrives at
 * the terminal) and then wait until the next truck arrival is
 * due.
 *
 * SimProcess is a subclasses of ModelComponent
 */
    public class SimProcessTruckGenerator : SimProcess
    {

        /// <summary>TruckGenerator constructor comment.</summary>
        /// <param name="owner">the model this truck generator belongs to</param>
        /// <param name="name">this truck generator's name</param>
        /// <param name="showInTrace">
        /// flag to indicate if this process shall produce output for the trace
        /// </param>
        public SimProcessTruckGenerator(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
        {

        }


        /**
         * describes this process's life cycle: continually generate new trucks.
         *  SuspendExecution put a "mark" where process execution can be interrupted for the internal process scheduling logic
         */

        //public override void lifeCycle()
        protected override IEnumerable Run()
        {

            // get a reference to the model
            var model = (SimProcessExample)getModel();

            // endless loop:
            while (true)
            {

                // create a new truck
                // Parameters:
                // model       = it's part of this model
                // "Truck"     = name of the object
                // true        = yes please, show the truck in trace file
                var truck = new SimProcessTruck(model, "Truck", true);

                // now let the newly created truck roll on the parking-lot
                // which means we will activate it after this truck generator
                truck.activateAfter(this);

                // wait until next truck arrival is due
                yield return Delay(model.getTruckArrivalTime());
            }
        }
    }

    /**
 * This class represents a van carrier in the ProcessesExample
 * model.
 * The VC waits until a truck requests its service.
 * It will then fetch the container and load it onto
 * the truck.
 * If there is another truck waiting it starts serving this
 * truck. Otherwise it waits again for the next truck to arrive.
 *
 * An active component is a subclass of {@link SimProcess}
 */
    public class SimProcessVanCarrier : SimProcess
    {

        /**
         * a reference to the model this process is a part of
         * useful shortcut to access the model's static components
         */
        private SimProcessExample myModel;

        /// <summary>Constructor of the van carrier process Used to create a new VC to serve trucks.</summary>
        /// <param name="owner">the model this process belongs to</param>
        /// <param name="name">this VC's name</param>
        /// <param name="showInTrace">
        /// flag to indicate if this process shall produce output for the trace
        /// </param>
        public SimProcessVanCarrier(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
        {

            //super(owner, name, showInTrace);
            // store a reference to the model this VC is associated with
            myModel = (SimProcessExample)owner;
        }

        //@Override
        /**
         * Describes this van carrier's life cycle.
         *
         * It will continually loop through the following steps:
         * Check if there is a customer waiting.
         * If there is someone waiting
         *   a) remove customer from queue
         *   b) serve customer
         *   c) return to top
         * If there is no one waiting
         *   a) wait (passivate) until someone arrives (who reactivates the VC)
         *   b) return to top
         */
        //public override void lifeCycle()

        protected override IEnumerable Run()
        {

            // the server is always on duty and will never stop working
            while (true)
            {
                // check if there is someone waiting
                if (myModel.truckQueue.Count == 0)
                {

                    // NO, there is no one waiting

                    // put the VC in the idle VC queue in order to wait for the next truck
                    myModel.idleVCQueue.Enqueue(this);
                    // and wait for things to happen
                    yield return Suspend();

                }
                else
                {

                    // YES, there is a customer (truck) waiting
                    // We start with removing the first truck in line to get it serviced.

                    // get a reference to the first truck from the truck queue
                    var nextTruck = myModel.truckQueue.Dequeue();
                    // remove the truck from the queue
                    //myModel.truckQueue.remove(nextTruck);

                    // now serve it (fetch container and load it onto truck)
                    // service time is represented by a hold of the VC process
                    yield return Delay(myModel.getServiceTime());

                    // now the truck has received its container and can leave
                    // we will reactivate it, to allow it to finish its life cycle
                    nextTruck.activate(new TimeSpan(0));
                    // the VC can return to top and check for a new customer

                }
            }
        }


    }



}
