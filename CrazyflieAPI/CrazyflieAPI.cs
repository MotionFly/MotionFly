using CrazyflieAPI.JSONContainer;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Diagnostics;

namespace CrazyflieAPI
{
    /// <summary>
    /// Crazyflie API class
    /// </summary>
    public sealed class CrazyflieAPI
    {
        /// <summary>
        /// Socket typs
        /// </summary>
        private enum ConnectionSocket
        {
            ParameterSocket,
            LEDRingSocket,
            InputSocket
        }

        #region Singleton Implementation
        /// <summary>
        /// Instance variable for singleton
        /// </summary>
        private static CrazyflieAPI instance;
        /// <summary>
        /// Lock variable for singleton
        /// </summary>
        private static object instanceLock = new object();
        /// <summary>
        /// Gets the Crazyflie API instance
        /// </summary>
        public static CrazyflieAPI Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CrazyflieAPI();
                    }
                }

                return instance;
            }
        }
        #endregion

        #region Connection Strings
        /// <summary>
        /// Connection string to the connect to the Crazyflie client
        /// </summary>
        private const string ConnectionAddress = "tcp://localhost:{0}";
        /// <summary>
        /// Port of the parameter interface socket
        /// </summary>
        private const int ParameterPort = 1213;
        /// <summary>
        /// Port of the LED ring interface socket
        /// </summary>
        private const int LEDRingPort = 1214;
        /// <summary>
        /// Port of the input interface socket
        /// </summary>
        private const int InputPort = 1212;
        #endregion

        /// <summary>
        /// Determines whether this instance is connected to the
        /// Crazyflie client
        /// </summary>
        public bool Connected { get; private set; }
        /// <summary>
        /// If true, the communication is logged to the debug console
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        /// NetMQ context
        /// </summary>
        private NetMQContext context;
        /// <summary>
        /// Parameter socket
        /// </summary>
        private RequestSocket parameterSocket;
        /// <summary>
        /// LED ring input
        /// </summary>
        private PushSocket ledRingSocket;
        /// <summary>
        /// Device input socket
        /// </summary>
        private PushSocket inputSocket;
        /// <summary>
        /// Lock variable for communication
        /// </summary>
        private object commLock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        private CrazyflieAPI()
        {
            context = NetMQContext.Create();
            parameterSocket = context.CreateRequestSocket();
            ledRingSocket = context.CreatePushSocket();
            inputSocket = context.CreatePushSocket();

            Connected = false;
            DebugMode = false;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~CrazyflieAPI()
        {
            Disconnect();

            inputSocket.Dispose();
            ledRingSocket.Dispose();
            parameterSocket.Dispose();
            context.Dispose();
        }

        /// <summary>
        /// Connects to the Crazyflie client
        /// </summary>
        public void Connect()
        {
            lock (commLock)
            {
                if (!Connected)
                {
                    parameterSocket.Connect(
                        string.Format(
                            ConnectionAddress,
                            ParameterPort
                        ));
                    ledRingSocket.Connect(
                        string.Format(
                            ConnectionAddress,
                            LEDRingPort
                        ));
                    inputSocket.Connect(
                        string.Format(
                            ConnectionAddress,
                            InputPort
                        ));

                    Connected = true;

                    Log("Connected to " + ConnectionAddress);
                }
            }
        }

        /// <summary>
        /// Disconnects from the Crazyflie client
        /// </summary>
        public void Disconnect()
        {
            lock (commLock)
            {
                if (Connected)
                {
                    parameterSocket.Disconnect(
                        string.Format(
                            ConnectionAddress,
                            ParameterPort
                        ));
                    ledRingSocket.Disconnect(
                        string.Format(
                            ConnectionAddress,
                            LEDRingPort
                        ));
                    inputSocket.Disconnect(
                        string.Format(
                            ConnectionAddress,
                            InputPort
                        ));

                    Connected = false;

                    Log("Disconnected from " + ConnectionAddress);
                }
            }
        }

        /// <summary>
        /// Sends parameter data
        /// </summary>
        /// <param name="container">Parameter data</param>
        public void SendParameterData(ParameterJSONContainer container)
        {
            SendFrame(ConnectionSocket.ParameterSocket, container.Serialize());
        }

        /// <summary>
        /// Sends LED ring data
        /// </summary>
        /// <param name="container">LED ring data</param>
        public void SendLEDRingData(LEDRingJSONContainer container)
        {
            SendFrame(ConnectionSocket.LEDRingSocket, container.Serialize());
        }

        /// <summary>
        /// Sends device input data
        /// </summary>
        /// <param name="container">Device input data</param>
        public void SendInputData(InputDeviceJSONContainer container)
        {
            SendFrame(ConnectionSocket.InputSocket, container.Serialize());
        }

        /// <summary>
        /// Sends a JSON package over a specified socket
        /// </summary>
        /// <param name="socket">Socket to use</param>
        /// <param name="data">JSON package</param>
        private void SendFrame(ConnectionSocket socket, string data)
        {
            lock (commLock)
            {
                if (Connected)
                {
                    switch (socket)
                    {
                        case ConnectionSocket.ParameterSocket:
                            parameterSocket.SendFrame(data);
                            break;
                        case ConnectionSocket.LEDRingSocket:
                            ledRingSocket.SendFrame(data);
                            break;
                        case ConnectionSocket.InputSocket:
                            inputSocket.SendFrame(data);
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    Log("Sent data over " + socket.ToString() + ":\n" + data);
                }
                else
                {
                    Log("Could not send data over " + socket.ToString() + ":\n" + data);
                }
            }
        }

        /// <summary>
        /// Logs data
        /// </summary>
        /// <param name="data">String to log</param>
        private void Log(string data)
        {
            if (DebugMode)
            {
                Debug.WriteLine(data);
            }
        }
    }
}
