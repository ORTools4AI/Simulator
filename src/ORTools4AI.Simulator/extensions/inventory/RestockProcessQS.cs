// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.advancedModellingFeatures;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ORTools4AI.Simulator.extensions.inventory
{
    /**
  * RestockProcessQS is a process restocking a <code>Stock</code> with a fixed
  * given quantity (Q) every time a given safety (S) stock level is reached.
  */
    public class RestockProcessQS<N> : SimProcess, PropertyChangeListener
    {

        /**
         * The fixed quantity of units the <code>Stock</code> will be replenished
         * with.
         */
        private long orderQuantity;

        /**
         * The client <code>Stock</code> which will be supplied by this
         * <code>RestockProcess</code>.
         */
        private Stock clientStock;

        /**
         * The random distribution stream determining the lead time. That is the
         * time between the placement and receipt of an order. If <code>null</code>
         * lead time is zero.
         */
        private NumericalDist<N> leadTime;

        /**
         * The safety stock level. In case this level is reached a new order will be
         * placed.
         */
        private long safetyStockLevel;

        /// <summary>
        /// Constructs a <c>RestockProcessQS</c> which restocks a client <c>Stock</c> after a fixed time
        /// period up to a maximum inventory level. The lead time (time gap between placement and receipt of
        /// an order) will be given as a real random number distribution.
        /// </summary>
        /// <param name="owner">The model this <c>RestockProcessQS</c> is associated to</param>
        /// <param name="name">The name of the <c>RestockProcessQS</c></param>
        /// <param name="q">The quantity supplied to the Stock with every order.</param>
        /// <param name="safetyStk">
        /// The safety stock. When this inventory level is reached an order will be placed
        /// </param>
        /// <param name="client">The <c>Stock</c> which will replenished.</param>
        /// <param name="lt">
        /// The lead time random number distribution to determine the time between placement and receipt of
        /// an order. If <c>null</c> the lead time is zero.
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing the <c>RestockProcessQS</c> in trace-files. Set it to <c>true</c> if
        /// RestockProcessQS should show up in trace. Set it to <c>false</c> if RestockProcessQS should not
        /// be shown in trace.
        /// </param>
        public RestockProcessQS(Model owner, string name, long q, long safetyStk,
                Stock client, NumericalDist<N> lt, bool showInTrace) : base(owner, name, true, showInTrace)
        {
            //super(owner, name, true, showInTrace); // make a sim-process

            this.orderQuantity = q;
            this.safetyStockLevel = safetyStk;
            this.clientStock = client;
            this.leadTime = lt;

            // check the safety stock level parameter
            if (safetyStk <= 0 || safetyStk > client.getCapacity())
            {
                sendWarning(
                        "The given safety stock level parameter is zero, "
                                + "negative or greater than the capacity of the Stock. "
                                + "The safety stock level will be set to one!",
                        "RestockProcessQS : "
                                + getName()
                                + " Constructor: RestockProcessQS(Model owner, String name, "
                                + "long q, long safetyStk, Stock client, boolean showInTrace)",
                        "A safety stock level that is zero, negative or "
                                + "greater than the capacity does not make sense.",
                        "Make sure to provide a safety stock level that is a "
                                + "valid positive integer number not greater than the capacity.");

                // set the safety stock level to one
                this.safetyStockLevel = 1; // better than nothing
            }

            // check the quantity parameter
            if (q <= 0)
            {
                sendWarning(
                        "The given quantity parameter is negative or zero!"
                                + "The quantity will be set to one!",
                        "RestockProcessQS : "
                                + getName()
                                + " Constructor: RestockProcessQS(Model owner, String name, "
                                + "long q, long safetyStk, Stock client, boolean showInTrace)",
                        "A negative or zero quantity for an order does not make "
                                + "sense.",
                        "Make sure to provide a valid positive integer number "
                                + "for the quantity of every order.");

                // set the quantity to at least one
                this.orderQuantity = 1; // better than nothing
            }

            // check the client Stock parameter
            if (client == null)
            {
                sendWarning(
                        "The given client parameter is only a null pointer!",
                        "RestockProcessQS : "
                                + getName()
                                + " Constructor: RestockProcessQS(Model owner, String name, "
                                + "long mil, SimTime t, Stock client, RealDist lt, boolean "
                                + "showInTrace)",
                        "The RestockProcessQS does not know which Stock to replenish "
                                + "and therefore is useless.",
                        "Make sure to provide a valid Stock object which should "
                                + "be replenished by this RestockProcessQS.");

                return; // forget about it
            }

            // register as PropertyChangeListener at the Stock to get the news
            // about the available inventoy level
            clientStock.addPropertyChangeListener("avail", this);

        }

        /// <summary>
        /// Constructs a <c>RestockProcessQS</c> which restocks a client <c>Stock</c> with a fixed quantity
        /// every time the safety stock is reached. The lead time is zero.
        /// </summary>
        /// <param name="owner">The model this <c>RestockProcessQS</c> is associated to</param>
        /// <param name="name">The name of the <c>RestockProcessQS</c></param>
        /// <param name="q">The quantity supplied to the Stock with every order.</param>
        /// <param name="safetyStk">
        /// The safety stock. When this inventory level is reached an order will be placed
        /// </param>
        /// <param name="client">The <c>Stock</c> which will replenished.</param>
        /// <param name="showInTrace">
        /// Flag for showing the <c>RestockProcessQS</c> in trace-files. Set it to <c>true</c> if
        /// RestockProcessQS should show up in trace. Set it to <c>false</c> if RestockProcessQS should not
        /// be shown in trace.
        /// </param>
        public RestockProcessQS(Model owner, string name, long q, long safetyStk,
                Stock client, bool showInTrace) : base(owner, name, true, showInTrace)
        {
            //super(owner, name, true, showInTrace); // make a sim-process

            this.orderQuantity = q;
            this.safetyStockLevel = safetyStk;
            this.clientStock = client;
            this.leadTime = null;

            // check the safety stock level parameter
            if (safetyStk <= 0 || safetyStk > client.getCapacity())
            {
                sendWarning(
                        "The given safety stock level parameter is zero, "
                                + "negative or greater than the capacity of the Stock. "
                                + "The safety stock level will be set to one!",
                        "RestockProcessQS : "
                                + getName()
                                + " Constructor: RestockProcessQS(Model owner, String name, "
                                + "long q, long safetyStk, Stock client, boolean showInTrace)",
                        "A safety stock level that is zero, negative or "
                                + "greater than the capacity does not make sense.",
                        "Make sure to provide a safety stock level that is a "
                                + "valid positive integer number not greater than the capacity.");

                // set the safety stock level to one
                this.safetyStockLevel = 1; // better than nothing
            }

            // check the quantity parameter
            if (q <= 0)
            {
                sendWarning(
                        "The given quantity parameter is negative or zero!"
                                + "The quantity will be set to one!",
                        "RestockProcessQS : "
                                + getName()
                                + " Constructor: RestockProcessQS(Model owner, String name, "
                                + "long q, long safetyStk, Stock client, boolean showInTrace)",
                        "A negative or zero quantity for an order does not make "
                                + "sense.",
                        "Make sure to provide a valid positive integer number "
                                + "for the quantity of every order.");

                // set the quantity to at least one
                this.orderQuantity = 1; // better than nothing
            }

            // check the client Stock parameter
            if (client == null)
            {
                sendWarning(
                        "The given client parameter is only a null pointer!",
                        "RestockProcessQS : "
                                + getName()
                                + " Constructor: RestockProcessQS(Model owner, String name, "
                                + "long q, long safetyStk, Stock client, boolean showInTrace)",
                        "The RestockProcessQS does not know which Stock to replenish "
                                + "and therefore is useless.",
                        "Make sure to provide a valid Stock object which should "
                                + "be replenished by this RestockProcessQS.");

                return; // forget about it
            }

            // register as PropertyChangeListener at the Stock to get the news
            // about the available inventoy level
            clientStock.addPropertyChangeListener("avail", this);

        }

        /// <summary>
        /// Returns the random number distribution for the lead time (time between placement and receipt of
        /// an order).
        /// </summary>
        /// <returns>
        /// The random number distribution for the lead time (time between placement and receipt of an
        /// order).
        /// </returns>
        public NumericalDist<N> getLeadTime()
        {
            return leadTime;
        }

        /// <summary>Returns the quantity (number of units) to be stored in the Stock.</summary>
        /// <returns>The Stock will be replenished with this number of units.</returns>
        public long getOrderQuantity()
        {
            return orderQuantity;
        }

        /// <summary>
        /// Returns the safety stock level. When this inventory level is reached a new order will be placed.
        /// </summary>
        /// <returns>
        /// The safety stock level. When this inventory level is reached a new order will be placed.
        /// </returns>
        public long getSafetyStockLevel()
        {

            return safetyStockLevel;
        }

        /// <summary>
        /// The <c>RestockProcessQS</c> replenishes the associated <c>Stock</c> with the given quantity of
        /// units (Q) every time the inventory level dropped below the safety (S) stock level.
        /// </summary>
        //public override void lifeCycle()
        protected override IEnumerable Run()
        {

            // wait until inventory level drops below safety stock
            //passivate();
            yield return Suspend();

            // woken up because inventory level dropped below safety stock
            // place order (and tell so in the debug file)
            if (currentlySendTraceNotes())
            {
                sendTraceNote("places an order over " + orderQuantity
                        + " units for " + "Stock "
                        + clientStock.getQuotedName());
            }

            // wait the lead time if necessary
            if (leadTime != null)
            {
                double leadDuration = leadTime.GetSampleValue();

                // check lead duration non-negative
                if (leadDuration < 0)
                {

                    sendWarning(
                            "Lead duration distribution sample is negative (" + leadDuration + "). Assuming"
                                    + " immediate delivery instead (i.e. duration 0).",
                            "RestockProcessQS : "
                                    + getName()
                                    + " lifeCycle()",
                            "The given lead time distribution " + leadTime.getName()
                                    + " has returned a negative sample.",
                            "Make sure to use a non-negativ lead time distribution."
                                    + " Distributions potentially yielding negative values"
                                    + " (like Normal distributions) should bet set to non-negative.");

                    // set lead duration to 0
                    leadDuration = 0;

                }

                hold(new SimTime(leadDuration));
            }

            // store the ordered quantity in the Stock
            clientStock.store(orderQuantity);

        }

        /// <summary>
        /// Informs the RestockProcessQS every time the inventory level of the Stock changes. If the
        /// inventory level of the Stock drops below the safety stock level this RestockProcessQS must be
        /// woken up to place an order.
        /// </summary>
        /// <param name="evt">Informing the event about an inventory level change of the Stock.</param>
        public void propertyChange(PropertyChangeEvent evt)
        {

            // if this RestockProcessQS is scheduled an order is already on its way
            // only if this RestockProcessQS is not scheduled yet it must be woken
            // up
            if (!isScheduled())
            {
                // if safety stock level is reached place a new order
                if (clientStock.getAvail() <= safetyStockLevel)
                {
                    activateAfter(currentSimProcess());
                }
            }

        }

        /// <summary>
        /// Set the lead time to a new real random number distribution. If set to <c>null</c> the lead time
        /// is zero.
        /// </summary>
        /// <param name="newLeadTime">
        /// The new real random number distribution determining the lead time.
        /// </param>
        public void setLeadTime(NumericalDist<N> newLeadTime)
        {

            leadTime = newLeadTime;
        }

        /// <summary>
        /// Sets the order quantity to a new value. Zero or negative values will be rejected.
        /// </summary>
        /// <param name="newOrderQuantity">
        /// Choose a postitive value greater than zero as new order quantity.
        /// </param>
        public void setOrderQuantity(long newOrderQuantity)
        {

            if (newOrderQuantity <= 0)
            {
                sendWarning(
                        "The given quantity parameter is negative or zero!"
                                + "The order quantity will remain unchanged!",
                        "RestockProcessQS : "
                                + getName()
                                + " Method: void setOrderQuantity(long newOrderQuantity)",
                        "A negative or zero quantity for an order does not make "
                                + "sense.",
                        "Make sure to provide a valid positive integer number "
                                + "for the quantity of every order.");

                return; // do nothing, just return. ignore that rubbish
            }

            orderQuantity = newOrderQuantity;
        }

        /// <summary>
        /// Sets the safety stock level to a new value. Make sure it is greater than zero and less than the
        /// capacity of the Stock.
        /// </summary>
        /// <param name="newSafetyStk">
        /// The new safety stock level. Make sure it is greater than zero and less than the capacity of the
        /// Stock.
        /// </param>
        public void setSafetyStockLevel(long newSafetyStk)
        {

            // check the new safety stock level value
            if (newSafetyStk <= 0 || newSafetyStk > clientStock.getCapacity())
            {
                sendWarning(
                        "The given safety stock level parameter is zero, "
                                + "negative or greater than the capacity of the Stock. "
                                + "The safety stock level will remain unchanged!",
                        "RestockProcessQS : "
                                + getName()
                                + " Method: void setSafetyStockLevel(long newSafetyStk)",
                        "A safety stock level that is zero, negative or "
                                + "greater than the capacity does not make sense.",
                        "Make sure to provide a safety stock level that is a "
                                + "valid positive integer number not greater than the capacity.");

                return; // leave that rubbish alone
            }

            safetyStockLevel = newSafetyStk;
        }
    }
}
