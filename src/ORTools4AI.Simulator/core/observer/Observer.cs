// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Christian Mentz
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// Modified for ORTools4AI.Simulator.

using System;

namespace ORTools4AI.Simulator.core.observer
{
	/**
	 * Interface which an Observer class can implement to get Information from a Subject Class
	 * 
	 * 
	 * @param <T>
	 *            Class of an possible Subject
	 * @param <X>
	 *            Type of the Event which is passed in the update Method
	 */
	public interface Observer<T, X>
	{

		/// <summary>This Method is called when a Subject calls his notify Method</summary>
		/// <param name="subject">Reference to the calling Subject</param>
		/// <param name="eventObject">
		/// An eventtype to have the chance of Making branches for the different eventtypes
		/// </param>
		public void update(T subject, X eventObject);
	}

    public class Observable
    {
        public event EventHandler SomethingHappened;

        public void DoSomething()
        {
            EventHandler handler = SomethingHappened;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public int countObservers()
		{
            throw new NotImplementedException();
        }

        public bool hasChanged()
		{
			throw new NotImplementedException();
		}

        protected void setChanged()
        {
        }

        public void notifyObservers(object arg)
		{

		}
        public void notifyObservers()
		{

		}
        protected void clearChanged()
		{

		}
        public void addObserver(Observer o)
		{

		}

		public void deleteObserver(Observer o)
		{
		}
    }

    public interface Observer
    {
        void update(object value1, object value2);
    }
}