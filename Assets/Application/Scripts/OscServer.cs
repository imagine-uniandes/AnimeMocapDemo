using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using uOSC;

public class OscServer : MonoBehaviour
{
    private UdpClient udp_;
    private AsyncCallback callback_;
    private Parser parser_ = new Parser();
    private object lockObject_ = new object();

    public class DataReceiveEvent : UnityEvent<Message> {};
    public DataReceiveEvent onDataReceived { get; private set; } = new DataReceiveEvent();

    public void Run(int port)
    {
        Stop();
        
        try
        {
            udp_ = new UdpClient(port);
            callback_ = new AsyncCallback(ReceiveCallback);
            udp_.BeginReceive(callback_, this);
        }
        catch(System.Exception)
        {
            Stop();
        }
    }

    public void Stop()
    {
        udp_?.Close();
        udp_ = null;
        callback_ = null;
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint remoteEP = null;
        byte[] buf = udp_.EndReceive(ar, ref remoteEP);
        int pos = 0;

        lock(lockObject_)
        {
            parser_.Parse(buf, ref pos, buf.Length);
        }
        
        udp_.BeginReceive(callback_, this);
    }

    private void Update()
    {
        lock(lockObject_)
        {
            while (parser_.messageCount > 0)
            {
                var message = parser_.Dequeue();
                onDataReceived.Invoke(message);
            }
        }
    }
}
