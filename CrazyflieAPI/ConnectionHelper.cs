using CrazyflieAPI.JSONContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyflieAPI
{
    /// <summary>
    /// Connection helper for the Crazyflie API
    /// </summary>
    public sealed class ConnectionHelper
    {
        #region Singleton Implementation
        /// <summary>
        /// Instance variable for singleton
        /// </summary>
        private static ConnectionHelper instance;
        /// <summary>
        /// Lock variable for singleton
        /// </summary>
        private static object instanceLock = new object();
        /// <summary>
        /// Gets the connection helper instance
        /// </summary>
        public static ConnectionHelper Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new ConnectionHelper();
                    }
                }

                return instance;
            }
        }
        #endregion

        /// <summary>
        /// Private variable for device input data
        /// </summary>
        private InputDeviceJSONContainer inputData;
        /// <summary>
        /// Property for private variable inputData
        /// </summary>
        public InputDeviceJSONContainer InputData
        {
            get
            {
                return inputData;
            }
            set
            {
                lock (commLock)
                {
                    inputData = value;
                }
            }
        }

        /// <summary>
        /// Communication thread
        /// </summary>
        private Thread commThread;
        /// <summary>
        /// Lock variable for communication thread
        /// </summary>
        private object commLock = new object();
        /// <summary>
        /// Boolean value to stop the communication thread
        /// </summary>
        private bool runCommThread;

        /// <summary>
        /// Determines whether the 
        /// </summary>
        public bool Running
        {
            get
            {
                return runCommThread ||
                    commThread.ThreadState == ThreadState.Running;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private ConnectionHelper()
        {
            inputData = new InputDeviceJSONContainer();
            commThread = new Thread(SendInputData);
            runCommThread = false;
        }

        /// <summary>
        /// Sends the device input data to the Crazyflie client
        /// Runs in a seperate method
        /// </summary>
        private void SendInputData()
        {
            var api = CrazyflieAPI.Instance;

            // Send zero input data to unlock the controls
            {
                var container = new InputDeviceJSONContainer();

                lock (commLock)
                {
                    container.ctrl.estop = inputData.ctrl.estop;
                    container.ctrl.alt1 = inputData.ctrl.alt1;
                    container.ctrl.alt2 = inputData.ctrl.alt2;
                }

                api.SendInputData(container);
            }

            while (runCommThread)
            {
                lock (commLock)
                {
                    api.SendInputData(inputData);
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Start communication thread
        /// </summary>
        public void StartThread()
        {
            if (!Running)
            {
                var api = CrazyflieAPI.Instance;

                if (!api.Connected)
                {
                    api.Connect();
                }

                runCommThread = true;
                commThread = new Thread(SendInputData);
                commThread.Start();
            }
        }

        /// <summary>
        /// Stop communication thread
        /// </summary>
        public void StopThread()
        {
            if (Running)
            {
                runCommThread = false;
            }
        }
    }
}
