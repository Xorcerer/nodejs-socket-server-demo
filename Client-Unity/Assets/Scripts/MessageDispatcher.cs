using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Linq;
using System.Threading;

namespace Camp.HogRider
{
    public class MessageDispatcher
    {
        Queue<object> m_queue = new Queue<object>();
        Dictionary<Type, Action<object>> m_callbacks = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// Push the specified message, thread-safe.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Push(object message)
        {
            lock (m_queue)
            {
                m_queue.Enqueue(message);
            }
        }

        #region Registration

        public void RegisterHandlerHost(object host, bool inherit = true)
        {
            foreach (var m in host.GetType().GetMethods())
            {
                if (!Attribute.IsDefined(m, typeof(MessageHandlerAttribute), inherit))
                    continue;

                if (m.GetParameters().Length != 1)
                    throw new ArgumentException("Method '{0}' should accept and only accept 1 argument.", m.Name);

                Type messageType = m.GetParameters().Single().ParameterType;
                m_callbacks[messageType] = o => m.Invoke(host, new object[] { o });
            }
        }

        public void RegisterHandler<T>(Action<T> callback)
        {
            m_callbacks[typeof(T)] = o => callback((T)o);
        }

        public void ClearAllRegistrations()
        {
            m_callbacks.Clear();
        }
        #endregion Registration

        #region Dispatching

        /// <summary>
        /// Gets single message in received queue, thread-safe.
        /// </summary>
        /// <returns>The message.</returns>
        /// <exception cref="InvalidOperationException">Throw while the queue is empty.</exception>
        public object Pop()
        {
            lock (m_queue)
            {
                return m_queue.Dequeue();
            }
        }

        /// <summary>
        /// Dispatch received messages to registered handlers, thread-safe.
        /// </summary>
        public void Dispatch()
        {
            object[] messages;
            lock (m_queue)
            {
                int messageCount = m_queue.Count;
                if (messageCount == 0)
                    return;

                messages = new object[messageCount];
                for (int i = 0; i < messageCount; i++)
                    messages[i] = m_queue.Dequeue();
            }

            foreach (var m in messages)
            {
                Action<object> callback;
                if (m_callbacks.TryGetValue(m.GetType(), out callback))
                    callback(m);
                else
                {
                    if (UnregisteredMessageReceived != null)
                        UnregisteredMessageReceived(m);
                }
            }
        }

        /// <summary>
        /// Occurs when unregistered message received.
        /// </summary>
        public event Action<object> UnregisteredMessageReceived;

        #endregion Dispatching
    }
}

