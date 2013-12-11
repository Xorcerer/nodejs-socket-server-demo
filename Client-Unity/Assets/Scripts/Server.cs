using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Runtime.Serialization;

namespace Camp.HogRider
{
    public class Server
    {
        TcpClient m_client;
        readonly string m_host;
        readonly int m_port;
        readonly int m_bufferSize;
        int m_nBytesAvailable;

        public event Action<object> MessageReceived;
        public event Action Connected;
        public event Action Disconnected;
        public event Action<Exception> ExceptionRaised;
		
		public const int DefaultBufferSize = 2 << 11;
		
        public Server(string host, int port, int bufferSize = DefaultBufferSize)
        {
            m_host = host;
            m_port = port;

            m_bufferSize = bufferSize;
        }

        void OnException(Exception ex)
        {
            Stop();

            if (ExceptionRaised != null)
                ExceptionRaised(ex);
        }

        public static IPAddress GetIPAddress(string host)
        {
            // Dns may be blocked without internet connection even if the host is actually an IP string.
            IPAddress result;
            if (IPAddress.TryParse(host, out result))
                return result;

            IPAddress[] candidates = Dns.GetHostAddresses(host);
            if (candidates.Length == 1)
                return candidates[0];

            var random = new Random();
            int index = random.Next(candidates.Length);
            return candidates[index];
        }

        #region Connecting

        public void Start()
        {
            if (m_client != null)
                return;

            IPAddress address = GetIPAddress(m_host);

            m_client = new TcpClient();
            m_client.BeginConnect(address, m_port, OnConnected, state: null);
        }

        void OnConnected(IAsyncResult result)
        {
            try
            {
                m_client.EndConnect(result);
                if (Connected != null) Connected();
                ReceiveLoop();
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        public void Stop()
        {
            if (m_client == null)
                return;

            m_client.Close();
            if (Disconnected != null) Disconnected();
            m_client = null;

            m_nBytesAvailable = 0;
        }

        #endregion

        #region Receiving

        void ReceiveLoop(byte[] buffer = null)
        {
            buffer = buffer ?? new byte[m_bufferSize];
            m_client.GetStream().BeginRead(buffer, 0, buffer.Length, OnReceived, state: buffer);
        }

        void OnReceived(IAsyncResult result)
        {
            var buffer = (byte[])result.AsyncState;
            try
            {
                int nByteReceived = m_client.GetStream().EndRead(result);
                if (nByteReceived == 0)
                {
                    Stop();
                    return;
                }
                m_nBytesAvailable += nByteReceived;
                TryDeserialize(buffer);
                ReceiveLoop(buffer);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        const int HeaderLength = sizeof(int) * 2;

        void TryDeserialize(byte[] buffer)
        {
			var json = System.Text.UTF8Encoding.Default.GetString(buffer);
			var jsonObj = LitJson.JsonMapper.ToObject(json);
			if (MessageReceived != null)
				MessageReceived(jsonObj);
        }

        #endregion

        #region Sending

        public void Send(object message)
        {
			var jObj = (LitJson.JsonData)message;
			var buffer = System.Text.UTF8Encoding.Default.GetBytes(jObj.ToJson());
            m_client.GetStream().BeginWrite(buffer, 0, buffer.Length, OnSent, state: null);
        }

        void OnSent(IAsyncResult result)
        {
            try
            {
                m_client.GetStream().EndWrite(result);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        #endregion
    }
}

